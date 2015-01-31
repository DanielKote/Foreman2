using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	class GhostNodeElement : GraphElement
	{
		public HashSet<Item> Items = new HashSet<Item>();

		private const int iconSize = 32;
		private List<Point> OffsetOrder = new List<Point>
		{
			new Point(0, 0),
			new Point(0, 35),
			new Point(0, -35),
			new Point(-35, 0),
			new Point(35, 0),
			new Point(-35, -35),
			new Point(35, 35),
			new Point(-35, 35),
			new Point(35, -35)
		};

		public GhostNodeElement(ProductionGraphViewer parent) : base(parent)
		{
			Width = 96;
			Height = 96;
		}
		
		public override void Paint(Graphics graphics)
		{
			int i = 0;

			foreach (Item item in Items)
			{
				if (i >= OffsetOrder.Count())
				{
					break;
				}
				Point position = Point.Subtract(OffsetOrder[i], new Size(iconSize / 2, iconSize / 2));
				graphics.DrawImage(item.Icon, position.X, position.Y, iconSize, iconSize);
				i++;
			}

			base.Paint(graphics);
		}
	}
}
