using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class AssemblerBox : GraphElement
	{
		public Dictionary<Assembler, int> AssemblerList;
		int padding = 4;

		public AssemblerBox(ProductionGraphViewer parent)
			: base(parent)
		{
			AssemblerList = new Dictionary<Assembler, int>();
		}
		
		public override void Paint(System.Drawing.Graphics graphics)
		{
			if (AssemblerList.Any())
			{
				int heightPerAssembler = (Height - padding) / AssemblerList.Count() - padding;
				int y = padding;
				StringFormat centreFormat = new StringFormat();
				centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;

				using (Font numberFont = new System.Drawing.Font(FontFamily.GenericSansSerif, heightPerAssembler / 3))
				{
					foreach (var assemblerKVP in AssemblerList)
					{

						String numberString = assemblerKVP.Value.ToString();
						float stringWidth = graphics.MeasureString(numberString, numberFont).Width;

						graphics.DrawImage(assemblerKVP.Key.Icon, (Width + stringWidth + heightPerAssembler) / 2 - heightPerAssembler, y, heightPerAssembler, heightPerAssembler);
						graphics.DrawString(numberString, numberFont, Brushes.White, new PointF((Width - stringWidth - heightPerAssembler) / 2 + stringWidth / 2, y + heightPerAssembler / 2), centreFormat);

						y += heightPerAssembler + padding;
					}
				}
			}

			base.Paint(graphics);
		}
	}
}
