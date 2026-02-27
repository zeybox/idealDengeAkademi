using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HizliOgren.Services;

/// <summary>Basit sayı resimli CAPTCHA üretir; bot engelleme için.</summary>
public class CaptchaService
{
    private const int Width = 115;
    private const int Height = 28;
    private const int DigitCount = 5;
    private static readonly char[] Chars = { '2', '3', '4', '5', '6', '7', '8', '9' };

    // 5x7 basit 7-segment benzeri rakam desenleri (0-9), her satır 5 bit
    private static readonly byte[][] DigitPatterns =
    {
        new byte[] { 0x0E, 0x11, 0x13, 0x15, 0x19, 0x11, 0x0E }, // 0
        new byte[] { 0x04, 0x0C, 0x04, 0x04, 0x04, 0x04, 0x0E }, // 1
        new byte[] { 0x0E, 0x11, 0x01, 0x02, 0x04, 0x08, 0x1F }, // 2
        new byte[] { 0x1F, 0x02, 0x04, 0x02, 0x01, 0x11, 0x0E }, // 3
        new byte[] { 0x02, 0x06, 0x0A, 0x12, 0x1F, 0x02, 0x02 }, // 4
        new byte[] { 0x1F, 0x10, 0x1E, 0x01, 0x01, 0x11, 0x0E }, // 5
        new byte[] { 0x06, 0x08, 0x10, 0x1E, 0x11, 0x11, 0x0E }, // 6
        new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x08, 0x08 }, // 7
        new byte[] { 0x0E, 0x11, 0x11, 0x0E, 0x11, 0x11, 0x0E }, // 8
        new byte[] { 0x0E, 0x11, 0x11, 0x0F, 0x01, 0x02, 0x0C }  // 9
    };

    public (string Code, byte[] ImageBytes) Generate()
    {
        var random = Random.Shared;
        var code = new char[DigitCount];
        for (int i = 0; i < DigitCount; i++)
            code[i] = Chars[random.Next(Chars.Length)];
        var codeStr = new string(code);
        var imageBytes = DrawCaptchaImage(codeStr);
        return (codeStr, imageBytes);
    }

    private static byte[] DrawCaptchaImage(string text)
    {
        const int cellW = 3;
        const int cellH = 3;
        const int digitW = 5 * cellW;   // 15
        const int spacing = 5;   // rakamlar arası boşluk

        using var image = new Image<Rgba32>(Width, Height);
        var random = Random.Shared;

        // Arka plan
        image.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.FromRgb(245, 245, 250)));

        // Gürültü noktaları
        for (int i = 0; i < 50; i++)
        {
            int x = random.Next(0, Width);
            int y = random.Next(0, Height);
            var c = SixLabors.ImageSharp.Color.FromRgb((byte)random.Next(200, 230), (byte)random.Next(200, 230), (byte)random.Next(210, 240));
            image[x, y] = c;
        }

        // Gürültü çizgileri (dikey/yatay kısa)
        for (int i = 0; i < 5; i++)
        {
            int x0 = random.Next(0, Width);
            int y0 = random.Next(0, Height);
            int len = random.Next(8, 22);
            var lc = SixLabors.ImageSharp.Color.FromRgba((byte)random.Next(180, 220), (byte)random.Next(180, 220), (byte)random.Next(190, 230), 120);
            for (int k = 0; k < len; k++)
            {
                int x = Math.Clamp(x0 + (random.Next(0, 2) == 0 ? k : -k), 0, Width - 1);
                int y = Math.Clamp(y0, 0, Height - 1);
                image[x, y] = lc;
                if (random.Next(0, 2) == 0) y0 = Math.Clamp(y0 + (random.Next(0, 2) == 0 ? 1 : -1), 0, Height - 1);
            }
        }

        int startX = 4;
        int startY = 2;

        for (int d = 0; d < text.Length; d++)
        {
            int digit = text[d] - '0';
            if (digit < 0 || digit > 9) continue;
            var pattern = DigitPatterns[digit];
            var color = SixLabors.ImageSharp.Color.FromRgb((byte)random.Next(30, 90), (byte)random.Next(30, 90), (byte)random.Next(60, 120));
            int dx = startX + d * (digitW + spacing);
            int dy = startY;
            for (int row = 0; row < 7; row++)
            for (int col = 0; col < 5; col++)
            {
                if ((pattern[row] & (1 << (4 - col))) != 0)
                {
                    int px = dx + col * cellW;
                    int py = dy + row * cellH;
                    for (int bx = 0; bx < cellW && px + bx < Width; bx++)
                    for (int by = 0; by < cellH && py + by < Height; by++)
                        image[px + bx, py + by] = color;
                }
            }
        }

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }
}
