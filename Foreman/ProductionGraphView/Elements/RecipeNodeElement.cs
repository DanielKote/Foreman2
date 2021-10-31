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
		private static readonly Pen productivityPen = new Pen(Brushes.DarkRed, 6);
		private static readonly Pen productivityPlusPen = new Pen(productivityPen.Brush, 2);

		private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

		private readonly AssemblerElement AssemblerElement;
		private readonly BeaconElement BeaconElement;

		private string RecipeName { get { return DisplayedNode.BaseRecipe.FriendlyName; } }

		private new readonly ReadOnlyRecipeNode DisplayedNode;

		public RecipeNodeElement(ProductionGraphViewer graphViewer, ReadOnlyRecipeNode node) : base(graphViewer, node)
		{
			DisplayedNode = node;

			AssemblerElement = new AssemblerElement(graphViewer, this);
			AssemblerElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			BeaconElement = new BeaconElement(graphViewer, this);
			BeaconElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			UpdateState();
		}

		public override void UpdateState()
		{
			if (InputTabs.Count == 0 && OutputTabs.Count != 0)
			{
				AssemblerElement.Location = new Point(-26, -6);
				BeaconElement.Location = new Point(-30, 36);
			}
			else if (OutputTabs.Count == 0 && InputTabs.Count != 0)
			{
				AssemblerElement.Location = new Point(-26, -26);
				BeaconElement.Location = new Point(-30, 16);
			}
			else
			{
				AssemblerElement.Location = new Point(-26, -16);
				BeaconElement.Location = new Point(-30, 26);
			}

			AssemblerElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);
			BeaconElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			Width = Math.Max(MinWidth, Math.Max(GetIconWidths(InputTabs), GetIconWidths(OutputTabs)) + 10);
			if (Width % WidthD != 0)
			{
				Width += WidthD;
				Width -= Width % WidthD;
			}
			Height = (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low) ? BaseSimpleHeight : BaseRecipeHeight;

			//update tabs (necessary now that it is possible that an item was added or removed)... I am looking at you furnaces!!!
			//done by first checking all old tabs and removing any that are no longer part of the displayed node, then looking at the displayed node io and adding any new tabs that are necessary.
			//could potentially be done by just deleting all the old ones and remaking them from scratch, but come on - thats much more intensive than just doing some checks!
			foreach (ItemTabElement oldTab in InputTabs.Where(tab => !DisplayedNode.Inputs.Contains(tab.Item)).ToList())
			{
				foreach (ReadOnlyNodeLink link in DisplayedNode.InputLinks.Where(link => link.Item == oldTab.Item).ToList())
					graphViewer.Graph.DeleteLink(link);
				InputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (ItemTabElement oldTab in OutputTabs.Where(tab => !DisplayedNode.Outputs.Contains(tab.Item)).ToList())
			{
				foreach (ReadOnlyNodeLink link in DisplayedNode.OutputLinks.Where(link => link.Item == oldTab.Item).ToList())
					graphViewer.Graph.DeleteLink(link);
				OutputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (Item item in DisplayedNode.Inputs)
				if (!InputTabs.Any(tab => tab.Item == item))
					InputTabs.Add(new ItemTabElement(item, LinkType.Input, graphViewer, this));
			foreach (Item item in DisplayedNode.Outputs)
				if (!OutputTabs.Any(tab => tab.Item == item))
					OutputTabs.Add(new ItemTabElement(item, LinkType.Output, graphViewer, this));

			base.UpdateState();
		}

		protected override void DetailsDraw(Graphics graphics, Point trans, bool simple)
		{
			if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low) //text only view
			{
				if (!simple)
				{
					//text
					bool oversupplied = DisplayedNode.IsOversupplied();
					Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 35, trans.Y - (Height / 2) + (oversupplied ? 30 : 25), (Width - 10 - 35), Height - (oversupplied ? 60 : 50));
					//graphics.DrawRectangle(devPen, textSlot);
					int textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, RecipeName, BaseFont, textSlot);

					//assembler icon
					graphics.DrawImage(DisplayedNode.SelectedAssembler == null ? DataCache.UnknownIcon : DisplayedNode.SelectedAssembler.Icon, trans.X - Math.Min((Width / 2) - 5, (textLength / 2) + 32), trans.Y - 16, 32, 32);
				}

				//productivity ticks
				int pModules = DisplayedNode.AssemblerModules.Count(m => m.ProductivityBonus > 0);
				pModules += (int)(DisplayedNode.BeaconModules.Count(m => m.ProductivityBonus > 0) * DisplayedNode.BeaconCount);

				for (int i = 0; i < pModules && i < 6; i++)
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

			if (graphViewer.ShowRecipeToolTip)
			{
				Recipe[] Recipes = new Recipe[] { DisplayedNode.BaseRecipe };
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
	}
}
