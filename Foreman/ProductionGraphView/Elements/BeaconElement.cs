using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foreman
{
	class BeaconElement : GraphElement
	{
		private RecipeNode recipeNode;

		private const int BeaconIconSize = 24;
		private const int ModuleIconSize = 9;
		private const int ModuleSpacing = 8;

		//in this case it is easier to work with 0,0 coordinates being the top-left most corner.
		private static readonly Point[] moduleLocations = new Point[] { new Point(ModuleSpacing * 2, 0), new Point(ModuleSpacing * 2, ModuleSpacing), new Point(ModuleSpacing, 0), new Point(ModuleSpacing, ModuleSpacing), new Point(0, 0), new Point(0, ModuleSpacing) };
		private static readonly Point moduleOffset = new Point(0, 5);

		private static readonly Pen speedModulePen = new Pen(Brushes.DarkBlue, 2);
		private static readonly Pen prodModulePen = new Pen(Brushes.DarkRed, 2);
		private static readonly Pen effModulePen = new Pen(Brushes.DarkGreen, 2);
		private static readonly Pen unknownModulePen = new Pen(Brushes.Black, 2);

		private static readonly Font counterBaseFont = new Font(FontFamily.GenericSansSerif, 8);
		private static readonly Brush textBrush = Brushes.Black;
		private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

		public BeaconElement(ProductionGraphViewer graphViewer, RecipeNodeElement parent) : base(graphViewer, parent)
		{
			Width = BeaconIconSize + (ModuleSpacing * 3) + 2;
			Height = BeaconIconSize;
			recipeNode = (RecipeNode)parent.DisplayedNode;
		}

		public void SetVisibility(bool visible)
		{
			Visible = visible;
		}

		protected override void Draw(Graphics graphics, bool simple)
		{
			if (recipeNode.SelectedBeacon == null || simple)
				return;

			Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
			//graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);

			//beacon
			graphics.DrawImage(recipeNode.SelectedBeacon.Icon, trans.X + ModuleSpacing * 3 + 2, trans.Y, BeaconIconSize, BeaconIconSize);

			//modules
			if (recipeNode.BeaconModules.Count <= 6)
			{

				for (int i = 0; i < moduleLocations.Length && i < recipeNode.BeaconModules.Count; i++)
					graphics.DrawImage(recipeNode.BeaconModules[i].Icon, trans.X + moduleLocations[i].X + moduleOffset.X, trans.Y + moduleLocations[i].Y + moduleOffset.Y, ModuleIconSize, ModuleIconSize);
			}
			else //resot to drawing circles for each module instead -> 8x4 set, so 32 max modules
			{
				for (int x = 0; x < 8; x++)
				{
					for (int y = 0; y < 4; y++)
					{
						if (recipeNode.BeaconModules.Count > (x * 4) + y)
						{
							Pen marker = recipeNode.BeaconModules[(x * 4) + y].ProductivityBonus > 0 ? prodModulePen :
								recipeNode.BeaconModules[(x * 4) + y].ConsumptionBonus < 0 ? effModulePen :
								recipeNode.BeaconModules[(x * 4) + y].SpeedBonus > 0 ? speedModulePen : unknownModulePen;
							graphics.DrawEllipse(marker, trans.X + moduleOffset.X + 20 - (x * 5), trans.Y + moduleOffset.Y + (y * 5), 2, 2);
						}
					}
				}
			}

			//quantity
			if (recipeNode.SelectedBeacon != null) // && recipeNode.BeaconCount > 0)
			{
				Rectangle textbox = new Rectangle(trans.X + Width, trans.Y + 6, 60, 18);
				//graphics.DrawRectangle(devPen, textbox);

				double beaconCount = recipeNode.GetTotalBeacons(graphViewer.GetRateMultipler());
				string sbeaconCount = (beaconCount >= 100000) ? beaconCount.ToString("0.##e0") : beaconCount.ToString("0");

				string text = graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Medium ? string.Format("x {0}", (recipeNode.BeaconCount).ToString("0.##")) : string.Format("x {0} (Σ{1})", (recipeNode.BeaconCount).ToString("0.##"), sbeaconCount);
				GraphicsStuff.DrawText(graphics, textBrush, textFormat, text, counterBaseFont, textbox, true);
			}
		}

		public override List<TooltipInfo> GetToolTips(Point graph_point)
		{
			if (!Visible)
				return null;
			if (recipeNode.SelectedBeacon == null)
				return null;

			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			Point localPoint = Point.Add(GraphToLocal(graph_point), new Size(Width / 2, Height / 2));
			if (recipeNode.BeaconModules.Count > 0 && localPoint.X < (ModuleSpacing * 3) + 2) //over modules
			{
				TooltipInfo tti = new TooltipInfo();
				tti.Direction = Direction.Up;
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + (recipeNode.BeaconModules.Count > 2 ? recipeNode.BeaconModules.Count > 4 ? recipeNode.BeaconModules.Count > 6 ? ModuleSpacing * 4 / 2 : ModuleSpacing * 1 / 2 : ModuleSpacing : ModuleSpacing * 3 / 2) - (Width / 2), Height / 2)));
				tti.Text = "Beacon Modules:";

				Dictionary<Module, int> moduleCounter = new Dictionary<Module, int>();
				foreach (Module m in recipeNode.BeaconModules)
				{
					if (moduleCounter.ContainsKey(m))
						moduleCounter[m]++;
					else
						moduleCounter.Add(m, 1);
				}

				foreach (Module m in moduleCounter.Keys.OrderBy(m => m.FriendlyName))
					tti.Text += string.Format("\n   {0} :{1}", moduleCounter[m], m.FriendlyName);
				tooltips.Add(tti);
			}
			else //over assembler
			{
				TooltipInfo tti = new TooltipInfo();
				tti.Direction = Direction.Up;
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point((ModuleSpacing * 3) + 2 + (BeaconIconSize / 2) - (Width / 2), Height / 2)));
				tti.Text = recipeNode.SelectedBeacon.FriendlyName;
				tooltips.Add(tti);
			}

			return tooltips;
		}
	}
}
