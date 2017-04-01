using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
    public partial class ProgressForm : Form
    {
        private List<string> enabledMods;
        private bool workerCompleted;

        public ProgressForm()
        {
            InitializeComponent();
        }

        public ProgressForm(List<string> enabledMods)
        {
            this.enabledMods = enabledMods;
            InitializeComponent();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
			DataCache.LoadAllData(this.enabledMods, worker);
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine(e.Cancelled);
            if (e.Cancelled)
            {
                Console.WriteLine("Cancelled");
                DataCache.Clear();
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
            this.workerCompleted = true;
			Close();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        // This event currently won't occur, since we hid the window controls for the dialog.
        // Other parts of the system (at least, save/load) don't handle cancellation well and can raise errors.
        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.workerCompleted)
            {
                backgroundWorker.CancelAsync();
                e.Cancel = true;
            }
        }
    }
}
