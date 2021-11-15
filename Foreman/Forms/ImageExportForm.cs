using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace Foreman
{
	public partial class ImageExportForm : Form
	{
		private readonly ProductionGraphViewer graphViewer;

		public ImageExportForm(ProductionGraphViewer graphViewer)
		{
			InitializeComponent();
			this.graphViewer = graphViewer;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.AddExtension = true;
				dialog.Filter = "PNG files (*.png)|*.png";
				dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Exported Graphs");
				if (!Directory.Exists(dialog.InitialDirectory))
					Directory.CreateDirectory(dialog.InitialDirectory);
				dialog.FileName = "Foreman Production Flowchart.png";
				dialog.ValidateNames = true;
				dialog.OverwritePrompt = true;
				var result = dialog.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK)
				{
					fileTextBox.Text = dialog.FileName;
				}
			}
		}

		private void ExportButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(fileTextBox.Text) || string.IsNullOrEmpty(Path.GetDirectoryName(fileTextBox.Text)) || !Directory.Exists(Path.GetDirectoryName(fileTextBox.Text)))
			{
				MessageBox.Show("Directory doesn't exist!");
			}
			else
			{
				int scale = 1;
				if (Scale2xCheckBox.Checked)
					scale = 2;
				else if (Scale3xCheckBox.Checked)
					scale = 3;

				Bitmap image = ViewLimitCheckBox.Checked? new Bitmap((int)(graphViewer.Width * scale / graphViewer.ViewScale), (int)(graphViewer.Height * scale / graphViewer.ViewScale)) : new Bitmap(graphViewer.Graph.Bounds.Width * scale, graphViewer.Graph.Bounds.Height * scale);
				using (Graphics graphics = Graphics.FromImage(image))
				{
					graphics.ResetTransform();

					if (ViewLimitCheckBox.Checked)
					{
						graphics.TranslateTransform(graphViewer.Width / (graphViewer.ViewScale * 2), graphViewer.Height / ( graphViewer.ViewScale * 2));
						graphics.TranslateTransform(graphViewer.ViewOffset.X, graphViewer.ViewOffset.Y);
						graphics.ScaleTransform(scale, scale);
					}
					else
					{
						graphics.ScaleTransform(scale, scale);
						graphics.TranslateTransform(-graphViewer.Graph.Bounds.X, -graphViewer.Graph.Bounds.Y);
					}

					graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

					if (!TransparencyCheckBox.Checked)
						graphics.Clear(Color.White);

					graphViewer.Paint(graphics, true);

					try
					{
						image.Save(fileTextBox.Text, ImageFormat.Png);
						Close();
					}
					catch (Exception exception)
					{
						MessageBox.Show("Error saving image: " + exception.Message);
						ErrorLogging.LogLine("Error saving image: " + exception.ToString());
					}
				}
			}
		}
	}
}
