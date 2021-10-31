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
		private const int AssemblerIconSize = 54;
		private const int ModuleIconSize = 13;
		private const int ModuleSpacing = 12;

		//in this case it is easier to work with 0,0 coordinates being the top-left most corner.
		private static readonly Point[] moduleLocations = new Point[] { new Point(ModuleSpacing, 0), new Point(ModuleSpacing, ModuleSpacing), new Point(ModuleSpacing, ModuleSpacing * 2), new Point(0, 0), new Point(0, ModuleSpacing), new Point(0, ModuleSpacing * 2) };
		private static readonly Point moduleOffset = new Point(0, 5);

		private static readonly Pen speedModulePen = new Pen(Brushes.DarkBlue, 3);
		private static readonly Pen prodModulePen = new Pen(Brushes.DarkRed, 3);
		private static readonly Pen effModulePen = new Pen(Brushes.DarkGreen, 3);
		private static readonly Pen unknownModulePen = new Pen(Brushes.Black, 3);
		private static readonly Font moduleFont = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Bold);

		private static readonly Font infoFont = new Font(FontFamily.GenericSansSerif, 5);
		private static readonly Font counterBaseFont = new Font(FontFamily.GenericSansSerif, 14);
		private static readonly Brush textBrush = Brushes.Black;
		private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

		private readonly ReadOnlyRecipeNode DisplayedNode;

		public AssemblerElement(ProductionGraphViewer graphViewer, RecipeNodeElement parent) : base(graphViewer, parent)
		{
			DisplayedNode = (ReadOnlyRecipeNode)parent.DisplayedNode;

			Width = AssemblerIconSize + (ModuleSpacing * 2) + 2;
			Height = AssemblerIconSize;
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
			graphics.DrawImage(DisplayedNode.SelectedAssembler.Icon, trans.X + ModuleSpacing * 2 + 2, trans.Y, AssemblerIconSize, AssemblerIconSize);

			//modules
			if (DisplayedNode.AssemblerModules.Count <= 6)
			{
				for (int i = 0; i < moduleLocations.Length && i < DisplayedNode.AssemblerModules.Count; i++)
					graphics.DrawImage(DisplayedNode.AssemblerModules[i].Icon, trans.X + moduleLocations[i].X + moduleOffset.X, trans.Y + moduleLocations[i].Y + moduleOffset.Y, ModuleIconSize, ModuleIconSize);
			}
			else if (DisplayedNode.AssemblerModules.Count <= 4 * 7) //resot to drawing circles for each module instead -> 4x7 set, so max 28 modules shown
			{
				for (int x = 0; x < 4; x++)
				{
					for (int y = 0; y < 7; y++)
					{
						if (DisplayedNode.AssemblerModules.Count > (x * 7) + y)
						{
							Pen marker = DisplayedNode.AssemblerModules[(x * 7) + y].ProductivityBonus > 0 ? prodModulePen :
								DisplayedNode.AssemblerModules[(x * 7) + y].ConsumptionBonus < 0 ? effModulePen :
								DisplayedNode.AssemblerModules[(x * 7) + y].SpeedBonus > 0 ? speedModulePen : unknownModulePen;
							graphics.DrawEllipse(marker, trans.X + moduleOffset.X + ModuleSpacing + ModuleIconSize - 3 - (x * 7), trans.Y + moduleOffset.Y + (y * 7), 3, 3);
						}
					}
				}
			}
			else
			{
				int prodModules = DisplayedNode.AssemblerModules.Count(m => m.ProductivityBonus > 0);
				int efficiencyModules = DisplayedNode.AssemblerModules.Count(m => m.ConsumptionBonus < 0 && m.ProductivityBonus <= 0);
				int speedModules = DisplayedNode.AssemblerModules.Count(m => m.SpeedBonus > 0 && m.ConsumptionBonus >= 0 && m.ProductivityBonus <= 0);
				int unknownModules = DisplayedNode.AssemblerModules.Count - prodModules - efficiencyModules - speedModules;
				graphics.DrawString(string.Format("S:{0}", speedModules), moduleFont, Brushes.DarkBlue, trans.X, trans.Y + 10);
				graphics.DrawString(string.Format("E:{0}", efficiencyModules), moduleFont, Brushes.DarkGreen, trans.X, trans.Y + 20);
				graphics.DrawString(string.Format("P:{0}", prodModules), moduleFont, Brushes.DarkRed, trans.X, trans.Y + 30);
				graphics.DrawString(string.Format("U:{0}", unknownModules), moduleFont, Brushes.Black, trans.X, trans.Y + 40);
			}

			//assembler info + quantity
			Rectangle textbox = new Rectangle(trans.X + Width, trans.Y + 10, (myParent.Width / 2) - this.X - (this.Width / 2) - 6, 30);
			if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.High && (DisplayedNode.SelectedAssembler.EntityType == EntityType.Assembler || DisplayedNode.SelectedAssembler.EntityType == EntityType.Miner))
			{
				//info text
				graphics.DrawString("Speed:\nProd:\nPower:", infoFont, textBrush, trans.X + Width + 2, trans.Y);
				graphics.DrawString(string.Format("{0:P0}\n{1:P0}\n{2:P0}", DisplayedNode.GetSpeedMultiplier(), DisplayedNode.GetProductivityMultiplier(), DisplayedNode.GetConsumptionMultiplier()), infoFont, textBrush, trans.X + Width + 26, trans.Y);

				textbox.Y = trans.Y + 24;
			}
			else if(graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.High && DisplayedNode.SelectedAssembler.EntityType == EntityType.Generator)
			{
				//info text
				graphics.DrawString("Power:", infoFont, textBrush, trans.X + Width, trans.Y + 10);
				graphics.DrawString(string.Format("{0:P0}", DisplayedNode.GetGeneratorEffectivity()), infoFont, textBrush, trans.X + Width + 26, trans.Y + 10);

				textbox.Y = trans.Y + 24;
			}

			//quantity
			//graphics.DrawRectangle(devPen, textbox);
			string text = "x";
			if (DisplayedNode.SelectedAssembler.IsMissing)
			{
				text += "---";
			}
			else
			{
				double assemblerCount = DisplayedNode.ActualAssemblerCount;
				if (assemblerCount >= 10000)
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
			if (localPoint.X < (ModuleSpacing * 2) + 2 && DisplayedNode.AssemblerModules.Count > 0) //over modules
			{
				TooltipInfo tti = new TooltipInfo();
				tti.Direction = Direction.Down;
				tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(1 + (DisplayedNode.AssemblerModules.Count > 3 ? DisplayedNode.AssemblerModules.Count > 6 ? ModuleSpacing * 3 / 2 : ModuleSpacing : ModuleSpacing * 3 / 2) - (Width / 2), -Height / 2)));
				tti.Text = "Assembler Modules:";

				Dictionary<Module, int> moduleCounter = new Dictionary<Module, int>();
				foreach (Module m in DisplayedNode.AssemblerModules)
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
				tti.Text = DisplayedNode.SelectedAssembler.FriendlyName;
				tooltips.Add(tti);
			}

			return tooltips;
		}
	}
}
