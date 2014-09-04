using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class AssemblerBox : GraphElement
	{
		public Dictionary<MachinePermutation, int> AssemblerList;

		public AssemblerBox(ProductionGraphViewer parent)
			: base(parent)
		{
			AssemblerList = new Dictionary<MachinePermutation, int>();
		}

		public void Update()
		{
			foreach (AssemblerIconElement element in SubElements.OfType<AssemblerIconElement>().ToList())
			{
				if (!AssemblerList.Keys.Contains(element.DisplayedMachine))
				{
					SubElements.Remove(element);
				}
			}

			foreach (var kvp in AssemblerList)
			{
				if (!SubElements.OfType<AssemblerIconElement>().Any(aie => aie.DisplayedMachine == kvp.Key))
				{
					SubElements.Add(new AssemblerIconElement(kvp.Key, kvp.Value, Parent));  
				}
			}

			int height = (int)(Height / Math.Ceiling(AssemblerList.Count / 2d));
			int width = this.Width / 2;
			
			int i = 0;
			foreach (AssemblerIconElement element in SubElements.OfType<AssemblerIconElement>())
			{
				element.DisplayedNumber = AssemblerList[element.DisplayedMachine];

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
		public MachinePermutation DisplayedMachine { get; set; }
		public int DisplayedNumber { get; set; }
		private StringFormat centreFormat = new StringFormat();

		public AssemblerIconElement(MachinePermutation assembler, int number, ProductionGraphViewer parent)
			: base(parent)
		{
			DisplayedMachine = assembler;
			DisplayedNumber = number;
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
		}

		public override void Paint(Graphics graphics)
		{
			using (Font font = new Font(FontFamily.GenericSansSerif, Math.Min(Height / 2, maxFontSize)))
			{
				float StringWidth = graphics.MeasureString(DisplayedNumber.ToString(), font).Width;
				int iconSize = (int)Math.Min(Width - StringWidth, Height);
				Point iconPoint = new Point((int)((Width + iconSize + StringWidth) / 2 - iconSize), (int)((Height - iconSize) / 2));

				graphics.DrawImage(DisplayedMachine.assembler.Icon, iconPoint.X, iconPoint.Y, iconSize, iconSize);
				graphics.DrawString(DisplayedNumber.ToString(), font, Brushes.Black, new Point((int)((Width - iconSize - StringWidth) / 2 + StringWidth / 2), Height / 2), centreFormat);

				if (DisplayedMachine.modules.Any())
				{
					int moduleCount = DisplayedMachine.modules.OfType<Module>().Count();
					int numModuleRows = (int)Math.Ceiling(moduleCount / 2d);
					int moduleSize = iconSize / 2 / numModuleRows;

					int i = 0;
					int x;
					
					if (moduleCount == 1)
					{
						x = iconPoint.X + (iconSize - moduleSize) / 2;
					} else {
						x = iconPoint.X + (iconSize - moduleSize - moduleSize) / 2;
					}
					int y = iconPoint.Y + (iconSize - (moduleSize * numModuleRows)) / 2;
					for (int r = 0; r < numModuleRows; r++)
					{
						graphics.DrawImage(DisplayedMachine.modules[i].Icon, x, y + (r * moduleSize), moduleSize, moduleSize);
						i++;
						if (i < DisplayedMachine.modules.Count && DisplayedMachine.modules[i] != null)
						{
							graphics.DrawImage(DisplayedMachine.modules[i].Icon, x + moduleSize, y + (r * moduleSize), moduleSize, moduleSize);
							i++;
						}
					}
				}
			}
		}
	}
}