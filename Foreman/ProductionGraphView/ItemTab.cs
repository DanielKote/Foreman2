using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Foreman
{
	public class ItemTab : GraphElement
	{
		public LinkType LinkType;
		public Item Item { get; private set; }

		private const int iconSize = 32;
		private const int border = 3;
		private int textHeight = 11;

		private static StringFormat centreFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
		private static Pen regularBorderPen = new Pen(Color.Gray, 3);
		private static Pen oversuppliedBorderPen = new Pen(Color.DarkRed, 3);
		private static Brush textBrush = new SolidBrush(Color.Black);
		private static Brush fillBrush = new SolidBrush(Color.White);

		private static Font textFont = new Font(FontFamily.GenericSansSerif, 6);

		private static string line1FormatA = "{0:0.##}{1}";
		private static string line1FormatB = "{0:0.#}{1}";
		private static string line1FormatC = "{0:0}{1}";
		private static string line2FormatA = "\n({0:0.##}{1})";
		private static string line2FormatB = "\n({0:0.#}{1})";
		private static string line2FormatC = "\n({0:0}{1})";

		private Pen borderPen;
		private string text = "";

		public ItemTab(Item item, LinkType type, ProductionGraphViewer parent)
			: base(parent)
		{
			this.Item = item;
			this.LinkType = type;
			borderPen = regularBorderPen;
			int textHeight = (int)myGraphViewer.CreateGraphics().MeasureString("a", textFont).Height;
			Width = iconSize + border * 3;
			Height = iconSize + textHeight + border + 3;
			X = 0; Y = 0;
		}

		public void UpdateValues(float consumeRate, float suppliedRate, bool isOversupplied)
        {
			string unit = "";
			if (myGraphViewer.SelectedAmountType == AmountType.Rate && myGraphViewer.SelectedRateUnit == RateUnit.PerSecond)
				unit = "/s";
			else if (myGraphViewer.SelectedAmountType == AmountType.Rate && myGraphViewer.SelectedRateUnit == RateUnit.PerMinute)
			{
				unit = "/m";
				consumeRate *= 60;
				suppliedRate *= 60;
			}

			text = "";
			borderPen = regularBorderPen;
			if (consumeRate >= 1000)
				text = String.Format(line1FormatC, consumeRate, unit);
			else if (consumeRate >= 100)
				text = String.Format(line1FormatB, consumeRate, unit);
			else
				text = String.Format(line1FormatA, consumeRate, unit);

			if (isOversupplied)
			{
				borderPen = oversuppliedBorderPen;
				if (suppliedRate >= 1000)
					text += String.Format(line2FormatC, suppliedRate, unit);
				else if (suppliedRate >= 100)
					text += String.Format(line2FormatB, suppliedRate, unit);
				else
					text += String.Format(line2FormatA, suppliedRate, unit);
			}

			int textHeight = (int)myGraphViewer.CreateGraphics().MeasureString(text, textFont).Height;
			Height = iconSize + textHeight + border + 3;
		}

		protected override void Draw(Graphics graphics, Point trans)
		{
			GraphicsStuff.FillRoundRect(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height, border, graphics, fillBrush);
			GraphicsStuff.DrawRoundRect(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height, border, graphics, borderPen);

			if (LinkType == LinkType.Output)
			{
				graphics.DrawString(text, textFont, textBrush, new PointF(trans.X, trans.Y + ((textHeight + border - Bounds.Height) / 2)), centreFormat);
				graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - (Bounds.Width / 2) + (int)(border * 1.5), trans.Y + (Bounds.Height / 2) - (int)(border * 1.5) - iconSize, iconSize, iconSize);
			}
			else
			{
				graphics.DrawString(text, textFont, textBrush, new PointF(trans.X, trans.Y - ((textHeight + border - Bounds.Height) / 2)), centreFormat);
				graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - (Bounds.Width / 2) + (int)(border * 1.5), trans.Y - (Bounds.Height / 2) + (int)(border * 1.5), iconSize, iconSize);
			}
		}
	}
}
