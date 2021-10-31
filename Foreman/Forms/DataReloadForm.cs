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
        private int currentPercent;
        private string currentText;

        private Preset selectedPreset;
        private DataCache createdDataCache;

        public DataLoadForm(Preset preset)
        {
            currentPercent = 0;
            currentText = "";

            selectedPreset = preset;

            InitializeComponent();
        }

        private async void ProgressForm_Load(object sender, EventArgs e)
        {
 #if DEBUG
            DateTime startTime = DateTime.Now;
            //ErrorLogging.LogLine("Init program.");
 #endif
            var progress = new Progress<KeyValuePair<int, string>>(value =>
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
            }) as IProgress<KeyValuePair<int, string>>;

            createdDataCache = new DataCache();
            await createdDataCache.LoadAllData(selectedPreset, progress);
            DialogResult = DialogResult.OK;
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
    }
}
