using System.Text;
using Application.Abstractions.Services.Avatars;
using Application.DTO.Avatars;
using SkiaSharp;
using Tests.Support;

namespace Tests;

public sealed class AvatarImageProcessorTests
{
    private readonly AvatarImageProcessor _processor = new();

    private ProcessedAvatar Process(byte[] bytes, string fileName, string contentType)
    {
        return _processor.Process(bytes, fileName, contentType);
    }

    private void AssertRejected(string code, byte[] bytes, string fileName, string contentType)
    {
        var exception = Assert.Throws<AvatarException>(() => Process(bytes, fileName, contentType));

        Assert.Equal(code, exception.Code);
    }

    private static (int Width, int Height) Size(byte[] encoded)
    {
        using var bitmap = SKBitmap.Decode(encoded);

        return (bitmap.Width, bitmap.Height);
    }

    // ---------- допустимые форматы ----------

    [Theory]
    [InlineData("jpg", "image/jpeg", "jpg")]
    [InlineData("jpeg", "image/jpeg", "jpg")]
    [InlineData("JPG", "image/jpeg", "jpg")]
    [InlineData("png", "image/png", "png")]
    [InlineData("webp", "image/webp", "webp")]
    public void Accepts_supported_formats_and_keeps_format(string extension, string contentType, string expectedExtension)
    {
        var source = expectedExtension switch
        {
            "jpg" => TestImages.Jpeg(120, 80),
            "png" => TestImages.Png(120, 80),
            _ => TestImages.Webp(120, 80)
        };

        var result = Process(source, $"photo.{extension}", contentType);

        Assert.Equal(expectedExtension, result.Extension);
        Assert.Equal(contentType, result.ContentType);
        Assert.Equal((120, 80), Size(result.Bytes));
    }

    [Fact]
    public void Content_type_parameters_are_ignored()
    {
        var result = Process(TestImages.Png(100, 100), "a.png", "image/png; charset=binary");

        Assert.Equal("png", result.Extension);
    }

    [Fact]
    public void Empty_content_is_file_required()
    {
        AssertRejected(AvatarException.FileRequired, [], "a.png", "image/png");
    }

    // ---------- размеры в пикселях ----------

    [Theory]
    [InlineData(63, 64)]
    [InlineData(64, 63)]
    [InlineData(63, 63)]
    [InlineData(4097, 64)]
    [InlineData(64, 4097)]
    public void Rejects_dimensions_outside_range(int width, int height)
    {
        AssertRejected(AvatarException.InvalidDimensions, TestImages.Png(width, height), "a.png", "image/png");
    }

    [Theory]
    [InlineData(64, 64)]
    [InlineData(4096, 64)]
    [InlineData(64, 4096)]
    [InlineData(200, 64)]
    public void Accepts_boundary_dimensions(int width, int height)
    {
        var result = Process(TestImages.Png(width, height), "a.png", "image/png");

        Assert.Equal((width, height), Size(result.Bytes));
    }

    [Fact]
    public void Accepts_maximum_square()
    {
        var result = Process(TestImages.Png(4096, 4096), "a.png", "image/png");

        Assert.Equal((4096, 4096), Size(result.Bytes));
    }

    [Fact]
    public void Rejects_small_jpeg_and_webp()
    {
        AssertRejected(AvatarException.InvalidDimensions, TestImages.Jpeg(32, 200), "a.jpg", "image/jpeg");
        AssertRejected(AvatarException.InvalidDimensions, TestImages.Webp(200, 32), "a.webp", "image/webp");
    }

    // ---------- размеры в байтах ----------

    [Fact]
    public void Accepts_file_of_exactly_one_megabyte()
    {
        var file = TestImages.PadTo(TestImages.Png(100, 100), AvatarLimits.MaxBytes);

        var result = Process(file, "a.png", "image/png");

        Assert.Equal((100, 100), Size(result.Bytes));
    }

    [Fact]
    public void Rejects_file_one_byte_over_the_limit()
    {
        var file = TestImages.PadTo(TestImages.Png(100, 100), AvatarLimits.MaxBytes + 1);

        AssertRejected(AvatarException.TooLarge, file, "a.png", "image/png");
    }

    [Fact]
    public void Rejects_when_reencoded_result_exceeds_the_limit()
    {
        // Исходный JPEG помещается в лимит, но после перекодирования с качеством 90 вырастает сверх него
        using var noise = TestImages.NoiseAround(2000, 2000, amplitude: 60);

        byte[]? source = null;

        for (var quality = 60; quality >= 5 && source is null; quality -= 5)
        {
            var candidate = TestImages.Encode(noise, SKEncodedImageFormat.Jpeg, quality);

            if (candidate.Length > AvatarLimits.MaxBytes)
            {
                continue;
            }

            using var decoded = SKBitmap.Decode(candidate);
            var reencoded = TestImages.Encode(decoded, SKEncodedImageFormat.Jpeg, 90);

            if (reencoded.Length > AvatarLimits.MaxBytes)
            {
                source = candidate;
            }
        }

        Assert.True(source is not null, "не удалось подобрать JPEG, который после перекодирования превышает лимит");
        Assert.True(source!.Length <= AvatarLimits.MaxBytes);

        AssertRejected(AvatarException.TooLarge, source, "a.jpg", "image/jpeg");
    }

    // ---------- запрещённые и несовпадающие форматы ----------

    [Fact]
    public void Rejects_gif_including_static()
    {
        AssertRejected(AvatarException.InvalidFormat, TestImages.Gif(), "a.gif", "image/gif");
        AssertRejected(AvatarException.InvalidFormat, TestImages.Gif(), "a.png", "image/png");
    }

    [Fact]
    public void Rejects_svg_and_heic()
    {
        AssertRejected(AvatarException.InvalidFormat, TestImages.Svg(), "a.svg", "image/svg+xml");
        AssertRejected(AvatarException.InvalidFormat, TestImages.Svg(), "a.png", "image/png");
        AssertRejected(AvatarException.InvalidFormat, TestImages.Heic(), "a.heic", "image/heic");
        AssertRejected(AvatarException.InvalidFormat, TestImages.Heic(), "a.jpg", "image/jpeg");
    }

    [Fact]
    public void Rejects_signature_that_does_not_match_extension_or_content_type()
    {
        var png = TestImages.Png(100, 100);

        AssertRejected(AvatarException.InvalidFormat, png, "a.jpg", "image/png");
        AssertRejected(AvatarException.InvalidFormat, png, "a.png", "image/jpeg");
        AssertRejected(AvatarException.InvalidFormat, png, "a.webp", "image/webp");
        AssertRejected(AvatarException.InvalidFormat, png, "a", "image/png");
        AssertRejected(AvatarException.InvalidFormat, png, "a.png", "");
    }

    [Fact]
    public void Rejects_plain_text()
    {
        AssertRejected(AvatarException.InvalidFormat, Encoding.UTF8.GetBytes("not an image"), "a.png", "image/png");
    }

    // ---------- анимация ----------

    [Fact]
    public void Rejects_apng()
    {
        AssertRejected(AvatarException.AnimationNotAllowed, TestImages.Apng(100, 100), "a.png", "image/png");
    }

    [Fact]
    public void Rejects_animated_webp()
    {
        AssertRejected(AvatarException.AnimationNotAllowed, TestImages.AnimatedWebp(100, 100), "a.webp", "image/webp");
    }

    // ---------- повреждённые файлы ----------

    [Theory]
    [InlineData("jpg", "image/jpeg")]
    [InlineData("png", "image/png")]
    [InlineData("webp", "image/webp")]
    public void Rejects_truncated_files(string extension, string contentType)
    {
        using var noise = TestImages.Noise(200, 200, seed: 7);
        var format = extension switch
        {
            "jpg" => SKEncodedImageFormat.Jpeg,
            "png" => SKEncodedImageFormat.Png,
            _ => SKEncodedImageFormat.Webp
        };

        var full = TestImages.Encode(noise, format);
        var truncated = full[..(full.Length * 6 / 10)];

        AssertRejected(AvatarException.CorruptedImage, truncated, $"a.{extension}", contentType);
    }

    [Theory]
    [InlineData("jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })]
    [InlineData("png", "image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    public void Rejects_garbage_after_valid_signature(string extension, string contentType, byte[] signature)
    {
        var garbage = signature.Concat(Enumerable.Range(0, 500).Select(i => (byte)(i * 31 % 251))).ToArray();

        AssertRejected(AvatarException.CorruptedImage, garbage, $"a.{extension}", contentType);
    }

    [Fact]
    public void Rejects_riff_without_image_data()
    {
        var riff = Encoding.ASCII.GetBytes("RIFF").Concat(new byte[] { 4, 0, 0, 0 }).Concat(Encoding.ASCII.GetBytes("WEBP")).ToArray();

        AssertRejected(AvatarException.CorruptedImage, riff, "a.webp", "image/webp");
    }

    // ---------- усечение хвоста контейнера ----------

    private static byte[] Encoded(string extension, int width = 200, int height = 200)
    {
        using var noise = TestImages.Noise(width, height, seed: 11);
        var format = extension switch
        {
            "jpg" => SKEncodedImageFormat.Jpeg,
            "png" => SKEncodedImageFormat.Png,
            _ => SKEncodedImageFormat.Webp
        };

        return TestImages.Encode(noise, format);
    }

    private static string MimeOf(string extension) => extension switch
    {
        "jpg" => "image/jpeg",
        "png" => "image/png",
        _ => "image/webp"
    };

    [Theory]
    [InlineData("png", 1)] // без последнего байта CRC у IEND
    [InlineData("png", 4)]
    [InlineData("png", 8)]
    [InlineData("png", 12)] // без чанка IEND целиком
    [InlineData("png", 13)]
    [InlineData("webp", 1)] // без последнего байта
    [InlineData("webp", 2)]
    [InlineData("webp", 8)]
    [InlineData("jpg", 1)] // без второго байта маркера EOI
    [InlineData("jpg", 2)] // без маркера EOI
    [InlineData("jpg", 10)]
    public void Rejects_files_with_truncated_tail(string extension, int bytesCut)
    {
        var full = Encoded(extension);
        var truncated = full[..(full.Length - bytesCut)];

        AssertRejected(AvatarException.CorruptedImage, truncated, $"a.{extension}", MimeOf(extension));
    }

    [Theory]
    [InlineData("png")]
    [InlineData("webp")]
    [InlineData("jpg")]
    public void Complete_files_are_accepted(string extension)
    {
        var result = Process(Encoded(extension), $"a.{extension}", MimeOf(extension));

        Assert.Equal((200, 200), Size(result.Bytes));
    }

    [Theory]
    [InlineData("png")]
    [InlineData("webp")]
    [InlineData("jpg")]
    public void Trailing_padding_after_the_container_end_is_accepted(string extension)
    {
        var full = Encoded(extension);
        var padded = full.Concat(new byte[16]).ToArray();

        var result = Process(padded, $"a.{extension}", MimeOf(extension));

        Assert.Equal((200, 200), Size(result.Bytes));
    }

    [Fact]
    public void Rejects_png_whose_chunk_runs_past_the_end_of_file()
    {
        var full = Encoded("png");

        // Обрыв посреди данных IDAT, а не на границе чанка
        AssertRejected(AvatarException.CorruptedImage, full[..(full.Length / 2)], "a.png", "image/png");
    }

    [Fact]
    public void Rejects_webp_whose_riff_size_exceeds_the_file()
    {
        var full = Encoded("webp");
        var tampered = (byte[])full.Clone();
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(tampered.AsSpan(4, 4), (uint)full.Length + 100);

        AssertRejected(AvatarException.CorruptedImage, tampered, "a.webp", "image/webp");
    }

    [Fact]
    public void Jpeg_with_exif_segment_and_comment_is_still_recognised_as_complete()
    {
        var jpeg = TestImages.JpegWithExif(Encoded("jpg"), 1, "marker");

        var result = Process(jpeg, "a.jpg", "image/jpeg");

        Assert.Equal((200, 200), Size(result.Bytes));
    }

    // ---------- ориентация ----------

    [Theory]
    [InlineData(2)] // зеркало по горизонтали
    [InlineData(3)] // поворот на 180 градусов
    public void Applies_orientation_that_moves_left_side_to_the_right(int orientation)
    {
        using var source = TestImages.LeftRedRightBlue(100, 80);
        var jpeg = TestImages.JpegWithExif(TestImages.Encode(source, SKEncodedImageFormat.Jpeg, 95), orientation, "x");

        var result = Process(jpeg, "a.jpg", "image/jpeg");

        Assert.Equal((100, 80), Size(result.Bytes));

        using var decoded = SKBitmap.Decode(result.Bytes);
        AssertBlue(decoded.GetPixel(10, 40));
        AssertRed(decoded.GetPixel(90, 40));
    }

    [Fact]
    public void Orientation_6_rotates_clockwise()
    {
        using var source = TestImages.LeftRedRightBlue(100, 80);
        var jpeg = TestImages.JpegWithExif(TestImages.Encode(source, SKEncodedImageFormat.Jpeg, 95), 6, "x");

        var result = Process(jpeg, "a.jpg", "image/jpeg");

        // После поворота по часовой стрелке левая (красная) часть оказывается сверху
        Assert.Equal((80, 100), Size(result.Bytes));

        using var decoded = SKBitmap.Decode(result.Bytes);
        AssertRed(decoded.GetPixel(40, 10));
        AssertBlue(decoded.GetPixel(40, 90));
    }

    [Fact]
    public void Orientation_8_rotates_counterclockwise()
    {
        using var source = TestImages.LeftRedRightBlue(100, 80);
        var jpeg = TestImages.JpegWithExif(TestImages.Encode(source, SKEncodedImageFormat.Jpeg, 95), 8, "x");

        var result = Process(jpeg, "a.jpg", "image/jpeg");

        // После поворота против часовой стрелки левая (красная) часть оказывается снизу
        Assert.Equal((80, 100), Size(result.Bytes));

        using var decoded = SKBitmap.Decode(result.Bytes);
        AssertBlue(decoded.GetPixel(40, 10));
        AssertRed(decoded.GetPixel(40, 90));
    }

    [Fact]
    public void Orientation_1_keeps_picture_as_is()
    {
        using var source = TestImages.LeftRedRightBlue(100, 80);
        var jpeg = TestImages.JpegWithExif(TestImages.Encode(source, SKEncodedImageFormat.Jpeg, 95), 1, "x");

        var result = Process(jpeg, "a.jpg", "image/jpeg");

        using var decoded = SKBitmap.Decode(result.Bytes);
        Assert.Equal((100, 80), (decoded.Width, decoded.Height));
        AssertRed(decoded.GetPixel(10, 40));
        AssertBlue(decoded.GetPixel(90, 40));
    }

    // ---------- метаданные ----------

    [Fact]
    public void Strips_jpeg_metadata()
    {
        const string marker = "SECRET-GPS-MARKER";
        var jpeg = TestImages.JpegWithExif(TestImages.Jpeg(100, 100), 1, marker);

        var result = Process(jpeg, "a.jpg", "image/jpeg");

        var text = Encoding.Latin1.GetString(result.Bytes);
        Assert.DoesNotContain(marker, text);
        Assert.DoesNotContain("Exif", text);
        Assert.DoesNotContain("http://ns.adobe.com/xap", text);
    }

    [Theory]
    [InlineData("tEXt")]
    [InlineData("iTXt")]
    [InlineData("eXIf")]
    public void Strips_png_metadata_chunks(string chunkType)
    {
        var data = Encoding.ASCII.GetBytes("Comment\0SECRET-GPS-MARKER");
        var png = TestImages.PngWithChunk(TestImages.Png(100, 100), chunkType, data);

        var result = Process(png, "a.png", "image/png");

        var text = Encoding.Latin1.GetString(result.Bytes);
        Assert.DoesNotContain("SECRET-GPS-MARKER", text);
        Assert.DoesNotContain(chunkType, text);
    }

    [Fact]
    public void Strips_webp_exif_and_xmp_chunks()
    {
        var webp = TestImages.Webp(100, 100);
        var text = Encoding.Latin1.GetString(Process(webp, "a.webp", "image/webp").Bytes);

        Assert.DoesNotContain("EXIF", text);
        Assert.DoesNotContain("XMP ", text);
    }

    // ---------- прозрачность ----------

    [Fact]
    public void Keeps_png_transparency()
    {
        using var source = TestImages.Solid(100, 100, new SKColor(255, 0, 0, 128));
        var png = TestImages.Encode(source, SKEncodedImageFormat.Png);

        var result = Process(png, "a.png", "image/png");

        using var decoded = SKBitmap.Decode(result.Bytes);
        var pixel = decoded.GetPixel(50, 50);
        Assert.InRange(pixel.Alpha, 120, 136);
        Assert.True(pixel.Red > 240);
    }

    [Fact]
    public void Keeps_webp_transparency()
    {
        using var source = TestImages.Solid(100, 100, new SKColor(255, 0, 0, 128));
        var webp = TestImages.Encode(source, SKEncodedImageFormat.Webp);

        var result = Process(webp, "a.webp", "image/webp");

        using var decoded = SKBitmap.Decode(result.Bytes);
        var pixel = decoded.GetPixel(50, 50);
        Assert.InRange(pixel.Alpha, 112, 144);
    }

    private static void AssertRed(SKColor color)
    {
        Assert.True(color.Red > 200 && color.Blue < 80, $"ожидался красный, получено {color}");
    }

    private static void AssertBlue(SKColor color)
    {
        Assert.True(color.Blue > 200 && color.Red < 80, $"ожидался синий, получено {color}");
    }
}
