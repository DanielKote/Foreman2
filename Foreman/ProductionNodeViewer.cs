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
				(iconSize + iconBorder * 5) * DisplayedNode.OutputLinks.Count() + iconBorder,
				(iconSize + iconBorder * 5) * DisplayedNode.InputLinks.Count() + iconBorder);
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

		public void MouseMoveHandler(object sender, MouseEventArgs e)
		{
			if (IsBeingDragged)
			{
				Parent.Invalidate();
				Parent.Update();
			}
		}

		public void Paint(Graphics graphics)
		{
			using (SolidBrush brush = new SolidBrush(backgroundColour))
			{
				FillRoundRect(0, 0, Width, Height, 8, graphics, brush);
			}
			if (Parent.SelectedNode == this)
			{
				using (Pen pen = new Pen(Color.DarkGray, 3f))
				{
					DrawRoundRect(0, 0, Width, Height, 8, graphics, pen);
				}
			}

			foreach (Item item in DisplayedNode.OutputLinks.Select<NodeLink, Item>(l => l.Item))
			{
				DrawItemIcon(item, getOutputIconPoint(item), true, DisplayedNode.GetTotalOutput(item).ToString("0.##"), graphics);
			}
			foreach (Item item in DisplayedNode.InputLinks.Select<NodeLink, Item>(l => l.Item))
			{
				DrawItemIcon(item, getInputIconPoint(item), false, DisplayedNode.GetTotalInput(item).ToString("0.##"), graphics);
			}

			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = StringAlignment.Center;
			centreFormat.LineAlignment = StringAlignment.Center;
			graphics.DrawString(nameText, new Font(FontFamily.GenericSansSerif, 8), new SolidBrush(Color.Black), new PointF(Width / 2, Height/ 2), centreFormat);
		}

		private void DrawItemIcon(Item item, Point drawPoint, bool output, String rateText, Graphics graphics)
		{
			int boxSize = iconSize + iconBorder + iconBorder;
			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = StringAlignment.Center;
			centreFormat.LineAlignment = StringAlignment.Center;

			using (Pen borderPen = new Pen(Color.Gray, 3))
			using (Brush fillBrush = new SolidBrush(Color.White))
			using (Brush textBrush = new SolidBrush(Color.Black))
			{
				if (output)
				{
					FillRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2) - iconTextHeight, boxSize, boxSize + iconTextHeight, iconBorder, graphics, fillBrush);
					DrawRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2) - iconTextHeight, boxSize, boxSize + iconTextHeight, iconBorder, graphics, borderPen);
					graphics.DrawString(rateText, new Font(FontFamily.GenericSansSerif, iconTextHeight - iconBorder + 1), textBrush, new PointF(drawPoint.X, drawPoint.Y - (boxSize + iconTextHeight) / 2 + iconBorder), centreFormat);
				}
				else
				{
					FillRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2), boxSize, boxSize + iconTextHeight, iconBorder, graphics, fillBrush);
					DrawRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2), boxSize, boxSize + iconTextHeight, iconBorder, graphics, borderPen);
					graphics.DrawString(rateText, new Font(FontFamily.GenericSansSerif, 7), textBrush, new PointF(drawPoint.X, drawPoint.Y + (boxSize + iconTextHeight) / 2 - iconBorder), centreFormat);
				}
			}
			graphics.DrawImage(item.Icon ?? DataCache.UnknownIcon, drawPoint.X - iconSize / 2, drawPoint.Y - iconSize / 2, iconSize, iconSize);
		}

		public static void DrawRoundRect(int x, int y, int width, int height, int radius, Graphics graphics, Pen pen)
		{
			int radius2 = radius * 2;
			int Left = x;
			int Top = y;
			int Bottom = y + height;
			int Right = x + width;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.StartFigure();

				path.AddArc(Left, Top, 2 * radius, 2 * radius, 180, 90f);
				path.AddArc(Right - radius2, Top, radius2, radius2, 270f, 90f);
				path.AddArc(Right - radius2, Bottom - radius2, radius2, radius2, 0f, 90f);
				path.AddArc(Left, Bottom - radius2, radius2, radius2, 90f, 90f);

				path.CloseFigure();

				graphics.DrawPath(pen, path);
			}
		}

		private static void FillRoundRect(int x, int y, int width, int height, int radius, Graphics graphics, Brush brush)
		{
			int radius2 = radius * 2;
			int Left = x;
			int Top = y;
			int Bottom = y + height;
			int Right = x + width;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.StartFigure();

				path.AddArc(Left, Top, 2 * radius, 2 * radius, 180, 90f);
				path.AddArc(Right - radius2, Top, radius2, radius2, 270f, 90f);
				path.AddArc(Right - radius2, Bottom - radius2, radius2, radius2, 0f, 90f);
				path.AddArc(Left, Bottom - radius2, radius2, radius2, 90f, 90f);

				path.CloseFigure();

				graphics.FillPath(brush, path);
			}
		}
	}
}