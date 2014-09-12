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
		public override Point Size
		{
			get
			{
				int leftColumnWidth = 0;
				int rightColumnWidth = 0;

				List<AssemblerIconElement> iconList = SubElements.OfType<AssemblerIconElement>().ToList();

				for (int i = 0; i < iconList.Count(); i += 2)
				{
					leftColumnWidth = Math.Max(iconList[i].Width, leftColumnWidth);
				}
				for (int i = 1; i < iconList.Count(); i += 2)
				{
					rightColumnWidth = Math.Max(iconList[i].Width, rightColumnWidth);
				}

				return new Point(leftColumnWidth + rightColumnWidth, ((int)Math.Ceiling(AssemblerList.Count() / 2f) * AssemblerIconElement.iconSize));
			}
		}
		
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

			int y = (int)(Height / Math.Ceiling(AssemblerList.Count / 2d));
			int widthOver2 = this.Width / 2;
			
			int i = 0;
			foreach (AssemblerIconElement element in SubElements.OfType<AssemblerIconElement>())
			{
				element.DisplayedNumber = AssemblerList[element.DisplayedMachine];

				if (i % 2 == 0)
				{
					element.X = widthOver2 - element.Width;
				}
				else
				{
					element.X = widthOver2;
				}
				element.Y = (int)Math.Floor(i / 2d) * y;

				if (AssemblerList.Count == 1)
				{
					element.X = (Width - element.Width) / 2;
				} else 			if (i == AssemblerList.Count - 1 && AssemblerList.Count % 2 != 0)
				{
					element.X = widthOver2 - (element.Width / 2);
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
		private int displayedNumber;
		public int DisplayedNumber
		{
			get
			{
				return displayedNumber;
			}
			set
			{
				displayedNumber = value;
				using (Graphics graphics = Parent.CreateGraphics())
				{
					stringWidth = graphics.MeasureString(DisplayedNumber.ToString(), Font).Width;
					UpdateSize();
				}
			}
		}
		private float stringWidth = 0f;
		private StringFormat centreFormat = new StringFormat();
		public const int iconSize = 32;
		private Font Font = new Font(FontFamily.GenericSansSerif, maxFontSize);

		public AssemblerIconElement(MachinePermutation assembler, int number, ProductionGraphViewer parent)
			: base(parent)
		{
			DisplayedMachine = assembler;
			DisplayedNumber = number;
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
		}

		private void UpdateSize()
		{
			Width = (int)stringWidth + iconSize;
			Height = iconSize;
		}

		public override void Paint(Graphics graphics)
		{
			Point iconPoint = new Point((int)((Width + iconSize + stringWidth) / 2 - iconSize), (int)((Height - iconSize) / 2));

			graphics.DrawImage(DisplayedMachine.assembler.Icon, iconPoint.X, iconPoint.Y, iconSize, iconSize);
			graphics.DrawString(DisplayedNumber.ToString(), Font, Brushes.Black, new Point((int)((Width - iconSize - stringWidth) / 2 + stringWidth / 2), Height / 2), centreFormat);

			if (DisplayedMachine.modules.Any())
			{
				int moduleCount = DisplayedMachine.modules.OfType<Module>().Count();
				int numModuleRows = (int)Math.Ceiling(moduleCount / 2d);
				int moduleSize = Math.Min((int)(iconSize / numModuleRows), iconSize / (2 - moduleCount % 2)) - 2;

				int i = 0;
				int x;

				if (moduleCount == 1)
				{
					x = iconPoint.X + (iconSize - moduleSize) / 2;
				}
				else
				{
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