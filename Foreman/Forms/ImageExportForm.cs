using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Foreman {
    public partial class ImageExportForm : Form {
        private readonly float[] _multipliers = [0.05f, 0.1f, 0.2f, 0.5f, 1f, 2f, 3f];
        private readonly string[] _multiplierNames = ["1/20", "1/10", "1/5", "1/2", "1", "2", "3"];
        private const int InitialIndex = 4;

        private readonly ProductionGraphViewer _graphViewer;

        public ImageExportForm(ProductionGraphViewer graphViewer) {
            InitializeComponent();
            _graphViewer = graphViewer;

            ScaleSelectionBox.Items.AddRange(_multiplierNames);
            ScaleSelectionBox.SelectedIndex = InitialIndex;
            UpdateSizeLabel();
        }

        private void button1_Click(object sender, EventArgs e) {
            using var dialog = new SaveFileDialog();

            dialog.AddExtension = true;
            dialog.Filter = "PNG files (*.png)|*.png";
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Exported Graphs");
            if (!Directory.Exists(dialog.InitialDirectory))
                Directory.CreateDirectory(dialog.InitialDirectory);
            dialog.FileName = "Foreman Production Flowchart.png";
            dialog.ValidateNames = true;
            dialog.OverwritePrompt = true;
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK) {
                fileTextBox.Text = dialog.FileName;
            }
        }

        private void ExportButton_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(fileTextBox.Text) || string.IsNullOrEmpty(Path.GetDirectoryName(fileTextBox.Text)) ||
                !Directory.Exists(Path.GetDirectoryName(fileTextBox.Text))) {
                MessageBox.Show("Directory doesn't exist!");
            } else {
                _graphViewer.ClearSelection();

                var scale = _multipliers[ScaleSelectionBox.SelectedIndex];

                var image = ViewLimitCheckBox.Checked
                    ? new Bitmap((int) (_graphViewer.Width * scale / _graphViewer.ViewScale), (int) (_graphViewer.Height * scale / _graphViewer.ViewScale))
                    : new Bitmap((int) (_graphViewer.Graph.Bounds.Width * scale), (int) (_graphViewer.Graph.Bounds.Height * scale));

                using var graphics = Graphics.FromImage(image);

                graphics.ResetTransform();

                if (ViewLimitCheckBox.Checked) {
                    graphics.TranslateTransform(_graphViewer.Width / (_graphViewer.ViewScale * 2), _graphViewer.Height / (_graphViewer.ViewScale * 2));
                    graphics.TranslateTransform(_graphViewer.ViewOffset.X, _graphViewer.ViewOffset.Y);
                    graphics.ScaleTransform(scale, scale);
                } else {
                    graphics.ScaleTransform(scale, scale);
                    graphics.TranslateTransform(-_graphViewer.Graph.Bounds.X, -_graphViewer.Graph.Bounds.Y);
                }

                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                if (!TransparencyCheckBox.Checked)
                    graphics.Clear(Color.White);

                _graphViewer.Paint(graphics, true);

                try {
                    image.Save(fileTextBox.Text, ImageFormat.Png);
                    Close();
                } catch (Exception exception) {
                    MessageBox.Show($"Error saving image: {exception.Message}");
                    ErrorLogging.LogLine($"Error saving image: {exception}");
                }
            }
        }

        private void UpdateSizeLabel() {
            var scale = _multipliers[ScaleSelectionBox.SelectedIndex];
            int x, y;

            if (ViewLimitCheckBox.Checked) {
                x = (int) (_graphViewer.Width * scale / _graphViewer.ViewScale);
                y = (int) (_graphViewer.Height * scale / _graphViewer.ViewScale);
            } else {
                x = (int) (_graphViewer.Graph.Bounds.Width * scale);
                y = (int) (_graphViewer.Graph.Bounds.Height * scale);
            }

            ImageSizeLabel.Text = $"Image Size: {x.ToString("N0")} x {y.ToString("N0")}";
        }

        private void ViewLimitCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateSizeLabel();
        }

        private void ScaleSelectionBox_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateSizeLabel();
        }
    }
}