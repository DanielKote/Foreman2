using Foreman.Graph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foreman {
    class BeaconElement : GraphElement {
        private const int BeaconIconSize = 28;
        private const int ModuleIconSize = 12;
        private const int ModuleSpacing = 11;

        //in this case it is easier to work with 0,0 coordinates being the top-left most corner.
        private static readonly Point[] moduleLocations = new Point[] { new Point(ModuleSpacing * 2, 0), new Point(ModuleSpacing * 2, ModuleSpacing), new Point(ModuleSpacing, 0), new Point(ModuleSpacing, ModuleSpacing), new Point(0, 0), new Point(0, ModuleSpacing) };
        private static readonly Point moduleOffset = new Point(10, 3);

        private static readonly Pen speedModulePen = new Pen(Brushes.DarkBlue, 2);
        private static readonly Pen prodModulePen = new Pen(Brushes.DarkRed, 2);
        private static readonly Pen effModulePen = new Pen(Brushes.DarkGreen, 2);
        private static readonly Pen qualityModulePen = new Pen(Brushes.Gold, 2);
        private static readonly Pen unknownModulePen = new Pen(Brushes.Black, 2);
        private static readonly Font moduleFont = new Font(FontFamily.GenericSansSerif, 5, FontStyle.Bold);

        private static readonly Font counterBaseFont = new Font(FontFamily.GenericSansSerif, 8);
        private static readonly Brush textBrush = Brushes.Black;
        private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

        private IRecipeNodeViewModel RecipeViewModel => (IRecipeNodeViewModel)parent.ViewModel;
        private readonly RecipeNodeElement parent;

        public BeaconElement(ProductionGraphViewer graphViewer, RecipeNodeElement parent) : base(graphViewer, parent) {
            this.parent = parent;

            Width = BeaconIconSize + (ModuleSpacing * 3) + 12;
            Height = BeaconIconSize;
        }

        public void SetVisibility(bool visible) {
            Visible = visible;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (!RecipeViewModel.SelectedBeacon || style == NodeDrawingStyle.IconsOnly || style == NodeDrawingStyle.Simple)
                return;

            Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
            //graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);

            //beacon
            if (RecipeViewModel.SelectedBeacon.Icon is Bitmap beaconIcon)
                graphics.DrawImage(beaconIcon, trans.X + moduleOffset.X + ModuleSpacing * 3 + 2, trans.Y, BeaconIconSize, BeaconIconSize);

            //modules
            if (RecipeViewModel.BeaconModules.Count <= 6) {

                for (int i = 0; i < moduleLocations.Length && i < RecipeViewModel.BeaconModules.Count; i++)
                    if (RecipeViewModel.BeaconModules[i].Icon is Bitmap moduleIcon)
                        graphics.DrawImage(moduleIcon, trans.X + moduleLocations[i].X + moduleOffset.X, trans.Y + moduleLocations[i].Y + moduleOffset.Y, ModuleIconSize, ModuleIconSize);
            } else if (RecipeViewModel.BeaconModules.Count <= 8 * 4) //resot to drawing circles for each module instead -> 8x4 set, so 32 max modules
              {
                for (int x = 0; x < 8; x++) {
                    for (int y = 0; y < 4; y++) {
                        if (RecipeViewModel.BeaconModules.Count > (x * 4) + y) {
                            Pen marker = RecipeViewModel.BeaconModules[(x * 7) + y].Module.GetProductivityBonus() > 0 ? prodModulePen :
                                RecipeViewModel.BeaconModules[(x * 7) + y].Module.GetQualityBonus() > 0 ? qualityModulePen :
                                RecipeViewModel.BeaconModules[(x * 7) + y].Module.GetConsumptionBonus() < 0 ? effModulePen :
                                RecipeViewModel.BeaconModules[(x * 7) + y].Module.GetSpeedBonus() > 0 ? speedModulePen :
                                unknownModulePen;
                            graphics.DrawEllipse(marker, trans.X + moduleOffset.X + (ModuleSpacing * 2) + ModuleIconSize - 5 - (x * 5), trans.Y + moduleOffset.Y + 2 + (y * 5), 2, 2);
                        }
                    }
                }
            } else {
                int prodModules = RecipeViewModel.BeaconModules.Count(m => m.Module.GetProductivityBonus() > 0);
                int qualityModules = RecipeViewModel.BeaconModules.Count(m => m.Module.GetQualityBonus() > 0 && m.Module.GetProductivityBonus() <= 0);
                int efficiencyModules = RecipeViewModel.BeaconModules.Count(m => m.Module.GetConsumptionBonus() < 0 && m.Module.GetProductivityBonus() <= 0 && m.Module.GetQualityBonus() <= 0);
                int speedModules = RecipeViewModel.BeaconModules.Count(m => m.Module.GetSpeedBonus() > 0 && m.Module.GetConsumptionBonus() >= 0 && m.Module.GetProductivityBonus() <= 0 && m.Module.GetQualityBonus() <= 0);
                int unknownModules = RecipeViewModel.BeaconModules.Count - prodModules - efficiencyModules - speedModules - qualityModules;
                graphics.DrawString(string.Format("S:{0}", speedModules), moduleFont, Brushes.DarkBlue, trans.X, trans.Y + 5);
                graphics.DrawString(string.Format("E:{0}", efficiencyModules), moduleFont, Brushes.DarkGreen, trans.X, trans.Y + 15);
                graphics.DrawString(string.Format("P:{0}", prodModules), moduleFont, Brushes.DarkRed, trans.X + 22, trans.Y + 5);
                graphics.DrawString(string.Format("Q:{0}", qualityModules), moduleFont, Brushes.Gold, trans.X + 22, trans.Y + 15);
                graphics.DrawString(string.Format("U:{0}", unknownModules), moduleFont, Brushes.Black, trans.X, trans.Y + 25);
            }

            //quantity
            if (RecipeViewModel.SelectedBeacon) // && recipeNode.BeaconCount > 0)
            {
                int parentHalfWidth = myParent is RecipeNodeElement recipeParent ? recipeParent.Width : Width;
                Rectangle textbox = new Rectangle(trans.X + Width, trans.Y + 5, (parentHalfWidth / 2) - this.X - (this.Width / 2) - 6, 18);
                //graphics.DrawRectangle(devPen, textbox);

                double beaconCount = RecipeViewModel.GetTotalBeacons();
                string sbeaconCount = (beaconCount >= 10000) ? beaconCount.ToString("0.##e0") : beaconCount.ToString("0");

                string text = graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Medium ? string.Format("x {0}", (RecipeViewModel.BeaconCount).ToString("0.##")) : string.Format("x {0} Σ{1}", (RecipeViewModel.BeaconCount).ToString("0.##"), sbeaconCount);
                GraphicsStuff.DrawText(graphics, textBrush, textFormat, text, counterBaseFont, textbox, true);
            }
        }

        public override List<TooltipInfo>? GetToolTips(Point graph_point) {
            if (!Visible)
                return null;
            if (!RecipeViewModel.SelectedBeacon)
                return null;

            List<TooltipInfo> tooltips = new List<TooltipInfo>();

            Point localPoint = Point.Add(GraphToLocal(graph_point), new Size(Width / 2, Height / 2));
            if (RecipeViewModel.BeaconModules.Count > 0 && localPoint.X < (ModuleSpacing * 3) + 2) //over modules
            {
                TooltipInfo tti = new TooltipInfo();
                tti.Direction = Direction.Up;
                tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + moduleOffset.X + (RecipeViewModel.BeaconModules.Count > 2 ? RecipeViewModel.BeaconModules.Count > 4 ? RecipeViewModel.BeaconModules.Count > 6 ? ModuleSpacing * 5 / 2 : ModuleSpacing * 3 / 2 : ModuleSpacing * 4 / 2 : ModuleSpacing * 5 / 2) - (Width / 2), Height / 2)));
                tti.Text = "Beacon Modules:";

                Dictionary<ModuleQualityPair, int> moduleCounter = new Dictionary<ModuleQualityPair, int>();
                foreach (ModuleQualityPair m in RecipeViewModel.BeaconModules) {
                    if (moduleCounter.ContainsKey(m))
                        moduleCounter[m]++;
                    else
                        moduleCounter.Add(m, 1);
                }

                foreach (ModuleQualityPair m in moduleCounter.Keys.OrderBy(m => m.Module.FriendlyName).ThenBy(m => m.Quality.Level).ThenBy(m => m.Quality.FriendlyName))
                    tti.Text += string.Format("\n   {0} :{1}", moduleCounter[m], m.FriendlyName);
                tooltips.Add(tti);
            } else //over assembler
              {
                TooltipInfo tti = new TooltipInfo();
                tti.Direction = Direction.Up;
                tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(moduleOffset.X + (ModuleSpacing * 3) + 2 + (BeaconIconSize / 2) - (Width / 2), Height / 2)));
                tti.Text = RecipeViewModel.SelectedBeacon.FriendlyName ?? "";
                tooltips.Add(tti);
            }

            return tooltips;
        }
    }
}