using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman {
    public class RecipeNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush => RecipeBgBrush;

        private static readonly Brush RecipeBgBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
        private static readonly Pen ProductivityPen = new(Brushes.DarkRed, 6);
        private static readonly Pen ProductivityPlusPen = new(ProductivityPen.Brush, 2);
        private static readonly Pen ExtraProductivityPen = new(Brushes.Crimson, 6);

        private static readonly StringFormat OwnTextFormat = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private readonly AssemblerElement _assemblerElement;
        private readonly BeaconElement _beaconElement;

        private string RecipeName => _displayedNode.BaseRecipe.FriendlyName;

        private readonly ReadOnlyRecipeNode _displayedNode;

        private static bool _optionsCopyAssemblerDefault = true;
        private static bool _optionsCopyExtraProductivityMinersDefault = true;
        private static bool _optionsCopyExtraProductivityNonMinersDefault = true;
        private static bool _optionsCopyFuelDefault = true;
        private static bool _optionsCopyModulesDefault = true;
        private static bool _optionsCopyBeaconDefault = true;
        private static bool _optionsCopyBeaconModulesDefault = true;

        public RecipeNodeElement(ProductionGraphViewer graphViewer, ReadOnlyRecipeNode node) : base(graphViewer, node) {
            _displayedNode = node;

            _assemblerElement = new AssemblerElement(graphViewer, this);
            _assemblerElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.Lod.Low);

            _beaconElement = new BeaconElement(graphViewer, this);
            _beaconElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.Lod.Low);

            UpdateState();
        }

        protected override void UpdateState() {
            // update tabs (necessary now that it is possible that an item was added or removed)... I am looking at you furnaces!!!
            // ... also - with quality added to the game it is possible that the outputs will drastically change based on selected modules (add/remove quality)
            // done by first checking all old tabs and removing any that are no longer part of the displayed node,
            // then looking at the displayed node io and adding any new tabs that are necessary.
            // could potentially be done by just deleting all the old ones and remaking them from scratch,
            // but come on - that's much more intensive than just doing some checks!

            foreach (var oldTab in InputTabs.Where(tab => !_displayedNode.Inputs.Contains(tab.Item)).ToList()) {
                InputTabs.Remove(oldTab);
                oldTab.Dispose();
            }

            foreach (var oldTab in OutputTabs.Where(tab => !_displayedNode.Outputs.Contains(tab.Item)).ToList()) {
                OutputTabs.Remove(oldTab);
                oldTab.Dispose();
            }

            foreach (var item in _displayedNode.Inputs)
                if (InputTabs.All(tab => tab.Item != item))
                    InputTabs.Add(new ItemTabElement(item, LinkType.Input, GraphViewer, this));
            foreach (var item in _displayedNode.Outputs)
                if (OutputTabs.All(tab => tab.Item != item))
                    OutputTabs.Add(new ItemTabElement(item, LinkType.Output, GraphViewer, this));

            // now that the tabs have been updated, update the size and positioning of the node:

            var yOffset = (_displayedNode.NodeDirection == NodeDirection.Up && InputTabs.Count == 0 && OutputTabs.Count != 0) ||
                (_displayedNode.NodeDirection == NodeDirection.Down && OutputTabs.Count == 0 && InputTabs.Count != 0) ? 10 :
                (_displayedNode.NodeDirection == NodeDirection.Down && InputTabs.Count == 0 && OutputTabs.Count != 0) ||
                (_displayedNode.NodeDirection == NodeDirection.Up && OutputTabs.Count == 0 && InputTabs.Count != 0) ? -10 : 0;
            yOffset += _displayedNode.NodeDirection == NodeDirection.Up ? 4 : 0;

            _assemblerElement.Location = new Point(-26, -14 + yOffset);
            _beaconElement.Location = new Point(-30, 27 + yOffset);

            _assemblerElement.SetVisibility(GraphViewer.LevelOfDetail != ProductionGraphViewer.Lod.Low);
            _beaconElement.SetVisibility(GraphViewer.LevelOfDetail != ProductionGraphViewer.Lod.Low);

            Width = Math.Max(MinWidth, Math.Max(GetIconWidths(InputTabs), GetIconWidths(OutputTabs)) + 10);
            if (Width % WidthD != 0) {
                Width += WidthD;
                Width -= Width % WidthD;
            }

            Height = GraphViewer.LevelOfDetail == ProductionGraphViewer.Lod.Low ? BaseSimpleHeight : BaseRecipeHeight;

            base.UpdateState();
        }

        protected override Bitmap NodeIcon() {
            return _displayedNode.BaseRecipe.Icon;
        }

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            if (GraphViewer.LevelOfDetail == ProductionGraphViewer.Lod.Low) { // text only view
                // text

                var overproducing = _displayedNode.IsOverproducing();
                var textSlot = new Rectangle(trans.X - Width / 2 + 40, trans.Y - Height / 2 + (overproducing ? 32 : 27), Width - 10 - 40,
                    Height - (overproducing ? 64 : 54));
                //graphics.DrawRectangle(devPen, textSlot);
                var textLength = GraphicsStuff.DrawText(graphics, TextBrush, OwnTextFormat, RecipeName, BaseFont, textSlot);

                // assembler icon

                graphics.DrawImage(_displayedNode.SelectedAssembler ? _displayedNode.SelectedAssembler.Icon : DataCache.UnknownIcon,
                    trans.X - Math.Min(Width / 2 - 10, textLength / 2 + 32), trans.Y - 16, 32, 32);

                // productivity ticks

                var pModules = _displayedNode.AssemblerModules.Count(m => m.Module.GetProductivityBonus() > 0);
                pModules += (int) (_displayedNode.BeaconModules.Count(m => m.Module.GetProductivityBonus() > 0) * _displayedNode.BeaconCount);

                var extraProductivity = _displayedNode.ExtraProductivity > 0
                    && (_displayedNode.SelectedAssembler.Assembler.EntityType == EntityType.Miner || GraphViewer.Graph.EnableExtraProductivityForNonMiners);
                pModules += extraProductivity ? 1 : 0;

                for (var i = 0; i < pModules && i < 6; i++) {
                    graphics.DrawEllipse(
                        extraProductivity && i == 0
                            ? ExtraProductivityPen
                            : ProductivityPen,
                        trans.X - Width / 2 - 1,
                        trans.Y - Height / 2 + 10 + i * 12,
                        6,
                        6
                    );
                }

                if (pModules > 6) {
                    graphics.DrawLine(
                        ProductivityPlusPen,
                        trans.X - Width / 2 - 4,
                        trans.Y - Height / 2 + 84,
                        trans.X - Width / 2 + 8,
                        trans.Y - Height / 2 + 84
                    );
                    graphics.DrawLine(
                        ProductivityPlusPen,
                        trans.X - Width / 2 + 2,
                        trans.Y - Height / 2 + 84 - 6,
                        trans.X - Width / 2 + 2,
                        trans.Y - Height / 2 + 84 + 6
                    );
                }
            } else if (_displayedNode.ExtraProductivity > 0
                && (_displayedNode.SelectedAssembler.Assembler.EntityType == EntityType.Miner || GraphViewer.Graph.EnableExtraProductivityForNonMiners)
            ) {
                graphics.DrawEllipse(ExtraProductivityPen, trans.X - Width / 2 - 1, trans.Y - Height / 2 + 10, 6, 6);
            }
        }

        protected override void AddRClickMenuOptions(bool nodeInSelection) {
            if (nodeInSelection) {
                var rNodes = new List<ReadOnlyRecipeNode>(GraphViewer.SelectedNodes.Where(ne => ne is RecipeNodeElement)
                    .Select(ne => (ReadOnlyRecipeNode) ne.DisplayedNode));
                if (!rNodes.Contains(_displayedNode))
                    rNodes.Add(_displayedNode);

                RightClickMenu.Items.Add(new ToolStripSeparator());

                RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default assembler(s)", null, (_, _) => {
                    RightClickMenu.Close();
                    foreach (var rNode in rNodes)
                        ((RecipeNodeController) GraphViewer.Graph.RequestNodeController(rNode)).AutoSetAssembler();
                }));
                RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default modules", null, (_, _) => {
                    RightClickMenu.Close();
                    foreach (var rNode in rNodes)
                        ((RecipeNodeController) GraphViewer.Graph.RequestNodeController(rNode)).AutoSetAssemblerModules();
                }));

                if (rNodes.Any(rn => rn.AssemblerModules.Count > 0)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Remove modules", null, (_, _) => {
                        RightClickMenu.Close();
                        foreach (var rNode in rNodes)
                            ((RecipeNodeController) GraphViewer.Graph.RequestNodeController(rNode)).RemoveAssemblerModules();
                    }));
                }

                if (rNodes.Any(rn => rn.SelectedBeacon)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Remove beacons", null, (_, _) => {
                        RightClickMenu.Close();
                        foreach (var rNode in rNodes)
                            ((RecipeNodeController) GraphViewer.Graph.RequestNodeController(rNode)).ClearBeacon();
                    }));
                }

                RightClickMenu.Items.Add(new ToolStripSeparator());
                var copiedOptions = NodeCopyOptions.GetNodeCopyOptions(Clipboard.GetText(), GraphViewer.DCache);
                if (copiedOptions != null) {
                    var canPasteAssembler = rNodes.Any(rn => rn.BaseRecipe.Recipe.Assemblers.Contains(copiedOptions.Assembler.Assembler));
                    var canPasteExtraProductivityMiners = rNodes.Any(rn => rn.SelectedAssembler.Assembler.EntityType == EntityType.Miner);
                    var canPasteExtraProductivityNonMiners = GraphViewer.Graph.EnableExtraProductivityForNonMiners &&
                        rNodes.Any(rn => rn.SelectedAssembler.Assembler.EntityType != EntityType.Miner);
                    var canPasteFuel = copiedOptions.Fuel != null &&
                        (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Recipe.Assemblers.Any(a => a.Fuels.Contains(copiedOptions.Fuel))));
                    var canPasteModules = copiedOptions.AssemblerModules.Count > 0 && (canPasteAssembler || rNodes.Any(rn =>
                        rn.BaseRecipe.Recipe.AssemblerModules.Count > 0 && rn.SelectedAssembler.Assembler.Modules.Count > 0 &&
                        rn.SelectedAssembler.Assembler.ModuleSlots > 0));
                    var canPasteBeacon = copiedOptions.Beacon && (canPasteAssembler || rNodes.Any(rn =>
                        rn.BaseRecipe.Recipe.AssemblerModules.Count > 0 && rn.SelectedAssembler.Assembler.Modules.Count > 0));

                    if (canPasteAssembler || canPasteFuel || canPasteModules || canPasteBeacon) {
                        RightClickMenu.ShowCheckMargin = true;

                        var assemblerCheck = new ToolStripMenuItem(copiedOptions.Assembler.Assembler.GetEntityTypeName(false))
                            { CheckOnClick = true, Checked = canPasteAssembler && _optionsCopyAssemblerDefault, Enabled = canPasteAssembler, Tag = "CheckBox" };
                        var extraProductivityMinersCheck = new ToolStripMenuItem("Bonus Productivity (Miners)") {
                            CheckOnClick = true, Checked = canPasteExtraProductivityMiners && _optionsCopyExtraProductivityMinersDefault,
                            Enabled = canPasteExtraProductivityMiners, Tag = "CheckBox"
                        };
                        var extraProductivityNonMinersCheck = new ToolStripMenuItem("Bonus Productivity (non-Miners)") {
                            CheckOnClick = true, Checked = canPasteExtraProductivityNonMiners && _optionsCopyExtraProductivityNonMinersDefault,
                            Enabled = canPasteExtraProductivityNonMiners, Tag = "CheckBox"
                        };
                        var fuelCheck = new ToolStripMenuItem("Fuel")
                            { CheckOnClick = true, Checked = canPasteFuel && _optionsCopyFuelDefault, Enabled = canPasteFuel, Tag = "CheckBox" };
                        var modulesCheck = new ToolStripMenuItem("Modules")
                            { CheckOnClick = true, Checked = canPasteModules && _optionsCopyModulesDefault, Enabled = canPasteModules, Tag = "CheckBox" };
                        var beaconCheck = new ToolStripMenuItem("Beacon")
                            { CheckOnClick = true, Checked = canPasteBeacon && _optionsCopyBeaconDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };
                        var beaconModuleCheck = new ToolStripMenuItem("Beacon Modules")
                            { CheckOnClick = true, Checked = canPasteBeacon && _optionsCopyBeaconModulesDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };

                        if (canPasteAssembler) RightClickMenu.Items.Add(assemblerCheck);
                        if (canPasteExtraProductivityMiners) RightClickMenu.Items.Add(extraProductivityMinersCheck);
                        if (canPasteExtraProductivityNonMiners) RightClickMenu.Items.Add(extraProductivityNonMinersCheck);
                        if (canPasteFuel) RightClickMenu.Items.Add(fuelCheck);
                        if (canPasteModules) RightClickMenu.Items.Add(modulesCheck);
                        if (canPasteBeacon) RightClickMenu.Items.Add(beaconCheck);
                        if (canPasteBeacon) RightClickMenu.Items.Add(beaconModuleCheck);
                        RightClickMenu.Items.Add(new ToolStripSeparator());
                        RightClickMenu.Items.Add(new ToolStripMenuItem("Paste selected options", null, (_, _) => {
                            RightClickMenu.Close();

                            if (canPasteAssembler)
                                _optionsCopyAssemblerDefault = assemblerCheck.Checked;
                            if (canPasteExtraProductivityMiners)
                                _optionsCopyExtraProductivityMinersDefault = extraProductivityMinersCheck.Checked;
                            if (canPasteExtraProductivityNonMiners)
                                _optionsCopyExtraProductivityNonMinersDefault = extraProductivityNonMinersCheck.Checked;
                            if (canPasteFuel)
                                _optionsCopyFuelDefault = fuelCheck.Checked;
                            if (canPasteModules)
                                _optionsCopyModulesDefault = modulesCheck.Checked;
                            if (canPasteBeacon)
                                _optionsCopyBeaconDefault = beaconCheck.Checked;
                            if (canPasteBeacon)
                                _optionsCopyBeaconModulesDefault = beaconCheck.Checked;

                            foreach (var rNode in rNodes) {
                                var controller = (RecipeNodeController) GraphViewer.Graph.RequestNodeController(rNode);

                                // if we do copy assembler, then all the other options are copied only if the assembler is.
                                // If we do not copy assembler, then paste options to everyone

                                var assemblerFilter = !assemblerCheck.Checked;
                                if (assemblerCheck.Checked
                                    && rNode.BaseRecipe.Recipe.Assemblers.Contains(copiedOptions.Assembler.Assembler)
                                ) { // assembler fits the given recipe
                                    controller.SetAssembler(copiedOptions.Assembler);
                                    assemblerFilter = true;
                                    if (rNode.SelectedAssembler.Assembler.EntityType == EntityType.Reactor)
                                        controller.SetNeighbourCount(copiedOptions.NeighbourCount);
                                }

                                if (extraProductivityMinersCheck.Checked && rNode.SelectedAssembler.Assembler.EntityType == EntityType.Miner)
                                    controller.SetExtraProductivityBonus(copiedOptions.ExtraProductivityBonus);
                                if (extraProductivityNonMinersCheck.Checked && rNode.SelectedAssembler.Assembler.EntityType != EntityType.Miner)
                                    controller.SetExtraProductivityBonus(copiedOptions.ExtraProductivityBonus);


                                // fuel fits the given recipe node
                                if (fuelCheck.Checked && rNode.SelectedAssembler.Assembler.Fuels.Contains(copiedOptions.Fuel))
                                    controller.SetFuel(copiedOptions.Fuel);

                                if (modulesCheck.Checked) {
                                    var acceptableAssemblerModules =
                                        new HashSet<Module>(rNode.BaseRecipe.Recipe.AssemblerModules.Intersect(rNode.SelectedAssembler.Assembler.Modules));

                                    // all modules we copied can be added to the selected recipe/assembler
                                    if (copiedOptions.AssemblerModules.All(module => acceptableAssemblerModules.Contains(module.Module)))
                                        controller.SetAssemblerModules(copiedOptions.AssemblerModules, true);
                                }

                                if (beaconCheck.Checked &&
                                    rNode.BaseRecipe.Recipe.AssemblerModules.Intersect(rNode.SelectedAssembler.Assembler.Modules).Any() &&
                                    copiedOptions.Beacon) {
                                    controller.SetBeacon(copiedOptions.Beacon);
                                    controller.SetBeaconCount(copiedOptions.BeaconCount);
                                    controller.SetBeaconsCont(copiedOptions.BeaconsConst);
                                    controller.SetBeaconsPerAssembler(copiedOptions.BeaconsPerAssembler);
                                }

                                if (beaconModuleCheck.Checked && rNode.SelectedBeacon) {
                                    var acceptableBeaconModules = new HashSet<Module>(rNode.BaseRecipe.Recipe.AssemblerModules
                                        .Intersect(rNode.SelectedAssembler.Assembler.Modules).Intersect(rNode.SelectedBeacon.Beacon.Modules));
                                    if (copiedOptions.BeaconModules.All(module => acceptableBeaconModules.Contains(module.Module)))
                                        controller.SetBeaconModules(copiedOptions.BeaconModules, true);
                                }
                            }

                            GraphViewer.Graph.UpdateNodeValues();
                        }));

                        RightClickMenu.Items.Add(new ToolStripSeparator());
                    }
                }
            } else
                RightClickMenu.Items.Add(new ToolStripSeparator());

            RightClickMenu.Items.Add(new ToolStripMenuItem("Copy this assembler's options", null, (_, _) => {
                RightClickMenu.Close();
                var stringBuilder = new StringBuilder();
                var writer = new JsonTextWriter(new StringWriter(stringBuilder));

                var serializer = JsonSerializer.Create();
                serializer.Formatting = Formatting.None;
                serializer.Serialize(writer, new NodeCopyOptions(_displayedNode));

                Clipboard.SetText(stringBuilder.ToString());
            }));
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) {
            var tooltips = new List<TooltipInfo>();

            if (GraphViewer.ShowRecipeToolTip) {
                var recipes = new[] { _displayedNode.BaseRecipe.Recipe };
                var ttiRecipe = new TooltipInfo {
                    Direction = Direction.Left,
                    ScreenLocation = GraphViewer.GraphToScreen(LocalToGraph(new Point(Width / 2, 0))),
                    ScreenSize = RecipePainter.GetSize(recipes),
                    CustomDraw = (g, offset) => { RecipePainter.Paint(recipes, g, offset); }
                };
                tooltips.Add(ttiRecipe);
            }

            if (!exclusive)
                return tooltips;

            var helpToolTipInfo = new TooltipInfo {
                Text =
                    $"Left click on this node to edit its {_displayedNode.SelectedAssembler.Assembler.GetEntityTypeName(false).ToLower()}, modules, beacon, etc.\nRight click for options.",
                Direction = Direction.None,
                ScreenLocation = new Point(10, 10)
            };
            tooltips.Add(helpToolTipInfo);

            return tooltips;
        }
    }
}