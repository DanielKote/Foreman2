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

		public ProductionNode DisplayedNode { get; private set; }

		public ProductionGraphViewer parentTreeViewer;
		public ProductionNodeViewer(ProductionNode node)
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);

			DisplayedNode = node;

			ProcessControl(this);

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

		private void ProcessControl(Control control)
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
				ProcessControl(child);
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

			const int radius = 7;
			const int radius2 = radius * 2;
			const int Left = 0;;
			const int Top = 0;
			int Bottom = Height;
			int Right = Width;
			GraphicsPath path = new GraphicsPath();

			path.StartFigure();

			path.AddArc(Left, Top, 2 * radius, 2 * radius, 180, 90f);
			path.AddArc(Right - radius2, Top, radius2, radius2, 270f, 90f);
			path.AddArc(Right - radius2, Bottom - radius2, radius2, radius2, 0f, 90f);
			path.AddArc(Left, Bottom - radius2, radius2, radius2, 90f, 90f);

			path.CloseFigure();

			//e.Graphics.Clear(this.BackColor);
			using (SolidBrush brush = new SolidBrush(backgroundColour))
			{
				e.Graphics.FillPath(brush, path);
			}
		}
	}
}