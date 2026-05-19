using Foreman.ProductionGraphView.Elements;
using System;
using System.Drawing;

namespace Foreman.ProductionGraphView.Annotations {
    /// <summary>Font/box sizing math for <see cref="TextAnnotationElement"/>.</summary>
    public static class TextAnnotationLayout {
        public const int DefaultPadding = 16;
        public const int MinBoxWidth = 60;
        public const int MinBoxHeight = 30;
        public const float MinFontSizePt = 6f;
        public const float MaxFontSizePt = 288f;

        public static Size MeasureBoxForText(
            string text,
            Font font,
            int padding = DefaultPadding,
            int minWidth = MinBoxWidth,
            int minHeight = MinBoxHeight) {
            if (string.IsNullOrEmpty(text))
                return new Size(minWidth, minHeight);

            using var bitmap = new Bitmap(1, 1);
            using var graphics = Graphics.FromImage(bitmap);
            SizeF natural = graphics.MeasureString(text, font);
            return new Size(
                Math.Max(minWidth, (int)Math.Ceiling(natural.Width) + padding),
                Math.Max(minHeight, (int)Math.Ceiling(natural.Height) + padding));
        }

        public static float ComputeResizeFontSize(
            float startFontSizePt,
            int startWidth,
            int startHeight,
            int newWidth,
            int newHeight) {
            if (startWidth <= 0 || startHeight <= 0)
                return startFontSizePt;

            float scale = (float)Math.Sqrt(
                (newWidth / (double)startWidth) * (newHeight / (double)startHeight));
            return Math.Clamp(startFontSizePt * scale, MinFontSizePt, MaxFontSizePt);
        }

        public static bool NearlyEqualFontSize(float a, float b) =>
            Math.Abs(a - b) < 0.05f;
    }
}
