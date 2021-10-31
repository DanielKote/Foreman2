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

            public List<string> Presets;
            public string SelectedPreset;

            public SettingsFormOptions()
            {
                Assemblers = new Dictionary<Assembler, bool>();
                Miners = new Dictionary<Miner, bool>();
                Modules = new Dictionary<Module, bool>();
                Mods = new List<string>();

                Presets = new List<string>();
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
                foreach (string preset in Presets)
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
        public bool ReloadRequired;
        private bool modListEnabled = false;

        public SettingsForm(SettingsFormOptions options)
        {
            originalOptions = options;
            CurrentOptions = options.Clone();
            ReloadRequired = false;

            InitializeComponent();

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
            ModSelectionBox.Sorted = true;
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

        //CONFIRM / RELOAD / CANCEL------------------------------------------------------------------------------------------
        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            ReloadRequired = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            CurrentOptions = originalOptions;
            this.Close();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {


        }
    }
}
