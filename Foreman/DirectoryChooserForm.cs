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
		private bool factorioCheck;

		public DirectoryChooserForm(string defaultDirectory, bool doFactorioDirCheck)
		{
			SelectedPath = defaultDirectory;
			factorioCheck = doFactorioDirCheck;
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
			if (factorioCheck)
			{
				if (IsValidFactorioDirectory(SelectedPath))
				{
					DialogResult = DialogResult.OK;
					Close();
				}

				else
				{
					MessageBox.Show("That doesn't seem to be a valid directory");
				}
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private static bool IsValidFactorioDirectory(String dir)
		{
			if (!Directory.Exists(dir))
			{
				return false;
			}
			if (!Directory.Exists(Path.Combine(dir, "data", "core")))
			{
				return false;
			}
			if (!Directory.Exists(Path.Combine(dir, "data", "base")))
			{
				return false;
			}

			return true;
		}
	}
}
