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
	public partial class InstallationChooserForm: Form
	{
        private List<FoundInstallation> installations;
		public String SelectedPath;

		public InstallationChooserForm(List<FoundInstallation> installations)
		{
            this.installations = installations;
			SelectedPath = null;
			InitializeComponent();

            foreach (FoundInstallation i in installations)
            {
                this.comboBox1.Items.Add(i.path + " v" + i.version);
            }
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
            DialogResult = DialogResult.OK;
            Close();
		}

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedPath = this.installations[this.comboBox1.SelectedIndex].path;
        }
    }
}
