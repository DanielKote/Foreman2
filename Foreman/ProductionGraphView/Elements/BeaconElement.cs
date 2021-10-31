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

		protected override void Draw(Graphics graphics)
		{
			Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
			//graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);

			if (recipeNode.SelectedBeacon != null)
			{
				//beacon
				graphics.DrawImage(recipeNode.SelectedBeacon.Icon, trans.X + ModuleSpacing * 3 + 2, trans.Y, BeaconIconSize, BeaconIconSize);

				//modules
				for (int i = 0; i < moduleLocations.Length && i < recipeNode.BeaconModules.Count; i++)
					graphics.DrawImage(recipeNode.BeaconModules[i].Icon, trans.X + moduleLocations[i].X + moduleOffset.X, trans.Y + moduleLocations[i].Y + moduleOffset.Y, ModuleIconSize, ModuleIconSize);

				//quantity
				if (recipeNode.SelectedBeacon != null) // && recipeNode.BeaconCount > 0)
				{
					Rectangle textbox = new Rectangle(trans.X + Width, trans.Y + 6, 65, 18);
					//graphics.DrawRectangle(devPen, textbox);

					float beaconCount = recipeNode.BeaconCount;
					string text = "x" + beaconCount.ToString("0.##");
					GraphicsStuff.DrawText(graphics, textBrush, textFormat, text, counterBaseFont, textbox, true);
				}
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
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + (recipeNode.BeaconModules.Count > 2 ? recipeNode.BeaconModules.Count > 4 ? ModuleSpacing * 1 / 2 : ModuleSpacing : ModuleSpacing * 3 / 2) - (Width / 2), Height / 2)));
				tti.Text = "Beacon Modules:";
				foreach (Module m in recipeNode.BeaconModules)
					tti.Text += "\n   " + m.FriendlyName;
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
