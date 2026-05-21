using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class SciencePacksLoadForm : Form {
        private readonly Dictionary<Button, bool> SciencePackButtons;
        private static readonly Color EnabledPackBGColor = Color.DeepSkyBlue;
        private static readonly Color DisabledPackBGColor = Color.DarkRed;
        private const int IconSize = 48; //actually a bit smaller due to button padding, but whatever.
        private const int MaxColumns = 14;

        private readonly DataCache DCache;
        private readonly HashSet<IDataObjectBase> EnabledObjects;

        public SciencePacksLoadForm(DataCache cache, HashSet<IDataObjectBase> enabledObjects) {
            DCache = cache;
            EnabledObjects = enabledObjects;

            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SciencePackTable.RowStyles[0].SizeType = SizeType.Absolute;
            SciencePackTable.RowStyles[0].Height = IconSize;

            SciencePackButtons = [];

            PopulateSciencePackOptions();
        }

        private void PopulateSciencePackOptions() {
            int rowCount = (DCache.SciencePacks.Count / MaxColumns) + (DCache.SciencePacks.Count % MaxColumns > 0 ? 1 : 0);
            int columnCount = (DCache.SciencePacks.Count / rowCount) + (DCache.SciencePacks.Count % rowCount > 0 ? 1 : 0);
            for (int i = 0; i < columnCount; i++)
                SciencePackTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, IconSize));
            SciencePackTable.ColumnStyles.RemoveAt(0);
            SciencePackTable.ColumnCount = SciencePackTable.ColumnStyles.Count;
            for (int i = 0; i < rowCount; i++)
                SciencePackTable.RowStyles.Add(new RowStyle(SizeType.Absolute, IconSize));
            SciencePackTable.RowStyles.RemoveAt(0);
            SciencePackTable.RowCount = SciencePackTable.RowStyles.Count;


            SciencePackTable.Height = IconSize;
            foreach (IItem sciencePack in DCache.SciencePacks) {
                Console.WriteLine(sciencePack);

                var button = new NFButton {
                    BackColor = DisabledPackBGColor,
                    ForeColor = Color.Gray,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    BackgroundImage = sciencePack.Icon,
                    UseVisualStyleBackColor = false,
                    FlatStyle = FlatStyle.Flat
                };
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = Color.Black;
                button.TabStop = false;
                button.Margin = new Padding(0);
                button.Size = new Size(1, 1);
                button.Dock = DockStyle.Fill;
                button.Tag = sciencePack;
                button.Enabled = true;

                button.MouseHover += new EventHandler(Button_MouseHover);
                button.MouseLeave += new EventHandler(Button_MouseLeave);
                button.Click += new EventHandler(Button_Click);

                SciencePackTable.Controls.Add(button);
                SciencePackButtons.Add(button, false);
            }

        }

        private void Button_Click(object? sender, EventArgs e) {
            if (sender is not Button sciPackButton)
                return;
            var sciPack = sciPackButton.Tag as IItem;
            bool enabled = !SciencePackButtons[sciPackButton];
            SciencePackButtons[sciPackButton] = enabled;
            sciPackButton.BackColor = enabled ? EnabledPackBGColor : DisabledPackBGColor;

            //NOTE: this is a bit wrong and can fail if there are multiple ways of getting to a given science pack (that are on different tech tree groups); ex: T3 science that can be researched either with T1A science or with T1B science (as this will consider it requiring both T1A AND T1B instead of T1A OR T1B)
            //but this situation should ideally never happen -> I dont know any mod that allows this sort of tech tree (wouldnt it be extremely confusing to have 2+ widely different techs that can grant you a science pack???)

            foreach (Button sciButton in SciencePackButtons.Keys.ToArray()) {
                if (enabled) //enable all science packs prerequisites of the clicked science pack
                {
                    if (sciPack is not null && sciButton.Tag is IItem item && DCache.SciencePackPrerequisites[sciPack].Contains(item)) {
                        sciButton.BackColor = EnabledPackBGColor;
                        SciencePackButtons[sciButton] = true;
                    }
                } else //disable all science packs that have the clicked science pack as their prerequisite
                  {
                    if (sciPack is not null && sciButton.Tag is IItem item && DCache.SciencePackPrerequisites[item].Contains(sciPack)) {
                        sciButton.BackColor = DisabledPackBGColor;
                        SciencePackButtons[sciButton] = false;
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------------------------Button hovers

        private void Button_MouseHover(object? sender, EventArgs e) {
            if (sender is Control control && control.Tag is IDataObjectBase dob) {
                ToolTip.SetText(dob.FriendlyName);
                ToolTip.Show(this, Point.Add(PointToClient(MousePosition), new Size(15, 5)));
            }
        }

        private void Button_MouseLeave(object? sender, EventArgs e) {
            if (sender is Control c)
                ToolTip.Hide(c);
        }

        private void ConfirmationButton_Click(object? sender, EventArgs e) {
            var acceptedSciencePacks = SciencePackButtons.Where(kvp => kvp.Value).Select(kvp => kvp.Key.Tag as IItem).OfType<IItem>().ToHashSet();
            EnabledObjects.Clear();
            if (DCache.PlayerAssembler is not null)
                EnabledObjects.Add(DCache.PlayerAssembler);

            //go through all technologies, check for fit compared to accepted science packs, and add its recipes to the set of enabled recipes
            foreach (ITechnology tech in DCache.Technologies.Values)
                if (tech.Available && !tech.SciPackList.Except(acceptedSciencePacks).Any())
                    EnabledObjects.UnionWith(tech.UnlockedRecipes);

            //go through all the assemblers, beacons, and modules and add them to the enabled set if at least one of their associated items has at least one production recipe that is in the enabled set.
            foreach (IAssembler assembler in DCache.Assemblers.Values) {
                bool enabled = false;
                foreach (IReadOnlyCollection<IRecipe> recipes in assembler.AssociatedItems.Select(item => item.ProductionRecipes))
                    foreach (IRecipe recipe in recipes)
                        enabled |= EnabledObjects.Contains(recipe);
                if (enabled)
                    EnabledObjects.Add(assembler);
            }

            foreach (IBeacon beacon in DCache.Beacons.Values) {
                bool enabled = false;
                foreach (IReadOnlyCollection<IRecipe> recipes in beacon.AssociatedItems.Select(item => item.ProductionRecipes))
                    foreach (IRecipe recipe in recipes)
                        enabled |= EnabledObjects.Contains(recipe);
                if (enabled)
                    EnabledObjects.Add(beacon);
            }

            foreach (IModule module in DCache.Modules.Values) {
                bool enabled = false;
                foreach (IRecipe recipe in module.AssociatedItem.ProductionRecipes)
                    enabled |= EnabledObjects.Contains(recipe);
                if (enabled)
                    EnabledObjects.Add(module);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
