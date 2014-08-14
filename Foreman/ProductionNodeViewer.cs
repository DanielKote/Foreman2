using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

		public ProductionNode DisplayedNode { get; private set; }

		public ProductionGraphViewer parentTreeViewer;
		public ProductionNodeViewer(ProductionNode node)
		{
			InitializeComponent();

			DisplayedNode = node;
			if (DisplayedNode.GetType() == typeof(RecipeNode))
			{
				BackColor = recipeColour;
			}
			else if (DisplayedNode.GetType() == typeof(ConsumerNode))
			{
				BackColor = supplyColour;
			}
			else if (DisplayedNode.GetType() == typeof(SupplyNode))
			{
				BackColor = consumerColour;
			}

			RegisterEvents(this);
		}

		private void RegisterEvents(Control control)
		{
			control.MouseUp += new MouseEventHandler(MouseUpHandler);
			control.MouseDown += new MouseEventHandler(MouseDownHandler);
			control.MouseMove += new MouseEventHandler(MouseMoveHandler);

			foreach (Control child in control.Controls)
			{
				RegisterEvents(child);
			}
		}
		
		private void ProductionNodeViewer_Paint(object sender, PaintEventArgs e)
		{
			NameBox.Text = DisplayedNode.DisplayName;
			if (DisplayedNode.GetType() == typeof(RecipeNode))
			{
				RateBox.Text = String.Format("Repeats {0} times/sec", DisplayedNode.Rate.ToString());
			}
			else if (DisplayedNode.GetType() == typeof(ConsumerNode))
			{
				RateBox.Text = String.Format("Consumes {0}/sec", DisplayedNode.Rate.ToString());
			}
			else if (DisplayedNode.GetType() == typeof(SupplyNode))
			{
				RateBox.Text = String.Format("Supplies {0}/sec", DisplayedNode.Rate.ToString());
			}
		}

		public void MouseMoveHandler(object sender, MouseEventArgs e)
		{
			//BackColor = Color.Black;
			if (isBeingDragged)
			{
				Location = new Point(Location.X + e.X - dragOffsetX, Location.Y + e.Y - dragOffsetY);
			}
		}

		public void MouseDownHandler(object sender, MouseEventArgs e)
		{
			isBeingDragged = true;
			//mousePanel.BackColor = Color.Pink;
			dragOffsetX = e.X;
			dragOffsetY = e.Y;
		}

		public void MouseUpHandler(object sender, MouseEventArgs e)
		{
			isBeingDragged = false;
			//mousePanel.BackColor = Color.Blue;
		} 
	}
}