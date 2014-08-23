using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Foreman
{
	class DraggedLinkElement : GraphElement
	{
		public NodeElement SupplierElement { get; set; }
		public NodeElement ConsumerElement { get; set; }
		public Item Item { get; set; }
		public LinkType StartConnectionType { get; private set; }

		public override Point Location
		{
			get { return new Point(); }
			set { }
		}
		public override int X
		{
			get { return 0; }
			set { }
		}
		public override int Y
		{
			get { return 0; }
			set { }
		}
		public override Point Size
		{
			get { return new Point(); }
			set { }
		}
		public override int Width
		{
			get { return 0; }
			set { }
		}
		public override int Height
		{
			get { return 0; }
			set { }
		}

		public DraggedLinkElement(ProductionGraphViewer parent, NodeElement startNode, LinkType startConnectionType, Item item)
			: base(parent)
		{
			if (startConnectionType == LinkType.Input)
			{
				ConsumerElement = startNode;
			}
			else
			{
				SupplierElement = startNode;
			}
			StartConnectionType = startConnectionType;
			Item = item;
		}

		public override void Paint(System.Drawing.Graphics graphics)
		{
			Point pointN = Parent.ScreenToGraph(Parent.PointToClient(Cursor.Position));
			Point pointM = pointN;

			if (SupplierElement != null)
			{
				pointN = SupplierElement.GetOutputLineConnectionPoint(Item);
			}
			if (ConsumerElement != null)
			{
				pointM = ConsumerElement.GetInputLineConnectionPoint(Item);
			}
			Point pointN2 = new Point(pointN.X, pointN.Y - Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));
			Point pointM2 = new Point(pointM.X, pointM.Y + Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));

			using (Pen pen = new Pen(DataCache.IconAverageColour(Item.Icon), 3f))
			{
				graphics.DrawBezier(pen, pointN, pointN2, pointM2, pointM);
			}
		}

		public override bool ContainsPoint(Point point)
		{
			return true;
		}

		public override void MouseDown(Point location, MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					if (SupplierElement != null && ConsumerElement != null)
					{
						if (StartConnectionType == LinkType.Input)
						{
							NodeLink.Create(SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode, Item, ConsumerElement.DisplayedNode.GetUnsatisfiedDemand(Item));
						}
						else
						{
							NodeLink.Create(SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode, Item, SupplierElement.DisplayedNode.GetExcessOutput(Item));
						}
					}
					break;
				case MouseButtons.Right:
					Dispose();
					break;
			}
		}

		public override void MouseMoved(Point location)
		{
			NodeElement mousedElement = Parent.GetElementsAtPoint(location).OfType<NodeElement>().FirstOrDefault();
			if (mousedElement != null)
			{
				if (StartConnectionType == LinkType.Input)
				{
					if (mousedElement.DisplayedNode.Outputs.Contains(Item))
					{
						SupplierElement = mousedElement;
					}
				}
				else
				{
					if (mousedElement.DisplayedNode.Inputs.Contains(Item))
					{
						ConsumerElement = mousedElement;
					}
				}
			}
		}
	}
}
