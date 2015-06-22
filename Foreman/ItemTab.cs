using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class ItemTab : GraphElement
	{
		public LinkType Type;

		private const int iconSize = 32;
		private const int border = 4;
		private int textHeight = 11;
		private StringFormat centreFormat = new StringFormat();
		private Pen borderPen = new Pen(Color.Gray, 3);
		private Brush textBrush = new SolidBrush(Color.Black);
		private Brush fillBrush;

		private Color fillColour;
		public Color FillColour
		{
			get
			{
				return fillColour;
			}
			set {
				fillColour = value;
				if (fillBrush != null)
				{
					fillBrush.Dispose();
				}
				fillBrush = new SolidBrush(value);
			}
		}

		private string text = "";
		public String Text
		{
			get { return text; }
			set
			{
				text = value;
				textHeight = (int)Parent.CreateGraphics().MeasureString(value, font).Height;
			}
		}

		public Font font = new Font(FontFamily.GenericSansSerif, 7);
		public Item Item { get; private set; }

		public ItemTab(Item item, LinkType type, ProductionGraphViewer parent)
			: base(parent)
		{
			this.Item = item;
			this.Type = type;
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
			FillColour = Color.White;
		}
		
		public override System.Drawing.Point Size
		{
			get
			{
				return new Point(iconSize + border * 3, iconSize + textHeight + border * 3);
			}
			set
			{
			}
		}

		public override void Paint(Graphics graphics)
		{
			Point iconPoint = Point.Empty;
			if (Type == LinkType.Output)
			{
				iconPoint = new Point((int)(border * 1.5), Height - (int)(border * 1.5) - iconSize);
			}
			else
			{
				iconPoint = new Point((int)(border * 1.5), (int)(border * 1.5));
			}

			if (Type == LinkType.Output)
			{
				GraphicsStuff.FillRoundRect(0, 0, Width, Height, border, graphics, fillBrush);
				GraphicsStuff.DrawRoundRect(0, 0, Width, Height, border, graphics, borderPen);
				graphics.DrawString(Text, font, textBrush, new PointF(Width / 2, (textHeight + border) / 2), centreFormat);
			}
			else
			{
				GraphicsStuff.FillRoundRect(0, 0, Width, Height, border, graphics, fillBrush);
				GraphicsStuff.DrawRoundRect(0, 0, Width, Height, border, graphics, borderPen);
				graphics.DrawString(Text, font, textBrush, new PointF(Width / 2, Height - (textHeight + border) / 2), centreFormat);
			}
			graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, iconPoint.X, iconPoint.Y, iconSize, iconSize);
		}

		public override void Dispose()
		{
			textBrush.Dispose();
			fillBrush.Dispose();
			centreFormat.Dispose();
			borderPen.Dispose();
			font.Dispose();
			base.Dispose();
		}
	}
}
