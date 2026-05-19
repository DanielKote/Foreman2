using Foreman.DataCaching;
using Foreman.ProductionGraphView.Annotations;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Foreman {
    public partial class ImageExportForm : Form {
        private static readonly float[] Multipliers = [0.05f, 0.1f, 0.2f, 0.5f, 1f, 2f, 3f];
        private static readonly string[] MultiplierNames = ["1/20", "1/10", "1/5", "1/2", "1", "2", "3"];
        private const int DefaultScaleIndex = 4;

        private readonly ProductionGraphViewer graphViewer;

        public ImageExportForm(ProductionGraphViewer graphViewer) {
            InitializeComponent();
            this.graphViewer = graphViewer;

            ScaleSelectionBox.Items.AddRange(MultiplierNames);
            ScaleSelectionBox.SelectedIndex = DefaultScaleIndex;
            UpdateSizeLabel();
        }

        private void button1_Click(object? sender, EventArgs e) {
            using SaveFileDialog dialog = new() {
                AddExtension = true,
                Filter = "PNG files (*.png)|*.png",
                InitialDirectory = Path.Combine(Application.StartupPath, "Exported Graphs"),
                FileName = "Foreman Production Flowchart.png",
                ValidateNames = true,
                OverwritePrompt = true
            };
            if (!Directory.Exists(dialog.InitialDirectory))
                Directory.CreateDirectory(dialog.InitialDirectory);

            if (dialog.ShowDialog() == DialogResult.OK)
                fileTextBox.Text = dialog.FileName;
        }

        private void ExportButton_Click(object? sender, EventArgs e) {
            string? directory = Path.GetDirectoryName(fileTextBox.Text);
            if (string.IsNullOrEmpty(fileTextBox.Text)
                || string.IsNullOrEmpty(directory)
                || !Directory.Exists(directory)) {
                UserMessages.Show("Directory doesn't exist!");
                return;
            }

            graphViewer.ClearSelection();
            float scale = Multipliers[ScaleSelectionBox.SelectedIndex];

            if (ViewLimitCheckBox.Checked) {
                ExportBitmap(ViewLimitedBounds(), scale, ConfigureViewLimitedTransform);
                return;
            }

            Rectangle exportBounds = graphViewer.GetExportBounds();
            if (!GraphExportBounds.IsExportable(exportBounds)) {
                UserMessages.Show("There is nothing to export. Add nodes or annotations to the graph first.");
                return;
            }

            ExportBitmap(exportBounds, scale, (graphics, bounds) => {
                graphics.ScaleTransform(scale, scale);
                graphics.TranslateTransform(-bounds.X, -bounds.Y);
            });
        }

        private void ExportBitmap(Rectangle bounds, float scale, Action<Graphics, Rectangle> configureTransform) {
            using Bitmap image = new(GraphExportBounds.ScaledWidth(bounds, scale), GraphExportBounds.ScaledHeight(bounds, scale));
            using var graphics = Graphics.FromImage(image);
            graphics.ResetTransform();
            configureTransform(graphics, bounds);
            graphics.SmoothingMode = SmoothingMode.HighQuality;

            if (!TransparencyCheckBox.Checked)
                graphics.Clear(graphViewer.BackColor);

            graphViewer.Paint(graphics, FullGraph: true);

            try {
                image.Save(fileTextBox.Text, ImageFormat.Png);
                Close();
            } catch (Exception exception) {
                UserMessages.Show("Error saving image. See log for more details.");
                ErrorLogging.LogException(exception, "Error saving image");
            }
        }

        private void ConfigureViewLimitedTransform(Graphics graphics, Rectangle _) {
            graphics.TranslateTransform(
                graphViewer.Width / (graphViewer.ViewScale * 2),
                graphViewer.Height / (graphViewer.ViewScale * 2));
            graphics.TranslateTransform(graphViewer.ViewOffset.X, graphViewer.ViewOffset.Y);
            graphics.ScaleTransform(Multipliers[ScaleSelectionBox.SelectedIndex], Multipliers[ScaleSelectionBox.SelectedIndex]);
        }

        private Rectangle ViewLimitedBounds() =>
            new(0, 0, (int)(graphViewer.Width / graphViewer.ViewScale), (int)(graphViewer.Height / graphViewer.ViewScale));

        private void UpdateSizeLabel() {
            float scale = Multipliers[ScaleSelectionBox.SelectedIndex];
            Rectangle bounds = ViewLimitCheckBox.Checked
                ? ViewLimitedBounds()
                : graphViewer.GetExportBounds();

            if (!GraphExportBounds.IsExportable(bounds)) {
                ImageSizeLabel.Text = "Image Size: — (nothing to export)";
                return;
            }

            int x = GraphExportBounds.ScaledWidth(bounds, scale);
            int y = GraphExportBounds.ScaledHeight(bounds, scale);
            ImageSizeLabel.Text = $"Image Size: {x:N0} x {y:N0}";
        }

        private void ViewLimitCheckBox_CheckedChanged(object? sender, EventArgs e) => UpdateSizeLabel();

        private void ScaleSelectionBox_SelectedIndexChanged(object? sender, EventArgs e) => UpdateSizeLabel();
    }
}
