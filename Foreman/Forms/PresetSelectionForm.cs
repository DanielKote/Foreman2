using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Foreman {
    public partial class PresetSelectionForm : Form {
        public Preset ChosenPreset;

        private List<PresetErrorPackage> _presetErrors;

        public PresetSelectionForm(List<PresetErrorPackage> presetErrors) {
            _presetErrors = presetErrors;
            _presetErrors.Sort();
            InitializeComponent();

            var totalColumnWidth = 0;
            PresetSelectionListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            for (var i = 1; i < PresetSelectionListView.Columns.Count - 1; i++)
                totalColumnWidth += PresetSelectionListView.Columns[i].Width;
            PresetSelectionListView.Columns[0].Width =
                Math.Max(PresetSelectionListView.Width - totalColumnWidth - 32, PresetSelectionListView.Columns[0].Width);
            PresetSelectionListView.Columns[PresetSelectionListView.Columns.Count - 1].Width = 1;


            foreach (var pePackage in presetErrors) {
                float[] compatibility = [
                    (float) (pePackage.RequiredMods.Count - pePackage.MissingMods.Count - pePackage.WrongVersionMods.Count - pePackage.AddedMods.Count) /
                    pePackage.RequiredMods.Count,
                    (float) (pePackage.RequiredItems.Count - pePackage.MissingItems.Count) / pePackage.RequiredItems.Count,
                    (float) (pePackage.RequiredRecipes.Count - pePackage.MissingRecipes.Count - pePackage.IncorrectRecipes.Count) /
                    pePackage.RequiredRecipes.Count
                ];

                var presetItem = new ListViewItem([
                    pePackage.Preset.Name,
                    compatibility[0].ToString("%00"),
                    compatibility[1].ToString("%00"),
                    compatibility[2].ToString("%00")
                ]);
                PresetSelectionListView.Items.Add(presetItem);
                presetItem.ToolTipText =
                    "Mods:\n" +
                    $"     ({pePackage.RequiredMods.Count - pePackage.MissingMods.Count - pePackage.WrongVersionMods.Count}) Correct\n" +
                    $"     ({pePackage.MissingMods.Count}) Missing\n" +
                    $"     ({pePackage.AddedMods.Count}) Extra\n" +
                    $"     ({pePackage.WrongVersionMods.Count}) Wrong Version\n" +
                    "Items:\n" +
                    $"     ({pePackage.RequiredItems.Count - pePackage.MissingItems.Count}) Correct\n" +
                    $"     ({pePackage.MissingItems.Count}) Missing\n" +
                    "RecipesView:\n" +
                    $"     ({pePackage.RequiredRecipes.Count - pePackage.MissingRecipes.Count - pePackage.IncorrectRecipes.Count}) Correct\n" +
                    $"     ({pePackage.MissingRecipes.Count}) Missing\n" +
                    $"     ({pePackage.IncorrectRecipes.Count}) Incorrect";
            }
        }

        private void ConfirmationButton_Click(object sender, EventArgs e) {
            if (PresetSelectionListView.SelectedIndices.Count <= 0)
                return;

            ChosenPreset = _presetErrors[PresetSelectionListView.SelectedIndices[0]].Preset;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancellingButton_Click(object sender, EventArgs e) {
            ChosenPreset = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void PresetSelectionListView_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (PresetSelectionListView.SelectedIndices.Count <= 0)
                return;

            ChosenPreset = _presetErrors[PresetSelectionListView.SelectedIndices[0]].Preset;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}