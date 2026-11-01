using System.Buffers.Binary;
using System.Text;
using Application.Abstractions.Services.Avatars.Contracts;
using Application.DTO.Avatars;
using SkiaSharp;

namespace Application.Abstractions.Services.Avatars;

/// <inheritdoc />
public sealed class AvatarImageProcessor : IAvatarImageProcessor
{
    private const int JpegQuality = 90;
    private const int WebpQuality = 90;

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <inheritdoc />
    public ProcessedAvatar Process(byte[] content, string fileName, string contentType)
    {
        if (content.Length == 0)
        {
            throw new AvatarException(AvatarException.FileRequired, "Файл не передан");
        }

        if (content.Length > AvatarLimits.MaxBytes)
        {
            throw new AvatarException(AvatarException.TooLarge, "Размер файла не должен превышать 1 МБ");
        }

        var format = DetectFormat(content);
        EnsureFormatMatchesDeclaration(format, fileName, contentType);

        // Признаки анимации читаются из заголовков файла и не зависят от того, примет ли такой файл декодер
        if (HasAnimationMarkers(format, content))
        {
            throw AnimationError();
        }

        // Успешное декодирование не доказывает, что файл дошёл целиком: Skia принимает PNG без IEND
        // и WebP без последних байтов, поэтому полноту контейнера проверяем отдельно
        if (!IsContainerComplete(format, content))
        {
            throw CorruptedError();
        }

        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);

        if (codec is null)
        {
            throw CorruptedError();
        }

        var info = codec.Info;

        if (info.Width < AvatarLimits.MinSide || info.Height < AvatarLimits.MinSide
            || info.Width > AvatarLimits.MaxSide || info.Height > AvatarLimits.MaxSide)
        {
            throw new AvatarException(
                AvatarException.InvalidDimensions,
                $"Каждая сторона изображения должна быть от {AvatarLimits.MinSide} до {AvatarLimits.MaxSide} пикселей");
        }

        // Несколько кадров у кодека - ещё один признак анимации (JPEG всегда одно изображение)
        if (format != AvatarFormat.Jpeg && codec.FrameCount > 1)
        {
            throw AnimationError();
        }

        using var decoded = Decode(codec);
        using var oriented = ApplyOrigin(decoded, codec.EncodedOrigin);

        var encoded = Encode(oriented, format);

        if (encoded.Length > AvatarLimits.MaxBytes)
        {
            throw new AvatarException(AvatarException.TooLarge, "Размер изображения после обработки превышает 1 МБ");
        }

        return new ProcessedAvatar(encoded, format.Extension, format.ContentType);
    }

    private static AvatarFormat DetectFormat(byte[] content)
    {
        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
        {
            return AvatarFormat.Jpeg;
        }

        if (content.AsSpan().StartsWith(PngSignature))
        {
            return AvatarFormat.Png;
        }

        if (content.Length >= 12
            && content.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && content.AsSpan(8, 4).SequenceEqual("WEBP"u8))
        {
            return AvatarFormat.Webp;
        }

        // GIF, SVG, HEIC и всё остальное
        throw new AvatarException(
            AvatarException.InvalidFormat,
            "Допустимы только изображения JPEG, PNG и WebP");
    }

    private static void EnsureFormatMatchesDeclaration(AvatarFormat format, string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).TrimStart('.').ToLowerInvariant();
        var mime = (contentType ?? string.Empty).Split(';')[0].Trim().ToLowerInvariant();

        var extensionMatches = format.Extensions.Contains(extension);
        var mimeMatches = mime == format.ContentType;

        if (!extensionMatches || !mimeMatches)
        {
            throw new AvatarException(
                AvatarException.InvalidFormat,
                "Формат файла не совпадает с расширением или Content-Type");
        }
    }

    private static bool HasAnimationMarkers(AvatarFormat format, byte[] content)
    {
        return (format == AvatarFormat.Png && HasPngAnimationChunk(content))
               || (format == AvatarFormat.Webp && HasWebpAnimation(content));
    }

    private static AvatarException CorruptedError() => new(
        AvatarException.CorruptedImage,
        "Изображение повреждено или не читается");

    private static bool IsContainerComplete(AvatarFormat format, byte[] content)
    {
        if (format == AvatarFormat.Png)
        {
            return IsPngComplete(content);
        }

        return format == AvatarFormat.Webp ? IsWebpComplete(content) : IsJpegComplete(content);
    }

    /// <summary>
    /// PNG: цепочка чанков должна дойти до пустого IEND после IHDR и хотя бы одного IDAT.
    /// Данные после IEND (например, дополнение нулями) допускаются
    /// </summary>
    private static bool IsPngComplete(byte[] content)
    {
        var offset = PngSignature.Length;
        var first = true;
        var seenData = false;

        while (offset + 12 <= content.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(content.AsSpan(offset, 4));
            var type = Encoding.ASCII.GetString(content, offset + 4, 4);

            if (length > content.Length - offset - 12)
            {
                return false;
            }

            if (first && type != "IHDR")
            {
                return false;
            }

            first = false;

            if (type == "IDAT")
            {
                seenData = true;
            }

            if (type == "IEND")
            {
                return length == 0 && seenData;
            }

            offset += 12 + (int)length;
        }

        return false;
    }

    /// <summary>
    /// WebP: размер в заголовке RIFF должен укладываться в файл, а чанки - точно покрывать RIFF-блок.
    /// Данные после RIFF-блока допускаются
    /// </summary>
    private static bool IsWebpComplete(byte[] content)
    {
        var riffSize = BinaryPrimitives.ReadUInt32LittleEndian(content.AsSpan(4, 4));
        var end = 8L + riffSize;

        if (riffSize < 4 + 8 || end > content.Length)
        {
            return false;
        }

        long offset = 12;

        while (offset < end)
        {
            if (offset + 8 > end)
            {
                return false;
            }

            var size = BinaryPrimitives.ReadUInt32LittleEndian(content.AsSpan((int)offset + 4, 4));

            if (offset + 8 + size > end)
            {
                return false;
            }

            // Размер чанка выровнен до чётного, дополняющий байт обязателен, если чанк не последний
            offset += 8 + size + (size & 1);
        }

        return true;
    }

    /// <summary>
    /// JPEG: после заголовков и данных сканов должен встретиться маркер EOI (FFD9).
    /// Хвост после EOI допускается (миниатюры, MPF)
    /// </summary>
    private static bool IsJpegComplete(byte[] content)
    {
        var offset = 2;

        while (offset + 1 < content.Length)
        {
            if (content[offset] != 0xFF)
            {
                // Внутри данных скана ищем следующий маркер
                offset++;
                continue;
            }

            var marker = content[offset + 1];

            if (marker == 0xFF)
            {
                offset++;
                continue;
            }

            if (marker == 0xD9)
            {
                return true;
            }

            // Заполнение (FF00), счётчики перезапуска и маркеры без длины
            if (marker == 0x00 || marker is >= 0xD0 and <= 0xD8 || marker == 0x01)
            {
                offset += 2;
                continue;
            }

            if (offset + 4 > content.Length)
            {
                return false;
            }

            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(offset + 2, 2));

            if (segmentLength < 2)
            {
                return false;
            }

            offset += 2 + segmentLength;
        }

        return false;
    }

    private static AvatarException AnimationError() => new(
        AvatarException.AnimationNotAllowed,
        "Анимированные изображения не поддерживаются");

    /// <summary>
    /// APNG содержит чанк acTL до первого IDAT
    /// </summary>
    private static bool HasPngAnimationChunk(byte[] content)
    {
        var offset = PngSignature.Length;

        while (offset + 8 <= content.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(content.AsSpan(offset, 4));
            var type = Encoding.ASCII.GetString(content, offset + 4, 4);

            if (type == "acTL")
            {
                return true;
            }

            if (type is "IDAT" or "IEND")
            {
                return false;
            }

            var next = (long)offset + 12 + length;

            if (next > content.Length)
            {
                return false;
            }

            offset = (int)next;
        }

        return false;
    }

    /// <summary>
    /// Анимированный WebP: флаг анимации в VP8X либо чанки ANIM / ANMF
    /// </summary>
    private static bool HasWebpAnimation(byte[] content)
    {
        var offset = 12;

        while (offset + 8 <= content.Length)
        {
            var type = Encoding.ASCII.GetString(content, offset, 4);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(content.AsSpan(offset + 4, 4));

            if (type == "VP8X" && offset + 9 <= content.Length && (content[offset + 8] & 0x02) != 0)
            {
                return true;
            }

            if (type is "ANIM" or "ANMF")
            {
                return true;
            }

            // Размер чанка выровнен до чётного
            var next = (long)offset + 8 + length + (length & 1);

            if (next > content.Length)
            {
                return false;
            }

            offset = (int)next;
        }

        return false;
    }

    private static SKBitmap Decode(SKCodec codec)
    {
        var info = codec.Info;
        var alphaType = info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Premul;
        var targetInfo = new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, alphaType);
        var bitmap = new SKBitmap(targetInfo);

        try
        {
            var result = codec.GetPixels(targetInfo, bitmap.GetPixels());

            // Усечённый файл даёт IncompleteInput - это такая же ошибка, как и нечитаемый
            if (result != SKCodecResult.Success)
            {
                throw new AvatarException(AvatarException.CorruptedImage, "Изображение повреждено или не читается");
            }

            return bitmap;
        }
        catch (AvatarException)
        {
            bitmap.Dispose();

            throw;
        }
        catch (Exception)
        {
            bitmap.Dispose();

            throw new AvatarException(AvatarException.CorruptedImage, "Изображение повреждено или не читается");
        }
    }

    /// <summary>
    /// Применяет EXIF Orientation, возвращает новый битмап
    /// </summary>
    private static SKBitmap ApplyOrigin(SKBitmap source, SKEncodedOrigin origin)
    {
        var width = source.Width;
        var height = source.Height;

        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

        // Матрицы: x' = ScaleX * x + SkewX * y + TransX, y' = SkewY * x + ScaleY * y + TransY
        var matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, width, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, width, 0, -1, height, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, height, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, height, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, height, -1, 0, width, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, width, 0, 0, 1),
            _ => SKMatrix.Identity
        };

        var targetInfo = new SKImageInfo(
            swap ? height : width,
            swap ? width : height,
            SKColorType.Rgba8888,
            source.AlphaType);

        var result = new SKBitmap(targetInfo);

        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(source, 0, 0);

        return result;
    }

    private static byte[] Encode(SKBitmap bitmap, AvatarFormat format)
    {
        using var image = SKImage.FromBitmap(bitmap);

        var skiaFormat = format == AvatarFormat.Jpeg
            ? SKEncodedImageFormat.Jpeg
            : format == AvatarFormat.Png
                ? SKEncodedImageFormat.Png
                : SKEncodedImageFormat.Webp;

        var quality = format == AvatarFormat.Jpeg ? JpegQuality : format == AvatarFormat.Webp ? WebpQuality : 100;

        using var encoded = image.Encode(skiaFormat, quality);

        if (encoded is null)
        {
            throw new AvatarException(AvatarException.CorruptedImage, "Не удалось закодировать изображение");
        }

        return encoded.ToArray();
    }

    private sealed class AvatarFormat
    {
        public static readonly AvatarFormat Jpeg = new("jpg", "image/jpeg", ["jpg", "jpeg"]);
        public static readonly AvatarFormat Png = new("png", "image/png", ["png"]);
        public static readonly AvatarFormat Webp = new("webp", "image/webp", ["webp"]);

        private AvatarFormat(string extension, string contentType, string[] extensions)
        {
            Extension = extension;
            ContentType = contentType;
            Extensions = extensions;
        }

        public string Extension { get; }

        public string ContentType { get; }

        public string[] Extensions { get; }
    }
}
