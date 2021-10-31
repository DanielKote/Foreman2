using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
    public partial class PresetSelectionForm : Form
    {
        public Preset ChosenPreset;

        private List<PresetErrorPackage> PresetErrors;

        public PresetSelectionForm(List<PresetErrorPackage> presetErrors)
        {
            PresetErrors = presetErrors;
            PresetErrors.Sort();
            InitializeComponent();

            foreach(PresetErrorPackage pePackage in presetErrors)
            {
                float[] compatibility = 
                {
                    ((float)(pePackage.RequiredMods.Count - pePackage.MissingMods.Count - pePackage.WrongVersionMods.Count - pePackage.AddedMods.Count) / pePackage.RequiredMods.Count),
                    ((float)(pePackage.RequiredItems.Count - pePackage.MissingItems.Count) / pePackage.RequiredItems.Count),
                    ((float)(pePackage.RequiredRecipes.Count - pePackage.MissingRecipes.Count - pePackage.IncorrectRecipes.Count) / pePackage.RequiredRecipes.Count)
                };

                ListViewItem presetItem = new ListViewItem(new string[]
                {
                    pePackage.Preset.Name,
                    compatibility[0].ToString("%00"),
                    compatibility[1].ToString("%00"),
                    compatibility[2].ToString("%00")
                });
                PresetSelectionListView.Items.Add(presetItem);
                presetItem.ToolTipText =
                    string.Format("Mods:\n") +
                    string.Format("     ({0}) Correct\n", (pePackage.RequiredMods.Count - pePackage.MissingMods.Count - pePackage.WrongVersionMods.Count)) +
                    string.Format("     ({0}) Missing\n", (pePackage.MissingMods.Count)) +
                    string.Format("     ({0}) Extra\n", (pePackage.AddedMods.Count)) +
                    string.Format("     ({0}) Wrong Version\n", (pePackage.WrongVersionMods.Count)) +
                    string.Format("Items:\n") +
                    string.Format("     ({0}) Correct\n", (pePackage.RequiredItems.Count - pePackage.MissingItems.Count)) +
                    string.Format("     ({0}) Missing\n", (pePackage.MissingItems.Count)) +
                    string.Format("Recipes:\n") +
                    string.Format("     ({0}) Correct\n", (pePackage.RequiredRecipes.Count - pePackage.MissingRecipes.Count - pePackage.IncorrectRecipes.Count)) +
                    string.Format("     ({0}) Missing\n", (pePackage.MissingRecipes.Count)) +
                    string.Format("     ({0}) Incorrect", (pePackage.IncorrectRecipes.Count));
            }
        }

        private void ConfirmationButton_Click(object sender, EventArgs e)
        {
            if(PresetSelectionListView.SelectedIndices.Count > 0)
            {
                ChosenPreset = PresetErrors[PresetSelectionListView.SelectedIndices[0]].Preset;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void CancellingButton_Click(object sender, EventArgs e)
        {
            ChosenPreset = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void PresetSelectionListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (PresetSelectionListView.SelectedIndices.Count > 0)
            {
                ChosenPreset = PresetErrors[PresetSelectionListView.SelectedIndices[0]].Preset;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
