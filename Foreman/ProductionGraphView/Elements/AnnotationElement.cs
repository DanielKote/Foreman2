using Foreman.Serialization;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    /// <summary>
    /// Abstract base for all annotation elements (shapes and text labels).
    /// Annotations live in ProductionGraphViewer, not in the graph model.
    /// Coordinate conventions follow GraphElement: X,Y is the element center
    /// in graph space; Bounds is local (0,0 at center).
    /// </summary>
    public abstract class AnnotationElement : GraphElement {
        // ----------------------------------------------------------------
        // Selection state
        // ----------------------------------------------------------------

        /// <summary>True when this annotation is part of the current selection set.</summary>
        public bool IsSelected { get; set; }
        private const float EdgeHitScreenPx = 8f; // clickable border width in screen pixels

        // ----------------------------------------------------------------
        // Drag / resize tracking
        // ----------------------------------------------------------------

        private Point _dragStartMouseLocation;
        private Point _dragStartElementLocation;
        private bool _dragStarted;

        // ----------------------------------------------------------------
        // Resize handle state
        // ----------------------------------------------------------------

        protected enum HandleType {
            None,
            TopLeft, TopCenter, TopRight,
            MiddleLeft, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }

        private HandleType _activeHandle = HandleType.None;
        private int _dragStartWidth;
        private int _dragStartHeight;

        private const float HandleDrawScreenPx = 5f;  // visual size in screen pixels
        private const float HandleHitScreenPx = 10f; // hit zone in screen pixels

        /// <summary>True while a resize handle is being dragged (not a move).</summary>
        public bool IsResizing => _activeHandle != HandleType.None;

        protected int ResizeStartWidth => _dragStartWidth;
        protected int ResizeStartHeight => _dragStartHeight;

        private const int MinAnnotationSize = 30; // minimum width or height in graph units

        // ----------------------------------------------------------------
        // Selection highlight visual
        // ----------------------------------------------------------------

        private const float SelectionHighlightScreenPx = 2.5f;
        private static readonly Color SelectionHighlightColor = Color.FromArgb(220, 80, 160, 255);

        // ----------------------------------------------------------------
        // Construction
        // ----------------------------------------------------------------

        protected AnnotationElement(ProductionGraphViewer graphViewer,
                                    Point graphLocation, int width, int height)
            : base(graphViewer) {
            X = graphLocation.X;
            Y = graphLocation.Y;
            Width = width;
            Height = height;

            IsSelected = false;
            _dragStarted = false;
        }

        // ----------------------------------------------------------------
        // Hit testing ? override to include resize handle areas
        // ----------------------------------------------------------------

        public override bool ContainsPoint(Point graphPoint) {
            if (!Visible)
                return false;

            // Resize handles sit outside bounds ? check them first when selected
            if (IsSelected && GetHandleAtPoint(graphPoint) != HandleType.None)
                return true;

            Point local = GraphToLocal(graphPoint);

            if (!Bounds.Contains(local))
                return false;

            // When selected, the full interior is clickable for dragging
            if (IsSelected)
                return true;

            // When unselected, only the edge band counts ? clicking inside
            // the hollow interior falls through to rubber-band selection
            float edge = EdgeHitScreenPx / graphViewer.ViewScale;
            int halfW = Width / 2;
            int halfH = Height / 2;

            return local.X <= -halfW + edge ||
                       local.X >= halfW - edge ||
                       local.Y <= -halfH + edge ||
                       local.Y >= halfH - edge;
        }

        // ----------------------------------------------------------------
        // Mouse handling
        // ----------------------------------------------------------------

        public override void MouseDown(Point graphPoint, MouseButtons button) {
            if (button == MouseButtons.Left) {
                _dragStartMouseLocation = graphPoint;
                _dragStartElementLocation = new Point(X, Y);
                _dragStartWidth = Width;
                _dragStartHeight = Height;
                _dragStarted = false;
                // Only activate a handle if the annotation is already selected
                _activeHandle = IsSelected ? GetHandleAtPoint(graphPoint) : HandleType.None;

                // Only claim the mouse if already selected ? this lets rubber-band
                // selection start when clicking inside an unselected annotation
                if (IsSelected)
                    graphViewer.MouseDownElement = this;
            }
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            _dragStarted = false;
            _activeHandle = HandleType.None;

            if (!wasDragged && button == MouseButtons.Right) {
                RightClickMenu.Items.Clear();

                RightClickMenu.Items.Add(new ToolStripMenuItem("Properties", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        ShowPropertiesDialog();
                    })));
                RightClickMenu.Items.Add(new ToolStripSeparator());

                bool inSelection = graphViewer.SelectedAnnotations.Any(a => a == this)
                    && (graphViewer.SelectedNodes.Count > 0 || graphViewer.SelectedAnnotations.Count > 1);
                RightClickMenu.Items.Add(new ToolStripMenuItem(inSelection ? "Delete selection" : "Delete", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        if (inSelection)
                            graphViewer.TryDeleteSelection();
                        else {
                            graphViewer.RemoveAnnotationElement(this);
                        }
                    })));

                RightClickMenu.Show(graphViewer, graphViewer.GraphToScreen(graphPoint));
            }
        }

        public override void Dragged(Point graphPoint) {
            // First call after minDragDiff is confirmed ? skip to avoid snap
            if (!_dragStarted) {
                _dragStarted = true;
                return;
            }

            if (_activeHandle == HandleType.None) {
                // Move ? offset from original click position
                var offset = Point.Subtract(graphPoint, (Size)_dragStartMouseLocation);
                X = _dragStartElementLocation.X + offset.X;
                Y = _dragStartElementLocation.Y + offset.Y;
            } else {
                // Resize
                ApplyResize(graphPoint);
            }
        }

        public void CancelMouseCapture() {
            _dragStarted = false;
            _activeHandle = HandleType.None;
        }

        // ----------------------------------------------------------------
        // Resize math
        // ----------------------------------------------------------------

        private void ApplyResize(Point mouseGraph) {
            // Compute movement delta from the original click point
            int dx = mouseGraph.X - _dragStartMouseLocation.X;
            int dy = mouseGraph.Y - _dragStartMouseLocation.Y;

            // Snapshot edges (graph coordinates)
            int startLeft = _dragStartElementLocation.X - _dragStartWidth / 2;
            int startRight = _dragStartElementLocation.X + _dragStartWidth / 2;
            int startTop = _dragStartElementLocation.Y - _dragStartHeight / 2;
            int startBottom = _dragStartElementLocation.Y + _dragStartHeight / 2;

            int newLeft = startLeft;
            int newRight = startRight;
            int newTop = startTop;
            int newBottom = startBottom;

            switch (_activeHandle) {
                case HandleType.TopLeft:
                    newLeft = startLeft + dx;
                    newTop = startTop + dy;
                    break;
                case HandleType.TopCenter:
                    newTop = startTop + dy;
                    break;
                case HandleType.TopRight:
                    newRight = startRight + dx;
                    newTop = startTop + dy;
                    break;
                case HandleType.MiddleLeft:
                    newLeft = startLeft + dx;
                    break;
                case HandleType.MiddleRight:
                    newRight = startRight + dx;
                    break;
                case HandleType.BottomLeft:
                    newLeft = startLeft + dx;
                    newBottom = startBottom + dy;
                    break;
                case HandleType.BottomCenter:
                    newBottom = startBottom + dy;
                    break;
                case HandleType.BottomRight:
                    newRight = startRight + dx;
                    newBottom = startBottom + dy;
                    break;
            }

            // Enforce minimum width ? clamp the moving edge
            if (newRight - newLeft < MinAnnotationSize) {
                bool movingLeft = _activeHandle == HandleType.TopLeft ||
                                  _activeHandle == HandleType.MiddleLeft ||
                                  _activeHandle == HandleType.BottomLeft;
                if (movingLeft)
                    newLeft = newRight - MinAnnotationSize;
                else
                    newRight = newLeft + MinAnnotationSize;
            }

            // Enforce minimum height ? clamp the moving edge
            if (newBottom - newTop < MinAnnotationSize) {
                bool movingTop = _activeHandle == HandleType.TopLeft ||
                                 _activeHandle == HandleType.TopCenter ||
                                 _activeHandle == HandleType.TopRight;
                if (movingTop)
                    newTop = newBottom - MinAnnotationSize;
                else
                    newBottom = newTop + MinAnnotationSize;
            }

            Width = newRight - newLeft;
            Height = newBottom - newTop;
            X = newLeft + Width / 2;
            Y = newTop + Height / 2;
            OnResized();
        }

        /// <summary>Called after bounds change from a resize-handle drag.</summary>
        protected virtual void OnResized() { }

        // ----------------------------------------------------------------
        // Resize handle geometry
        // ----------------------------------------------------------------

        // Visual size ? small and unobtrusive
        private int GetHandleDrawHalfSize() {
            float elementCap = Math.Min(Width, Height) / 5f;
            return (int)Math.Max(3f, Math.Min(HandleDrawScreenPx / graphViewer.ViewScale, Math.Max(elementCap, 4f)));
        }

        // Hit-test size ? much larger so handles are easy to click
        private int GetHandleHitHalfSize() {
            float elementCap = Math.Min(Width, Height) / 4f;
            return (int)Math.Max(5f, Math.Min(HandleHitScreenPx / graphViewer.ViewScale, Math.Max(elementCap, 6f)));
        }

        private Rectangle GetHandleRect(HandleType handle, int half) {
            int size = half * 2;

            // Anchor points in graph space
            int cx = X, cy = Y;
            int left = cx - Width / 2;
            int right = cx + Width / 2;
            int top = cy - Height / 2;
            int bottom = cy + Height / 2;

            switch (handle) {
                case HandleType.TopLeft:
                    return new Rectangle(left - half, top - half, size, size);
                case HandleType.TopCenter:
                    return new Rectangle(cx - half, top - half, size, size);
                case HandleType.TopRight:
                    return new Rectangle(right - half, top - half, size, size);
                case HandleType.MiddleLeft:
                    return new Rectangle(left - half, cy - half, size, size);
                case HandleType.MiddleRight:
                    return new Rectangle(right - half, cy - half, size, size);
                case HandleType.BottomLeft:
                    return new Rectangle(left - half, bottom - half, size, size);
                case HandleType.BottomCenter:
                    return new Rectangle(cx - half, bottom - half, size, size);
                case HandleType.BottomRight:
                    return new Rectangle(right - half, bottom - half, size, size);
                default:
                    return Rectangle.Empty;
            }
        }

        protected HandleType GetHandleAtPoint(Point graphPoint) {
            if (!IsSelected)
                return HandleType.None;

            HandleType[] handles = [
                HandleType.TopLeft,    HandleType.TopCenter,    HandleType.TopRight,
                HandleType.MiddleLeft, HandleType.MiddleRight,
                HandleType.BottomLeft, HandleType.BottomCenter, HandleType.BottomRight
            ];

            int hitHalf = GetHandleHitHalfSize();
            foreach (HandleType handle in handles)
                if (GetHandleRect(handle, hitHalf).Contains(graphPoint))
                    return handle;

            return HandleType.None;
        }

        // ----------------------------------------------------------------
        // Drawing helpers for subclasses
        // ----------------------------------------------------------------

        /// <summary>
        /// Returns the bounding rectangle in graph coordinates (top-left + size),
        /// ready to pass directly to GDI drawing calls.
        /// </summary>
        protected Rectangle GetGraphRect() {
            Point topLeft = LocalToGraph(new Point(-Width / 2, -Height / 2));
            return new Rectangle(topLeft.X, topLeft.Y, Width, Height);
        }

        /// <summary>
        /// Draws the selection highlight border around graphRect.
        /// Call first inside subclass Draw() so the highlight sits behind the shape.
        /// </summary>
        protected void DrawSelectionHighlight(Graphics graphics, Rectangle graphRect) {
            if (!IsSelected)
                return;

            float pw = SelectionHighlightScreenPx / graphViewer.ViewScale;
            using var p = new Pen(SelectionHighlightColor, pw);
            graphics.DrawRectangle(p,
                graphRect.X - pw,
                graphRect.Y - pw,
                graphRect.Width + pw * 2,
                graphRect.Height + pw * 2);
        }

        /// <summary>
        /// Draws the 8 resize handles when the annotation is selected.
        /// Call at the end of each subclass Draw() method.
        /// </summary>
        protected void DrawResizeHandles(Graphics graphics) {
            if (!IsSelected)
                return;

            float penWidth = Math.Max(0.5f, 1f / graphViewer.ViewScale);

            using var fill = new SolidBrush(Color.White);
            using var border = new Pen(Color.FromArgb(60, 100, 200), penWidth);
            HandleType[] handles = [
                    HandleType.TopLeft,    HandleType.TopCenter,    HandleType.TopRight,
                    HandleType.MiddleLeft, HandleType.MiddleRight,
                    HandleType.BottomLeft, HandleType.BottomCenter, HandleType.BottomRight
                ];

            int drawHalf = GetHandleDrawHalfSize();
            foreach (HandleType handle in handles) {
                Rectangle r = GetHandleRect(handle, drawHalf);
                graphics.FillRectangle(fill, r);
                graphics.DrawRectangle(border, r);
            }
        }

        // ----------------------------------------------------------------
        // Public accessor for the graph viewer (used by property forms)
        // ----------------------------------------------------------------

        /// <summary>Exposes the graph viewer so property forms can do live updates.</summary>
        public ProductionGraphViewer GraphViewer => graphViewer;

        // ----------------------------------------------------------------
        // Abstract interface
        // ----------------------------------------------------------------

        public abstract AnnotationSaveData ToSaveData();
        public abstract void ShowPropertiesDialog();

        public static AnnotationElement FromSaveData(AnnotationSaveData data, ProductionGraphViewer graphViewer) =>
            data switch {
                TextAnnotationSaveData text => TextAnnotationElement.FromSaveData(text, graphViewer),
                ShapeAnnotationSaveData shape => ShapeAnnotationElement.FromSaveData(shape, graphViewer),
                _ => throw new InvalidOperationException("Unknown annotation type in save: " + data.Type)
            };

        protected static ColorSaveData ColorToSave(Color c) => new(c.A, c.R, c.G, c.B);

        protected static Color ColorFromSave(ColorSaveData c) => Color.FromArgb(c.A, c.R, c.G, c.B);

        /// <summary>
        /// Returns true if the lasso rectangle intersects the EDGE of this annotation ?
        /// i.e. overlaps the bounding box but is not entirely contained within it.
        /// This prevents a lasso drawn entirely inside a shape from selecting it.
        /// </summary>
        public bool LassoIntersectsEdge(Rectangle lasso) {
            // Compute annotation bounds in graph space
            int annLeft = X - Width / 2;
            int annRight = X + Width / 2;
            int annTop = Y - Height / 2;
            int annBottom = Y + Height / 2;

            // Must overlap the annotation at all
            bool overlaps = lasso.Right > annLeft &&
                            lasso.Left < annRight &&
                            lasso.Bottom > annTop &&
                            lasso.Top < annBottom;

            if (!overlaps)
                return false;

            // Lasso is entirely inside the annotation ? don't select
            bool lassoInsideAnnotation = lasso.Left >= annLeft &&
                                         lasso.Right <= annRight &&
                                         lasso.Top >= annTop &&
                                         lasso.Bottom <= annBottom;

            return !lassoInsideAnnotation;
        }

        public bool ContainsPointFull(Point graphPoint) {
            return Visible && Bounds.Contains(GraphToLocal(graphPoint));
        }

        /// <summary>Hit-test for mouse picking. When selected, includes resize handles drawn outside bounds.</summary>
        public bool PickAtPoint(Point graphPoint) =>
            IsSelected ? ContainsPoint(graphPoint) : ContainsPointFull(graphPoint);

        /// <summary>Forces this annotation to be visible regardless of bounds check.
        /// Used during full-graph export so off-graph annotations aren't clipped.</summary>
        public void ForceVisible() { Visible = true; }

        /// <summary>
        /// Returns the appropriate cursor when the mouse is at the given graph point,
        /// or null if this annotation doesn't own that point.
        /// </summary>
        public Cursor? GetCursorForPoint(Point graphPoint) {
            if (!Visible)
                return null;

            if (IsSelected) {
                switch (GetHandleAtPoint(graphPoint)) {
                    case HandleType.TopLeft:
                    case HandleType.BottomRight:
                        return Cursors.SizeNWSE;
                    case HandleType.TopRight:
                    case HandleType.BottomLeft:
                        return Cursors.SizeNESW;
                    case HandleType.TopCenter:
                    case HandleType.BottomCenter:
                        return Cursors.SizeNS;
                    case HandleType.MiddleLeft:
                    case HandleType.MiddleRight:
                        return Cursors.SizeWE;
                }

                // Inside selected annotation ? move cursor
                if (Bounds.Contains(GraphToLocal(graphPoint)))
                    return Cursors.SizeAll;
            }

            // Edge band (shapes) or full bounds (text) ? move cursor
            return ContainsPoint(graphPoint) ? Cursors.SizeAll : null;
        }
    }
}
