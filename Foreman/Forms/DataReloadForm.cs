using Foreman.Controls;
using Foreman.DataCaching;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataCache = Foreman.DataCaching.DataCache;

namespace Foreman {
    public partial class DataLoadForm : Form {
        private int currentPercent;
        private string currentText;

        private readonly Preset selectedPreset;
        private DataCache? createdDataCache;
        private bool loadInProgress;
        private bool loadCompleted;

        public DataLoadForm(Preset preset) {
            currentPercent = 0;
            currentText = "";

            selectedPreset = preset;

            InitializeComponent();
        }

        private async void ProgressForm_Load(object? sender, EventArgs e) {
#if DEBUG
            DateTime startTime = DateTime.Now;
            //ErrorLogging.LogLine("Init program.");
#endif
            var progress = new Progress<KeyValuePair<int, string>>(value => {
                if (value.Key > currentPercent) {
                    currentPercent = value.Key;
                    progressBar.Value = value.Key;
                }
                if (!string.IsNullOrEmpty(value.Value) && value.Value != currentText) {
                    currentText = value.Value;
                    Text = "Preparing Foreman: " + value.Value;
                }
            }) as IProgress<KeyValuePair<int, string>>;

            createdDataCache = new DataCache(Properties.Settings.Default.UseRecipeBWfilters);
            loadInProgress = true;
            DialogResult loadResult = DialogResult.Abort;
            try {
                await createdDataCache.LoadAllData(selectedPreset, progress).ConfigureAwait(false);
                loadCompleted = true;
                loadResult = DialogResult.OK;
            } catch (Exception ex) {
                ErrorLogging.LogException(ex, string.Format(CultureInfo.InvariantCulture, "Failed to load preset '{0}'", selectedPreset.Name));
                createdDataCache = new DataCache(true); //blank data cache in case of error.
                loadResult = DialogResult.Abort;
            } finally {
                loadInProgress = false;
            }
            await this.InvokeOnUiThreadAsync(() => {
                DialogResult = loadResult;
                Close();
            }).ConfigureAwait(false);

#if DEBUG
            TimeSpan diff = DateTime.Now.Subtract(startTime);
            Console.WriteLine("Load time: " + Math.Round(diff.TotalSeconds, 2) + " seconds.");
            ErrorLogging.LogLine("Load time: " + Math.Round(diff.TotalSeconds, 2) + " seconds.");
#endif
        }

        public DataCache? GetDataCache() {
            return createdDataCache;
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            if (loadInProgress && !loadCompleted) {
                UserMessages.Show(
                    "Preset loading is still in progress.\n\n" +
                    "If you close this window now, Foreman may use an incomplete set of items and recipes.",
                    "Preset load interrupted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            base.OnFormClosing(e);
        }
    }
}
