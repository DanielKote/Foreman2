using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using Foreman.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    public class RecipeNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return recipeBgBrush; } }
        private static readonly Brush recipeBgBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
        private static readonly Pen productivityPen = new(Brushes.DarkRed, 6);
        private static readonly Pen productivityPlusPen = new(productivityPen.Brush, 2);
        private static readonly Pen extraProductivityPen = new(Brushes.Crimson, 6);

        private static readonly StringFormat textFormat = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private readonly AssemblerElement AssemblerElement;
        private readonly BeaconElement BeaconElement;

        internal RecipeNodeViewModel RecipeViewModel => (RecipeNodeViewModel)ViewModel;
        private string RecipeName => RecipeViewModel.BaseRecipe.FriendlyName ?? "";

        private static bool OptionsCopyAssemblerDefault = true;
        private static bool OptionsCopyExtraProductivityMinersDefault = true;
        private static bool OptionsCopyExtraProductivityNonMinersDefault = true;
        private static bool OptionsCopyFuelDefault = true;
        private static bool OptionsCopyModulesDefault = true;
        private static bool OptionsCopyBeaconDefault = true;
        private static bool OptionsCopyBeaconModulesDefault = true;

        public RecipeNodeElement(ProductionGraphViewer graphViewer, IRecipeNodeViewModel viewModel) : base(graphViewer, viewModel) {
            AssemblerElement = new AssemblerElement(graphViewer, this);
            AssemblerElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

            BeaconElement = new BeaconElement(graphViewer, this);
            BeaconElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

            UpdateState();
        }

        protected override void UpdateState() {
            //update tabs (necessary now that it is possible that an item was added or removed)... I am looking at you furnaces!!! ... also - with quality added to the game it is possible that the outputs will drastically change based on selected modules (add/remove quality)
            //done by first checking all old tabs and removing any that are no longer part of the displayed node, then looking at the displayed node io and adding any new tabs that are necessary.
            //could potentially be done by just deleting all the old ones and remaking them from scratch, but come on - thats much more intensive than just doing some checks!
            foreach (ItemTabElement oldTab in InputTabs.Where(tab => !RecipeViewModel.Inputs.Contains(tab.Item)).ToList()) {
                InputTabs.Remove(oldTab);
                oldTab.Dispose();
            }
            foreach (ItemTabElement oldTab in OutputTabs.Where(tab => !RecipeViewModel.Outputs.Contains(tab.Item)).ToList()) {
                OutputTabs.Remove(oldTab);
                oldTab.Dispose();
            }
            foreach (ItemQualityPair item in RecipeViewModel.Inputs)
                if (!InputTabs.Any(tab => tab.Item == item))
                    InputTabs.Add(new ItemTabElement(item, LinkType.Input, graphViewer, this));
            foreach (ItemQualityPair item in RecipeViewModel.Outputs)
                if (!OutputTabs.Any(tab => tab.Item == item))
                    OutputTabs.Add(new ItemTabElement(item, LinkType.Output, graphViewer, this));

            //now that the tabs have been updated, update the size and positioning of the node:
            int yOffset = (RecipeViewModel.NodeDirection == NodeDirection.Up && InputTabs.Count == 0 && OutputTabs.Count != 0) || (RecipeViewModel.NodeDirection == NodeDirection.Down && OutputTabs.Count == 0 && InputTabs.Count != 0) ? 10 :
                          (RecipeViewModel.NodeDirection == NodeDirection.Down && InputTabs.Count == 0 && OutputTabs.Count != 0) || (RecipeViewModel.NodeDirection == NodeDirection.Up && OutputTabs.Count == 0 && InputTabs.Count != 0) ? -10 : 0;
            yOffset += RecipeViewModel.NodeDirection == NodeDirection.Up ? 4 : 0;

            AssemblerElement.Location = new Point(-26, -14 + yOffset);
            BeaconElement.Location = new Point(-30, 27 + yOffset);

            AssemblerElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);
            BeaconElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

            Width = Math.Max(MinWidth, Math.Max(GetIconWidths(InputTabs), GetIconWidths(OutputTabs)) + 10);
            if (Width % WidthD != 0) {
                Width += WidthD;
                Width -= Width % WidthD;
            }
            Height = (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low) ? BaseSimpleHeight : BaseRecipeHeight;

            base.UpdateState();
        }

        protected override Bitmap? NodeIcon() => RecipeViewModel.BaseRecipe.Icon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low) //text only view
            {
                //text
                bool overproducing = RecipeViewModel.IsOverproducing();
                var textSlot = new Rectangle(trans.X - (Width / 2) + 40, trans.Y - (Height / 2) + (overproducing ? 32 : 27), (Width - 10 - 40), Height - (overproducing ? 64 : 54));
                //graphics.DrawRectangle(devPen, textSlot);
                int textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, RecipeName, BaseFont, textSlot);

                //assembler icon
                Bitmap assemblerIcon = RecipeViewModel.SelectedAssembler ? RecipeViewModel.SelectedAssembler.Icon ?? DataCache.UnknownIcon : DataCache.UnknownIcon;
                graphics.DrawImage(assemblerIcon, trans.X - Math.Min((Width / 2) - 10, (textLength / 2) + 32), trans.Y - 16, 32, 32);

                //productivity ticks
                int pModules = RecipeViewModel.AssemblerModules.Count(m => m.Module.GetProductivityBonus() > 0);
                pModules += (int)(RecipeViewModel.BeaconModules.Count(m => m.Module.GetProductivityBonus() > 0) * RecipeViewModel.BeaconCount);

                bool extraProductivity = RecipeViewModel.ExtraProductivity > 0 && (RecipeViewModel.SelectedAssembler.Assembler.EntityType == EntityType.Miner || graphViewer.Graph.EnableExtraProductivityForNonMiners);
                pModules += extraProductivity ? 1 : 0;

                for (int i = 0; i < pModules && i < 6; i++)
                    graphics.DrawEllipse((extraProductivity && i == 0) ? extraProductivityPen : productivityPen, trans.X - (Width / 2) - 1, trans.Y - (Height / 2) + 10 + i * 12, 6, 6);
                if (pModules > 6) {
                    graphics.DrawLine(productivityPlusPen, trans.X - (Width / 2) - 4, trans.Y - (Height / 2) + 84, trans.X - (Width / 2) + 8, trans.Y - (Height / 2) + 84);
                    graphics.DrawLine(productivityPlusPen, trans.X - (Width / 2) + 2, trans.Y - (Height / 2) + 84 - 6, trans.X - (Width / 2) + 2, trans.Y - (Height / 2) + 84 + 6);
                }
            } else if (RecipeViewModel.ExtraProductivity > 0 && (RecipeViewModel.SelectedAssembler.Assembler.EntityType == EntityType.Miner || graphViewer.Graph.EnableExtraProductivityForNonMiners)) {
                graphics.DrawEllipse(extraProductivityPen, trans.X - (Width / 2) - 1, trans.Y - (Height / 2) + 10, 6, 6);
            }
        }

        protected override void AddRClickMenuOptions(bool nodeInSelection) {
            if (nodeInSelection) {
                var rNodes = graphViewer.SelectedNodes.OfType<RecipeNodeElement>().Select(ne => (RecipeNodeViewModel)ne.ViewModel).ToList();
                if (!rNodes.Contains(RecipeViewModel))
                    rNodes.Add(RecipeViewModel);

                RightClickMenu.Items.Add(new ToolStripSeparator());

                RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default assembler(s)", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        foreach (RecipeNodeViewModel rNode in rNodes)
                            if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                controller.AutoSetAssembler();
                    })));
                RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default modules", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        foreach (RecipeNodeViewModel rNode in rNodes)
                            if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                controller.AutoSetAssemblerModules();
                    })));
                if (rNodes.Any(rn => rn.AssemblerModules.Count > 0))
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Remove modules", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            foreach (RecipeNodeViewModel rNode in rNodes)
                                if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                    controller.RemoveAssemblerModules();
                        })));
                if (rNodes.Any(rn => rn.SelectedBeacon))
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Remove beacons", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            foreach (RecipeNodeViewModel rNode in rNodes)
                                if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                    controller.ClearBeacon();
                        })));

                RightClickMenu.Items.Add(new ToolStripSeparator());
                if (graphViewer.DCache is DataCache readCache) {
                    if (NodeCopyOptions.GetNodeCopyOptions(Clipboard.GetText(), readCache) is NodeCopyOptions pasteOptions
                        && pasteOptions.Assembler.Assembler is IAssembler pastedAssembler) {
                        bool canPasteAssembler = rNodes.Any(rn => rn.BaseRecipe.Recipe is IRecipe rnRecipe && rnRecipe.Assemblers.Contains(pastedAssembler));
                        bool canPasteExtraProductivityMiners = rNodes.Any(rn => rn.SelectedAssembler.Assembler is IAssembler sa && sa.EntityType == EntityType.Miner);
                        bool canPasteExtraProductivityNonMiners = graphViewer.Graph.EnableExtraProductivityForNonMiners && rNodes.Any(rn => rn.SelectedAssembler.Assembler is IAssembler sa && sa.EntityType != EntityType.Miner);
                        bool canPasteFuel = pasteOptions.Fuel is IItem pasteFuelOption && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Recipe is IRecipe rnRecipe && rnRecipe.Assemblers.Any(a => a.Fuels.Contains(pasteFuelOption))));
                        bool canPasteModules = pasteOptions.AssemblerModules.Count > 0 && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Recipe is IRecipe rnRecipe && rnRecipe.AssemblerModules.Count > 0 && rn.SelectedAssembler.Assembler is IAssembler sa && sa.Modules.Count > 0 && sa.ModuleSlots > 0));
                        bool canPasteBeacon = pasteOptions.Beacon && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Recipe is IRecipe rnRecipe && rnRecipe.AssemblerModules.Count > 0 && rn.SelectedAssembler.Assembler is IAssembler sa && sa.Modules.Count > 0));

                        if (canPasteAssembler || canPasteFuel || canPasteModules || canPasteBeacon) {
                            RightClickMenu.ShowCheckMargin = true;

                            var tsAssemblerCheck = new ToolStripMenuItem(pastedAssembler.GetEntityTypeName(false)) { CheckOnClick = true, Checked = canPasteAssembler && OptionsCopyAssemblerDefault, Enabled = canPasteAssembler, Tag = "CheckBox" };
                            var tsExtraProductivityMinersCheck = new ToolStripMenuItem("Bonus Productivity (Miners)") { CheckOnClick = true, Checked = canPasteExtraProductivityMiners && OptionsCopyExtraProductivityMinersDefault, Enabled = canPasteExtraProductivityMiners, Tag = "CheckBox" };
                            var tsExtraProductivityNonMinersCheck = new ToolStripMenuItem("Bonus Productivity (non-Miners)") { CheckOnClick = true, Checked = canPasteExtraProductivityNonMiners && OptionsCopyExtraProductivityNonMinersDefault, Enabled = canPasteExtraProductivityNonMiners, Tag = "CheckBox" };
                            var tsFuelCheck = new ToolStripMenuItem("Fuel") { CheckOnClick = true, Checked = canPasteFuel && OptionsCopyFuelDefault, Enabled = canPasteFuel, Tag = "CheckBox" };
                            var tsModulesCheck = new ToolStripMenuItem("Modules") { CheckOnClick = true, Checked = canPasteModules && OptionsCopyModulesDefault, Enabled = canPasteModules, Tag = "CheckBox" };
                            var tsBeaconCheck = new ToolStripMenuItem("Beacon") { CheckOnClick = true, Checked = canPasteBeacon && OptionsCopyBeaconDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };
                            var tsBeaconModuleCheck = new ToolStripMenuItem("Beacon Modules") { CheckOnClick = true, Checked = canPasteBeacon && OptionsCopyBeaconModulesDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };

                            try {
                                var assemblerCheck = new WeakReference<ToolStripMenuItem>(tsAssemblerCheck);
                                var extraProductivityMinersCheck = new WeakReference<ToolStripMenuItem>(tsExtraProductivityMinersCheck);
                                var extraProductivityNonMinersCheck = new WeakReference<ToolStripMenuItem>(tsExtraProductivityNonMinersCheck);
                                var fuelCheck = new WeakReference<ToolStripMenuItem>(tsFuelCheck);
                                var modulesCheck = new WeakReference<ToolStripMenuItem>(tsModulesCheck);
                                var beaconCheck = new WeakReference<ToolStripMenuItem>(tsBeaconCheck);
                                var beaconModuleCheck = new WeakReference<ToolStripMenuItem>(tsBeaconModuleCheck);
                                if (canPasteAssembler) {
                                    RightClickMenu.Items.Add(tsAssemblerCheck);
                                    tsAssemblerCheck = null;
                                }
                                if (canPasteExtraProductivityMiners) {
                                    RightClickMenu.Items.Add(tsExtraProductivityMinersCheck);
                                    tsExtraProductivityMinersCheck = null;
                                }
                                if (canPasteExtraProductivityNonMiners) {
                                    RightClickMenu.Items.Add(tsExtraProductivityNonMinersCheck);
                                    tsExtraProductivityNonMinersCheck = null;
                                }
                                if (canPasteFuel) {
                                    RightClickMenu.Items.Add(tsFuelCheck);
                                    tsFuelCheck = null;
                                }
                                if (canPasteModules) {
                                    RightClickMenu.Items.Add(tsModulesCheck);
                                    tsModulesCheck = null;
                                }
                                if (canPasteBeacon) {
                                    RightClickMenu.Items.Add(tsBeaconCheck);
                                    tsBeaconCheck = null;
                                }
                                if (canPasteBeacon) {
                                    RightClickMenu.Items.Add(tsBeaconModuleCheck);
                                    tsBeaconModuleCheck = null;
                                }
                                RightClickMenu.Items.Add(new ToolStripSeparator());
                                RightClickMenu.Items.Add(new ToolStripMenuItem("Paste selected options", null,
                                    new EventHandler((o, e) => {
                                        RightClickMenu.Close();
                                        assemblerCheck.TryGetTarget(out var tsAssembler);
                                        extraProductivityMinersCheck.TryGetTarget(out var tsEpMiners);
                                        extraProductivityNonMinersCheck.TryGetTarget(out var tsEpNonMiners);
                                        fuelCheck.TryGetTarget(out var tsFuel);
                                        modulesCheck.TryGetTarget(out var tsModules);
                                        beaconCheck.TryGetTarget(out var tsBeacon);
                                        beaconModuleCheck.TryGetTarget(out var tsBeaconModule);
                                        if (canPasteAssembler && tsAssembler is not null)
                                            OptionsCopyAssemblerDefault = tsAssembler.Checked;
                                        if (canPasteExtraProductivityMiners && tsEpMiners is not null)
                                            OptionsCopyExtraProductivityMinersDefault = tsEpMiners.Checked;
                                        if (canPasteExtraProductivityNonMiners && tsEpNonMiners is not null)
                                            OptionsCopyExtraProductivityNonMinersDefault = tsEpNonMiners.Checked;
                                        if (canPasteFuel && tsFuel is not null)
                                            OptionsCopyFuelDefault = tsFuel.Checked;
                                        if (canPasteModules && tsModules is not null)
                                            OptionsCopyModulesDefault = tsModules.Checked;
                                        if (canPasteBeacon && tsBeacon is not null)
                                            OptionsCopyBeaconDefault = tsBeacon.Checked;
                                        if (canPasteBeacon && tsBeaconModule is not null)
                                            OptionsCopyBeaconModulesDefault = tsBeaconModule.Checked;

                                        foreach (RecipeNodeViewModel rNode in rNodes) {
                                            if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is not RecipeNodeController controller)
                                                continue;

                                            if (tsAssembler?.Checked is true && rNode.BaseRecipe.Recipe is IRecipe nodeRecipe && nodeRecipe.Assemblers.Contains(pastedAssembler)) {
                                                controller.SetAssembler(pasteOptions.Assembler);
                                                if (rNode.SelectedAssembler.Assembler is IAssembler selectedAssembler && selectedAssembler.EntityType == EntityType.Reactor)
                                                    controller.SetNeighbourCount(pasteOptions.NeighbourCount);
                                            }

                                            if (tsEpMiners?.Checked is true && rNode.SelectedAssembler.Assembler is IAssembler minerAssembler && minerAssembler.EntityType == EntityType.Miner)
                                                controller.SetExtraProductivityBonus(pasteOptions.ExtraProductivityBonus);
                                            if (tsEpNonMiners?.Checked is true && rNode.SelectedAssembler.Assembler is IAssembler nonMinerAssembler && nonMinerAssembler.EntityType != EntityType.Miner)
                                                controller.SetExtraProductivityBonus(pasteOptions.ExtraProductivityBonus);

                                            if (tsFuel?.Checked is true && pasteOptions.Fuel is IItem pasteFuel && rNode.SelectedAssembler.Assembler is IAssembler fuelAssembler && fuelAssembler.Fuels.Contains(pasteFuel))
                                                controller.SetFuel(pasteFuel);

                                            if (tsModules?.Checked is true && rNode.SelectedAssembler.Assembler is IAssembler moduleAssembler && rNode.BaseRecipe.Recipe is IRecipe moduleRecipe) {
                                                var acceptableAssemblerModules = new HashSet<IModule>(moduleRecipe.AssemblerModules.Intersect(moduleAssembler.Modules));
                                                if (!pasteOptions.AssemblerModules.Any(module => module.Module is IModule copiedModule && !acceptableAssemblerModules.Contains(copiedModule)))
                                                    controller.SetAssemblerModules(pasteOptions.AssemblerModules, true);
                                            }

                                            if (tsBeacon?.Checked is true && rNode.SelectedAssembler.Assembler is IAssembler beaconHostAssembler && rNode.BaseRecipe.Recipe is IRecipe beaconRecipe && beaconRecipe.AssemblerModules.Intersect(beaconHostAssembler.Modules).Any() && pasteOptions.Beacon) {
                                                controller.SetBeacon(pasteOptions.Beacon);
                                                controller.SetBeaconCount(pasteOptions.BeaconCount);
                                                controller.SetBeaconsCont(pasteOptions.BeaconsConst);
                                                controller.SetBeaconsPerAssembler(pasteOptions.BeaconsPerAssembler);
                                            }

                                            if (tsBeaconModule?.Checked is true && rNode.SelectedBeacon && rNode.SelectedBeacon.Beacon is IBeacon selectedBeacon && rNode.SelectedAssembler.Assembler is IAssembler beaconModuleHostAssembler && rNode.BaseRecipe.Recipe is IRecipe beaconModuleRecipe) {
                                                var acceptableBeaconModules = new HashSet<IModule>(beaconModuleRecipe.AssemblerModules.Intersect(beaconModuleHostAssembler.Modules).Intersect(selectedBeacon.Modules));
                                                if (!pasteOptions.BeaconModules.Any(module => module.Module is IModule copiedBeaconModule && !acceptableBeaconModules.Contains(copiedBeaconModule)))
                                                    controller.SetBeaconModules(pasteOptions.BeaconModules, true);
                                            }
                                        }

                                        graphViewer.Graph.UpdateNodeValues();
                                    })));

                                RightClickMenu.Items.Add(new ToolStripSeparator());
                            } finally {
                                tsAssemblerCheck?.Dispose();
                                tsExtraProductivityMinersCheck?.Dispose();
                                tsExtraProductivityNonMinersCheck?.Dispose();
                                tsFuelCheck?.Dispose();
                                tsModulesCheck?.Dispose();
                                tsBeaconCheck?.Dispose();
                                tsBeaconModuleCheck?.Dispose();
                            }
                        }
                    }
                }
            } else
                RightClickMenu.Items.Add(new ToolStripSeparator());

            RightClickMenu.Items.Add(new ToolStripMenuItem("Copy this assembler's options", null,
                new EventHandler((o, e) => {
                    RightClickMenu.Close();
                    Clipboard.SetText(GraphSaveCodec.WriteNodeCopyOptionsToString(new NodeCopyOptions(RecipeViewModel)));

                })));
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) {
            var tooltips = new List<TooltipInfo>();

            if (graphViewer.ShowRecipeToolTip) {
                if (RecipeViewModel.BaseRecipe.Recipe is IRecipe recipe) {
                    IRecipe[] recipes = [recipe];
                    var ttiRecipe = new TooltipInfo {
                        Direction = Direction.Left,
                        ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(Width / 2, 0))),
                        ScreenSize = RecipePainter.GetSize(recipes),
                        CustomDraw = (g, offset) => RecipePainter.Paint(recipes, g, offset)
                    };
                    tooltips.Add(ttiRecipe);
                }
            }

            string entityName = RecipeViewModel.SelectedAssembler.Assembler is IAssembler helpAssembler
                ? helpAssembler.GetEntityTypeName(false).ToLowerInvariant()
                : "assembler";
            tooltips.AddRange(ExclusiveHelpTooltip(
                string.Format(DisplayCulture.Format, "Left click on this node to edit its {0}, modules, beacon, etc.\nRight click for options.", entityName),
                exclusive));

            return tooltips;
        }
    }
}
