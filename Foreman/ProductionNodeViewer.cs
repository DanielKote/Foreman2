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
				return new Rectangle(Parent.viewOffset.X + X, Parent.viewOffset.Y + Y, Width, Height);
			}
		}

		public ProductionNode DisplayedNode { get; private set; }

		public ProductionGraphViewer Parent;
		public ProductionNodeViewer(ProductionNode node)
		{
			Width = 100;
			Height = 100;

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
			nameText = String.Format("Recipe: {0}", DisplayedNode.DisplayName);
			if (DisplayedNode.GetType() == typeof(RecipeNode))
			{
				rateText = String.Format("{0}/sec", DisplayedNode.Rate.ToString());
			}
			else if (DisplayedNode.GetType() == typeof(ConsumerNode))
			{
				rateText = String.Format("{0}/sec", DisplayedNode.Rate.ToString());
			}
			else if (DisplayedNode.GetType() == typeof(SupplyNode))
			{
				rateText = String.Format("{0}/sec", DisplayedNode.Rate.ToString());
			}

			SizeF stringSize = Parent.CreateGraphics().MeasureString(nameText, SystemFonts.DefaultFont);
			Width = Math.Max((int)stringSize.Width, getIconWidths());
		}

		private int getIconWidths()
		{
			return Math.Max(
				(iconSize + iconBorder * 5) * DisplayedNode.Outputs.Count() + iconBorder,
				(iconSize + iconBorder * 5) * DisplayedNode.Inputs.Count() + iconBorder);
		}

		public Point getOutputIconPoint(Item item)
		{
			var sortedOutputs = DisplayedNode.Outputs.Keys.OrderBy(i => i.Name).ToList();
			int x = Convert.ToInt32((float)Width / (sortedOutputs.Count()) * (sortedOutputs.IndexOf(item) + 0.5f));
			int y = 0;
			return new Point(x, y + 1);
		}

		public Point getInputIconPoint(Item item)
		{
			var sortedInputs = DisplayedNode.Inputs.Keys.OrderBy(i => i.Name).ToList();
			int x = Convert.ToInt32((float)Width / (sortedInputs.Count()) * (sortedInputs.IndexOf(item) + 0.5f));
			int y = Height;
			return new Point(x, y - 2);
		}

		public Point getOutputLineConnectionPoint(Item item)
		{
			 return Point.Add(getOutputIconPoint(item), new Size(X, Y - (iconSize + iconBorder + iconBorder) / 2));
		}

		public Point getInputLineConnectionPoint(Item item)
		{
			return Point.Add(getInputIconPoint(item), new Size(X, Y + (iconSize + iconBorder + iconBorder) / 2));
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

			foreach (Item item in DisplayedNode.Outputs.Keys)
			{
				DrawItemIcon(item, getOutputIconPoint(item), graphics);
			}
			foreach (Item item in DisplayedNode.Inputs.Keys)
			{
				DrawItemIcon(item, getInputIconPoint(item), graphics);
			}

			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = StringAlignment.Center;
			centreFormat.LineAlignment = StringAlignment.Center;
			graphics.DrawString(nameText, SystemFonts.DefaultFont, new SolidBrush(Color.Black), new PointF(Width / 2, Height/ 2), centreFormat);

		}

		private void DrawItemIcon(Item item, Point drawPoint, Graphics graphics)
		{
			int boxSize = iconSize + iconBorder + iconBorder;

			if (item.Icon != null)
			{
				using (Pen pen = new Pen(Color.Gray, 3))
				using (Brush brush = new SolidBrush(Color.White))
				{
					FillRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2), boxSize, boxSize, iconBorder, graphics, brush);
					DrawRoundRect(drawPoint.X - (boxSize / 2), drawPoint.Y - (boxSize / 2), boxSize, boxSize, iconBorder, graphics, pen);
				}
				graphics.DrawImage(item.Icon, drawPoint.X - iconSize / 2, drawPoint.Y - iconSize / 2, iconSize, iconSize);
			}
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