using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Foreman
{
	public partial class DirectoryChooserForm : Form
	{
		public String SelectedPath;

		public DirectoryChooserForm(string defaultDirectory)
		{
			SelectedPath = defaultDirectory;
			InitializeComponent();

			DirTextBox.Text = SelectedPath;
		}

		private void BrowseButton_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				if (Directory.Exists(SelectedPath))
				{
					dialog.SelectedPath = SelectedPath;
				}
				var result = dialog.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK)
				{
					DirTextBox.Text = dialog.SelectedPath;
					SelectedPath = dialog.SelectedPath;
				}
			}
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(SelectedPath))
			{
				MessageBox.Show("That directory doesn't seem to exist");
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private void DirTextBox_TextChanged(object sender, EventArgs e)
		{
			SelectedPath = DirTextBox.Text;
		}
	}
}
