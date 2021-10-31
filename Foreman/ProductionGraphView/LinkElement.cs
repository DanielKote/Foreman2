using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	class LinkElement : GraphElement
	{
		public NodeLink DisplayedLink { get; private set; }
		public NodeElement SupplierElement { get; private set; }
		public NodeElement ConsumerElement { get; private set; }
		public ItemTab SupplierTab { get; private set; }
		public ItemTab ConsumerTab { get; private set; }
		public Item Item { get { return DisplayedLink.Item; } }

		private Point pointM, pointM2;
		private Point pointN, pointN2;
		private Point pointMidM, pointMidA, pointMidB, pointMidN;
		private bool linkingUp;
		public float LinkWidth { get; set; }

		public Rectangle CalculatedBounds { get; private set; }

		public override Point Location
		{
			get { return new Point(); }
			set { }
		}
		public override int X { get { return 0; } set { } }
		public override int Y { get { return 0; } set { } }

		public LinkElement(ProductionGraphViewer parent, NodeLink displayedLink, NodeElement supplierElement, NodeElement consumerElement)
			: base(parent)
		{
			DisplayedLink = displayedLink;
			SupplierElement = supplierElement;
			ConsumerElement = consumerElement;
			SupplierTab = supplierElement.GetOutputLineItemTab(Item);
			ConsumerTab = consumerElement.GetInputLineItemTab(Item);
			LinkWidth = 3f;
			UpdateCurve();
		}

		public override void UpdateVisibility(Rectangle bounds, int xborder, int yborder)
		{
			UpdateCurve();
			Visible =
					 	CalculatedBounds.X + CalculatedBounds.Width > bounds.X - xborder &&
						CalculatedBounds.X < bounds.X + bounds.Width + xborder &&
						CalculatedBounds.Y + CalculatedBounds.Height > bounds.Y - yborder &&
						CalculatedBounds.Y < bounds.Y + bounds.Height + yborder;
		}

		public void UpdateCurve() //updates all points & boundaries (important for occluding objects outside view)
        {
			Point pointNupdate = (SupplierTab is null) ? new Point(SupplierElement.X, Y - (ConsumerElement.Height / 2)) : new Point(SupplierElement.X + SupplierTab.X, SupplierElement.Y  +SupplierTab.Y - (ConsumerTab.Height / 2));
			Point pointMupdate = (ConsumerTab is null) ? new Point(ConsumerElement.X, Y + (ConsumerElement.Height / 2)) : new Point(ConsumerElement.X + ConsumerTab.X, ConsumerElement.Y + ConsumerTab.Y + (ConsumerTab.Height / 2));

			if (pointNupdate != pointN || pointMupdate != pointM)
			{
				pointN = pointNupdate;
				pointM = pointMupdate;

				linkingUp = (pointN.Y > pointM.Y);
				if (linkingUp)
				{
					//directLine = (pointN.X == pointM.X || (Math.Abs(pointN.X - pointM.X) + Math.Abs(pointN.Y - pointM.Y) < 40)); //just in case we want to draw it as a straight
					pointN2 = new Point(pointN.X, pointN.Y - Math.Max((int)((pointN.Y - pointM.Y) / 2), 20));
					pointM2 = new Point(pointM.X, pointM.Y + Math.Max((int)((pointN.Y - pointM.Y) / 2), 20));

					CalculatedBounds = new Rectangle(
						Math.Min(pointM.X, pointN.X),
						pointM.Y,
						Math.Abs(pointM.X - pointN.X),
						pointN.Y - pointM.Y
						);
				}
				else
				{
					int midX = Math.Abs(pointN.X - pointM.X) > 200 ? (pointN.X + pointM.X) / 2 : pointN.X > pointM.X ? pointN.X + 150 : pointN.X - 150;

					pointMidA = new Point(midX, pointN.Y);
					pointMidB = new Point(midX, pointM.Y);


					pointN2 = new Point(pointN.X, pointN.Y - 120);
					pointMidN = new Point(midX, pointN.Y - 120);

					pointM2 = new Point(pointM.X, pointM.Y + 120);
					pointMidM = new Point(midX, pointM.Y + 120);

					CalculatedBounds = new Rectangle(
						Math.Min(pointM.X, pointN.X),
						pointN2.Y,
						Math.Abs(pointM.X - pointN.X),
						pointM2.Y - pointN2.Y
						);
				}
			}
		}

		protected override void Draw(Graphics graphics, Point trans)
		{
			if (Visible)
			{
				UpdateCurve();

				using (Pen pen = new Pen(Item.AverageColor, LinkWidth))
				{
					if (linkingUp)
						graphics.DrawBeziers(pen, new Point[] {
							new Point(pointN.X + trans.X + 0, pointN.Y + trans.Y + 10),
							new Point(pointN.X + trans.X + 0, pointN.Y + trans.Y + 10),
							Point.Add(pointN,(Size)trans),
							Point.Add(pointN,(Size)trans),
							Point.Add(pointN2,(Size)trans),
							Point.Add(pointM2,(Size)trans),
							Point.Add(pointM,(Size)trans),
							Point.Add(pointM,(Size)trans),
							new Point(pointM.X + trans.X + 0, pointM.Y + trans.Y - 10),
							new Point(pointM.X + trans.X + 0, pointM.Y + trans.Y - 10),
						});
					else
						graphics.DrawBeziers(pen, new Point[]{
							Point.Add(pointN, (Size)trans),
							Point.Add(pointN2, (Size)trans),
							Point.Add(pointMidN, (Size)trans),
							Point.Add(pointMidA, (Size)trans),
							Point.Add(pointMidA, (Size)trans),
							Point.Add(pointMidB, (Size)trans),
							Point.Add(pointMidB, (Size)trans),
							Point.Add(pointMidM, (Size)trans),
							Point.Add(pointM2, (Size)trans),
							Point.Add(pointM, (Size)trans)
						});
				}
			}
		}

		public static bool IsValidTemperatureConnection(Item item, NodeElement supplier, NodeElement consumer)
		{
			fRange supplierTempRange = GetTemperatureRange(item, supplier, LinkType.Output);
			fRange consumerTempRange = GetTemperatureRange(item, consumer, LinkType.Input);

			if (supplierTempRange.Ignore || consumerTempRange.Ignore)
				return true;
			return consumerTempRange.Contains(supplierTempRange);
		}

		public static fRange GetTemperatureRange(Item item, NodeElement node, LinkType direction)
        {
			//LinkType.Input : means we have a bunch of nodes ABOVE consuming the items, and we are connecting them to a single source
			//					SO: we need to check all directly-up connected recipes for min&max temp consumption. minTemp is set to be the maximum minTemp of each consumer, and maxTemp is set to be the minimum maxTemp of each consumer
			//					THIS CAN ALLOW FOR WRONG SIDE RANGES (ex: 20 -> 0 range), which means NO VALID TEMP WOULD WORK. Any valid producer must fit inside this consumer range.

			//LinkType.Output: means we have a bunch of nodes BELOW supplying the items, and we are connecting them to a single consumer
			//					SO: we need to check all directly-down connected recipes for min&max temp production. minTemp is set to be the minimum produced temperature, and maxTemp is set to be the maximum produced temperature
			//					ALL RANGES ARE RIGHT SIDE RANGES (ex: 0 -> 20), and basically require the consumer to accept any temperature within this range (producer range must be inside consumer range)

			float minTemp = (direction == LinkType.Input)? float.NegativeInfinity : float.PositiveInfinity;
			float maxTemp = (direction == LinkType.Input)? float.PositiveInfinity : float.NegativeInfinity;

			bool gotOne = false;
			Queue<BaseNode> neQueue = new Queue<BaseNode>(); //RecipeNode or PassthroughNodes
			if (node.DisplayedNode is RecipeNode || node.DisplayedNode is PassthroughNode)
				neQueue.Enqueue(node.DisplayedNode);
			while (neQueue.Count > 0)
			{
				BaseNode cNode = neQueue.Dequeue();
				if (cNode is PassthroughNode)
				{
					if(direction == LinkType.Input)
						foreach (NodeLink link in cNode.OutputLinks.Where(n => n.Consumer is RecipeNode || n.Consumer is PassthroughNode))
							neQueue.Enqueue(link.Consumer);
					else //if(direction == LinkType.Output)
						foreach (NodeLink link in cNode.InputLinks.Where(n => n.Supplier is RecipeNode || n.Supplier is PassthroughNode))
							neQueue.Enqueue(link.Supplier);
				}
				else //RecipeNode
				{
					gotOne = true;
					Recipe recipe = ((RecipeNode)cNode).BaseRecipe;
					if (direction == LinkType.Input)
					{
						minTemp = Math.Max(minTemp, recipe.IngredientTemperatureMap[item].Min);
						maxTemp = Math.Min(maxTemp, recipe.IngredientTemperatureMap[item].Max);
					}
					else //if(direction == LinkType.Output)
					{
						minTemp = Math.Min(minTemp, recipe.ProductTemperatureMap[item]);
						maxTemp = Math.Max(maxTemp, recipe.ProductTemperatureMap[item]);
					}
				}
			}
			if (gotOne)
				return new fRange(minTemp, maxTemp, false);
			else
				return new fRange(0, 0, true);
		}

	}
}
