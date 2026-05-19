using Foreman.ProductionGraphView.Annotations;
using Foreman.Serialization;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    /// <summary>Freehand text label on the canvas, centred inside element bounds.</summary>
    public class TextAnnotationElement : AnnotationElement {
        private const int DefaultWidth = 200;
        private const int DefaultHeight = 60;

        public string Text { get; set; }
        public Font TextFont { get; set; }
        public Color TextColor { get; set; }
        public Color BackColor { get; set; }
        public StringAlignment TextAlign { get; set; }

        private SolidBrush? _textBrush;
        private SolidBrush? _backBrush;
        private StringFormat? _textFormat;
        private float _resizeStartFontSize;

        private static string _defaultFontFamily = Properties.Settings.Default.AnnotTextFontFamily;
        private static float _defaultFontSize = float.TryParse(Properties.Settings.Default.AnnotTextFontSize, out float fs) ? fs : 14f;
        private static FontStyle _defaultFontStyle = (FontStyle)Properties.Settings.Default.AnnotTextFontStyle;
        private static Color _defaultTextColor = Color.FromArgb(Properties.Settings.Default.AnnotTextColorARGB);
        private static Color _defaultBackColor = Color.FromArgb(Properties.Settings.Default.AnnotTextBackColorARGB);
        private static StringAlignment _defaultTextAlign = (StringAlignment)Properties.Settings.Default.AnnotTextAlign;

        public static void SaveDefaults(TextAnnotationElement element) {
            _defaultFontFamily = element.TextFont.FontFamily.Name;
            _defaultFontSize = element.TextFont.SizeInPoints;
            _defaultFontStyle = element.TextFont.Style;
            _defaultTextColor = element.TextColor;
            _defaultBackColor = element.BackColor;
            _defaultTextAlign = element.TextAlign;

            var settings = Properties.Settings.Default;
            settings.AnnotTextFontFamily = _defaultFontFamily;
            settings.AnnotTextFontSize = _defaultFontSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
            settings.AnnotTextFontStyle = (int)_defaultFontStyle;
            settings.AnnotTextColorARGB = _defaultTextColor.ToArgb();
            settings.AnnotTextBackColorARGB = _defaultBackColor.ToArgb();
            settings.AnnotTextAlign = (int)_defaultTextAlign;
            settings.Save();
        }

        public TextAnnotationElement(ProductionGraphViewer graphViewer, Point graphLocation)
            : base(graphViewer, graphLocation, DefaultWidth, DefaultHeight) {
            Text = "Label";
            TextFont = CreateDefaultFont();
            TextColor = _defaultTextColor;
            BackColor = _defaultBackColor;
            TextAlign = _defaultTextAlign;
            RebuildGdiObjects();
            FitBoxToTextAtCenter();
        }

        private TextAnnotationElement(
            ProductionGraphViewer graphViewer,
            Point location,
            Size size,
            string text,
            Font textFont,
            Color textColor,
            Color backColor,
            StringAlignment textAlign)
            : base(graphViewer, location, size.Width, size.Height) {
            Text = text;
            TextFont = textFont;
            TextColor = textColor;
            BackColor = backColor;
            TextAlign = textAlign;
            RebuildGdiObjects();
        }

        private static Font CreateDefaultFont() {
            try {
                return new Font(_defaultFontFamily, _defaultFontSize, _defaultFontStyle, GraphicsUnit.Point);
            } catch {
                return new Font("Segoe UI", 14f, FontStyle.Bold, GraphicsUnit.Point);
            }
        }

        /// <summary>Measures text and sets Width/Height around the unchanged center (X, Y).</summary>
        public void FitBoxToTextAtCenter() {
            Size box = TextAnnotationLayout.MeasureBoxForText(Text, TextFont);
            Width = box.Width;
            Height = box.Height;
        }

        public void SetFontSizeInPoints(float sizeInPoints) {
            sizeInPoints = Math.Clamp(sizeInPoints, TextAnnotationLayout.MinFontSizePt, TextAnnotationLayout.MaxFontSizePt);
            if (TextAnnotationLayout.NearlyEqualFontSize(TextFont.SizeInPoints, sizeInPoints))
                return;

            Font previous = TextFont;
            TextFont = new Font(previous.FontFamily, sizeInPoints, previous.Style, GraphicsUnit.Point);
            previous.Dispose();
            RebuildGdiObjects();
        }

        public override void MouseDown(Point graphPoint, MouseButtons button) {
            base.MouseDown(graphPoint, button);
            if (button == MouseButtons.Left && IsResizing)
                _resizeStartFontSize = TextFont.SizeInPoints;
        }

        protected override void OnResized() {
            float newSize = TextAnnotationLayout.ComputeResizeFontSize(
                _resizeStartFontSize,
                ResizeStartWidth,
                ResizeStartHeight,
                Width,
                Height);
            SetFontSizeInPoints(newSize);
        }

        public void RebuildGdiObjects() {
            DisposeGdiObjects();

            _textBrush = new SolidBrush(TextColor);
            _backBrush = BackColor.A > 0 ? new SolidBrush(BackColor) : null;
            _textFormat = new StringFormat {
                Alignment = TextAlign,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None,
                FormatFlags = StringFormatFlags.NoWrap
            };
        }

        private void DisposeGdiObjects() {
            _textBrush?.Dispose();
            _backBrush?.Dispose();
            _textFormat?.Dispose();
            _textBrush = null;
            _backBrush = null;
            _textFormat = null;
        }

        public override bool ContainsPoint(Point graphPoint) {
            return Visible && (IsSelected && GetHandleAtPoint(graphPoint) != HandleType.None || Bounds.Contains(GraphToLocal(graphPoint)));
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            Rectangle bounds = GetGraphRect();
            DrawSelectionHighlight(graphics, bounds);

            if (_backBrush is not null)
                graphics.FillRectangle(_backBrush, bounds);

            if (!string.IsNullOrEmpty(Text) && _textBrush is not null && _textFormat is not null)
                graphics.DrawString(Text, TextFont, _textBrush, (RectangleF)bounds, _textFormat);

            DrawResizeHandles(graphics);
        }

        public override void ShowPropertiesDialog() {
            using var form = new TextPropertiesForm(this);
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog(graphViewer.FindForm()) == DialogResult.OK) {
                RebuildGdiObjects();
                graphViewer.Invalidate();
            }
        }

        public override AnnotationSaveData ToSaveData() => new TextAnnotationSaveData {
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Text = Text,
            FontFamily = TextFont.FontFamily.Name,
            FontSize = TextFont.SizeInPoints,
            FontStyle = (int)TextFont.Style,
            TextColor = ColorToSave(TextColor),
            BackColor = ColorToSave(BackColor),
            TextAlign = (int)TextAlign
        };

        public static TextAnnotationElement FromSaveData(TextAnnotationSaveData data, ProductionGraphViewer graphViewer) {
            Font font = TryCreateFont(data.FontFamily, data.FontSize, (FontStyle)data.FontStyle);
            var align = data.TextAlign is >= 0 and <= 2 ? (StringAlignment)data.TextAlign : StringAlignment.Center;
            return new TextAnnotationElement(
                graphViewer,
                new Point(data.X, data.Y),
                new Size(data.Width, data.Height),
                data.Text,
                font,
                ColorFromSave(data.TextColor),
                ColorFromSave(data.BackColor),
                align);
        }

        private static Font TryCreateFont(string family, float size, FontStyle style) {
            try {
                return new Font(family, size, style, GraphicsUnit.Point);
            } catch {
                return new Font("Segoe UI", 14f, FontStyle.Bold, GraphicsUnit.Point);
            }
        }

        public override void Dispose() {
            GC.SuppressFinalize(this);
            DisposeGdiObjects();
            TextFont.Dispose();
            base.Dispose();
        }
    }
}
