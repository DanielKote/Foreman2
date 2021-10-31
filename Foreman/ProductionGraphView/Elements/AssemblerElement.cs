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

		private static readonly Pen speedModulePen = new Pen(Brushes.DarkBlue, 3);
		private static readonly Pen prodModulePen = new Pen(Brushes.DarkRed, 3);
		private static readonly Pen effModulePen = new Pen(Brushes.DarkGreen, 3);
		private static readonly Pen unknownModulePen = new Pen(Brushes.Black, 3);

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

		protected override void Draw(Graphics graphics, bool simple)
		{
			if (simple)
				return;

			Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
			//graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);

			//assembler
			graphics.DrawImage(recipeNode.SelectedAssembler.Icon, trans.X + ModuleSpacing * 2 + 2, trans.Y, AssemblerIconSize, AssemblerIconSize);

			//modules
			if (recipeNode.AssemblerModules.Count <= 6)
			{
				for (int i = 0; i < moduleLocations.Length && i < recipeNode.AssemblerModules.Count; i++)
					graphics.DrawImage(recipeNode.AssemblerModules[i].Icon, trans.X + moduleLocations[i].X + moduleOffset.X, trans.Y + moduleLocations[i].Y + moduleOffset.Y, ModuleIconSize, ModuleIconSize);
			}
			else //resot to drawing circles for each module instead -> 5x6 set, so max 30 modules shown
			{
				for (int x = 0; x < 5; x++)
				{
					for (int y = 0; y < 6; y++)
					{
						if (recipeNode.AssemblerModules.Count > (x * 6) + y)
						{
							Pen marker = recipeNode.AssemblerModules[(x * 6) + y].ProductivityBonus > 0 ? prodModulePen :
								recipeNode.AssemblerModules[(x * 6) + y].ConsumptionBonus < 0 ? effModulePen :
								recipeNode.AssemblerModules[(x * 6) + y].SpeedBonus > 0 ? speedModulePen : unknownModulePen;
							graphics.DrawEllipse(marker, trans.X + moduleOffset.X + 20 - (x * 7), trans.Y + moduleOffset.Y + (y * 7), 3, 3);
						}
					}
				}
			}

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
			string text = "x";
			if (recipeNode.SelectedAssembler.IsMissing)
			{
				text += "---";
			}
			else
			{
				double assemblerCount = recipeNode.ActualAssemblerCount / graphViewer.GetRateMultipler();
				if (assemblerCount >= 1000)
					text += assemblerCount.ToString("0.##e0");
				else if (assemblerCount >= 0.1)
					text += assemblerCount.ToString("0.#");
				else if (assemblerCount != 0)
					text += "<0.1";
				else
					text += "0";
			}

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
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + (recipeNode.AssemblerModules.Count > 3 ? recipeNode.AssemblerModules.Count > 6 ? ModuleSpacing * 3 / 2 : ModuleSpacing : ModuleSpacing * 3 / 2) - (Width / 2), -Height / 2)));
				tti.Text = "Assembler Modules:";

				Dictionary<Module, int> moduleCounter = new Dictionary<Module, int>();
				foreach (Module m in recipeNode.AssemblerModules)
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
				tti.Direction = Direction.Down;
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point((ModuleSpacing * 2) + 2 + (AssemblerIconSize / 2) - (Width / 2), -Height / 2)));
				tti.Text = recipeNode.SelectedAssembler.FriendlyName;
				tooltips.Add(tti);
			}

			return tooltips;
		}
	}
}
