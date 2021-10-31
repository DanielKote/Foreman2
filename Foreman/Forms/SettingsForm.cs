using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Foreman
{
    public partial class SettingsForm : Form
    {
        public class SettingsFormOptions
        {
            public Dictionary<Assembler, bool> Assemblers;
            public Dictionary<Miner, bool> Miners;
            public Dictionary<Module, bool> Modules;
            public List<string> Mods;

            public List<Preset> Presets;
            public string SelectedPreset;

            public SettingsFormOptions()
            {
                Assemblers = new Dictionary<Assembler, bool>();
                Miners = new Dictionary<Miner, bool>();
                Modules = new Dictionary<Module, bool>();
                Mods = new List<string>();

                Presets = new List<Preset>();
            }

            public SettingsFormOptions Clone()
            {
                SettingsFormOptions clone = new SettingsFormOptions();
                foreach (KeyValuePair<Assembler, bool> kvp in Assemblers)
                    clone.Assemblers.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<Miner, bool> kvp in Miners)
                    clone.Miners.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<Module, bool> kvp in Modules)
                    clone.Modules.Add(kvp.Key, kvp.Value);
                foreach (string mod in Mods)
                    clone.Mods.Add(mod);
                foreach (Preset preset in Presets)
                    clone.Presets.Add(preset);

                clone.SelectedPreset = this.SelectedPreset;

                return clone;
            }

            public bool Equals(SettingsFormOptions other, bool ignoreAssemblersMinersModules = true)
            {
                bool same = (other.SelectedPreset == this.SelectedPreset);

                if (!ignoreAssemblersMinersModules)
                {
                    if (same)
                        foreach (KeyValuePair<Assembler, bool> kvp in Assemblers)
                            same = same && other.Assemblers.Contains(kvp) && (kvp.Value == other.Assemblers[kvp.Key]);
                    if (same)
                        foreach (KeyValuePair<Miner, bool> kvp in Miners)
                            same = same && other.Miners.Contains(kvp) && (kvp.Value == other.Miners[kvp.Key]);
                    if (same)
                        foreach (KeyValuePair<Module, bool> kvp in Modules)
                            same = same && other.Modules.Contains(kvp) && (kvp.Value == other.Modules[kvp.Key]);
                }

                //ignore mods - its a readonly field

                return same;
            }
        }

        private SettingsFormOptions originalOptions;
        public SettingsFormOptions CurrentOptions;

        public SettingsForm(SettingsFormOptions options)
        {
            originalOptions = options;
            CurrentOptions = options.Clone();

            InitializeComponent();

            SelectPresetMenuItem.Click += SelectPresetMenuItem_Click;
            DeletePresetMenuItem.Click += DeletePresetMenuItem_Click;

            AssemblerSelectionBox.Items.AddRange(CurrentOptions.Assemblers.Keys.ToArray());
            AssemblerSelectionBox.Sorted = true;
            AssemblerSelectionBox.DisplayMember = "FriendlyName";
            for (int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
            {
                if (CurrentOptions.Assemblers[(Assembler)AssemblerSelectionBox.Items[i]])
                {
                    AssemblerSelectionBox.SetItemChecked(i, true);
                }
            }

            MinerSelectionBox.Items.AddRange(CurrentOptions.Miners.Keys.ToArray());
            MinerSelectionBox.Sorted = true;
            MinerSelectionBox.DisplayMember = "FriendlyName";
            for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
            {
                if (CurrentOptions.Miners[(Miner)MinerSelectionBox.Items[i]])
                {
                    MinerSelectionBox.SetItemChecked(i, true);
                }
            }

            ModuleSelectionBox.Items.AddRange(CurrentOptions.Modules.Keys.ToArray());
            ModuleSelectionBox.Sorted = true;
            ModuleSelectionBox.DisplayMember = "FriendlyName";
            for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
            {
                if (CurrentOptions.Modules[(Module)ModuleSelectionBox.Items[i]])
                {
                    ModuleSelectionBox.SetItemChecked(i, true);
                }
            }

            ModSelectionBox.Items.AddRange(CurrentOptions.Mods.ToArray());
            for (int i = 0; i < ModSelectionBox.Items.Count; i++)
                ModSelectionBox.SetItemChecked(i, true);

            CurrentPresetLabel.Text = CurrentOptions.SelectedPreset;
            PresetsListBox.Items.AddRange(CurrentOptions.Presets.ToArray());
            PresetsListBox.Items.RemoveAt(0); //0 is the currently active preset.
        }

        private void PresetsListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var index = PresetsListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                Preset rclickedPreset = ((Preset)PresetsListBox.Items[index]);
                PresetsListBox.SelectedIndex = index;

                if(rclickedPreset.IsCurrentlySelected)
                {
                    SelectPresetMenuItem.Text = "Current Preset";
                    SelectPresetMenuItem.Enabled = false;
                }
                else
                {
                    SelectPresetMenuItem.Text = "Use This Preset";
                    SelectPresetMenuItem.Enabled = true;
                }
                SelectPresetMenuItem.Enabled = !rclickedPreset.IsCurrentlySelected;
                if (rclickedPreset.IsDefaultPreset)
                {
                    DeletePresetMenuItem.Text = "Default Preset";
                    DeletePresetMenuItem.Enabled = false;
                }
                else
                {
                    DeletePresetMenuItem.Text = "Delete This Preset";
                    DeletePresetMenuItem.Enabled = !rclickedPreset.IsCurrentlySelected;
                }

                PresetMenuStrip.Show(Cursor.Position);
                PresetMenuStrip.Visible = true;
            }
            else
                PresetMenuStrip.Visible = false;
        }

        private void DeletePresetMenuItem_Click(object sender, EventArgs e)
        {
            Preset selectedPreset = (Preset)PresetsListBox.SelectedItem;
            if(!selectedPreset.IsCurrentlySelected && !selectedPreset.IsDefaultPreset) //safety check - should always pass
            {
               if(MessageBox.Show("Are you sure you wish to delete the \""+selectedPreset.Name+"\" preset? This is irreversible.", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string jsonPath = Path.Combine(new string[] { Application.StartupPath, "Presets", selectedPreset.Name + ".json" });
                    string iconPath = Path.Combine(new string[] { Application.StartupPath, "Presets", selectedPreset.Name + ".dat" });

                    if (File.Exists(jsonPath))
                        File.Delete(jsonPath);
                    if (File.Exists(iconPath))
                        File.Delete(iconPath);

                    PresetsListBox.Items.Remove(selectedPreset);
                    CurrentOptions.Presets.Remove(selectedPreset);
                }
            }
        }

        private void SelectPresetMenuItem_Click(object sender, EventArgs e)
        {
            Preset selectedPreset = (Preset)PresetsListBox.SelectedItem;
            if (!selectedPreset.IsCurrentlySelected) //safety check - should always pass
            {
                CurrentOptions.SelectedPreset = selectedPreset.Name;
                this.Close();
            }
        }

        //ASSEMBLER------------------------------------------------------------------------------------------
        private void AssemblerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CurrentOptions.Assemblers[(Assembler)AssemblerSelectionBox.Items[e.Index]] = (e.NewValue == CheckState.Checked);
        }
        private void AssemblerSelectionAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
            {
                AssemblerSelectionBox.SetItemChecked(i, true);
                CurrentOptions.Assemblers[(Assembler)AssemblerSelectionBox.Items[i]] = true;
            }
        }
        private void AssemblerSelectionNoneButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
            {
                AssemblerSelectionBox.SetItemChecked(i, false);
                CurrentOptions.Assemblers[(Assembler)AssemblerSelectionBox.Items[i]] = false;
            }
        }

        private void AssemblerSelectionBox_Leave(object sender, EventArgs e)
        {
            AssemblerSelectionBox.SelectedItem = null;
        }

        //MINER------------------------------------------------------------------------------------------
        private void MinerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CurrentOptions.Miners[(Miner)MinerSelectionBox.Items[e.Index]] = (e.NewValue == CheckState.Checked);
        }
        private void MinerSelectionAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
            {
                MinerSelectionBox.SetItemChecked(i, true);
                CurrentOptions.Miners[(Miner)MinerSelectionBox.Items[i]] = true;
            }
        }
        private void MinerSelectionNoneButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
            {
                MinerSelectionBox.SetItemChecked(i, false);
                CurrentOptions.Miners[(Miner)MinerSelectionBox.Items[i]] = false;
            }
        }

        private void MinerSelectionBox_Leave(object sender, EventArgs e)
        {
            MinerSelectionBox.SelectedItem = null;
        }

        //MODULE------------------------------------------------------------------------------------------
        private void ModuleSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CurrentOptions.Modules[(Module)ModuleSelectionBox.Items[e.Index]] = (e.NewValue == CheckState.Checked);
        }
        private void ModuleSelectionAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
            {
                ModuleSelectionBox.SetItemChecked(i, true);
                CurrentOptions.Modules[(Module)ModuleSelectionBox.Items[i]] = true;
            }
        }
        private void ModuleSelectionNoneButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
            {
                ModuleSelectionBox.SetItemChecked(i, false);
                CurrentOptions.Modules[(Module)ModuleSelectionBox.Items[i]] = false;
            }
        }

        private void ModuleSelectionBox_Leave(object sender, EventArgs e)
        {
            ModuleSelectionBox.SelectedItem = null;
        }

        //CONFIRM / RELOAD / CANCEL------------------------------------------------------------------------------------------
        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            CurrentOptions = originalOptions;
            this.Close();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) { }

        //PRESET FORMS (Import / compare)------------------------------------------------------------------------------------------

        private void ImportPresetButton_Click(object sender, EventArgs e)
        {
            using (ImportPresetForm form = new ImportPresetForm())
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = this.Left + 150;
                form.Top = this.Top + 100;
                DialogResult result = form.ShowDialog();

                if(result == DialogResult.OK && !string.IsNullOrEmpty(form.NewPresetName)) //we have added a new preset
                {
                    GC.Collect(); //we just added a new preset - this required the opening of (potentially) alot of zip files and processing of a ton of bitmaps that are now stuck in garbate. In large mod packs like A&B this could clear out 2GB+ of memory.
                    Preset newPreset = new Preset(form.NewPresetName, false, false);
                    CurrentOptions.Presets.Add(newPreset);
                    PresetsListBox.Items.Add(newPreset);

                    if(MessageBox.Show("Preset import complete! Do you wish to switch to the new preset?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentOptions.SelectedPreset = newPreset.Name;
                        this.Close();
                    }
                }
            }
        }

        private void ComparePresetsButton_Click(object sender, EventArgs e)
        {

        }
    }
}
