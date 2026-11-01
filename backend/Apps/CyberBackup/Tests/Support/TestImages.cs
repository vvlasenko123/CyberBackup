using System.Buffers.Binary;
using System.Text;
using SkiaSharp;

namespace Tests.Support;

/// <summary>
/// Генерация тестовых изображений и ручная сборка файлов с метаданными и анимацией
/// </summary>
internal static class TestImages
{
    private static readonly uint[] CrcTable = BuildCrcTable();

    public static SKBitmap Solid(int width, int height, SKColor color)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        bitmap.Erase(color);

        return bitmap;
    }

    /// <summary>
    /// Слева красный, справа синий
    /// </summary>
    public static SKBitmap LeftRedRightBlue(int width, int height)
    {
        var bitmap = Solid(width, height, SKColors.Red);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { Color = SKColors.Blue };
        canvas.DrawRect(width / 2f, 0, width - width / 2f, height, paint);

        return bitmap;
    }

    public static SKBitmap Noise(int width, int height, int seed = 1)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));
        var random = new Random(seed);
        var pixels = new byte[width * height * 4];
        random.NextBytes(pixels);

        for (var i = 3; i < pixels.Length; i += 4)
        {
            pixels[i] = 255;
        }

        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);

        return bitmap;
    }

    /// <summary>
    /// Серый фон с шумом заданной амплитуды: после JPEG с низким качеством остаётся мелкая структура
    /// </summary>
    public static SKBitmap NoiseAround(int width, int height, int amplitude, int seed = 1)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));
        var random = new Random(seed);
        var pixels = new byte[width * height * 4];

        for (var i = 0; i < pixels.Length; i += 4)
        {
            for (var c = 0; c < 3; c++)
            {
                pixels[i + c] = (byte)Math.Clamp(128 + random.Next(-amplitude, amplitude + 1), 0, 255);
            }

            pixels[i + 3] = 255;
        }

        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);

        return bitmap;
    }

    public static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format, int quality = 90)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, quality);

        return data.ToArray();
    }

    public static byte[] Png(int width, int height, SKColor? color = null)
    {
        using var bitmap = Solid(width, height, color ?? SKColors.SteelBlue);

        return Encode(bitmap, SKEncodedImageFormat.Png);
    }

    public static byte[] Jpeg(int width, int height, SKColor? color = null)
    {
        using var bitmap = Solid(width, height, color ?? SKColors.SteelBlue);

        return Encode(bitmap, SKEncodedImageFormat.Jpeg);
    }

    public static byte[] Webp(int width, int height, SKColor? color = null)
    {
        using var bitmap = Solid(width, height, color ?? SKColors.SteelBlue);

        return Encode(bitmap, SKEncodedImageFormat.Webp);
    }

    /// <summary>
    /// Вставляет в JPEG сегмент EXIF с тегом Orientation и комментарий-маркер,
    /// который не должен пережить перекодирование
    /// </summary>
    public static byte[] JpegWithExif(byte[] jpeg, int orientation, string commentMarker)
    {
        var tiff = new List<byte>
        {
            0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, (byte)orientation, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        var exifPayload = Encoding.ASCII.GetBytes("Exif\0\0").Concat(tiff).ToArray();
        var comment = Encoding.ASCII.GetBytes(commentMarker);

        var result = new List<byte> { 0xFF, 0xD8 };
        result.AddRange(Segment(0xE1, exifPayload));
        result.AddRange(Segment(0xFE, comment));
        result.AddRange(jpeg.Skip(2));

        return result.ToArray();
    }

    private static IEnumerable<byte> Segment(byte marker, byte[] payload)
    {
        var length = payload.Length + 2;

        return new byte[] { 0xFF, marker, (byte)(length >> 8), (byte)length }.Concat(payload);
    }

    /// <summary>
    /// Вставляет в PNG чанк сразу после IHDR (с корректной контрольной суммой)
    /// </summary>
    public static byte[] PngWithChunk(byte[] png, string type, byte[] data)
    {
        const int afterIhdr = 8 + 25;

        var chunk = new byte[12 + data.Length];
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(0, 4), (uint)data.Length);
        Encoding.ASCII.GetBytes(type).CopyTo(chunk, 4);
        data.CopyTo(chunk, 8);

        var crcInput = chunk.AsSpan(4, 4 + data.Length).ToArray();
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(8 + data.Length, 4), Crc32(crcInput));

        return png[..afterIhdr].Concat(chunk).Concat(png[afterIhdr..]).ToArray();
    }

    /// <summary>
    /// PNG с заголовком анимации acTL (признак APNG)
    /// </summary>
    public static byte[] Apng(int width, int height)
    {
        var actl = new byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(actl.AsSpan(0, 4), 2);
        BinaryPrimitives.WriteUInt32BigEndian(actl.AsSpan(4, 4), 0);

        return PngWithChunk(Png(width, height), "acTL", actl);
    }

    /// <summary>
    /// WebP с флагом анимации в VP8X и чанками ANIM/ANMF
    /// </summary>
    public static byte[] AnimatedWebp(int width, int height)
    {
        var still = Webp(width, height);
        var stillChunks = still[12..];

        var body = new List<byte>();
        body.AddRange(Encoding.ASCII.GetBytes("WEBP"));

        var vp8x = new byte[10];
        vp8x[0] = 0x02;
        WriteUInt24(vp8x.AsSpan(4, 3), width - 1);
        WriteUInt24(vp8x.AsSpan(7, 3), height - 1);
        body.AddRange(RiffChunk("VP8X", vp8x));
        body.AddRange(RiffChunk("ANIM", new byte[6]));

        var anmf = new byte[16].Concat(stillChunks).ToArray();
        body.AddRange(RiffChunk("ANMF", anmf));

        var riff = new List<byte>();
        riff.AddRange(Encoding.ASCII.GetBytes("RIFF"));
        riff.AddRange(BitConverter.GetBytes((uint)body.Count));
        riff.AddRange(body);

        return riff.ToArray();
    }

    /// <summary>
    /// Дополняет файл нулями после конца данных до нужной длины: декодеры игнорируют хвост
    /// </summary>
    public static byte[] PadTo(byte[] file, int length)
    {
        var result = new byte[length];
        file.CopyTo(result, 0);

        return result;
    }

    public static byte[] Gif() => Encoding.ASCII.GetBytes("GIF89a")
        .Concat(new byte[] { 1, 0, 1, 0, 0x80, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0x2C, 0, 0, 0, 0, 1, 0, 1, 0, 0, 2, 2, 0x44, 1, 0, 0x3B })
        .ToArray();

    public static byte[] Svg() => Encoding.UTF8.GetBytes(
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\"><rect width=\"100\" height=\"100\"/></svg>");

    public static byte[] Heic() => new byte[] { 0, 0, 0, 0x18 }
        .Concat(Encoding.ASCII.GetBytes("ftypheic"))
        .Concat(new byte[64])
        .ToArray();

    private static IEnumerable<byte> RiffChunk(string type, byte[] data)
    {
        var result = new List<byte>();
        result.AddRange(Encoding.ASCII.GetBytes(type));
        result.AddRange(BitConverter.GetBytes((uint)data.Length));
        result.AddRange(data);

        if (data.Length % 2 == 1)
        {
            result.Add(0);
        }

        return result;
    }

    private static void WriteUInt24(Span<byte> target, int value)
    {
        target[0] = (byte)value;
        target[1] = (byte)(value >> 8);
        target[2] = (byte)(value >> 16);
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];

        for (uint n = 0; n < 256; n++)
        {
            var c = n;

            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFF;

        foreach (var b in data)
        {
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }

        return crc ^ 0xFFFFFFFF;
    }
}
