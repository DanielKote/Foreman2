using Foreman.Controls;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman.ProductionGraphView.Elements {
    public class AssemblerElement : GraphElement {
        private const int AssemblerIconSize = 54;
        private const int ModuleIconSize = 13;
        private const int ModuleSpacing = 12;

        //in this case it is easier to work with 0,0 coordinates being the top-left most corner.
        private static readonly Point[] moduleLocations = [new(ModuleSpacing, 0), new(ModuleSpacing, ModuleSpacing), new(ModuleSpacing, ModuleSpacing * 2), new(0, 0), new(0, ModuleSpacing), new(0, ModuleSpacing * 2)];
        private static readonly Point moduleOffset = new(0, 5);

        private static readonly Pen speedModulePen = new(Brushes.DarkBlue, 3);
        private static readonly Pen prodModulePen = new(Brushes.DarkRed, 3);
        private static readonly Pen effModulePen = new(Brushes.DarkGreen, 3);
        private static readonly Pen qualityModulePen = new(Brushes.Gold, 3);
        private static readonly Pen unknownModulePen = new(Brushes.Black, 3);
        private static readonly Font moduleFont = new(FontFamily.GenericSansSerif, 6, FontStyle.Bold);

        private static readonly Font infoFont = new(FontFamily.GenericSansSerif, 5);
        private static readonly Font counterBaseFont = new(FontFamily.GenericSansSerif, 14);
        private static readonly Brush textBrush = Brushes.Black;
        private static readonly StringFormat textFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

        private RecipeNodeViewModel RecipeViewModel => (RecipeNodeViewModel)parent.ViewModel;
        private readonly RecipeNodeElement parent;

        public AssemblerElement(ProductionGraphViewer graphViewer, RecipeNodeElement parent) : base(graphViewer, parent) {
            this.parent = parent;

            Width = AssemblerIconSize + (ModuleSpacing * 2) + 2;
            Height = AssemblerIconSize;
        }

        public void SetVisibility(bool visible) {
            Visible = visible;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style == NodeDrawingStyle.IconsOnly || style == NodeDrawingStyle.Simple)
                return;

            Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
            //graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);

            //assembler
            if (RecipeViewModel.SelectedAssembler.Icon is not null)
                graphics.DrawImage(RecipeViewModel.SelectedAssembler.Icon, trans.X + ModuleSpacing * 2 + 2, trans.Y, AssemblerIconSize, AssemblerIconSize);

            //modules
            if (RecipeViewModel.AssemblerModules.Count <= 6) {
                for (int i = 0; i < moduleLocations.Length && i < RecipeViewModel.AssemblerModules.Count; i++)
                    if (RecipeViewModel.AssemblerModules[i].Icon is Bitmap icon)
                        graphics.DrawImage(icon, trans.X + moduleLocations[i].X + moduleOffset.X, trans.Y + moduleLocations[i].Y + moduleOffset.Y, ModuleIconSize, ModuleIconSize);
            } else if (RecipeViewModel.AssemblerModules.Count <= 4 * 7) //resot to drawing circles for each module instead -> 4x7 set, so max 28 modules shown
              {
                for (int x = 0; x < 4; x++) {
                    for (int y = 0; y < 7; y++) {
                        if (RecipeViewModel.AssemblerModules.Count > (x * 7) + y) {
                            Pen marker = RecipeViewModel.AssemblerModules[(x * 7) + y].Module.GetProductivityBonus() > 0 ? prodModulePen :
                                RecipeViewModel.AssemblerModules[(x * 7) + y].Module.GetQualityBonus() > 0 ? qualityModulePen :
                                RecipeViewModel.AssemblerModules[(x * 7) + y].Module.GetConsumptionBonus() < 0 ? effModulePen :
                                RecipeViewModel.AssemblerModules[(x * 7) + y].Module.GetSpeedBonus() > 0 ? speedModulePen :
                                unknownModulePen;
                            graphics.DrawEllipse(marker, trans.X + moduleOffset.X + ModuleSpacing + ModuleIconSize - 3 - (x * 7), trans.Y + moduleOffset.Y + (y * 7), 3, 3);
                        }
                    }
                }
            } else {
                int prodModules = RecipeViewModel.AssemblerModules.Count(m => m.Module.GetProductivityBonus() > 0);
                int qualityModules = RecipeViewModel.AssemblerModules.Count(m => m.Module.GetQualityBonus() > 0 && m.Module.GetProductivityBonus() <= 0);
                int efficiencyModules = RecipeViewModel.AssemblerModules.Count(m => m.Module.GetConsumptionBonus() < 0 && m.Module.GetProductivityBonus() <= 0 && m.Module.GetQualityBonus() <= 0);
                int speedModules = RecipeViewModel.AssemblerModules.Count(m => m.Module.GetSpeedBonus() > 0 && m.Module.GetConsumptionBonus() >= 0 && m.Module.GetProductivityBonus() <= 0 && m.Module.GetQualityBonus() <= 0);
                int unknownModules = RecipeViewModel.AssemblerModules.Count - prodModules - efficiencyModules - speedModules - qualityModules;
                graphics.DrawString(string.Format(DisplayCulture.Format, "S:{0}", speedModules), moduleFont, Brushes.DarkBlue, trans.X, trans.Y + 10);
                graphics.DrawString(string.Format(DisplayCulture.Format, "E:{0}", efficiencyModules), moduleFont, Brushes.DarkGreen, trans.X, trans.Y + 20);
                graphics.DrawString(string.Format(DisplayCulture.Format, "P:{0}", prodModules), moduleFont, Brushes.DarkRed, trans.X, trans.Y + 30);
                graphics.DrawString(string.Format(DisplayCulture.Format, "Q:{0}", qualityModules), moduleFont, Brushes.Gold, trans.X, trans.Y + 40);
                graphics.DrawString(string.Format(DisplayCulture.Format, "U:{0}", unknownModules), moduleFont, Brushes.Black, trans.X, trans.Y + 50);
            }

            //assembler info + quantity
            int parentHalfWidth = myParent is RecipeNodeElement recipeParent ? recipeParent.Width : Width;
            var textbox = new Rectangle(trans.X + Width, trans.Y + 10, (parentHalfWidth / 2) - this.X - (this.Width / 2) - 6, 30);
            if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.High && (RecipeViewModel.SelectedAssembler.Assembler.EntityType == EntityType.Assembler || RecipeViewModel.SelectedAssembler.Assembler.EntityType == EntityType.Miner || RecipeViewModel.SelectedAssembler.Assembler.EntityType == EntityType.OffshorePump)) {
                //info text
                if (RecipeViewModel.GetQualityMultiplier() > 0) {
                    graphics.DrawString("Speed:\nProd:\nPower:\nQuality:", infoFont, textBrush, trans.X + Width + 2, trans.Y);
                    graphics.DrawString(string.Format(DisplayCulture.Format, "{0:+0%; -0%; 0%}\n{1:+0%; -0%; 0%}\n{2:+0%; -0%; 0%}\n{3:+0%; -0%; 0%}", (RecipeViewModel.GetSpeedMultiplier() - 1), (RecipeViewModel.GetProductivityMultiplier() - 1), (RecipeViewModel.GetConsumptionMultiplier() - 1), RecipeViewModel.GetQualityMultiplier()), infoFont, textBrush, trans.X + Width + 26, trans.Y);
                } else {
                    graphics.DrawString("Speed:\nProd:\nPower:", infoFont, textBrush, trans.X + Width + 2, trans.Y);
                    graphics.DrawString(string.Format(DisplayCulture.Format, "{0:+0%; -0%; 0%}\n{1:+0%; -0%; 0%}\n{2:+0%; -0%; 0%}", (RecipeViewModel.GetSpeedMultiplier() - 1), (RecipeViewModel.GetProductivityMultiplier() - 1), (RecipeViewModel.GetConsumptionMultiplier() - 1)), infoFont, textBrush, trans.X + Width + 26, trans.Y);
                }

                textbox.Y = trans.Y + 28;
            } else if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.High && RecipeViewModel.SelectedAssembler.Assembler.EntityType == EntityType.Generator) {
                //info text
                graphics.DrawString("Power:", infoFont, textBrush, trans.X + Width, trans.Y + 10);
                double generatorEffectivity = RecipeViewModel.GetGeneratorEffectivity();
                graphics.DrawString(string.Format(DisplayCulture.Format, "{0:P0}", generatorEffectivity), infoFont, textBrush, trans.X + Width + 26, trans.Y + 10);

                textbox.Y = trans.Y + 24;
            }

            //quantity
            //graphics.DrawRectangle(devPen, textbox);
            string text = "x";
            if (RecipeViewModel.SelectedAssembler.Assembler.IsMissing)
                text += "---";
            else
                text += BuildingQuantityToText(RecipeViewModel.ActualSetValue);

            GraphicsStuff.DrawText(graphics, textBrush, textFormat, text, counterBaseFont, textbox, true);
        }

        public override List<TooltipInfo>? GetToolTips(Point graphPoint) {
            if (!Visible)
                return null;

            List<TooltipInfo> tooltips = [];

            var localPoint = Point.Add(GraphToLocal(graphPoint), new Size(Width / 2, Height / 2));
            if (localPoint.X < (ModuleSpacing * 2) + 2 && RecipeViewModel.AssemblerModules.Count > 0) //over modules
            {
                var tti = new TooltipInfo {
                    Direction = Direction.Down,
                    ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + (RecipeViewModel.AssemblerModules.Count > 3 ? RecipeViewModel.AssemblerModules.Count > 6 ? ModuleSpacing * 3 / 2 : ModuleSpacing : ModuleSpacing * 3 / 2) - (Width / 2), -Height / 2))),
                    Text = "Assembler Modules:"
                };

                var moduleCounter = new Dictionary<ModuleQualityPair, int>();
                foreach (ModuleQualityPair m in RecipeViewModel.AssemblerModules) {
                    moduleCounter[m] = moduleCounter.TryGetValue(m, out int count) ? count + 1 : 1;
                }

                foreach (ModuleQualityPair m in moduleCounter.Keys.OrderBy(m => m.Module.FriendlyName).ThenBy(m => m.Quality.Level).ThenBy(m => m.Quality.FriendlyName))
                    tti.Text += string.Format(DisplayCulture.Format, "\n   {0} :{1}", moduleCounter[m], m.FriendlyName);
                tooltips.Add(tti);
            } else //over assembler
              {
                var tti = new TooltipInfo {
                    Direction = Direction.Down,
                    ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point((ModuleSpacing * 2) + 2 + (AssemblerIconSize / 2) - (Width / 2), -Height / 2))),
                    Text = RecipeViewModel.SelectedAssembler.FriendlyName ?? ""
                };
                tooltips.Add(tti);
            }

            return tooltips;
        }
    }
}
