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
				dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
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
			int scale = 1;
			if (Scale2xCheckBox.Checked)
			{
				scale = 2;
			} else if (Scale3xCheckBox.Checked)
			{
				scale = 3;
			}

			Bitmap image = new Bitmap(graphViewer.graphBounds.Width * scale, graphViewer.graphBounds.Height * scale);
			using (Graphics graphics = Graphics.FromImage(image))
			{
				graphics.ScaleTransform(scale, scale);
				graphics.TranslateTransform(-graphViewer.graphBounds.X, -graphViewer.graphBounds.Y);
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

				graphViewer.Paint(graphics);

				if (!Directory.Exists(Path.GetDirectoryName(fileTextBox.Text)))
				{
					MessageBox.Show("Directory doesn't exist!");
				}
				else
				{
					try
					{
						image.Save(fileTextBox.Text, ImageFormat.Png);
						Close();
						Dispose();
					}
					catch (Exception exception)
					{
						MessageBox.Show("Error saving image: " + exception.Message);
					}
				}
			}
		}
	}
}
