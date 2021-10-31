using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class RecipeNodeElement : BaseNodeElement
	{
		protected override Brush CleanBgBrush { get { return recipeBgBrush; } }
		private static readonly Brush recipeBgBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
		private static readonly Pen productivityPen = new Pen(new SolidBrush(Color.FromArgb(166, 0, 0)), 6);
		private static readonly Pen productivityPlusPen = new Pen(productivityPen.Brush, 2);

		private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

		private AssemblerElement assembler;
		private BeaconElement beacon;

		public RecipeNodeElement(ProductionGraphViewer graphViewer, BaseNode node) : base(graphViewer, node)
		{
			Width = MinWidth;
			base.Height = (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low)? BaseSimpleHeight : BaseRecipeHeight;

			assembler = new AssemblerElement(graphViewer, this);
			assembler.Location = new Point(-30, -12);
			assembler.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			beacon = new BeaconElement(graphViewer, this);
			beacon.Location = new Point(-18, 22);
			beacon.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);
		}

		public override void Update()
		{
			assembler.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);
			beacon.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			int width = Math.Max(MinWidth, Math.Max(GetIconWidths(InputTabs), GetIconWidths(OutputTabs)) + 10);
			if (width % WidthD != 0)
			{
				width += WidthD;
				width -= width % WidthD;
			}
			Width = width - 2;
			Height = (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low )? BaseSimpleHeight : BaseRecipeHeight;

			//update tabs (necessary now that it is possible that an item was added or removed)... I am looking at you furnaces!!!
			//done by first checking all old tabs and removing any that are no longer part of the displayed node, then looking at the displayed node io and adding any new tabs that are necessary.
			//could potentially be done by just deleting all the old ones and remaking them from scratch, but come on - thats much more intensive than just doing some checks!
			foreach (ItemTabElement oldTab in InputTabs.Where(tab => !DisplayedNode.Inputs.Contains(tab.Item)).ToList())
			{
				foreach (NodeLink link in DisplayedNode.InputLinks.Where(link => link.Item == oldTab.Item).ToList())
					link.Delete();
				InputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (ItemTabElement oldTab in OutputTabs.Where(tab => !DisplayedNode.Outputs.Contains(tab.Item)).ToList())
			{
				foreach (NodeLink link in DisplayedNode.OutputLinks.Where(link => link.Item == oldTab.Item).ToList())
					link.Delete();
				OutputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (Item item in DisplayedNode.Inputs)
				if (InputTabs.FirstOrDefault(tab => tab.Item == item) == null)
					InputTabs.Add(new ItemTabElement(item, LinkType.Input, graphViewer, this));
			foreach (Item item in DisplayedNode.Outputs)
				if (OutputTabs.FirstOrDefault(tab => tab.Item == item) == null)
					OutputTabs.Add(new ItemTabElement(item, LinkType.Output, graphViewer, this));

			base.Update();
		}

		protected override void DetailsDraw(Graphics graphics, Point trans)
		{
			if(graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low) //text only view
			{
				//text
				string text = DisplayedNode.DisplayName;
				bool oversupplied = DisplayedNode.IsOversupplied();
				Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 35, trans.Y - (Height / 2) + (oversupplied ? 30 : 25), (Width - 10 - 35), Height - (oversupplied ? 60 : 50));
				//graphics.DrawRectangle(devPen, textSlot);
				int textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, text, BaseFont, textSlot);

				//assembler icon
				graphics.DrawImage(((RecipeNode)DisplayedNode).SelectedAssembler == null ? DataCache.UnknownIcon : ((RecipeNode)DisplayedNode).SelectedAssembler.Icon, trans.X - Math.Min((Width / 2) - 5, (textLength / 2) + 32), trans.Y - 16, 32, 32);

				//productivity ticks
				int pModules = ((RecipeNode)DisplayedNode).AssemblerModules.Count(m => m.ProductivityBonus > 0);
				pModules += (int)(((RecipeNode)DisplayedNode).BeaconModules.Count(m => m.ProductivityBonus > 0) * ((RecipeNode)DisplayedNode).BeaconCount);

				for(int i=0; i<pModules && i < 6; i++)
					graphics.DrawEllipse(productivityPen, trans.X - (Width / 2) - 3, trans.Y - (Height / 2) + 10 + i * 12, 6, 6);
				if (pModules > 6)
				{
					graphics.DrawLine(productivityPlusPen, trans.X - (Width / 2) - 6, trans.Y - (Height / 2) + 84, trans.X - (Width / 2) + 6, trans.Y - (Height / 2) + 84);
					graphics.DrawLine(productivityPlusPen, trans.X - (Width / 2), trans.Y - (Height / 2) + 84 - 6, trans.X - (Width / 2), trans.Y - (Height / 2) + 84 + 6);
				}
			}
		}

		protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive)
		{
			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			if (graphViewer.RecipeTooltipEnabled)
			{
				Recipe[] Recipes = new Recipe[] { ((RecipeNode)DisplayedNode).BaseRecipe };
				TooltipInfo ttiRecipe = new TooltipInfo();
				ttiRecipe.Direction = Direction.Left;
				ttiRecipe.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(Width / 2, 0)));
				ttiRecipe.ScreenSize = RecipePainter.GetSize(Recipes);
				ttiRecipe.CustomDraw = new Action<Graphics, Point>((Graphics g, Point offset) => { RecipePainter.Paint(Recipes, g, offset); });
				tooltips.Add(ttiRecipe);
			}

			if (exclusive)
			{
				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = "Left click on this node to edit how fast it runs.\nRight click for options.";
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				tooltips.Add(helpToolTipInfo);
			}

			return tooltips;
		}

		protected override void MouseUpAction(Point graph_point, MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				DevNodeOptionsPanel newPanel = new DevNodeOptionsPanel(DisplayedNode, graphViewer);
				new FloatingTooltipControl(newPanel, Direction.Right, new Point(Location.X - (Width / 2), Location.Y), graphViewer);
			}
			else
			{
				base.MouseUpAction(graph_point, button); //the standard menu (for now)
			}
		}
	}
}
