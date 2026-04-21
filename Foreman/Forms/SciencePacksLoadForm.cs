using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class SciencePacksLoadForm : Form {
        private Dictionary<Button, bool> _sciencePackButtons;
        private static Color _enabledPackBgColor = Color.DarkGreen;
        private static Color _disabledPackBgColor = Color.DarkRed;
        // actually a bit smaller due to button padding, but whatever.
        private const int IconSize = 48;
        private const int MaxColumns = 14;

        private readonly DataCache _dCache;
        private readonly HashSet<DataObjectBase> _enabledObjects;

        public SciencePacksLoadForm(DataCache cache, HashSet<DataObjectBase> enabledObjects) {
            _dCache = cache;
            _enabledObjects = enabledObjects;

            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SciencePackTable.RowStyles[0].SizeType = SizeType.Absolute;
            SciencePackTable.RowStyles[0].Height = IconSize;

            _sciencePackButtons = new Dictionary<Button, bool>();

            PopulateSciencePackOptions();
        }

        private void PopulateSciencePackOptions() {
            var rowCount = _dCache.SciencePacks.Count / MaxColumns + (_dCache.SciencePacks.Count % MaxColumns > 0 ? 1 : 0);
            var columnCount = _dCache.SciencePacks.Count / rowCount + (_dCache.SciencePacks.Count % rowCount > 0 ? 1 : 0);
            for (var i = 0; i < columnCount; i++)
                SciencePackTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, IconSize));
            SciencePackTable.ColumnStyles.RemoveAt(0);
            SciencePackTable.ColumnCount = SciencePackTable.ColumnStyles.Count;
            for (var i = 0; i < rowCount; i++)
                SciencePackTable.RowStyles.Add(new RowStyle(SizeType.Absolute, IconSize));
            SciencePackTable.RowStyles.RemoveAt(0);
            SciencePackTable.RowCount = SciencePackTable.RowStyles.Count;


            SciencePackTable.Height = IconSize;
            foreach (var sciencePack in _dCache.SciencePacks) {
                Console.WriteLine(sciencePack);

                var button = new NfButton();
                button.BackColor = _disabledPackBgColor;
                button.ForeColor = Color.Gray;
                button.BackgroundImageLayout = ImageLayout.Zoom;
                button.BackgroundImage = sciencePack.Icon;
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = Color.Black;
                button.TabStop = false;
                button.Margin = new Padding(0);
                button.Size = new Size(1, 1);
                button.Dock = DockStyle.Fill;
                button.Tag = sciencePack;
                button.Enabled = true;

                button.MouseHover += Button_MouseHover;
                button.MouseLeave += Button_MouseLeave;
                button.Click += Button_Click;

                SciencePackTable.Controls.Add(button);
                _sciencePackButtons.Add(button, false);
            }
        }

        private void Button_Click(object sender, EventArgs e) {
            var sciPackButton = (Button) sender;
            var sciPack = sciPackButton.Tag as Item;
            var enabled = !_sciencePackButtons[sciPackButton];
            _sciencePackButtons[sciPackButton] = enabled;
            sciPackButton.BackColor = enabled ? _enabledPackBgColor : _disabledPackBgColor;

            // NOTE: this is a bit wrong and can fail if there are multiple ways of getting to a given science pack
            // (that are on different tech tree groups); ex: T3 science that can be researched either with T1A science
            // or with T1B science (as this will consider it requiring both T1A AND T1B instead of T1A OR T1B)
            // but this situation should ideally never happen -> I don't know any mod that allows this sort of tech tree
            // (wouldn't it be extremely confusing to have 2+ widely different techs that can grant you a science pack???)

            foreach (var sciButton in _sciencePackButtons.Keys.ToArray()) {
                if (enabled) { // enable all science packs prerequisites of the clicked science pack
                    if (!_dCache.SciencePackPrerequisites[sciPack].Contains((Item) sciButton.Tag))
                        continue;

                    sciButton.BackColor = _enabledPackBgColor;
                    _sciencePackButtons[sciButton] = true;
                } else { // disable all science packs that have the clicked science pack as their prerequisite
                    if (!_dCache.SciencePackPrerequisites[(Item) sciButton.Tag].Contains(sciPack))
                        continue;

                    sciButton.BackColor = _disabledPackBgColor;
                    _sciencePackButtons[sciButton] = false;
                }
            }
        }

        //------------------------------------------------------------------------------------------------------Button hovers

        private void Button_MouseHover(object sender, EventArgs e) {
            var control = (Control) sender;
            if (control.Tag is not DataObjectBase dob)
                return;

            ToolTip.SetText(dob.FriendlyName);
            ToolTip.Show(this, Point.Add(PointToClient(MousePosition), new Size(15, 5)));
        }

        private void Button_MouseLeave(object sender, EventArgs e) {
            ToolTip.Hide((Control) sender);
        }

        private void ConfirmationButton_Click(object sender, EventArgs e) {
            var acceptedSciencePacks = new HashSet<Item>(_sciencePackButtons.Where(kvp => kvp.Value).Select(kvp => kvp.Key.Tag as Item));
            _enabledObjects.Clear();
            _enabledObjects.Add(_dCache.PlayerAssembler);

            // go through all technologies, check for fit compared to accepted science packs, and add its recipes to the set of enabled recipes

            foreach (var tech in _dCache.Technologies.Values) {
                if (tech.Available && !tech.SciPackList.Except(acceptedSciencePacks).Any())
                    _enabledObjects.UnionWith(tech.UnlockedRecipes);
            }

            // go through all the assemblers, beacons, and modules and add them to the enabled set if at least one of their associated items has at least one production recipe that is in the enabled set.

            foreach (var assembler in _dCache.Assemblers.Values) {
                var enabled = false;
                foreach (var recipes in assembler.AssociatedItems.Select(item => item.ProductionRecipes))
                foreach (var recipe in recipes)
                    enabled |= _enabledObjects.Contains(recipe);
                if (enabled)
                    _enabledObjects.Add(assembler);
            }

            foreach (var beacon in _dCache.Beacons.Values) {
                var enabled = false;
                foreach (var recipes in beacon.AssociatedItems.Select(item => item.ProductionRecipes))
                foreach (var recipe in recipes)
                    enabled |= _enabledObjects.Contains(recipe);
                if (enabled)
                    _enabledObjects.Add(beacon);
            }

            foreach (var module in _dCache.Modules.Values) {
                var enabled = false;
                foreach (var recipe in module.AssociatedItem.ProductionRecipes)
                    enabled |= _enabledObjects.Contains(recipe);
                if (enabled)
                    _enabledObjects.Add(module);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}