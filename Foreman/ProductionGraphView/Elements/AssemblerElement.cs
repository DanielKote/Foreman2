using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public class AssemblerElement : GraphElement {
        private const int AssemblerIconSize = 54;
        private const int ModuleIconSize = 13;
        private const int ModuleSpacing = 12;

        // in this case it is easier to work with 0,0 coordinates being the top-left most corner.
        private static readonly Point[] ModuleLocations = [
            new(ModuleSpacing, 0), new(ModuleSpacing, ModuleSpacing), new(ModuleSpacing, ModuleSpacing * 2), new(0, 0),
            new(0, ModuleSpacing), new(0, ModuleSpacing * 2)
        ];

        private static readonly Point ModuleOffset = new(0, 5);

        private static readonly Pen SpeedModulePen = new(Brushes.DarkBlue, 3);
        private static readonly Pen ProdModulePen = new(Brushes.DarkRed, 3);
        private static readonly Pen EffModulePen = new(Brushes.DarkGreen, 3);
        private static readonly Pen QualityModulePen = new(Brushes.Gold, 3);
        private static readonly Pen UnknownModulePen = new(Brushes.Black, 3);
        private static readonly Font ModuleFont = new(FontFamily.GenericSansSerif, 6, FontStyle.Bold);

        private static readonly Font InfoFont = new(FontFamily.GenericSansSerif, 5);
        private static readonly Font CounterBaseFont = new(FontFamily.GenericSansSerif, 14);
        private static readonly Brush TextBrush = Brushes.Black;
        private static readonly StringFormat TextFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

        private readonly ReadOnlyRecipeNode _displayedNode;

        public AssemblerElement(ProductionGraphViewer graphViewer, RecipeNodeElement parent) : base(graphViewer, parent) {
            _displayedNode = (ReadOnlyRecipeNode) parent.DisplayedNode;

            Width = AssemblerIconSize + ModuleSpacing * 2 + 2;
            Height = AssemblerIconSize;
        }

        public void SetVisibility(bool visible) {
            Visible = visible;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style is NodeDrawingStyle.IconsOnly or NodeDrawingStyle.Simple)
                return;

            var trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
            //graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);

            // assembler

            graphics.DrawImage(_displayedNode.SelectedAssembler.Icon, trans.X + ModuleSpacing * 2 + 2, trans.Y, AssemblerIconSize, AssemblerIconSize);

            // modules

            if (_displayedNode.AssemblerModules.Count <= 6) {
                for (var i = 0; i < ModuleLocations.Length && i < _displayedNode.AssemblerModules.Count; i++)
                    graphics.DrawImage(_displayedNode.AssemblerModules[i].Icon, trans.X + ModuleLocations[i].X + ModuleOffset.X,
                        trans.Y + ModuleLocations[i].Y + ModuleOffset.Y, ModuleIconSize, ModuleIconSize);
            } else if (_displayedNode.AssemblerModules.Count <= 4 * 7) { // reset to drawing circles for each module instead -> 4x7 set, so max 28 modules shown
                for (var x = 0; x < 4; x++) {
                    for (var y = 0; y < 7; y++) {
                        if (_displayedNode.AssemblerModules.Count > x * 7 + y) {
                            var marker = _displayedNode.AssemblerModules[x * 7 + y].Module.GetProductivityBonus() > 0 ? ProdModulePen :
                                _displayedNode.AssemblerModules[x * 7 + y].Module.GetQualityBonus() > 0 ? QualityModulePen :
                                _displayedNode.AssemblerModules[x * 7 + y].Module.GetConsumptionBonus() < 0 ? EffModulePen :
                                _displayedNode.AssemblerModules[x * 7 + y].Module.GetSpeedBonus() > 0 ? SpeedModulePen :
                                UnknownModulePen;
                            graphics.DrawEllipse(marker, trans.X + ModuleOffset.X + ModuleSpacing + ModuleIconSize - 3 - x * 7,
                                trans.Y + ModuleOffset.Y + y * 7, 3, 3);
                        }
                    }
                }
            } else {
                var prodModules = _displayedNode.AssemblerModules.Count(m => m.Module.GetProductivityBonus() > 0);
                var qualityModules = _displayedNode.AssemblerModules.Count(m => m.Module.GetQualityBonus() > 0 && m.Module.GetProductivityBonus() <= 0);
                var efficiencyModules = _displayedNode.AssemblerModules.Count(m =>
                    m.Module.GetConsumptionBonus() < 0 && m.Module.GetProductivityBonus() <= 0 && m.Module.GetQualityBonus() <= 0);
                var speedModules = _displayedNode.AssemblerModules.Count(m =>
                    m.Module.GetSpeedBonus() > 0 && m.Module.GetConsumptionBonus() >= 0 && m.Module.GetProductivityBonus() <= 0 &&
                    m.Module.GetQualityBonus() <= 0);
                var unknownModules = _displayedNode.AssemblerModules.Count - prodModules - efficiencyModules - speedModules - qualityModules;
                graphics.DrawString($"S:{speedModules}", ModuleFont, Brushes.DarkBlue, trans.X, trans.Y + 10);
                graphics.DrawString($"E:{efficiencyModules}", ModuleFont, Brushes.DarkGreen, trans.X, trans.Y + 20);
                graphics.DrawString($"P:{prodModules}", ModuleFont, Brushes.DarkRed, trans.X, trans.Y + 30);
                graphics.DrawString($"Q:{qualityModules}", ModuleFont, Brushes.Gold, trans.X, trans.Y + 40);
                graphics.DrawString($"U:{unknownModules}", ModuleFont, Brushes.Black, trans.X, trans.Y + 50);
            }

            // assembler info + quantity

            var textbox = new Rectangle(trans.X + Width, trans.Y + 10, MyParent.Width / 2 - X - Width / 2 - 6, 30);
            if (GraphViewer.LevelOfDetail == ProductionGraphViewer.Lod.High &&
                _displayedNode.SelectedAssembler.Assembler.EntityType is EntityType.Assembler or EntityType.Miner or EntityType.OffshorePump) {
                //info text

                if (_displayedNode.GetQualityMultiplier() > 0) {
                    graphics.DrawString("Speed:\nProd:\nPower:\nQuality:", InfoFont, TextBrush, trans.X + Width + 2, trans.Y);
                    graphics.DrawString(
                        $"{_displayedNode.GetSpeedMultiplier() - 1:+0%; -0%; 0%}\n{_displayedNode.GetProductivityMultiplier() - 1:+0%; -0%; 0%}\n{_displayedNode.GetConsumptionMultiplier() - 1:+0%; -0%; 0%}\n{_displayedNode.GetQualityMultiplier():+0%; -0%; 0%}",
                        InfoFont, TextBrush, trans.X + Width + 26, trans.Y);
                } else {
                    graphics.DrawString("Speed:\nProd:\nPower:", InfoFont, TextBrush, trans.X + Width + 2, trans.Y);
                    graphics.DrawString(
                        $"{_displayedNode.GetSpeedMultiplier() - 1:+0%; -0%; 0%}\n{_displayedNode.GetProductivityMultiplier() - 1:+0%; -0%; 0%}\n{_displayedNode.GetConsumptionMultiplier() - 1:+0%; -0%; 0%}",
                        InfoFont, TextBrush,
                        trans.X + Width + 26, trans.Y);
                }

                textbox.Y = trans.Y + 28;
            } else if (GraphViewer.LevelOfDetail == ProductionGraphViewer.Lod.High &&
                _displayedNode.SelectedAssembler.Assembler.EntityType == EntityType.Generator) {
                // info text

                graphics.DrawString("Power:", InfoFont, TextBrush, trans.X + Width, trans.Y + 10);
                graphics.DrawString($"{_displayedNode.GetGeneratorEffectivity():P0}", InfoFont, TextBrush, trans.X + Width + 26, trans.Y + 10);

                textbox.Y = trans.Y + 24;
            }

            // quantity

            //graphics.DrawRectangle(devPen, textbox);
            var text = "x";
            if (_displayedNode.SelectedAssembler.Assembler.IsMissing)
                text += "---";
            else
                text += BuildingQuantityToText(_displayedNode.ActualSetValue);

            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, text, CounterBaseFont, textbox, true);
        }

        public override List<TooltipInfo> GetToolTips(Point graphPoint) {
            if (!Visible)
                return null;

            var tooltips = new List<TooltipInfo>();

            var localPoint = Point.Add(GraphToLocal(graphPoint), new Size(Width / 2, Height / 2));
            if (localPoint.X < ModuleSpacing * 2 + 2 && _displayedNode.AssemblerModules.Count > 0) { // over modules
                var tti = new TooltipInfo {
                    Direction = Direction.Down,
                    ScreenLocation = GraphViewer.GraphToScreen(LocalToGraph(new Point(
                        1 + (_displayedNode.AssemblerModules.Count > 3
                            ? _displayedNode.AssemblerModules.Count > 6 ? ModuleSpacing * 3 / 2 : ModuleSpacing
                            : ModuleSpacing * 3 / 2) - Width / 2, -Height / 2))),
                    Text = "Assembler Modules:"
                };

                var moduleCounter = new Dictionary<ModuleQualityPair, int>();
                foreach (var m in _displayedNode.AssemblerModules) {
                    if (moduleCounter.ContainsKey(m))
                        moduleCounter[m]++;
                    else
                        moduleCounter.Add(m, 1);
                }

                foreach (var m in moduleCounter.Keys.OrderBy(m => m.Module.FriendlyName)
                    .ThenBy(m => m.Quality.Level)
                    .ThenBy(m => m.Quality.FriendlyName)) {
                    tti.Text += $"\n   {moduleCounter[m]} :{m.FriendlyName}";
                }

                tooltips.Add(tti);
            } else { // over assembler
                var tti = new TooltipInfo {
                    Direction = Direction.Down,
                    ScreenLocation = GraphViewer.GraphToScreen(LocalToGraph(new Point(ModuleSpacing * 2 + 2 + AssemblerIconSize / 2 - Width / 2, -Height / 2))),
                    Text = _displayedNode.SelectedAssembler.FriendlyName
                };
                tooltips.Add(tti);
            }

            return tooltips;
        }
    }
}