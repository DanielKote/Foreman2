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

	public partial class NodeElement : GraphElement
	{
		public int DragOffsetX;
		public int DragOffsetY;

		public bool Moused { get { return Parent.MousedNode == this; }}
		public Point MousePosition = Point.Empty;

		private Color recipeColour = Color.FromArgb(190, 217, 212);
		private Color supplyColour = Color.FromArgb(249, 237, 195);
		private Color consumerColour = Color.FromArgb(231, 214, 224);
		private Color backgroundColour;

		private const int iconSize = 32;
		private const int iconBorder = 4;
		private const int iconTextHeight = 11;
		private const int iconTextHeight2Line = 21;

		private string rateText = "";
		private string nameText = "";

		TextBox editorBox;
		Item editedItem;
		LinkType editedLinkType;
		float originalEditorValue;
		
		public ProductionNode DisplayedNode { get; private set; }

		public NodeElement(ProductionNode node, ProductionGraphViewer parent) : base(parent)
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
			int textHeight = iconTextHeight;
			if (linkType == LinkType.Input && DisplayedNode.GetUnsatisfiedDemand(item) > 0)
			{
				textHeight = iconTextHeight2Line;
			}

			int rectX = 0;
			int rectY = 0;
			int	rectWidth = iconSize + iconBorder + iconBorder;
			int	rectHeight = iconSize + iconBorder + iconBorder + textHeight;

			if (linkType == LinkType.Output)
			{
				Point iconPoint = GetOutputIconPoint(item);
				var sortedOutputs = DisplayedNode.Outputs.OrderBy(i => getXSortValue(i, LinkType.Output)).ToList();
				rectX = iconPoint.X - rectWidth / 2;
				rectY = iconPoint.Y -(iconSize + iconBorder + iconBorder) / 2 - textHeight;
			}
			else
			{
				Point iconPoint = GetInputIconPoint(item);
				var sortedInputs = DisplayedNode.Inputs.OrderBy(i => getXSortValue(i, LinkType.Input)).ToList();
				rectX = iconPoint.X - rectWidth / 2;
				rectY = iconPoint.Y -(iconSize + iconBorder + iconBorder) / 2;
			}

			return new Rectangle(rectX, rectY, rectWidth, rectHeight);
		}

		public Point GetOutputIconPoint(Item item)
		{
			if (DisplayedNode.Outputs.Contains(item))
			{
				var sortedOutputs = DisplayedNode.Outputs.OrderBy(i => getXSortValue(i, LinkType.Output)).ToList();
				int x = Convert.ToInt32((float)Width / (sortedOutputs.Count()) * (sortedOutputs.IndexOf(item) + 0.5f));
				int y = 0;
				return new Point(x, y + iconBorder);
			}
			else
			{
				return new Point();
			}
		}

		public Point GetInputIconPoint(Item item)
		{
			if (DisplayedNode.Inputs.Contains(item))
			{
				var sortedInputs = DisplayedNode.Inputs.OrderBy(i => getXSortValue(i, LinkType.Input)).ToList();
				int x = Convert.ToInt32((float)Width / (sortedInputs.Count()) * (sortedInputs.IndexOf(item) + 0.5f));
				int y = Height;
				return new Point(x, y - iconBorder);
			}
			else
			{
				return new Point();
			}
		}

		public Point GetOutputLineConnectionPoint(Item item)
		{
			Rectangle iconRect = GetIconBounds(item, LinkType.Output);
			return new Point(X + iconRect.X + iconRect.Width / 2, Y + iconRect.Y);
		}

		public Point GetInputLineConnectionPoint(Item item)
		{
			Rectangle iconRect = GetIconBounds(item, LinkType.Input);
			return new Point(X + iconRect.X + iconRect.Width / 2, Y + iconRect.Y + iconRect.Height);
		}

		//Used to sort items in the input/output lists
		public int getXSortValue(Item item, LinkType linkType)
		{
			int total = 0;
			if (linkType == LinkType.Input)
			{
				foreach (NodeLink link in DisplayedNode.InputLinks.Where(l => l.Item == item))
				{
					total += Parent.GetElementForNode(link.Supplier).X;
				}
			}
			else
			{
				foreach (NodeLink link in DisplayedNode.OutputLinks.Where(l => l.Item == item))
				{
					total += Parent.GetElementForNode(link.Consumer).X;
				}
			}
			return total;
		}

		public override void Paint(Graphics graphics)
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

			foreach (Item item in DisplayedNode.Outputs)
			{
				DrawItemIcon(item, GetOutputIconPoint(item), LinkType.Output, getIconString(item, LinkType.Output), graphics, chooseIconColour(item, LinkType.Output));
			}
			foreach (Item item in DisplayedNode.Inputs)
			{
				DrawItemIcon(item, GetInputIconPoint(item), LinkType.Input, getIconString(item, LinkType.Input), graphics, chooseIconColour(item, LinkType.Input));
			}

			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
			graphics.DrawString(nameText, new Font(FontFamily.GenericSansSerif, 8), new SolidBrush(Color.Black), new PointF(Width / 2, Height / 2), centreFormat);

			if (editorBox != null)
			{
				TooltipInfo ttinfo = new TooltipInfo();
				ttinfo.ScreenLocation = Parent.GraphToScreen(GetInputLineConnectionPoint(editedItem));
				ttinfo.Direction = Direction.Up;
				ttinfo.ScreenSize = new Point(editorBox.Size);
				Parent.AddTooltip(ttinfo);
			}
		}

		private String getIconString(Item item, LinkType linkType)
		{
			String line1Format = "{0}{1}";
			String line2Format = "\n({0:0.##}{1})";
			String finalString = "";

			String unit = "";
			if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerSecond)
			{
				unit = "/s";
			}
			else if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerMinute)
			{
				unit = "/m";
			}

			if (linkType == LinkType.Input)
			{
				finalString = String.Format(line1Format, DisplayedNode.GetTotalInput(item), unit);
				if (DisplayedNode.GetRequiredInput(item) > DisplayedNode.GetTotalInput(item))
				{
					finalString += String.Format(line2Format, DisplayedNode.GetRequiredInput(item), unit);
				}
			} else {
				finalString = String.Format(line1Format, DisplayedNode.GetTotalOutput(item), unit);
			}

			return finalString;
		}

		private Color chooseIconColour(Item item, LinkType linkType)
		{
			Color notEnough = Color.FromArgb(255, 255, 193, 193);
			Color enough = Color.White;
			Color tooMuch = Color.FromArgb(255, 174, 198, 206);

			if (linkType == LinkType.Input)
			{
				if (DisplayedNode.GetRequiredInput(item) <= DisplayedNode.GetTotalInput(item))
				{
					return enough;
				}
				else
				{
					return notEnough;
				}
			}
			else
			{
				if (DisplayedNode.GetTotalOutput(item) > DisplayedNode.GetRequiredOutput(item))
				{
					return tooMuch;
				}
				else
				{
					return enough;
				}
			}
		}

		private void DrawItemIcon(Item item, Point drawPoint, LinkType linkType, String rateText, Graphics graphics, Color fillColour)
		{
			int textHeight = iconTextHeight;
			if (linkType == LinkType.Input && DisplayedNode.GetUnsatisfiedDemand(item) > 0)
			{
				textHeight = iconTextHeight2Line;
			}

			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;

			Rectangle iconBounds = GetIconBounds(item, linkType);

			using (Pen borderPen = new Pen(Color.Gray, 3))
			using (Brush fillBrush = new SolidBrush(fillColour))
			using (Brush textBrush = new SolidBrush(Color.Black))
			{
				if (linkType == LinkType.Output)
				{
					GraphicsStuff.FillRoundRect(iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height, iconBorder, graphics, fillBrush);
					GraphicsStuff.DrawRoundRect(iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height, iconBorder, graphics, borderPen);
					graphics.DrawString(rateText, new Font(FontFamily.GenericSansSerif, 7), textBrush, new PointF(iconBounds.X + iconBounds.Width / 2, iconBounds.Y + (textHeight + iconBorder) / 2), centreFormat);
				}
				else
				{
					GraphicsStuff.FillRoundRect(iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height, iconBorder, graphics, fillBrush);
					GraphicsStuff.DrawRoundRect(iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height, iconBorder, graphics, borderPen);
					graphics.DrawString(rateText, new Font(FontFamily.GenericSansSerif, 7), textBrush, new PointF(iconBounds.X + iconBounds.Width / 2, iconBounds.Y + iconBounds.Height - (textHeight + iconBorder) / 2), centreFormat);
				}
			}
			graphics.DrawImage(item.Icon ?? DataCache.UnknownIcon, drawPoint.X - iconSize / 2, drawPoint.Y - iconSize / 2, iconSize, iconSize);
		}

		public override void MouseUp(Point location, MouseButtons button)
		{
			Item clickedItem = null;
			LinkType clickedLinkType = LinkType.Input;
			foreach (Item item in DisplayedNode.Inputs)
			{
				if (GetIconBounds(item, LinkType.Input).Contains(location))
				{
					clickedItem = item;
					clickedLinkType = LinkType.Input;
				}
			}
			foreach (Item item in DisplayedNode.Outputs)
			{
				if (GetIconBounds(item, LinkType.Output).Contains(location))
				{
					clickedItem = item;
					clickedLinkType = LinkType.Output;
				}
			}

			if (button == MouseButtons.Left)
			{
				if (clickedItem != null && clickedLinkType == LinkType.Input && DisplayedNode is ConsumerNode)
				{
					BeginEditingInputAmount(clickedItem);
				}
			}
			else if (button == MouseButtons.Right)
			{
				ContextMenu rightClickMenu = new ContextMenu();
				if (clickedItem != null)
				{
					if (clickedLinkType == LinkType.Input)
					{
						if (DisplayedNode.GetUnsatisfiedDemand(clickedItem) > 0)
						{
							rightClickMenu.MenuItems.Add(new MenuItem("Automatically choose/create a node to produce this item",
								new EventHandler((o, e) =>
								{
									DisplayedNode.Graph.AutoSatisfyNodeDemand(DisplayedNode, clickedItem);
									Parent.UpdateElements();
									Parent.Invalidate();
								})));

							rightClickMenu.MenuItems.Add(new MenuItem("Manually create a node to produce this item",
								new EventHandler((o, e) =>
									{
										RecipeChooserForm form = new RecipeChooserForm(clickedItem);
										var result = form.ShowDialog();
										if (result == DialogResult.OK)
										{
											if (form.selectedRecipe != null)
											{
												DisplayedNode.Graph.CreateRecipeNodeToSatisfyItemDemand(DisplayedNode, clickedItem, form.selectedRecipe);
											}
											else
											{
												DisplayedNode.Graph.CreateSupplyNodeToSatisfyItemDemand(DisplayedNode, clickedItem);
											}
											Parent.UpdateElements();
											Parent.Invalidate();
										}
									})));

							rightClickMenu.MenuItems.Add(new MenuItem("Connect this input to an existing node",
								new EventHandler((o, e) =>
								{
									DraggedLinkElement newLink = new DraggedLinkElement(Parent, this, LinkType.Input, clickedItem);
									newLink.ConsumerElement = this;
									})));
						}
					}
					else
					{
						rightClickMenu.MenuItems.Add(new MenuItem("Connect this output to an existing node",
							new EventHandler((o, e) =>
								{
									DraggedLinkElement newLink = new DraggedLinkElement(Parent, this, LinkType.Output, clickedItem);
									newLink.SupplierElement = this;
								})));
					}
				}

				rightClickMenu.MenuItems.Add(new MenuItem("Delete node",
					new EventHandler((o, e) =>
						{
							Parent.DeleteNode(this);
						})));

				rightClickMenu.Show(Parent, Parent.GraphToScreen(Point.Add(location, new Size(X, Y))));
			}
		}

		public void BeginEditingInputAmount(Item item)
		{
			if (editorBox != null)
			{
				stopEditingInputAmount();
			}

			editorBox = new TextBox();
			editedItem = item;
			editedLinkType = LinkType.Input;
			editorBox.Text = (DisplayedNode as ConsumerNode).ConsumptionAmount.ToString();
			originalEditorValue = (DisplayedNode as ConsumerNode).ConsumptionAmount;
			editorBox.SelectAll();
			editorBox.Size = new Size(100, 30);
			Rectangle tooltipRect = Parent.getTooltipScreenBounds(Parent.GraphToScreen(GetInputLineConnectionPoint(item)), new Point(editorBox.Size), Direction.Up);
			editorBox.Location = new Point(tooltipRect.X, tooltipRect.Y);
			Parent.Controls.Add(editorBox);
			editorBox.Focus();
			editorBox.TextChanged += editorBoxTextChanged;
			editorBox.KeyDown += new KeyEventHandler(editorBoxKeyDown);
			editorBox.LostFocus += new EventHandler((sender, e) => stopEditingInputAmount());
		}

		private void stopEditingInputAmount()
		{
			if (editorBox != null && !editorBox.IsDisposed)
			{
				Parent.Controls.Remove(editorBox);
				editorBox = null;
				editedItem = null;
				Parent.Focus();
			}
		}

		private void editorBoxTextChanged(object sender, EventArgs e)
		{
			float amount;
			bool amountIsValid = float.TryParse((sender as TextBox).Text, out amount);

			if (amountIsValid)
			{
				(DisplayedNode as ConsumerNode).ConsumptionAmount = amount;
				DisplayedNode.Graph.UpdateNodeAmounts();
				Parent.Invalidate();
			}
		}

		private void editorBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				stopEditingInputAmount();
			}
			else if (e.KeyCode == Keys.Escape)
			{
				if (editorBox != null) //This line only happens if the window is closed with the esc button with the editor box open.
				{
					editorBox.Text = originalEditorValue.ToString();
				}
				stopEditingInputAmount();
			}			
		}

		public override void MouseMoved(Point location)
		{
			if (editorBox == null)
			{
				foreach (Item item in DisplayedNode.Inputs)
				{
					if (GetIconBounds(item, LinkType.Input).Contains(location))
					{
						String tooltipText;
						if (DisplayedNode is ConsumerNode)
						{
							tooltipText = String.Format("{0} (Click to edit amount)", item.Name);
						}
						else
						{
							tooltipText = item.Name;
						}

						Parent.AddTooltip(new TooltipInfo(Parent.GraphToScreen(GetInputLineConnectionPoint(item)), new Point(), Direction.Up, tooltipText));
					}
				}
				foreach (Item item in DisplayedNode.Outputs)
				{
					if (GetIconBounds(item, LinkType.Output).Contains(location))
					{
						Parent.AddTooltip(new TooltipInfo(Parent.GraphToScreen(GetOutputLineConnectionPoint(item)), new Point(), Direction.Down, item.Name));
					}
				}
			}
		}

		public override bool ContainsPoint(Point point)
		{
			if (new Rectangle(0, 0, Width, Height).Contains(point.X, point.Y))
			{
				return true;
			}
			foreach (Item item in DisplayedNode.Inputs)
			{
				Rectangle iconBounds = GetIconBounds(item, LinkType.Input);
				if (iconBounds.Contains(point.X, point.Y))
				{
					return true;
				}
			}
			foreach (Item item in DisplayedNode.Outputs)
			{
				Rectangle iconBounds = GetIconBounds(item, LinkType.Output);
				if (iconBounds.Contains(point.X, point.Y))
				{
					return true;
				}
			}
			return false;
		}

		public override void MouseDown(Point location, MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				Parent.DraggedElement = this;
				DragOffsetX = location.X;
				DragOffsetY = location.Y;
			}
		}

		public override void Dragged(Point location)
		{
			X += location.X - DragOffsetX;
			Y += location.Y - DragOffsetY;
		}
	}
}