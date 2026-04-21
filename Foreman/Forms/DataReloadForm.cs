using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Foreman {
    public partial class DataLoadForm : Form {
        private int _currentPercent;
        private string _currentText;

        private Preset _selectedPreset;
        private DataCache _createdDataCache;

        public DataLoadForm(Preset preset) {
            _currentPercent = 0;
            _currentText = "";

            _selectedPreset = preset;

            InitializeComponent();
        }

        private async void ProgressForm_Load(object sender, EventArgs e) {
#if DEBUG
            var startTime = DateTime.Now;
            //ErrorLogging.LogLine("Init program.");
#endif
            IProgress<KeyValuePair<int, string>> progress = new Progress<KeyValuePair<int, string>>(value => {
                if (value.Key > _currentPercent) {
                    _currentPercent = value.Key;
                    progressBar.Value = value.Key;
                }

                if (!string.IsNullOrEmpty(value.Value) && value.Value != _currentText) {
                    _currentText = value.Value;
                    Text = $"Preparing Foreman: {value.Value}";
                }
            });

            _createdDataCache = new DataCache(Properties.Settings.Default.UseRecipeBWfilters);
            try {
                await _createdDataCache.LoadAllData(_selectedPreset, progress);
                DialogResult = DialogResult.OK;
            } catch {
                // blank data cache in case of error.
                _createdDataCache = new DataCache(true);
                DialogResult = DialogResult.Abort;
            }

            Close();

#if DEBUG
            var diff = DateTime.Now.Subtract(startTime);
            Console.WriteLine($"Load time: {Math.Round(diff.TotalSeconds, 2)} seconds.");
            ErrorLogging.LogLine($"Load time: {Math.Round(diff.TotalSeconds, 2)} seconds.");
#endif
        }

        public DataCache GetDataCache() {
            return _createdDataCache;
        }
    }
}