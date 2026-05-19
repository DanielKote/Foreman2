using Foreman.Controls;
using Foreman.Graph;
using Foreman.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman.ProductionGraphView.Elements {
    internal class BeaconElement : GraphElement {
        private const int BeaconIconSize = 28;
        private const int ModuleIconSize = 12;
        private const int ModuleSpacing = 11;

        //in this case it is easier to work with 0,0 coordinates being the top-left most corner.
        private static readonly Point[] moduleLocations = [new(ModuleSpacing * 2, 0), new(ModuleSpacing * 2, ModuleSpacing), new(ModuleSpacing, 0), new(ModuleSpacing, ModuleSpacing), new(0, 0), new(0, ModuleSpacing)];
        private static readonly Point moduleOffset = new(10, 3);

        private static readonly Pen speedModulePen = new(Brushes.DarkBlue, 2);
        private static readonly Pen prodModulePen = new(Brushes.DarkRed, 2);
        private static readonly Pen effModulePen = new(Brushes.DarkGreen, 2);
        private static readonly Pen qualityModulePen = new(Brushes.Gold, 2);
        private static readonly Pen unknownModulePen = new(Brushes.Black, 2);
        private static readonly Font moduleFont = new(FontFamily.GenericSansSerif, 5, FontStyle.Bold);

        private static readonly Font counterBaseFont = new(FontFamily.GenericSansSerif, 8);
        private static readonly Brush textBrush = Brushes.Black;
        private static readonly StringFormat textFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

        private RecipeNodeViewModel RecipeViewModel => (RecipeNodeViewModel)parent.ViewModel;
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
                        int moduleIndex = (x * 4) + y;
                        if (RecipeViewModel.BeaconModules.Count > moduleIndex) {
                            Pen marker = RecipeViewModel.BeaconModules[moduleIndex].Module.GetProductivityBonus() > 0 ? prodModulePen :
                                RecipeViewModel.BeaconModules[moduleIndex].Module.GetQualityBonus() > 0 ? qualityModulePen :
                                RecipeViewModel.BeaconModules[moduleIndex].Module.GetConsumptionBonus() < 0 ? effModulePen :
                                RecipeViewModel.BeaconModules[moduleIndex].Module.GetSpeedBonus() > 0 ? speedModulePen :
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
                graphics.DrawString(string.Format(DisplayCulture.Format, "S:{0}", speedModules), moduleFont, Brushes.DarkBlue, trans.X, trans.Y + 5);
                graphics.DrawString(string.Format(DisplayCulture.Format, "E:{0}", efficiencyModules), moduleFont, Brushes.DarkGreen, trans.X, trans.Y + 15);
                graphics.DrawString(string.Format(DisplayCulture.Format, "P:{0}", prodModules), moduleFont, Brushes.DarkRed, trans.X + 22, trans.Y + 5);
                graphics.DrawString(string.Format(DisplayCulture.Format, "Q:{0}", qualityModules), moduleFont, Brushes.Gold, trans.X + 22, trans.Y + 15);
                graphics.DrawString(string.Format(DisplayCulture.Format, "U:{0}", unknownModules), moduleFont, Brushes.Black, trans.X, trans.Y + 25);
            }

            //quantity
            if (RecipeViewModel.SelectedBeacon) // && recipeNode.BeaconCount > 0)
            {
                int parentHalfWidth = myParent is RecipeNodeElement recipeParent ? recipeParent.Width : Width;
                var textbox = new Rectangle(trans.X + Width, trans.Y + 5, (parentHalfWidth / 2) - this.X - (this.Width / 2) - 6, 18);
                //graphics.DrawRectangle(devPen, textbox);

                double beaconCount = RecipeViewModel.GetTotalBeacons();
                string sbeaconCount = (beaconCount >= 10000) ? beaconCount.ToString("0.##e0", DisplayCulture.Format) : beaconCount.ToString("0", DisplayCulture.Format);

                string text = graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Medium ? string.Format(DisplayCulture.Format, "x {0}", (RecipeViewModel.BeaconCount).ToString("0.##", DisplayCulture.Format)) : string.Format(DisplayCulture.Format, "x {0} Σ{1}", (RecipeViewModel.BeaconCount).ToString("0.##", DisplayCulture.Format), sbeaconCount);
                GraphicsStuff.DrawText(graphics, textBrush, textFormat, text, counterBaseFont, textbox, true);
            }
        }

        public override List<TooltipInfo>? GetToolTips(Point graphPoint) {
            if (!Visible)
                return null;
            if (!RecipeViewModel.SelectedBeacon)
                return null;

            var tooltips = new List<TooltipInfo>();

            var localPoint = Point.Add(GraphToLocal(graphPoint), new Size(Width / 2, Height / 2));
            if (RecipeViewModel.BeaconModules.Count > 0 && localPoint.X < (ModuleSpacing * 3) + 2) //over modules
            {
                var tti = new TooltipInfo {
                    Direction = Direction.Up,
                    ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + moduleOffset.X + (RecipeViewModel.BeaconModules.Count > 2 ? RecipeViewModel.BeaconModules.Count > 4 ? RecipeViewModel.BeaconModules.Count > 6 ? ModuleSpacing * 5 / 2 : ModuleSpacing * 3 / 2 : ModuleSpacing * 4 / 2 : ModuleSpacing * 5 / 2) - (Width / 2), Height / 2))),
                    Text = "Beacon Modules:"
                };

                var moduleCounter = new Dictionary<ModuleQualityPair, int>();
                foreach (ModuleQualityPair m in RecipeViewModel.BeaconModules) {
                    moduleCounter[m] = moduleCounter.TryGetValue(m, out int count) ? count + 1 : 1;
                }

                foreach (ModuleQualityPair m in moduleCounter.Keys.OrderBy(m => m.Module.FriendlyName).ThenBy(m => m.Quality.Level).ThenBy(m => m.Quality.FriendlyName))
                    tti.Text += string.Format(DisplayCulture.Format, "\n   {0} :{1}", moduleCounter[m], m.FriendlyName);
                tooltips.Add(tti);
            } else //over assembler
              {
                var tti = new TooltipInfo {
                    Direction = Direction.Up,
                    ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(moduleOffset.X + (ModuleSpacing * 3) + 2 + (BeaconIconSize / 2) - (Width / 2), Height / 2))),
                    Text = RecipeViewModel.SelectedBeacon.FriendlyName ?? ""
                };
                tooltips.Add(tti);
            }

            return tooltips;
        }
    }
}
