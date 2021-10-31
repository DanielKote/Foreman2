using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Foreman
{
	class DraggedLinkElement : BaseLinkElement
	{
		public override Item Item { get; protected set; }
		public LinkType StartConnectionType { get; private set; }
		private Point newObjectLocation;

		private bool dragEnded;

		public DraggedLinkElement(ProductionGraphViewer graphViewer, NodeElement startNode, LinkType startConnectionType, Item item) : base(graphViewer)
		{
			if (startConnectionType == LinkType.Input)
				ConsumerElement = startNode;
			else
				SupplierElement = startNode;

			StartConnectionType = startConnectionType;
			Item = item;

			dragEnded = false;
		}

		public override void UpdateVisibility(Rectangle graph_zone, int xborder, int yborder) { Visible = true; } //always visible.

		protected override Point[] GetCurveEndpoints()
		{
			if (dragEnded)
				return null; //no update

			Point pointNUpdate = myGraphViewer.ScreenToGraph(myGraphViewer.PointToClient(Cursor.Position));
			Point pointMUpdate = pointNUpdate;
			newObjectLocation = pointNUpdate;

			//only snap to grid if grid exists and its a free link (not linking 2 existing objects)
			if ((SupplierElement == null || ConsumerElement == null) && myGraphViewer.Grid.ShowGrid && myGraphViewer.Grid.CurrentGridUnit > 0)
			{
				pointNUpdate = myGraphViewer.Grid.AlignToGrid(pointNUpdate);
				pointMUpdate = pointNUpdate;
				newObjectLocation = pointNUpdate;
			}

			if (SupplierElement != null)
				pointNUpdate = SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
			if (ConsumerElement != null)
				pointMUpdate = ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();

			return new Point[] { pointMUpdate, pointNUpdate };
		}

		private void EndDrag(Point graph_point)
		{
			dragEnded = true;

			if (SupplierElement != null && ConsumerElement != null) //no nulls -> this is a 'link 2 nodes' operation
			{
				myGraphViewer.Graph.CreateLink(SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode, this.Item);

				myGraphViewer.Graph.UpdateNodeValues();
				myGraphViewer.UpdateGraphBounds();
				myGraphViewer.Invalidate();
				myGraphViewer.DisposeLinkDrag();
			}
			else //at least one null -> this is an 'add new recipe' operation
			{
				Point screenPoint = new Point(myGraphViewer.GraphToScreen(graph_point).X - 150, 15);
				screenPoint.X = Math.Max(15, Math.Min(myGraphViewer.Width - 650, screenPoint.X)); //want to position the recipe selector such that it is well visible.

				bool includeSuppliers = StartConnectionType == LinkType.Input && SupplierElement == null;
				bool includeConsumers = StartConnectionType == LinkType.Output && ConsumerElement == null;
				if (includeSuppliers)
					myGraphViewer.AddRecipe(screenPoint, Item, Point.Add(newObjectLocation, new Size(0, myGraphViewer.SimpleView ? (NodeElement.baseHeight / 2) : (NodeElement.baseWIconHeight / 2))), ProductionGraphViewer.NewNodeType.Supplier, ConsumerElement);
				else if (includeConsumers)
					myGraphViewer.AddRecipe(screenPoint, Item, Point.Add(newObjectLocation, new Size(0, myGraphViewer.SimpleView ? (-NodeElement.baseHeight / 2) : (-NodeElement.baseWIconHeight / 2))), ProductionGraphViewer.NewNodeType.Consumer, SupplierElement);
				else
					Trace.Fail("Both null dragged link!");
			}
		}

		public override void MouseDown(Point graph_point, MouseButtons button)
		{
			if (button == MouseButtons.Left)
				EndDrag(graph_point);
			else if (button == MouseButtons.Right) //cancel drag-link
				myGraphViewer.DisposeLinkDrag();
		}

		public override void MouseUp(Point graph_point, MouseButtons button, bool wasDragged)
		{
			if (button == MouseButtons.Left)
				EndDrag(graph_point);
		}

		public override void MouseMoved(Point graph_point)
		{
			if (dragEnded)
				return;

			NodeElement mousedElement = myGraphViewer.GetNodeAtPoint(graph_point);
			if (mousedElement != null)
			{
				if (StartConnectionType == LinkType.Input && mousedElement.DisplayedNode.Outputs.Contains(Item))
					SupplierElement = mousedElement;
				else if (StartConnectionType == LinkType.Output && mousedElement.DisplayedNode.Inputs.Contains(Item))
					ConsumerElement = mousedElement;

				//if we have found a possible connection above (both supplier & consumer are no longer null), but the item is temperature dependent AND the temperature check fails, break connection
				if (SupplierElement != null &&
					ConsumerElement != null &&
					Item.IsTemperatureDependent &&
					!LinkChecker.IsValidTemperatureConnection(Item, SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode))
				{
					if (StartConnectionType == LinkType.Input)
						SupplierElement = null;
					else  //if(StartConnectionType == LinkType.Output)
						ConsumerElement = null;
				}
			}
			else //no node under mouse, break any previously established connections (ex:when mouse drag leaves a possible connection)
			{
				if (StartConnectionType == LinkType.Input)
					SupplierElement = null;
				else  //if(StartConnectionType == LinkType.Output)
					ConsumerElement = null;
			}
		}
	}
}
