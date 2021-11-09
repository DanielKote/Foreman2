using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman
{
	public class ItemTabElement : GraphElement
	{
		public static int TabWidth { get { return iconSize + border * 3; } } //I just use these two to get a decent aproximation as to how far to space new nodes when bulk-added
		public static int TabBorder { get { return border; } }

		public LinkType LinkType;
		public Item Item { get; private set; }
		public IEnumerable<ReadOnlyNodeLink> Links { get { return LinkType == LinkType.Input ? DisplayedNode.InputLinks.Where(l => l.Item == Item) : DisplayedNode.OutputLinks.Where(l => l.Item == Item); } }

		public bool HideItemTab { get; set; }

		private const int iconSize = 32;
		private const int border = 3;
		private int textHeight = 11;

		private static StringFormat bottomFormat = new StringFormat() { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Center };
		private static StringFormat topFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };

		private static Brush directionBrush = new SolidBrush(Color.FromArgb(40, Color.Black));

		private static Pen regularBorderPen = new Pen(Color.DimGray, 3);
		private static Pen oversuppliedBorderPen = new Pen(Color.DarkGoldenrod, 3);
		private static Pen disconnectedBorderPen = new Pen(Color.DarkRed, 3);

		private static Brush textBrush = Brushes.Black;
		private static Brush fillBrush = Brushes.White;

		private static Font textFont = new Font(FontFamily.GenericSansSerif, 6);

		private Pen borderPen;
		private string text = "";

		private readonly ReadOnlyBaseNode DisplayedNode;

		public ItemTabElement(Item item, LinkType type, ProductionGraphViewer graphViewer, BaseNodeElement node) : base(graphViewer, node)
		{
			DisplayedNode = node.DisplayedNode;
			Item = item;
			LinkType = type;
			HideItemTab = false;

			borderPen = regularBorderPen;
			int textHeight = (int)base.graphViewer.CreateGraphics().MeasureString("a", textFont).Height;
			Width = TabWidth;
			Height = iconSize + textHeight + border + 3;
			X = 0; Y = 0;
		}

		public Point GetConnectionPoint() //in graph coordinates
		{
			if ((LinkType == LinkType.Input && DisplayedNode.NodeDirection == NodeDirection.Up) || (LinkType == LinkType.Output && DisplayedNode.NodeDirection == NodeDirection.Down))
				return LocalToGraph(new Point(0, Height / 2));
			else //if ((LinkType == LinkType.Input && DisplayedNode.NodeDirection == NodeDirection.down) || (LinkType == LinkType.Output && DisplayedNode.NodeDirection == NodeDirection.Up))
				return LocalToGraph(new Point(0, -Height / 2));
		}

		public void UpdateValues(double recipeRate, double suppliedRate, bool isOversupplied) //if input then: recipe rate = consume rate; if output then recipe rate = production rate
		{
			borderPen = regularBorderPen;
			text = GraphicsStuff.DoubleToString(recipeRate);

			if (isOversupplied)
			{
				borderPen = oversuppliedBorderPen;
				text += "\n" + GraphicsStuff.DoubleToString(suppliedRate);
			}
			else if (!Links.Any())
				borderPen = disconnectedBorderPen;

			int textHeight = (int)graphViewer.CreateGraphics().MeasureString(text, textFont).Height;
			Height = iconSize + textHeight + border + 3;
		}

		protected override void Draw(Graphics graphics, NodeDrawingStyle style)
		{
			if (style == NodeDrawingStyle.IconsOnly || HideItemTab)
				return;

			Point trans = LocalToGraph(new Point(0, 0));

			//background
			GraphicsStuff.FillRoundRect(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height, border, graphics, fillBrush);

			//direction signs (only if using dynamic link width or not using arrows on links)
			if (graphViewer.DynamicLinkWidth || !graphViewer.ArrowsOnLinks)
			{
				if (DisplayedNode.NodeDirection == NodeDirection.Up)
					graphics.FillPolygon(directionBrush, new Point[] { new Point(trans.X - (Bounds.Width / 2), trans.Y + (Bounds.Height / 2)), new Point(trans.X + (Bounds.Width / 2), trans.Y + (Bounds.Height / 2)), new Point(trans.X, trans.Y - (Bounds.Height / 2)) });
				else
					graphics.FillPolygon(directionBrush, new Point[] { new Point(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2)), new Point(trans.X + (Bounds.Width / 2), trans.Y - (Bounds.Height / 2)), new Point(trans.X, trans.Y + (Bounds.Height / 2)) });
			}

			//border
			GraphicsStuff.DrawRoundRect(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height, border, graphics, borderPen);

			//text & icon
			if (style == NodeDrawingStyle.Regular || style == NodeDrawingStyle.PrintStyle)
			{
				if (LinkType == LinkType.Output)
				{
					graphics.DrawString(text, textFont, textBrush, new PointF(trans.X, trans.Y + ((textHeight + border - Bounds.Height - 10) / 2)), topFormat);
					graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - (Bounds.Width / 2) + (int)(border * 1.5), trans.Y + (Bounds.Height / 2) - border - iconSize, iconSize, iconSize);
				}
				else
				{
					graphics.DrawString(text, textFont, textBrush, new PointF(trans.X, trans.Y - ((textHeight + border - Bounds.Height - 10) / 2)), bottomFormat);
					graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - (Bounds.Width / 2) + (int)(border * 1.5), trans.Y - (Bounds.Height / 2) + border, iconSize, iconSize);
				}
			}
		}

		public override List<TooltipInfo> GetToolTips(Point graph_point)
		{
			List<TooltipInfo> toolTips = new List<TooltipInfo>();
			TooltipInfo tti = new TooltipInfo();
			BaseNodeElement parentNode = (BaseNodeElement)myParent;

			if (parentNode.DisplayedNode is ReadOnlyRecipeNode rNode)
			{
				if (LinkType == LinkType.Input)
					tti.Text = rNode.BaseRecipe.GetIngredientFriendlyName(Item);
				else //if(LinkType == LinkType.Output)
					tti.Text = rNode.BaseRecipe.GetProductFriendlyName(Item);
			}
			else if ((Item is Fluid fluid) && fluid.IsTemperatureDependent)
			{
				fRange tempRange = LinkChecker.GetTemperatureRange(fluid, parentNode.DisplayedNode, (LinkType == LinkType.Input) ? LinkType.Output : LinkType.Input, true); //input type tab means output of connection link and vice versa
				if (tempRange.Ignore && DisplayedNode is ReadOnlyPassthroughNode)
					tempRange = LinkChecker.GetTemperatureRange(fluid, parentNode.DisplayedNode, LinkType, true); //if there was no temp range on this side of this throughput node, try to just copy the other side
				tti.Text = fluid.GetTemperatureRangeFriendlyName(tempRange);
			}
			else
				tti.Text = Item.FriendlyName;

			tti.Direction = ((LinkType == LinkType.Input && DisplayedNode.NodeDirection == NodeDirection.Up) || (LinkType == LinkType.Output && DisplayedNode.NodeDirection == NodeDirection.Down)) ? Direction.Up : Direction.Down;
			tti.ScreenLocation = graphViewer.GraphToScreen(GetConnectionPoint());
			toolTips.Add(tti);

			TooltipInfo helpToolTipInfo = new TooltipInfo();
			helpToolTipInfo.Text = "Drag to create a new connection.\nRight click for options.";
			helpToolTipInfo.Direction = Direction.None;
			helpToolTipInfo.ScreenLocation = new Point(10, 10);
			toolTips.Add(helpToolTipInfo);

			return toolTips;
		}

		public override void MouseUp(Point graph_point, MouseButtons button, bool wasDragged)
		{
			if (button == MouseButtons.Right)
			{
				List<ReadOnlyNodeLink> connections = new List<ReadOnlyNodeLink>();
				if (LinkType == LinkType.Input)
					connections.AddRange(DisplayedNode.InputLinks.Where(l => l.Item == Item));
				else //if (LinkType == LinkType.Output)
					connections.AddRange(DisplayedNode.OutputLinks.Where(l => l.Item == Item));

				RightClickMenu.Items.Add(new ToolStripMenuItem("Delete connections", null,
					new EventHandler((o, e) =>
					{
						RightClickMenu.Close();
						foreach (ReadOnlyNodeLink link in connections)
							graphViewer.Graph.DeleteLink(link);
						graphViewer.Graph.UpdateNodeValues();
					}))
				{ Enabled = connections.Count > 0 });

				RightClickMenu.Show(graphViewer, graphViewer.GraphToScreen(graph_point));
			}
		}
	}
}
