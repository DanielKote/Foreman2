using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Foreman
{
    public partial class DataLoadForm : Form
    {
        private CancellationTokenSource cts;
        private int currentPercent;
        private string currentText;

        private string JsonDataPath;
        private string IconDataPath;
        private DataCache createdDataCache;

        public DataLoadForm(List<KeyValuePair<string,string>> activeMods = null)
        {
            currentPercent = 0;
            currentText = "";
            cts = new CancellationTokenSource();

            if (activeMods == null) //load the currently selected preset - dont care about mod compatibilities
            {
                JsonDataPath = Path.Combine(new string[] { Application.StartupPath, "Presets", Properties.Settings.Default.CurrentPresetName + ".json" }); ;
                IconDataPath = Path.Combine(new string[] { Application.StartupPath, "Presets", Properties.Settings.Default.CurrentPresetName + ".dat" });
            }
            else //need to load each of the presets and find their mod list - if the mod list fits the activeMods list, then use it.
            {

            }

            InitializeComponent();
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

            createdDataCache = new DataCache();
            JObject jsonData = JObject.Parse(File.ReadAllText(JsonDataPath));
            var IconCache = await IconProcessor.LoadIconCache(IconDataPath, progress, token, 0, 80);
            await createdDataCache.LoadAllData(jsonData, IconCache, progress, token, 80, 100);

            if (token.IsCancellationRequested)
            {
                createdDataCache.Clear();
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

        public DataCache GetDataCache()
        {
            return createdDataCache;
        }

        // This event currently won't occur, since we hid the window controls for the dialog.
        // Other parts of the system (at least, save/load) don't handle cancellation well and can raise errors.
        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts.Cancel();
        }
    }
}
