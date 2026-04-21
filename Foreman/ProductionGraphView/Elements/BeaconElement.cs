using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman {
    class BeaconElement : GraphElement {
        private const int BeaconIconSize = 28;
        private const int ModuleIconSize = 12;
        private const int ModuleSpacing = 11;

        // in this case it is easier to work with 0,0 coordinates being the top-left most corner.
        private static readonly Point[] ModuleLocations = [
            new(ModuleSpacing * 2, 0), new(ModuleSpacing * 2, ModuleSpacing), new(ModuleSpacing, 0), new(ModuleSpacing, ModuleSpacing),
            new(0, 0), new(0, ModuleSpacing)
        ];

        private static readonly Point ModuleOffset = new(10, 3);

        private static readonly Pen SpeedModulePen = new(Brushes.DarkBlue, 2);
        private static readonly Pen ProdModulePen = new(Brushes.DarkRed, 2);
        private static readonly Pen EffModulePen = new(Brushes.DarkGreen, 2);
        private static readonly Pen QualityModulePen = new(Brushes.Gold, 2);
        private static readonly Pen UnknownModulePen = new(Brushes.Black, 2);
        private static readonly Font ModuleFont = new(FontFamily.GenericSansSerif, 5, FontStyle.Bold);

        private static readonly Font CounterBaseFont = new(FontFamily.GenericSansSerif, 8);
        private static readonly Brush TextBrush = Brushes.Black;
        private static readonly StringFormat TextFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

        private readonly ReadOnlyRecipeNode _displayedNode;

        public BeaconElement(ProductionGraphViewer graphViewer, RecipeNodeElement parent) : base(graphViewer, parent) {
            _displayedNode = (ReadOnlyRecipeNode) parent.DisplayedNode;

            Width = BeaconIconSize + ModuleSpacing * 3 + 12;
            Height = BeaconIconSize;
        }

        public void SetVisibility(bool visible) {
            Visible = visible;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (!_displayedNode.SelectedBeacon || style == NodeDrawingStyle.IconsOnly || style == NodeDrawingStyle.Simple)
                return;

            var trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
            //graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);

            // beacon

            graphics.DrawImage(_displayedNode.SelectedBeacon.Icon, trans.X + ModuleOffset.X + ModuleSpacing * 3 + 2, trans.Y, BeaconIconSize, BeaconIconSize);

            //modules

            if (_displayedNode.BeaconModules.Count <= 6) {
                for (var i = 0; i < ModuleLocations.Length && i < _displayedNode.BeaconModules.Count; i++) {
                    graphics.DrawImage(
                        _displayedNode.BeaconModules[i].Icon,
                        trans.X + ModuleLocations[i].X + ModuleOffset.X,
                        trans.Y + ModuleLocations[i].Y + ModuleOffset.Y,
                        ModuleIconSize,
                        ModuleIconSize
                    );
                }
            } else if (_displayedNode.BeaconModules.Count <= 8 * 4) { // reset to drawing circles for each module instead -> 8x4 set, so 32 max modules
                for (var x = 0; x < 8; x++) {
                    for (var y = 0; y < 4; y++) {
                        if (_displayedNode.BeaconModules.Count > x * 4 + y) {
                            var module = _displayedNode.BeaconModules[x * 4 + y].Module;
                            Pen marker;

                            if (module.GetProductivityBonus() > 0) {
                                marker = ProdModulePen;
                            } else if (module.GetQualityBonus() > 0) {
                                marker = QualityModulePen;
                            } else if (module.GetConsumptionBonus() < 0) {
                                marker = EffModulePen;
                            } else if (module.GetSpeedBonus() > 0) {
                                marker = SpeedModulePen;
                            } else {
                                marker = UnknownModulePen;
                            }

                            graphics.DrawEllipse(
                                marker,
                                trans.X + ModuleOffset.X + ModuleSpacing * 2 + ModuleIconSize - 5 - x * 5,
                                trans.Y + ModuleOffset.Y + 2 + y * 5,
                                2,
                                2
                            );
                        }
                    }
                }
            } else {
                var prodModules = _displayedNode.BeaconModules.Count(m => m.Module.GetProductivityBonus() > 0);
                var qualityModules = _displayedNode.BeaconModules.Count(m => m.Module.GetQualityBonus() > 0 && m.Module.GetProductivityBonus() <= 0);
                var efficiencyModules = _displayedNode.BeaconModules.Count(m =>
                    m.Module.GetConsumptionBonus() < 0 && m.Module.GetProductivityBonus() <= 0 && m.Module.GetQualityBonus() <= 0);
                var speedModules = _displayedNode.BeaconModules.Count(m =>
                    m.Module.GetSpeedBonus() > 0 && m.Module.GetConsumptionBonus() >= 0 && m.Module.GetProductivityBonus() <= 0 &&
                    m.Module.GetQualityBonus() <= 0);
                var unknownModules = _displayedNode.BeaconModules.Count - prodModules - efficiencyModules - speedModules - qualityModules;
                graphics.DrawString($"S:{speedModules}", ModuleFont, Brushes.DarkBlue, trans.X, trans.Y + 5);
                graphics.DrawString($"E:{efficiencyModules}", ModuleFont, Brushes.DarkGreen, trans.X, trans.Y + 15);
                graphics.DrawString($"P:{prodModules}", ModuleFont, Brushes.DarkRed, trans.X + 22, trans.Y + 5);
                graphics.DrawString($"Q:{qualityModules}", ModuleFont, Brushes.Gold, trans.X + 22, trans.Y + 15);
                graphics.DrawString($"U:{unknownModules}", ModuleFont, Brushes.Black, trans.X, trans.Y + 25);
            }

            // quantity

            if (_displayedNode.SelectedBeacon) /* && recipeNode.BeaconCount > 0) */ {
                var textbox = new Rectangle(trans.X + Width, trans.Y + 5, MyParent.Width / 2 - X - Width / 2 - 6, 18);
                //graphics.DrawRectangle(devPen, textbox);

                double beaconCount = _displayedNode.GetTotalBeacons();
                var sBeaconCount = beaconCount >= 10000 ? beaconCount.ToString("0.##e0") : beaconCount.ToString("0");

                var text = GraphViewer.LevelOfDetail == ProductionGraphViewer.Lod.Medium
                    ? $"x {_displayedNode.BeaconCount:0.##}"
                    : $"x {_displayedNode.BeaconCount:0.##} Σ{sBeaconCount}";
                GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, text, CounterBaseFont, textbox, true);
            }
        }

        public override List<TooltipInfo> GetToolTips(Point graphPoint) {
            if (!Visible)
                return null;
            if (!_displayedNode.SelectedBeacon)
                return null;

            var tooltips = new List<TooltipInfo>();

            var localPoint = Point.Add(GraphToLocal(graphPoint), new Size(Width / 2, Height / 2));
            if (_displayedNode.BeaconModules.Count > 0 && localPoint.X < ModuleSpacing * 3 + 2) { // over modules
                var tti = new TooltipInfo {
                    Direction = Direction.Up,
                    ScreenLocation = GraphViewer.GraphToScreen(LocalToGraph(new Point(
                        1 + ModuleOffset.X + (_displayedNode.BeaconModules.Count > 2
                            ? _displayedNode.BeaconModules.Count > 4
                                ? _displayedNode.BeaconModules.Count > 6
                                    ? ModuleSpacing * 5 / 2
                                    : ModuleSpacing * 3 / 2
                                : ModuleSpacing * 4 / 2
                            : ModuleSpacing * 5 / 2) - Width / 2, Height / 2))),
                    Text = "Beacon Modules:"
                };

                var moduleCounter = new Dictionary<ModuleQualityPair, int>();
                foreach (var m in _displayedNode.BeaconModules) {
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
                    Direction = Direction.Up,
                    ScreenLocation = GraphViewer.GraphToScreen(
                        LocalToGraph(new Point(ModuleOffset.X + ModuleSpacing * 3 + 2 + BeaconIconSize / 2 - Width / 2, Height / 2))),
                    Text = _displayedNode.SelectedBeacon.FriendlyName
                };
                tooltips.Add(tti);
            }

            return tooltips;
        }
    }
}