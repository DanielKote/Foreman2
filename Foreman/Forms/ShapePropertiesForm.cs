using Foreman.ProductionGraphView.Elements;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    public partial class ShapePropertiesForm : Form {
        private readonly ShapeAnnotationElement _element;
        private readonly ProductionGraphViewer _graphViewer;

        private readonly ShapeAnnotationElement.ShapeType _originalShapeType;
        private readonly Color _originalFillColor;
        private readonly Color _originalBorderColor;
        private readonly int _originalBorderWidth;

        public ShapePropertiesForm(ShapeAnnotationElement element) {
            InitializeComponent();
            _element = element;
            _graphViewer = element.GraphViewer;

            _originalShapeType = element.CurrentShapeType;
            _originalFillColor = element.FillColor;
            _originalBorderColor = element.BorderColor;
            _originalBorderWidth = element.BorderWidth;

            ShapeTypeCombo.SelectedIndex = element.CurrentShapeType == ShapeAnnotationElement.ShapeType.Ellipse ? 1 : 0;
            BorderWidthInput.Value = Math.Max(BorderWidthInput.Minimum, Math.Min(BorderWidthInput.Maximum, element.BorderWidth));

            NoFillCheckBox.CheckedChanged -= NoFillCheckBox_CheckedChanged;
            NoFillCheckBox.Checked = element.FillColor.A == 0;
            NoFillCheckBox.CheckedChanged += NoFillCheckBox_CheckedChanged;

            bool noFill = NoFillCheckBox.Checked;
            FillColorButton.Enabled = !noFill;
            FillAlphaTrack.Enabled = !noFill;
            FillColorButton.BackColor = Color.FromArgb(255, element.FillColor.R, element.FillColor.G, element.FillColor.B);
            FillColorButton.ForeColor = element.FillColor.R + element.FillColor.G + element.FillColor.B > 382
                ? Color.Black
                : Color.White;
            FillAlphaTrack.Value = element.FillColor.A;
            FillAlphaLabel.Text = string.Format(DisplayCulture.Format, "Opacity: {0}", element.FillColor.A);
            UpdateBorderButton();
        }

        private void ShapeTypeCombo_SelectedIndexChanged(object? sender, EventArgs e) {
            _element.CurrentShapeType = ShapeTypeCombo.SelectedIndex == 1
                ? ShapeAnnotationElement.ShapeType.Ellipse
                : ShapeAnnotationElement.ShapeType.Rectangle;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
        }

        private void FillColorButton_Click(object? sender, EventArgs e) {
            using var dlg = new ColorDialog {
                Color = Color.FromArgb(255, _element.FillColor.R, _element.FillColor.G, _element.FillColor.B),
                AnyColor = true,
                FullOpen = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            _element.FillColor = Color.FromArgb(_element.FillColor.A, dlg.Color.R, dlg.Color.G, dlg.Color.B);
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
            UpdateFillButton();
        }

        private void UpdateFillButton() {
            FillColorButton.BackColor = Color.FromArgb(255, _element.FillColor.R, _element.FillColor.G, _element.FillColor.B);
            FillColorButton.ForeColor = _element.FillColor.R + _element.FillColor.G + _element.FillColor.B > 382
                ? Color.Black
                : Color.White;
            FillAlphaLabel.Text = string.Format(DisplayCulture.Format, "Opacity: {0}", _element.FillColor.A);
            FillAlphaTrack.Value = _element.FillColor.A;
        }

        private void FillAlphaTrack_Scroll(object? sender, EventArgs e) {
            _element.FillColor = Color.FromArgb(FillAlphaTrack.Value, _element.FillColor.R, _element.FillColor.G, _element.FillColor.B);
            FillAlphaLabel.Text = string.Format(DisplayCulture.Format, "Opacity: {0}", _element.FillColor.A);
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
        }

        private void NoFillCheckBox_CheckedChanged(object? sender, EventArgs e) {
            FillColorButton.Enabled = !NoFillCheckBox.Checked;
            FillAlphaTrack.Enabled = !NoFillCheckBox.Checked;
            if (NoFillCheckBox.Checked) {
                _element.FillColor = Color.FromArgb(0, _element.FillColor.R, _element.FillColor.G, _element.FillColor.B);
                FillAlphaTrack.Value = 0;
                FillAlphaLabel.Text = "Opacity: 0";
            } else {
                _element.FillColor = Color.FromArgb(255, _element.FillColor.R, _element.FillColor.G, _element.FillColor.B);
                FillAlphaTrack.Value = 255;
                FillAlphaLabel.Text = "Opacity: 255";
            }
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
        }

        private void BorderColorButton_Click(object? sender, EventArgs e) {
            using var dlg = new ColorDialog { Color = _element.BorderColor, AnyColor = true, FullOpen = true };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            _element.BorderColor = dlg.Color;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
            UpdateBorderButton();
        }

        private void UpdateBorderButton() {
            BorderColorButton.BackColor = Color.FromArgb(255, _element.BorderColor.R, _element.BorderColor.G, _element.BorderColor.B);
            BorderColorButton.ForeColor = _element.BorderColor.R + _element.BorderColor.G + _element.BorderColor.B > 382
                ? Color.Black
                : Color.White;
        }

        private void BorderWidthInput_ValueChanged(object? sender, EventArgs e) {
            _element.BorderWidth = (int)BorderWidthInput.Value;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
        }

        private void OKButton_Click(object? sender, EventArgs e) {
            ShapeAnnotationElement.SaveDefaults(_element);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object? sender, EventArgs e) {
            _element.CurrentShapeType = _originalShapeType;
            _element.FillColor = _originalFillColor;
            _element.BorderColor = _originalBorderColor;
            _element.BorderWidth = _originalBorderWidth;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
