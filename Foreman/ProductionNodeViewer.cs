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
	public partial class ProductionNodeViewer : UserControl
	{
		private bool isBeingDragged = false;
		private int dragOffsetX;
		private int dragOffsetY;
		private Panel mousePanel = new Panel();

		private Color recipeColour = Color.FromArgb(190, 217, 212);
		private Color supplyColour = Color.FromArgb(249, 237, 195);
		private Color consumerColour = Color.FromArgb(231, 214, 224);
		private Color backgroundColour;

		public const int iconSize = 32;
		public const int iconBorder = 4;

		public ProductionNode DisplayedNode { get; private set; }

		public ProductionGraphViewer parentTreeViewer;
		public ProductionNodeViewer(ProductionNode node)
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);

			DisplayedNode = node;
			DoControlSettings(this);
			OutputIconSizePanel.Height = InputIconSizePanel.Height = iconSize + iconBorder * 4;
			this.BackColor = Color.Transparent;

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

		private void DoControlSettings(Control control)
		{
			if (control != this)
			{
				control.BackColor = Color.Transparent;
			}

			control.MouseUp += new MouseEventHandler(MouseUpHandler);
			control.MouseDown += new MouseEventHandler(MouseDownHandler);
			control.MouseMove += new MouseEventHandler(MouseMoveHandler);

			foreach (Control child in control.Controls)
			{
				DoControlSettings(child);
			}
		}

		public void UpdateText()
		{
			NameBox.Text = DisplayedNode.DisplayName;
			if (DisplayedNode.GetType() == typeof(RecipeNode))
			{
				RateBox.Text = String.Format("{0}/sec", DisplayedNode.Rate.ToString());
			}
			else if (DisplayedNode.GetType() == typeof(ConsumerNode))
			{
				RateBox.Text = String.Format("{0}/sec", DisplayedNode.Rate.ToString());
			}
			else if (DisplayedNode.GetType() == typeof(SupplyNode))
			{
				RateBox.Text = String.Format("{0}/sec", DisplayedNode.Rate.ToString());
			}

			OutputIconSizePanel.Width = DisplayedNode.Outputs.Count() * (iconSize + iconBorder * 6) + iconBorder;
			InputIconSizePanel.Width = DisplayedNode.Inputs.Count() * (iconSize + iconBorder * 6) + iconBorder;
		}

		public Point getOutputIconPoint(Item item)
		{
			var sortedOutputs = DisplayedNode.Outputs.Keys.OrderBy(i => i.Name).ToList();
			int x = (Width / (sortedOutputs.Count() + 1)) * (sortedOutputs.IndexOf(item) + 1);
			int y = (iconSize + iconBorder + iconBorder) / 2;
			return new Point(x, y + 1);
		}

		public Point getInputIconPoint(Item item)
		{
			var sortedOutputs = DisplayedNode.Inputs.Keys.OrderBy(i => i.Name).ToList();
			int x = (Width / (sortedOutputs.Count() + 1)) * (sortedOutputs.IndexOf(item) + 1);
			int y = Height - (iconSize + iconBorder + iconBorder) / 2;
			return new Point(x, y - 2);
		}

		public Point getOutputLineConnectionPoint(Item item)
		{
			 return Point.Add(getOutputIconPoint(item), new Size(Location.X, Location.Y - (iconSize + iconBorder + iconBorder) / 2));
		}

		public Point getInputLineConnectionPoint(Item item)
		{
			return Point.Add(getInputIconPoint(item), new Size(Location.X, Location.Y + (iconSize + iconBorder + iconBorder) / 2));
		}

		public void MouseMoveHandler(object sender, MouseEventArgs e)
		{
			if (isBeingDragged)
			{
				Location = new Point(Location.X + e.X - dragOffsetX, Location.Y + e.Y - dragOffsetY);
				Invalidate();
				Parent.Invalidate();
				Parent.Update();
			}
		}

		public void MouseDownHandler(object sender, MouseEventArgs e)
		{
			isBeingDragged = true;
			dragOffsetX = e.X;
			dragOffsetY = e.Y;
		}

		public void MouseUpHandler(object sender, MouseEventArgs e)
		{
			isBeingDragged = false;
		}

		protected void OnPaint(object sender, PaintEventArgs e)
		{
			base.OnPaint(e);
			
				using (SolidBrush brush = new SolidBrush(backgroundColour))
				{
					FillRoundRect(0, ((iconSize + iconBorder) / 2), Width, Height - (iconSize + iconBorder), 8, e.Graphics, brush);
				}

			foreach (Item item in DisplayedNode.Outputs.Keys)
			{
				DrawItemIcon(item, getOutputIconPoint(item), e.Graphics);
			}
			foreach (Item item in DisplayedNode.Inputs.Keys)
			{
				DrawItemIcon(item, getInputIconPoint(item), e.Graphics);
			}
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

		private static void DrawRoundRect(int x, int y, int width, int height, int radius, Graphics graphics, Pen pen)
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