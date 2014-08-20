using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	public enum LinkType {Input, Output};

	public partial class ProductionNodeViewer
	{
		public bool IsBeingDragged = false;
		public int DragOffsetX;
		public int DragOffsetY;

		public bool Moused { get { return Parent.MousedNode == this; }}
		public Point MousePosition = Point.Empty;

		private Color recipeColour = Color.FromArgb(190, 217, 212);
		private Color supplyColour = Color.FromArgb(249, 237, 195);
		private Color consumerColour = Color.FromArgb(231, 214, 224);
		private Color backgroundColour;

		public const int iconSize = 32;
		public const int iconBorder = 4;
		public const int iconTextHeight = 10;

		private string rateText = "";
		private string nameText = "";

		public int X;
		public int Y;
		public int Width;
		public int Height;
		public Rectangle bounds
		{
			get
			{
				return new Rectangle(X, Y, Width, Height);
			}
		}
		public Rectangle screenBounds
		{
			get
			{
				return new Rectangle(Parent.ViewOffset.X + X, Parent.ViewOffset.Y + Y, Width, Height);
			}
		}

		public ProductionNode DisplayedNode { get; private set; }

		public ProductionGraphViewer Parent;
		public ProductionNodeViewer(ProductionNode node)
		{
			Width = 100;
			Height = 80;

			DisplayedNode = node;

			if (DisplayedNode.GetType() == typeof(ConsumerNode))
			{
				backgroundColour = supplyColour;
			}
			else if (DisplayedNode.GetType() == typeof(SupplyNode))
			{
				backgroundColour = consumerColour;
			}
			else
			{
				backgroundColour = recipeColour;
			}
		}
		
		public void Update()
		{
			if (DisplayedNode.GetType() == typeof(RecipeNode))
			{
				nameText = String.Format("Recipe: {0}", DisplayedNode.DisplayName);
			}
			else if (DisplayedNode.GetType() == typeof(ConsumerNode))
			{
				nameText = String.Format("Output: {0}", DisplayedNode.DisplayName);
			}
			else if (DisplayedNode.GetType() == typeof(SupplyNode))
			{
				nameText = String.Format("Input: {0}", DisplayedNode.DisplayName);
			}

			SizeF stringSize = Parent.CreateGraphics().MeasureString(nameText, SystemFonts.DefaultFont);
			Width = Math.Max((int)stringSize.Width + iconBorder * 2, getIconWidths());
		}

		private int getIconWidths()
		{
			return Math.Max(
				(iconSize + iconBorder * 5) * DisplayedNode.Outputs.Count() + iconBorder,
				(iconSize + iconBorder * 5) * DisplayedNode.Inputs.Count() + iconBorder);
		}

		public Rectangle GetIconBounds(Item item, LinkType linkType)
		{
			int X = 0;
			int Y = 0;
			int	Width = iconSize + iconBorder + iconBorder;
			int	Height = iconSize + iconBorder + iconBorder + iconTextHeight;

			if (linkType == LinkType.Output)
			{
				Point iconPoint = getOutputIconPoint(item);
				var sortedOutputs = DisplayedNode.Outputs.OrderBy(i => getXSortValue(i, LinkType.Output)).ToList();
				X = iconPoint.X - Width / 2;
				Y = iconPoint.Y -(iconSize + iconBorder + iconBorder + iconTextHeight) / 2;
			}
			else
			{
				Point iconPoint = getInputIconPoint(item);
				var sortedInputs = DisplayedNode.Inputs.OrderBy(i => getXSortValue(i, LinkType.Input)).ToList();
				X = iconPoint.X - Width / 2;
				Y = iconPoint.Y -(iconSize + iconBorder + iconBorder) / 2;
			}

			return new Rectangle(X, Y, Width, Height);
		}

		public Point getOutputIconPoint(Item item)
		{
			var sortedOutputs = DisplayedNode.Outputs.OrderBy(i => getXSortValue(i, LinkType.Output)).ToList();
			int x = Convert.ToInt32((float)Width / (sortedOutputs.Count()) * (sortedOutputs.IndexOf(item) + 0.5f));
			int y = 0;
			return new Point(x, y + iconBorder);
		}

		public Point getInputIconPoint(Item item)
		{
			var sortedInputs = DisplayedNode.Inputs.OrderBy(i => getXSortValue(i, LinkType.Input)).ToList();
			int x = Convert.ToInt32((float)Width / (sortedInputs.Count()) * (sortedInputs.IndexOf(item) + 0.5f));
			int y = Height;
			return new Point(x, y - iconBorder);
		}

		public Point getOutputLineConnectionPoint(Item item)
		{
			return Point.Add(getOutputIconPoint(item), new Size(X, Y - (iconSize + iconBorder + iconBorder) / 2 - iconTextHeight));
		}

		public Point getInputLineConnectionPoint(Item item)
		{
			return Point.Add(getInputIconPoint(item), new Size(X, Y + (iconSize + iconBorder + iconBorder) / 2 + iconTextHeight));
		}

		//Used to sort items in the input/output lists
		public int getXSortValue(Item item, LinkType linkType)
		{
			int total = 0;
			if (linkType == LinkType.Input)
			{
				foreach (NodeLink link in DisplayedNode.InputLinks.Where(l => l.Item == item))
				{
					total += Parent.nodeControls[link.Supplier].X;
				}
			}
			else
			{
				foreach (NodeLink link in DisplayedNode.OutputLinks.Where(l => l.Item == item))
				{
					total += Parent.nodeControls[link.Consumer].X;
				}
			}
			return total;
		}

		public void Paint(Graphics graphics)
		{
			using (SolidBrush brush = new SolidBrush(backgroundColour))
			{
				GraphicsStuff.FillRoundRect(0, 0, Width, Height, 8, graphics, brush);
			}

			if (Parent.ClickedNode == this)
			{
				using (Pen pen = new Pen(Color.WhiteSmoke, 3f))
				{
					GraphicsStuff.DrawRoundRect(0, 0, Width, Height, 8, graphics, pen);
				}
			}
			else if (Parent.MousedNode == this)
			{
				using (Pen pen = new Pen(Color.LightGray, 3f))
				{
					GraphicsStuff.DrawRoundRect(0, 0, Width, Height, 8, graphics, pen);
				}
			}
			else if (Parent.SelectedNode == this)
			{
				using (Pen pen = new Pen(Color.DarkGray, 3f))
				{
					GraphicsStuff.DrawRoundRect(0, 0, Width, Height, 8, graphics, pen);
				}
			}

			String unit = "";
			if (Parent.graph.SelectedAmountType == AmountType.Rate && Parent.graph.SelectedUnit == RateUnit.PerSecond)
			{
				unit = "/s";
			}
			else if (Parent.graph.SelectedAmountType == AmountType.Rate && Parent.graph.SelectedUnit == RateUnit.PerMinute)
			{
				unit = "/m";
			}
			String formatString = "{0:0.##}{1}";
			foreach (Item item in DisplayedNode.Outputs)
			{
				DrawItemIcon(item, getOutputIconPoint(item), LinkType.Output, String.Format(formatString, DisplayedNode.GetTotalOutput(item), unit), graphics);
			}
			foreach (Item item in DisplayedNode.Inputs)
			{
				DrawItemIcon(item, getInputIconPoint(item), LinkType.Input, String.Format(formatString, DisplayedNode.GetTotalInput(item), unit), graphics);
			}

			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
			graphics.DrawString(nameText, new Font(FontFamily.GenericSansSerif, 8), new SolidBrush(Color.Black), new PointF(Width / 2, Height / 2), centreFormat);
		}

		private void DrawItemIcon(Item item, Point drawPoint, LinkType linkType, String rateText, Graphics graphics)
		{
			int boxSize = iconSize + iconBorder + iconBorder;
			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;

			using (Pen borderPen = new Pen(Color.Gray, 3))
			using (Brush fillBrush = new SolidBrush(Color.White))
			using (Brush textBrush = new SolidBrush(Color.Black))
			{
				if (linkType == LinkType.Output)
				{
					GraphicsStuff.FillRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2) - iconTextHeight, boxSize, boxSize + iconTextHeight, iconBorder, graphics, fillBrush);
					GraphicsStuff.DrawRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2) - iconTextHeight, boxSize, boxSize + iconTextHeight, iconBorder, graphics, borderPen);
					graphics.DrawString(rateText, new Font(FontFamily.GenericSansSerif, iconTextHeight - iconBorder + 1), textBrush, new PointF(drawPoint.X, drawPoint.Y - (boxSize + iconTextHeight) / 2 + iconBorder), centreFormat);
				}
				else
				{
					GraphicsStuff.FillRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2), boxSize, boxSize + iconTextHeight, iconBorder, graphics, fillBrush);
					GraphicsStuff.DrawRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2), boxSize, boxSize + iconTextHeight, iconBorder, graphics, borderPen);
					graphics.DrawString(rateText, new Font(FontFamily.GenericSansSerif, 7), textBrush, new PointF(drawPoint.X, drawPoint.Y + (boxSize + iconTextHeight) / 2 - iconBorder), centreFormat);
				}
			}
			graphics.DrawImage(item.Icon ?? DataCache.UnknownIcon, drawPoint.X - iconSize / 2, drawPoint.Y - iconSize / 2, iconSize, iconSize);
		}
	}
}