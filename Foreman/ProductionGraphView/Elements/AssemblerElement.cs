using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foreman
{
	public class AssemblerElement : GraphElement
	{
		private RecipeNode recipeNode;

		private const int AssemblerIconSize = 48;
		private const int ModuleIconSize = 13;
		private const int ModuleSpacing = 12;

		//in this case it is easier to work with 0,0 coordinates being the top-left most corner.
		private static readonly Point[] moduleLocations = new Point[] { new Point(ModuleSpacing, 0), new Point(ModuleSpacing, ModuleSpacing), new Point(ModuleSpacing, ModuleSpacing * 2), new Point(0, 0), new Point(0, ModuleSpacing), new Point(0, ModuleSpacing * 2) };
		private static readonly Point moduleOffset = new Point(0, 5);

		private static readonly Font infoFont = new Font(FontFamily.GenericSansSerif, 5);
		private static readonly Font counterBaseFont = new Font(FontFamily.GenericSansSerif, 14);
		private static readonly Brush textBrush = Brushes.Black;
		private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };


		public AssemblerElement(ProductionGraphViewer graphViewer, RecipeNodeElement parent) : base(graphViewer, parent)
		{
			Width = AssemblerIconSize + (ModuleSpacing * 2) + 2;
			Height = AssemblerIconSize;
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

			//assembler
			graphics.DrawImage(recipeNode.SelectedAssembler.Icon, trans.X + ModuleSpacing * 2 + 2, trans.Y, AssemblerIconSize, AssemblerIconSize);

			//modules
			for (int i = 0; i < moduleLocations.Length && i < recipeNode.AssemblerModules.Count; i++)
				graphics.DrawImage(recipeNode.AssemblerModules[i].Icon, trans.X + moduleLocations[i].X + moduleOffset.X, trans.Y + moduleLocations[i].Y + moduleOffset.Y, ModuleIconSize, ModuleIconSize);

			//assembler info + quantity
			Rectangle textbox;
			if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.High)
			{
				//info text
				graphics.DrawString("Speed:\nProd:\nPower:", infoFont, textBrush, trans.X + Width, trans.Y);
				graphics.DrawString(string.Format("{0:P0}\n{1:P0}\n{2:P0}", recipeNode.GetSpeedMultiplier(), recipeNode.GetProductivityMultiplier(), recipeNode.GetConsumptionMultiplier()), infoFont, textBrush, trans.X + Width + 30, trans.Y);

				textbox = new Rectangle(trans.X + Width, trans.Y + 26, 60, 30);
			}
			else
			{
				textbox = new Rectangle(trans.X + Width, trans.Y + 10, 60, 30);
			}

			//quantity
			//graphics.DrawRectangle(devPen, textbox);
			float assemblerCount = recipeNode.GetBaseNumberOfAssemblers() * graphViewer.GetRateMultipler();
			string text = "x";
			if (assemblerCount >= 1000)
				text += assemblerCount.ToString("0.##e0");
			else if (assemblerCount >= 0.1)
				text += assemblerCount.ToString("0.#");
			else if (assemblerCount != 0)
				text += "<0.1";
			else
				text += "0";

			GraphicsStuff.DrawText(graphics, textBrush, textFormat, text, counterBaseFont, textbox, true);
		}

		public override List<TooltipInfo> GetToolTips(Point graph_point)
		{
			if (!Visible)
				return null;

			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			Point localPoint = Point.Add(GraphToLocal(graph_point), new Size(Width / 2, Height / 2));
			if (localPoint.X < (ModuleSpacing * 2) + 2 && recipeNode.AssemblerModules.Count > 0) //over modules
			{
				TooltipInfo tti = new TooltipInfo();
				tti.Direction = Direction.Down;
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + (recipeNode.AssemblerModules.Count > 3 ? ModuleSpacing : 3 * ModuleSpacing / 2) - (Width / 2), -Height / 2)));
				tti.Text = "Assembler Modules:";
				foreach (Module m in recipeNode.AssemblerModules)
					tti.Text += "\n   " + m.FriendlyName;
				tooltips.Add(tti);
			}
			else //over assembler
			{
				TooltipInfo tti = new TooltipInfo();
				tti.Direction = Direction.Down;
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point((ModuleSpacing * 2) + 2 + (AssemblerIconSize / 2) - (Width / 2), -Height / 2)));
				tti.Text = recipeNode.SelectedAssembler.FriendlyName;
				tooltips.Add(tti);
			}

			return tooltips;
		}
	}
}
