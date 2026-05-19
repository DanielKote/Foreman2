using Foreman.ProductionGraphView.Elements;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    public partial class TextPropertiesForm : Form {
        private readonly TextAnnotationElement _element;
        private readonly ProductionGraphViewer _graphViewer;

        private readonly string _originalText;
        private readonly Font _originalFont;
        private readonly Color _originalTextColor;
        private readonly Color _originalBackColor;
        private readonly StringAlignment _originalTextAlign;

        private Font? _workingFont;

        public TextPropertiesForm(TextAnnotationElement element) {
            InitializeComponent();
            _element = element;
            _graphViewer = element.GraphViewer;

            _originalText = element.Text;
            _originalFont = new Font(element.TextFont, element.TextFont.Style);
            _originalTextColor = element.TextColor;
            _originalBackColor = element.BackColor;
            _originalTextAlign = element.TextAlign;
            _workingFont = new Font(element.TextFont, element.TextFont.Style);

            TextInput.TextChanged -= TextInput_TextChanged;
            TextInput.Text = element.Text;
            TextInput.TextChanged += TextInput_TextChanged;

            UpdateFontLabel();
            UpdateAlignRadios();
            UpdateTextColorButton();
            UpdateBackColorButton();

            TransparentCheckBox.CheckedChanged -= TransparentCheckBox_CheckedChanged;
            TransparentCheckBox.Checked = element.BackColor.A == 0;
            TransparentCheckBox.CheckedChanged += TransparentCheckBox_CheckedChanged;
            BackColorButton.Enabled = !TransparentCheckBox.Checked;
            Shown += (_, _) => { TextInput.Focus(); TextInput.SelectAll(); };
        }

        private void TextInput_TextChanged(object? sender, EventArgs e) {
            _element.Text = TextInput.Text;
            _element.FitBoxToTextAtCenter();
            _graphViewer.Invalidate();
        }

        private void FontButton_Click(object? sender, EventArgs e) {
            using var dlg = new FontDialog {
                Font = _workingFont ?? _originalFont,
                ShowEffects = true,
                ShowColor = false,
                FontMustExist = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            _workingFont?.Dispose();
            var chosenFont = dlg.Font ?? _originalFont;
            _workingFont = chosenFont;
            _element.TextFont?.Dispose();
            _element.TextFont = new Font(chosenFont, chosenFont.Style);
            _element.RebuildGdiObjects();
            _element.FitBoxToTextAtCenter();
            _graphViewer.Invalidate();
            UpdateFontLabel();
        }

        private void UpdateAlignRadios() {
            AlignLeftRadio.CheckedChanged -= AlignRadio_CheckedChanged;
            AlignCenterRadio.CheckedChanged -= AlignRadio_CheckedChanged;
            AlignRightRadio.CheckedChanged -= AlignRadio_CheckedChanged;

            AlignLeftRadio.Checked = _element.TextAlign == StringAlignment.Near;
            AlignCenterRadio.Checked = _element.TextAlign == StringAlignment.Center;
            AlignRightRadio.Checked = _element.TextAlign == StringAlignment.Far;

            AlignLeftRadio.CheckedChanged += AlignRadio_CheckedChanged;
            AlignCenterRadio.CheckedChanged += AlignRadio_CheckedChanged;
            AlignRightRadio.CheckedChanged += AlignRadio_CheckedChanged;
        }

        private void AlignRadio_CheckedChanged(object? sender, EventArgs e) {
            if (AlignLeftRadio.Checked)
                _element.TextAlign = StringAlignment.Near;
            else if (AlignCenterRadio.Checked)
                _element.TextAlign = StringAlignment.Center;
            else if (AlignRightRadio.Checked)
                _element.TextAlign = StringAlignment.Far;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
        }

        private void UpdateFontLabel() {
            if (_workingFont is null)
                return;
            FontPreviewLabel.Text = string.Format(DisplayCulture.Format, "{0}, {1}pt{2}{3}",
                _workingFont.FontFamily.Name,
                (int)_workingFont.SizeInPoints,
                _workingFont.Bold ? " Bold" : "",
                _workingFont.Italic ? " Italic" : "");
        }

        private void TextColorButton_Click(object? sender, EventArgs e) {
            using var dlg = new ColorDialog { Color = _element.TextColor, AnyColor = true, FullOpen = true };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            _element.TextColor = dlg.Color;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
            UpdateTextColorButton();
        }

        private void UpdateTextColorButton() {
            TextColorButton.BackColor = _element.TextColor;
            TextColorButton.ForeColor = _element.TextColor.R + _element.TextColor.G + _element.TextColor.B > 382
                ? Color.Black
                : Color.White;
        }

        private void BackColorButton_Click(object? sender, EventArgs e) {
            using var dlg = new ColorDialog {
                Color = Color.FromArgb(255, _element.BackColor.R, _element.BackColor.G, _element.BackColor.B),
                AnyColor = true,
                FullOpen = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            _element.BackColor = dlg.Color;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
            UpdateBackColorButton();
        }

        private void UpdateBackColorButton() {
            var display = Color.FromArgb(255, _element.BackColor.R, _element.BackColor.G, _element.BackColor.B);
            BackColorButton.BackColor = display;
            BackColorButton.ForeColor = display.R + display.G + display.B > 382 ? Color.Black : Color.White;
        }

        private void TransparentCheckBox_CheckedChanged(object? sender, EventArgs e) {
            BackColorButton.Enabled = !TransparentCheckBox.Checked;
            _element.BackColor = TransparentCheckBox.Checked
                ? Color.Transparent
                : Color.FromArgb(255, _element.BackColor.R, _element.BackColor.G, _element.BackColor.B);
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
        }

        private void OKButton_Click(object? sender, EventArgs e) {
            TextAnnotationElement.SaveDefaults(_element);
            _workingFont?.Dispose();
            _workingFont = null;
            _originalFont.Dispose();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object? sender, EventArgs e) {
            _element.Text = _originalText;
            _element.TextFont?.Dispose();
            _element.TextFont = _originalFont;
            _element.TextColor = _originalTextColor;
            _element.BackColor = _originalBackColor;
            _element.TextAlign = _originalTextAlign;
            _element.RebuildGdiObjects();
            _graphViewer.Invalidate();
            _workingFont?.Dispose();
            _workingFont = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e) {
            base.OnFormClosed(e);
            _workingFont?.Dispose();
        }
    }
}
