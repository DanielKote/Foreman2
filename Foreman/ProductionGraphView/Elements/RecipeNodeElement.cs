using Foreman.Graph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public class RecipeNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return recipeBgBrush; } }
        private static readonly Brush recipeBgBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
        private static readonly Pen productivityPen = new Pen(Brushes.DarkRed, 6);
        private static readonly Pen productivityPlusPen = new Pen(productivityPen.Brush, 2);
        private static readonly Pen extraProductivityPen = new Pen(Brushes.Crimson, 6);

        private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private readonly AssemblerElement AssemblerElement;
        private readonly BeaconElement BeaconElement;

        internal IRecipeNodeViewModel RecipeViewModel => (IRecipeNodeViewModel)ViewModel;
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
                Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 40, trans.Y - (Height / 2) + (overproducing ? 32 : 27), (Width - 10 - 40), Height - (overproducing ? 64 : 54));
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
                List<IRecipeNodeViewModel> rNodes = graphViewer.SelectedNodes.OfType<RecipeNodeElement>().Select(ne => (IRecipeNodeViewModel)ne.ViewModel).ToList();
                if (!rNodes.Contains(RecipeViewModel))
                    rNodes.Add(RecipeViewModel);

                RightClickMenu.Items.Add(new ToolStripSeparator());

                RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default assembler(s)", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        foreach (IRecipeNodeViewModel rNode in rNodes)
                            if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                controller.AutoSetAssembler();
                    })));
                RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default modules", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        foreach (IRecipeNodeViewModel rNode in rNodes)
                            if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                controller.AutoSetAssemblerModules();
                    })));
                if (rNodes.Any(rn => rn.AssemblerModules.Count > 0))
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Remove modules", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            foreach (IRecipeNodeViewModel rNode in rNodes)
                                if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                    controller.RemoveAssemblerModules();
                        })));
                if (rNodes.Any(rn => rn.SelectedBeacon))
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Remove beacons", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            foreach (IRecipeNodeViewModel rNode in rNodes)
                                if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is RecipeNodeController controller)
                                    controller.ClearBeacon();
                        })));

                RightClickMenu.Items.Add(new ToolStripSeparator());
                if (graphViewer.DCache is DataCache readCache) {
                    if (NodeCopyOptions.GetNodeCopyOptions(Clipboard.GetText(), readCache) is NodeCopyOptions pasteOptions
                        && pasteOptions.Assembler.Assembler is Assembler pastedAssembler) {
                        bool canPasteAssembler = rNodes.Any(rn => rn.BaseRecipe.Recipe is Recipe rnRecipe && rnRecipe.Assemblers.Contains(pastedAssembler));
                        bool canPasteExtraProductivityMiners = rNodes.Any(rn => rn.SelectedAssembler.Assembler is Assembler sa && sa.EntityType == EntityType.Miner);
                        bool canPasteExtraProductivityNonMiners = graphViewer.Graph.EnableExtraProductivityForNonMiners && rNodes.Any(rn => rn.SelectedAssembler.Assembler is Assembler sa && sa.EntityType != EntityType.Miner);
                        bool canPasteFuel = pasteOptions.Fuel is Item pasteFuelOption && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Recipe is Recipe rnRecipe && rnRecipe.Assemblers.Any(a => a.Fuels.Contains(pasteFuelOption))));
                        bool canPasteModules = pasteOptions.AssemblerModules.Count > 0 && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Recipe is Recipe rnRecipe && rnRecipe.AssemblerModules.Count > 0 && rn.SelectedAssembler.Assembler is Assembler sa && sa.Modules.Count > 0 && sa.ModuleSlots > 0));
                        bool canPasteBeacon = pasteOptions.Beacon && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Recipe is Recipe rnRecipe && rnRecipe.AssemblerModules.Count > 0 && rn.SelectedAssembler.Assembler is Assembler sa && sa.Modules.Count > 0));

                        if (canPasteAssembler || canPasteFuel || canPasteModules || canPasteBeacon) {
                            RightClickMenu.ShowCheckMargin = true;

                            ToolStripMenuItem assemblerCheck = new ToolStripMenuItem(pastedAssembler.GetEntityTypeName(false)) { CheckOnClick = true, Checked = canPasteAssembler && OptionsCopyAssemblerDefault, Enabled = canPasteAssembler, Tag = "CheckBox" };
                            ToolStripMenuItem extraProductivityMinersCheck = new ToolStripMenuItem("Bonus Productivity (Miners)") { CheckOnClick = true, Checked = canPasteExtraProductivityMiners && OptionsCopyExtraProductivityMinersDefault, Enabled = canPasteExtraProductivityMiners, Tag = "CheckBox" };
                            ToolStripMenuItem extraProductivityNonMinersCheck = new ToolStripMenuItem("Bonus Productivity (non-Miners)") { CheckOnClick = true, Checked = canPasteExtraProductivityNonMiners && OptionsCopyExtraProductivityNonMinersDefault, Enabled = canPasteExtraProductivityNonMiners, Tag = "CheckBox" };
                            ToolStripMenuItem fuelCheck = new ToolStripMenuItem("Fuel") { CheckOnClick = true, Checked = canPasteFuel && OptionsCopyFuelDefault, Enabled = canPasteFuel, Tag = "CheckBox" };
                            ToolStripMenuItem modulesCheck = new ToolStripMenuItem("Modules") { CheckOnClick = true, Checked = canPasteModules && OptionsCopyModulesDefault, Enabled = canPasteModules, Tag = "CheckBox" };
                            ToolStripMenuItem beaconCheck = new ToolStripMenuItem("Beacon") { CheckOnClick = true, Checked = canPasteBeacon && OptionsCopyBeaconDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };
                            ToolStripMenuItem beaconModuleCheck = new ToolStripMenuItem("Beacon Modules") { CheckOnClick = true, Checked = canPasteBeacon && OptionsCopyBeaconModulesDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };

                            if (canPasteAssembler)
                                RightClickMenu.Items.Add(assemblerCheck);
                            if (canPasteExtraProductivityMiners)
                                RightClickMenu.Items.Add(extraProductivityMinersCheck);
                            if (canPasteExtraProductivityNonMiners)
                                RightClickMenu.Items.Add(extraProductivityNonMinersCheck);
                            if (canPasteFuel)
                                RightClickMenu.Items.Add(fuelCheck);
                            if (canPasteModules)
                                RightClickMenu.Items.Add(modulesCheck);
                            if (canPasteBeacon)
                                RightClickMenu.Items.Add(beaconCheck);
                            if (canPasteBeacon)
                                RightClickMenu.Items.Add(beaconModuleCheck);
                            RightClickMenu.Items.Add(new ToolStripSeparator());
                            RightClickMenu.Items.Add(new ToolStripMenuItem("Paste selected options", null,
                                new EventHandler((o, e) => {
                                    RightClickMenu.Close();
                                    if (canPasteAssembler)
                                        OptionsCopyAssemblerDefault = assemblerCheck.Checked;
                                    if (canPasteExtraProductivityMiners)
                                        OptionsCopyExtraProductivityMinersDefault = extraProductivityMinersCheck.Checked;
                                    if (canPasteExtraProductivityNonMiners)
                                        OptionsCopyExtraProductivityNonMinersDefault = extraProductivityNonMinersCheck.Checked;
                                    if (canPasteFuel)
                                        OptionsCopyFuelDefault = fuelCheck.Checked;
                                    if (canPasteModules)
                                        OptionsCopyModulesDefault = modulesCheck.Checked;
                                    if (canPasteBeacon)
                                        OptionsCopyBeaconDefault = beaconCheck.Checked;
                                    if (canPasteBeacon)
                                        OptionsCopyBeaconModulesDefault = beaconCheck.Checked;

                                    foreach (IRecipeNodeViewModel rNode in rNodes) {
                                        if (graphViewer.Session.Editor.RequestNodeController(rNode.Id) is not RecipeNodeController controller)
                                            continue;

                                        if (assemblerCheck.Checked && rNode.BaseRecipe.Recipe is Recipe nodeRecipe && nodeRecipe.Assemblers.Contains(pastedAssembler)) {
                                            controller.SetAssembler(pasteOptions.Assembler);
                                            if (rNode.SelectedAssembler.Assembler is Assembler selectedAssembler && selectedAssembler.EntityType == EntityType.Reactor)
                                                controller.SetNeighbourCount(pasteOptions.NeighbourCount);
                                        }

                                        if (extraProductivityMinersCheck.Checked && rNode.SelectedAssembler.Assembler is Assembler minerAssembler && minerAssembler.EntityType == EntityType.Miner)
                                            controller.SetExtraProductivityBonus(pasteOptions.ExtraProductivityBonus);
                                        if (extraProductivityNonMinersCheck.Checked && rNode.SelectedAssembler.Assembler is Assembler nonMinerAssembler && nonMinerAssembler.EntityType != EntityType.Miner)
                                            controller.SetExtraProductivityBonus(pasteOptions.ExtraProductivityBonus);

                                        if (fuelCheck.Checked && pasteOptions.Fuel is Item pasteFuel && rNode.SelectedAssembler.Assembler is Assembler fuelAssembler && fuelAssembler.Fuels.Contains(pasteFuel))
                                            controller.SetFuel(pasteFuel);

                                        if (modulesCheck.Checked && rNode.SelectedAssembler.Assembler is Assembler moduleAssembler && rNode.BaseRecipe.Recipe is Recipe moduleRecipe) {
                                            HashSet<Module> acceptableAssemblerModules = new HashSet<Module>(moduleRecipe.AssemblerModules.Intersect(moduleAssembler.Modules));
                                            if (!pasteOptions.AssemblerModules.Any(module => module.Module is Module copiedModule && !acceptableAssemblerModules.Contains(copiedModule)))
                                                controller.SetAssemblerModules(pasteOptions.AssemblerModules, true);
                                        }

                                        if (beaconCheck.Checked && rNode.SelectedAssembler.Assembler is Assembler beaconHostAssembler && rNode.BaseRecipe.Recipe is Recipe beaconRecipe && beaconRecipe.AssemblerModules.Intersect(beaconHostAssembler.Modules).Any() && pasteOptions.Beacon) {
                                            controller.SetBeacon(pasteOptions.Beacon);
                                            controller.SetBeaconCount(pasteOptions.BeaconCount);
                                            controller.SetBeaconsCont(pasteOptions.BeaconsConst);
                                            controller.SetBeaconsPerAssembler(pasteOptions.BeaconsPerAssembler);
                                        }

                                        if (beaconModuleCheck.Checked && rNode.SelectedBeacon && rNode.SelectedBeacon.Beacon is Beacon selectedBeacon && rNode.SelectedAssembler.Assembler is Assembler beaconModuleHostAssembler && rNode.BaseRecipe.Recipe is Recipe beaconModuleRecipe) {
                                            HashSet<Module> acceptableBeaconModules = new HashSet<Module>(beaconModuleRecipe.AssemblerModules.Intersect(beaconModuleHostAssembler.Modules).Intersect(selectedBeacon.Modules));
                                            if (!pasteOptions.BeaconModules.Any(module => module.Module is Module copiedBeaconModule && !acceptableBeaconModules.Contains(copiedBeaconModule)))
                                                controller.SetBeaconModules(pasteOptions.BeaconModules, true);
                                        }
                                    }

                                    graphViewer.Graph.UpdateNodeValues();
                                })));

                            RightClickMenu.Items.Add(new ToolStripSeparator());
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

        protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive) {
            List<TooltipInfo> tooltips = new List<TooltipInfo>();

            if (graphViewer.ShowRecipeToolTip) {
                if (RecipeViewModel.BaseRecipe.Recipe is Recipe recipe) {
                    Recipe[] recipes = [recipe];
                    TooltipInfo ttiRecipe = new TooltipInfo();
                    ttiRecipe.Direction = Direction.Left;
                    ttiRecipe.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(Width / 2, 0)));
                    ttiRecipe.ScreenSize = RecipePainter.GetSize(recipes);
                    ttiRecipe.CustomDraw = (Graphics g, Point offset) => RecipePainter.Paint(recipes, g, offset);
                    tooltips.Add(ttiRecipe);
                }
            }

            string entityName = RecipeViewModel.SelectedAssembler.Assembler is Assembler helpAssembler
                ? helpAssembler.GetEntityTypeName(false).ToLower()
                : "assembler";
            tooltips.AddRange(ExclusiveHelpTooltip(
                string.Format("Left click on this node to edit its {0}, modules, beacon, etc.\nRight click for options.", entityName),
                exclusive));

            return tooltips;
        }
    }
}