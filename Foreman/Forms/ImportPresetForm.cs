using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Drawing;

namespace Foreman
{
	public partial class ImportPresetForm : Form
	{
		public string NewPresetName { get; private set; }
		private string SelectedPath;


		public ImportPresetForm()
		{
			SelectedPath = "";
			NewPresetName = "";
			InitializeComponent();

			//check default folders for a factorio installation (to fill in the path as the 'default')
			List<string> factorioConfigPaths = new List<string>();

			//program files install
			string pfConfigPath = Path.Combine(new string[] { "c:\\", "Program Files", "Factorio", "config-path.cfg" });
			if (File.Exists(pfConfigPath))
				factorioConfigPaths.Add(pfConfigPath);

			//steam
			object steamPathA = Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "SteamPath", "");
			object steamPathB = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", "");
			string steamPath = (steamPathA != null && !string.IsNullOrEmpty((string)steamPathA)) ? (string)steamPathA : (steamPathB != null && !string.IsNullOrEmpty((string)steamPathB)) ? (string)steamPathB : "";
			if (!string.IsNullOrEmpty((string)steamPath))
            {
				string libraryFoldersFilePath = Path.Combine(new string[] { (string)steamPath, "steamapps", "libraryfolders.vdf" });
				if(File.Exists(libraryFoldersFilePath))
                {
					string[] steamLSettings = File.ReadAllLines(libraryFoldersFilePath);
					foreach(string line in steamLSettings)
                    {
						if(line.Contains("\"path\""))
                        {
							string libraryPath = line.Substring(0, line.LastIndexOf("\""));
							libraryPath = libraryPath.Substring(libraryPath.LastIndexOf("\"") + 1);
							factorioConfigPaths.Add(Path.Combine(new string[] { libraryPath, "steamapps", "common", "Factorio", "config-path.cfg" }));
                        }
                    }
                }
            }

			//set locations to be the list of factorioConfigPaths (or user can browse)

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
