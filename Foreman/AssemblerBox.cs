using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class AssemblerBox : GraphElement
	{
		public Dictionary<ProductionEntity, int> AssemblerList;

		public AssemblerBox(ProductionGraphViewer parent)
			: base(parent)
		{
			AssemblerList = new Dictionary<ProductionEntity, int>();
		}

		public void Update()
		{
			foreach (AssemblerIconElement element in SubElements.OfType<AssemblerIconElement>().ToList())
			{
				if (!AssemblerList.Keys.Contains(element.DisplayedAssembler))
				{
					SubElements.Remove(element);
				}
			}

			foreach (var kvp in AssemblerList)
			{
				if (!SubElements.OfType<AssemblerIconElement>().Any(aie => aie.DisplayedAssembler == kvp.Key))
				{
					SubElements.Add(new AssemblerIconElement(kvp.Key, kvp.Value, Parent));
				}
			}

			int height = (int)(Height / Math.Ceiling(AssemblerList.Count / 2d));
			int width = this.Width / 2;
			
			int i = 0;
			foreach (AssemblerIconElement element in SubElements.OfType<AssemblerIconElement>())
			{
				element.DisplayedNumber = AssemblerList[element.DisplayedAssembler];

				element.Width = width;
				element.Height = height;
				element.X = (i % 2) * width;
				element.Y = (int)Math.Floor(i / 2d) * height;

				if (i == AssemblerList.Count - 1 && AssemblerList.Count % 2 != 0)
				{
					element.Width = this.Width;
				}

				i++;
			}
		}

		public override void Paint(System.Drawing.Graphics graphics)
		{
			base.Paint(graphics);
		}
	}

	public class AssemblerIconElement : GraphElement
	{
		const int maxFontSize = 14;
		public ProductionEntity DisplayedAssembler { get; set; }
		public int DisplayedNumber { get; set; }
		private StringFormat centreFormat = new StringFormat();

		public AssemblerIconElement(ProductionEntity assembler, int number, ProductionGraphViewer parent)
			: base(parent)
		{
			DisplayedAssembler = assembler;
			DisplayedNumber = number;
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
		}

		public override void Paint(Graphics graphics)
		{
			using (Font font = new Font(FontFamily.GenericSansSerif, Math.Min(Height / 2, maxFontSize)))
			{
				float StringWidth = graphics.MeasureString(DisplayedNumber.ToString(), font).Width;
				int iconSize = (int)Math.Min(Width - StringWidth, Height);

				graphics.DrawImage(DisplayedAssembler.Icon, (Width + iconSize + StringWidth) / 2 - iconSize, (Height - iconSize) / 2, iconSize, iconSize);
				graphics.DrawString(DisplayedNumber.ToString(), font, Brushes.Black, new Point((int)((Width - iconSize - StringWidth) / 2 + StringWidth / 2), Height / 2), centreFormat);
			}
		}
	}
}
