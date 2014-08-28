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

		public DirectoryChooserForm()
		{
			InitializeComponent();
		}

		private void BrowseButton_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
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
			bool pathValid = true;

			if (!Directory.Exists(SelectedPath))
			{
				pathValid = false;
			}
			if (!Directory.Exists(Path.Combine(SelectedPath, "data", "core")))
			{
				pathValid = false;
			}
			if (!Directory.Exists(Path.Combine(SelectedPath, "data", "base")))
			{
				pathValid = false;
			}

			if (pathValid)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				MessageBox.Show("That doesn't seem to be a valid Factorio directory");
			}
		}
	}
}
