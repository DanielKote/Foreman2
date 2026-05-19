using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    public abstract class GraphElement : IDisposable {
        public List<GraphElement> SubElements { get; private set; }

        //bounds assumes 0,0 is the center of this element. X,Y (or location) is the difference between this origin and the parent element origin (NOT! the graph origin! -> this means that moving the parent element will not change the x,y of the child elements)
        //to simplify things, most point transformations make use of ConvertToLocal funtion which goes through the entire element-subelement ownership to convert a graph point to local coordinates (for in-class use).
        //so any calls OUTSIDE a GraphElement assumes graph origin (0,0 is the center of the graph)
        //and any work inside is usually done with local origin (0,0 is the center of the element)
        //ex: mouse clicked at 10,10 on the graph. the first node (offset -5,-5) will get a call mouseClick(10,10) which will internally be converted to local coordinate (15,15).
        //		a pass of that mouse click to its sub node (offset -5,-5 further from the parent node) will get a call mouseClick(10,10), which will internally be converted to local coordinate (20,20)
        public Rectangle Bounds { get { return new Rectangle(-Width / 2, -Height / 2, Width, Height); } }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public Size Size { get { return new Size(Width, Height); } set { Width = value.Width; Height = value.Height; } }

        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual Point Location //relative to parent element
        {
            get { return new Point(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        public virtual bool Visible { get; protected set; }
        protected ProductionGraphViewer graphViewer { get; }
        protected GraphElement? myParent { get; }
        protected ContextMenuStrip RightClickMenu { get; set; }
        protected static readonly Pen devPen = new(new SolidBrush(Color.OrangeRed), 1);

        public GraphElement(ProductionGraphViewer graphViewer, GraphElement? parent = null) {
            this.graphViewer = graphViewer;
            myParent = parent;
            if (myParent != null)
                myParent.SubElements.Add(this);

            RightClickMenu = new ContextMenuStrip {
                ShowItemToolTips = false,
                ShowImageMargin = false
            };
            RightClickMenu.Closing += (o, e) => {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true; //we will handle closing from item clicking within the items themselves
                else {
                    RightClickMenu.Items.Clear();
                    RightClickMenu.ShowCheckMargin = false;
                }
            };

            SubElements = [];
            Visible = true;
        }

        public Point GraphToLocal(Point graphPoint) //converts the point (in graph coordinates) to the local (0,0 is the center of this element's bound) point
        {
            return myParent == null
                ? Point.Subtract(graphPoint, (Size)Location)
                : Point.Subtract(myParent.GraphToLocal(graphPoint), (Size)Location);
        }

        public Point LocalToGraph(Point localPoint) {
            return myParent == null ? Point.Add(localPoint, (Size)Location) : Point.Add(myParent.LocalToGraph(localPoint), (Size)Location);
        }

        public bool IntersectsWithZone(Rectangle graphZone, int xborder, int yborder) {
            Point local_graphZone_origin = GraphToLocal(graphZone.Location);
            return (Width / 2) > local_graphZone_origin.X - xborder &&
                    -(Width / 2) < local_graphZone_origin.X + graphZone.Width + xborder &&
                     (Height / 2) > local_graphZone_origin.Y - yborder &&
                    -(Height / 2) < local_graphZone_origin.Y + graphZone.Height + yborder;
        }

        public virtual void UpdateVisibility(Rectangle graphZone, int xborder = 0, int yborder = 0) {
            Visible = IntersectsWithZone(graphZone, xborder, yborder);
        }

        public virtual bool ContainsPoint(Point graphPoint) {
            return Visible && Bounds.Contains(GraphToLocal(graphPoint));
        }

        public virtual void PrePaint() { }
        public void Paint(Graphics graphics, NodeDrawingStyle style) {
            if (Visible) {
                //call own draw operation
                Draw(graphics, style);

                //call paint operations (this function) for each of the sub-elements owned by this element (who will call their own draw and further paint operations on their own sub elements)
                foreach (GraphElement element in SubElements)
                    element.Paint(graphics, style);
            }
        }

        protected abstract void Draw(Graphics graphics, NodeDrawingStyle style);

        public virtual List<TooltipInfo>? GetToolTips(Point graphPoint) => [];
        public virtual void MouseMoved(Point graphPoint) { }
        public virtual void MouseDown(Point graphPoint, MouseButtons button) { }
        public virtual void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) { }
        public virtual void Dragged(Point graphPoint) { }

        public virtual void Dispose() {
            GC.SuppressFinalize(this);
            foreach (GraphElement element in SubElements.ToArray())
                element.Dispose();
            SubElements.Clear();
            if (myParent != null)
                myParent.SubElements.Remove(this);

            RightClickMenu.Dispose();

            graphViewer.Invalidate();
        }

        internal static string BuildingQuantityToText(double quantity) {
            string text = "";
            if (quantity >= 10000)
                text += quantity.ToString("0.##e0", DisplayCulture.Format);
            else if (Properties.Settings.Default.RoundAssemblerCount)
                text += Math.Ceiling(quantity).ToString("0", DisplayCulture.Format);
            else if (quantity >= 0.1)
                text += quantity.ToString("0.#", DisplayCulture.Format);
            else if (quantity != 0)
                text += "<0.1";
            else
                text += "0";
            return text;
        }
    }
}
