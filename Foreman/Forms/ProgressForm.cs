using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
    public partial class ProgressForm : Form
    {
        private List<string> enabledMods;
        private CancellationTokenSource cts; 

        public ProgressForm()
        {
            this.cts = new CancellationTokenSource();
            InitializeComponent();
        }

        public ProgressForm(List<string> enabledMods) : base()
        {
            this.enabledMods = enabledMods;
        }

        private async void ProgressForm_Load(object sender, EventArgs e)
        {
            var progressHandler = new Progress<int>(value =>
            {
                progressBar.Value = value;
            });
            var progress = progressHandler as IProgress<int>;
            var token = cts.Token;

            await Task.Run(() =>
            {
                DataCache.LoadAllData(this.enabledMods, new CancellableProgress(progress, token));
            });

            if (token.IsCancellationRequested)
            {
                DataCache.Clear();
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
			Close();
        }

        // This event currently won't occur, since we hid the window controls for the dialog.
        // Other parts of the system (at least, save/load) don't handle cancellation well and can raise errors.
        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts.Cancel();
        }
    }
}
