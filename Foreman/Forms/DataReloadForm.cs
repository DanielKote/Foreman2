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
        private List<string> enabledMods;
        private CancellationTokenSource cts;
        private int currentPercent;
        private static readonly string[] ProcessNames = { "Reading Factorio mod data...", "Processing Factorio mod LUA code...", "Creating icons...", "Checking for cyclic recipes..." };
        public static readonly int[] ProcessBreakpoints = { 0, 15, 30, 98, 200 };
        private int currentBPindex;

        public DataReloadForm()
        {
            this.cts = new CancellationTokenSource();
            InitializeComponent();
            currentPercent = 0;
            currentBPindex = 0;
            this.Text = "Preparing Foreman: " + ProcessNames[currentBPindex];
        }

        public DataReloadForm(List<string> enabledMods, DataCache.GenerationType generationType) : this()
        {
            this.enabledMods = enabledMods;
        }

        private async void ProgressForm_Load(object sender, EventArgs e)
        {
            #if DEBUG
            DateTime startTime = DateTime.Now;
            //ErrorLogging.LogLine("Init program.");
            #endif
            var progressHandler = new Progress<int>(value =>
            {
                if (value > currentPercent)
                {
                    if(value >= ProcessBreakpoints[currentBPindex+1])
                    {
                        currentBPindex++;
                        if ((DataCache.GenerationType)Properties.Settings.Default.GenerationType == DataCache.GenerationType.ForemanMod && currentBPindex == 1)
                            currentBPindex = 2; //skip lua title if we are loading directly with mod

                        this.Text = "Preparing Foreman: " + ProcessNames[currentBPindex];
                    }
                    currentPercent = value;
                    progressBar.Value = value;
                }
            });
            var progress = progressHandler as IProgress<int>;
            var token = cts.Token;

            await DataCache.LoadAllData(enabledMods, progress, token);

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
