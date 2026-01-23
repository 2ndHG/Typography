using System.Drawing;
using System.Diagnostics;
using Typography.OpenFont;
using SimpleSdf;

namespace Demo
{
    public static class Program
    {
        static void Main(string[] args)
        {
            OpenFontReader fontReader = new OpenFontReader();
            using FileStream file = File.OpenRead("huninn.ttf");
            Typeface? typeface = fontReader.Read(file);
            if (typeface is not null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                SimpleSDF(typeface, '3');
                Console.WriteLine(stopwatch.Elapsed.TotalSeconds);
            }
        }

        private static void SimpleSDF(Typeface typeface, char c)
        {
            int unitsPerEm = typeface.UnitsPerEm;
            ushort glyphIndex = typeface.GetGlyphIndex(c);
            Glyph glyph = typeface.GetGlyph(glyphIndex);

            Bounds bounds = glyph.Bounds;

            float fontSize = 48f;
            float scale = fontSize / unitsPerEm;

            int padding = 4;
            float range = 4f;

            float contentW = (bounds.XMax - bounds.XMin) * scale;
            float contentH = (bounds.YMax - bounds.YMin) * scale;

            int bitmapWidth = (int)Math.Ceiling(contentW) + padding * 2;
            int bitmapHeight = (int)Math.Ceiling(contentH) + padding * 2;

            float translateX = -bounds.XMin * scale + padding;
            float translateY = bounds.YMax * scale + padding;

            Shape shape = new (glyph);
            float[] sdfData = SdfGenerator.Generate(shape,
                bitmapWidth,
                bitmapHeight,
                scale,
                translateX,
                translateY,
                range
            );

            byte[] pixelData = new byte[sdfData.Length];
            for (int i = 0; i < sdfData.Length; i++)
            {
                pixelData[i] = (byte)(sdfData[i] * 255);
            }

            using (Bitmap bitmap = new (bitmapWidth, bitmapHeight))
            {
                for (int y = 0; y < bitmapHeight; y++)
                {
                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        int val = pixelData[y * bitmapWidth + x];
                        bitmap.SetPixel(x, y, Color.FromArgb(val, val, val));
                    }
                }
                bitmap.Save($"{c}_sdf.png");
            }
        }
    }
}
