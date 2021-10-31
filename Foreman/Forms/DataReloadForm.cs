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
    public partial class DataReloadForm : Form
    {
        private CancellationTokenSource cts;
        private int currentPercent;
        private string currentText;
        private bool defaultEnabled;

        public DataReloadForm(bool defaultEnabled)
        {
            this.cts = new CancellationTokenSource();
            InitializeComponent();
            currentPercent = 0;
            currentText = "";
            this.defaultEnabled = defaultEnabled;
        }

        private async void ProgressForm_Load(object sender, EventArgs e)
        {
            #if DEBUG
            DateTime startTime = DateTime.Now;
            //ErrorLogging.LogLine("Init program.");
            #endif
            var progressHandler = new Progress<KeyValuePair<int, string>>(value =>
            {
                if (value.Key > currentPercent)
                {
                    currentPercent = value.Key;
                    progressBar.Value = value.Key;
                }
                if(!String.IsNullOrEmpty(value.Value) && value.Value != currentText)
                {
                    currentText = value.Value;
                    Text = "Preparing Foreman: " + value.Value;
                }
            });
            var progress = progressHandler as IProgress<KeyValuePair<int, string>>;
            var token = cts.Token;

            await DataCache.LoadAllData(defaultEnabled, progress, token);

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

            #if DEBUG
            TimeSpan diff = DateTime.Now.Subtract(startTime);
            Console.WriteLine("Load time: " + Math.Round(diff.TotalSeconds, 2) + " seconds.");
            ErrorLogging.LogLine("Load time: " + Math.Round(diff.TotalSeconds, 2) + " seconds.");
            #endif
        }

        // This event currently won't occur, since we hid the window controls for the dialog.
        // Other parts of the system (at least, save/load) don't handle cancellation well and can raise errors.
        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts.Cancel();
        }
    }
}
