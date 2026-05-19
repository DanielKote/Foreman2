using Foreman.Serialization;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    /// <summary>Rectangle or ellipse annotation on the canvas.</summary>
    public class ShapeAnnotationElement : AnnotationElement {
        public enum ShapeType { Rectangle, Ellipse }

        private const int DefaultWidth = 200;
        private const int DefaultHeight = 150;

        public ShapeType CurrentShapeType { get; set; }
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }

        private SolidBrush? _fillBrush;
        private Pen? _borderPen;

        private static ShapeType _defaultShapeType = (ShapeType)Properties.Settings.Default.AnnotShapeType;
        private static Color _defaultFillColor = Color.FromArgb(Properties.Settings.Default.AnnotShapeFillColorARGB);
        private static Color _defaultBorderColor = Color.FromArgb(Properties.Settings.Default.AnnotShapeBorderColorARGB);
        private static int _defaultBorderWidth = Properties.Settings.Default.AnnotShapeBorderWidth;

        public static void SaveDefaults(ShapeAnnotationElement element) {
            _defaultShapeType = element.CurrentShapeType;
            _defaultFillColor = element.FillColor;
            _defaultBorderColor = element.BorderColor;
            _defaultBorderWidth = element.BorderWidth;

            var s = Properties.Settings.Default;
            s.AnnotShapeType = (int)_defaultShapeType;
            s.AnnotShapeFillColorARGB = _defaultFillColor.ToArgb();
            s.AnnotShapeBorderColorARGB = _defaultBorderColor.ToArgb();
            s.AnnotShapeBorderWidth = _defaultBorderWidth;
            s.Save();
        }

        public ShapeAnnotationElement(ProductionGraphViewer graphViewer, Point graphLocation)
            : base(graphViewer, graphLocation, DefaultWidth, DefaultHeight) {
            ApplyDefaults();
        }

        public ShapeAnnotationElement(ProductionGraphViewer graphViewer, Point graphLocation, int width, int height)
            : base(graphViewer, graphLocation, width, height) {
            ApplyDefaults();
        }

        private void ApplyDefaults() {
            CurrentShapeType = _defaultShapeType;
            FillColor = _defaultFillColor;
            BorderColor = _defaultBorderColor;
            BorderWidth = _defaultBorderWidth;
            RebuildGdiObjects();
        }

        private ShapeAnnotationElement(ProductionGraphViewer graphViewer,
            Point location, Size size, ShapeType shapeType,
            Color fillColor, Color borderColor, int borderWidth)
            : base(graphViewer, location, size.Width, size.Height) {
            CurrentShapeType = shapeType;
            FillColor = fillColor;
            BorderColor = borderColor;
            BorderWidth = borderWidth;
            RebuildGdiObjects();
        }

        public void RebuildGdiObjects() {
            _fillBrush?.Dispose();
            _borderPen?.Dispose();
            _fillBrush = new SolidBrush(FillColor);
            _borderPen = new Pen(BorderColor, Math.Max(1, BorderWidth)) { Alignment = PenAlignment.Inset };
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            Rectangle r = GetGraphRect();
            DrawSelectionHighlight(graphics, r);

            if (FillColor.A > 0 && _fillBrush is not null) {
                switch (CurrentShapeType) {
                    case ShapeType.Rectangle:
                        graphics.FillRectangle(_fillBrush, r);
                        break;
                    case ShapeType.Ellipse:
                        graphics.FillEllipse(_fillBrush, r);
                        break;
                }
            }

            if (BorderWidth > 0 && BorderColor.A > 0 && _borderPen is not null) {
                switch (CurrentShapeType) {
                    case ShapeType.Rectangle:
                        graphics.DrawRectangle(_borderPen, r);
                        break;
                    case ShapeType.Ellipse:
                        graphics.DrawEllipse(_borderPen, r);
                        break;
                }
            }

            DrawResizeHandles(graphics);
        }

        public override void ShowPropertiesDialog() {
            using var form = new ShapePropertiesForm(this);
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog(graphViewer.FindForm()) == DialogResult.OK) {
                RebuildGdiObjects();
                graphViewer.Invalidate();
            }
        }

        public override AnnotationSaveData ToSaveData() => new ShapeAnnotationSaveData {
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            ShapeType = CurrentShapeType.ToString(),
            FillColor = ColorToSave(FillColor),
            BorderColor = ColorToSave(BorderColor),
            BorderWidth = BorderWidth
        };

        public static ShapeAnnotationElement FromSaveData(ShapeAnnotationSaveData data, ProductionGraphViewer graphViewer) {
            var shapeType = Enum.TryParse<ShapeType>(data.ShapeType, out ShapeType parsed)
                ? parsed
                : ShapeType.Rectangle;
            return new ShapeAnnotationElement(
                graphViewer,
                new Point(data.X, data.Y),
                new Size(data.Width, data.Height),
                shapeType,
                ColorFromSave(data.FillColor),
                ColorFromSave(data.BorderColor),
                data.BorderWidth);
        }

        public override void Dispose() {
            GC.SuppressFinalize(this);
            _fillBrush?.Dispose();
            _borderPen?.Dispose();
            base.Dispose();
        }
    }
}
