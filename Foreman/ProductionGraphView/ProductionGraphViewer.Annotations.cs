using Foreman.DataCaching;
using Foreman.ProductionGraphView.Annotations;
using Foreman.ProductionGraphView.Elements;
using Foreman.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class ProductionGraphViewer {
        private static readonly Pen drawShapePen = new(Color.FromArgb(200, 60, 120, 220), 2) {
            DashStyle = DashStyle.Dash
        };

        private readonly List<AnnotationElement> annotationElements = [];
        private readonly HashSet<AnnotationElement> selectedAnnotations = [];
        private bool inDrawShapeMode;

        public IReadOnlyCollection<AnnotationElement> SelectedAnnotations => selectedAnnotations;

        private void ClearAnnotations() {
            foreach (AnnotationElement ann in annotationElements.ToList())
                ann.Dispose();
            annotationElements.Clear();
            selectedAnnotations.Clear();
            inDrawShapeMode = false;
        }

        public IReadOnlyList<AnnotationSaveData> GetAnnotationSaveData() =>
            [.. annotationElements.Select(a => a.ToSaveData())];

        /// <summary>Graph-space bounds for full-graph image export (nodes/links and annotations).</summary>
        public Rectangle GetExportBounds() =>
            GraphExportBounds.Compute(
                Graph.Bounds,
                annotationElements.Select(GraphExportBounds.GetGraphBounds));

        public void LoadAnnotationsFromSave(IReadOnlyList<AnnotationSaveData> annotations, int? savedDpi) {
            ClearAnnotations();
            if (annotations.Count == 0)
                return;

            float dpiScale = 1f;
            if (savedDpi is int dpi && dpi > 0)
                dpiScale = DeviceDpi / (float)dpi;

            foreach (AnnotationSaveData data in annotations) {
                AnnotationElement? ann = null;
                try {
                    if (TryCreateAnnotationFromSave(data, dpiScale, out ann))
                        AddAnnotationElement(ann);
                    ann = null;
                } finally {
                    ann?.Dispose();
                }
            }
        }

        private bool TryCreateAnnotationFromSave(
            AnnotationSaveData data,
            float dpiScale,
            [NotNullWhen(true)] out AnnotationElement? annotation) {
            annotation = null;
            try {
                annotation = AnnotationElement.FromSaveData(data, this);
                if (Math.Abs(dpiScale - 1f) > 0.01f && annotation is TextAnnotationElement text) {
                    text.Width = (int)Math.Round(text.Width * dpiScale);
                    text.Height = (int)Math.Round(text.Height * dpiScale);
                }
                return true;
            } catch (Exception ex) {
                ErrorLogging.LogLine($"Skipping bad annotation: {ex.Message}");
                return false;
            }
        }

        public AnnotationElement? GetAnnotationAtPoint(Point point) {
            for (int i = annotationElements.Count - 1; i >= 0; i--) {
                if (annotationElements[i].PickAtPoint(point))
                    return annotationElements[i];
            }
            return null;
        }

        public void AddAnnotationElement(AnnotationElement element) {
            annotationElements.Add(element);
            Invalidate();
        }

        public void RemoveAnnotationElement(AnnotationElement element) {
            annotationElements.Remove(element);
            selectedAnnotations.Remove(element);
            element.Dispose();
            Invalidate();
        }

        public void AddShapeAnnotation(Point graphPoint) {
            inDrawShapeMode = true;
            Cursor = Cursors.Cross;
            SelectionZoneOriginPoint = graphPoint;
            SelectionZone = new Rectangle();
            Invalidate();
        }

        public void AddTextAnnotation(Point graphPoint) {
            var element = new TextAnnotationElement(this, graphPoint) {
                IsSelected = true
            };
            AddAnnotationElement(element);

            using var form = new TextPropertiesForm(element);
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog(FindForm()) == DialogResult.OK) {
                ClearNodeAndAnnotationSelection(keepAnnotation: element);
                element.IsSelected = true;
                Cursor = Cursors.Default;
                Invalidate();
            } else {
                RemoveAnnotationElement(element);
            }
        }

        public void TryDeleteSelection() {
            int total = selectedNodes.Count + selectedAnnotations.Count;
            if (total == 0)
                return;

            if (total > 10
                && UserMessages.Show(
                    $"You are deleting {total} items.\nAre you sure?",
                    "Confirm delete.",
                    MessageBoxButtons.YesNo) != DialogResult.Yes) {
                return;
            }

            foreach (BaseNodeElement node in selectedNodes.ToList())
                Session.Editor.DeleteNode(node.ViewModel.Id);
            selectedNodes.Clear();

            foreach (AnnotationElement ann in selectedAnnotations.ToList())
                RemoveAnnotationElement(ann);
            selectedAnnotations.Clear();

            Graph.UpdateNodeValues();
        }

        private void ClearAnnotationSelection() {
            foreach (AnnotationElement ann in selectedAnnotations)
                ann.IsSelected = false;
            selectedAnnotations.Clear();
        }

        private void ClearNodeAndAnnotationSelection(AnnotationElement? keepAnnotation = null) {
            foreach (BaseNodeElement node in selectedNodes)
                node.Highlighted = false;
            selectedNodes.Clear();

            foreach (AnnotationElement ann in selectedAnnotations) {
                if (ann != keepAnnotation)
                    ann.IsSelected = false;
            }
            selectedAnnotations.Clear();
            if (keepAnnotation is not null)
                selectedAnnotations.Add(keepAnnotation);
        }

        private void CommitAnnotationLassoSelection() =>
            ApplyAnnotationZoneSelection(GetAnnotationsIntersectingLasso(), commit: true);

        private void UpdateAnnotationLassoPreview() =>
            ApplyAnnotationZoneSelection(GetAnnotationsIntersectingLasso(), commit: false);

        private HashSet<AnnotationElement> GetAnnotationsIntersectingLasso() =>
            [.. annotationElements.Where(a => a.LassoIntersectsEdge(SelectionZone))];

        private void ApplyAnnotationZoneSelection(HashSet<AnnotationElement> zoneAnnotations, bool commit) {
            if (AnnotationSelectionModifiers.IsRemoveFromSelection) {
                if (commit) {
                    foreach (AnnotationElement ann in selectedAnnotations.Where(zoneAnnotations.Contains).ToList()) {
                        ann.IsSelected = false;
                        selectedAnnotations.Remove(ann);
                    }
                } else {
                    foreach (AnnotationElement ann in annotationElements)
                        ann.IsSelected = selectedAnnotations.Contains(ann) && !zoneAnnotations.Contains(ann);
                }
                return;
            }

            if (AnnotationSelectionModifiers.IsAddToSelection) {
                foreach (AnnotationElement ann in annotationElements)
                    ann.IsSelected = selectedAnnotations.Contains(ann) || zoneAnnotations.Contains(ann);
                if (commit) {
                    foreach (AnnotationElement ann in zoneAnnotations)
                        selectedAnnotations.Add(ann);
                }
                return;
            }

            foreach (AnnotationElement ann in annotationElements)
                ann.IsSelected = zoneAnnotations.Contains(ann);
            if (commit) {
                ClearAnnotationSelection();
                foreach (AnnotationElement ann in zoneAnnotations) {
                    ann.IsSelected = true;
                    selectedAnnotations.Add(ann);
                }
            }
        }

        private void ImportAnnotationsAtOrigin(IReadOnlyList<AnnotationSaveData> annotations, Point origin) {
            if (annotations.Count == 0)
                return;

            List<AnnotationElement> imported = [];
            foreach (AnnotationSaveData data in annotations) {
                AnnotationElement? ann = null;
                try {
                    if (TryCreateAnnotationFromSave(data, dpiScale: 1f, out ann))
                        imported.Add(ann);
                    ann = null;
                } finally {
                    ann?.Dispose();
                }
            }
            if (imported.Count == 0)
                return;

            long xAve = imported.Sum(a => (long)a.X);
            long yAve = imported.Sum(a => (long)a.Y);
            xAve /= imported.Count;
            yAve /= imported.Count;
            Point offset = new(origin.X - (int)xAve, origin.Y - (int)yAve);

            foreach (AnnotationElement ann in imported) {
                ann.X += offset.X;
                ann.Y += offset.Y;
                ann.IsSelected = true;
                AddAnnotationElement(ann);
                selectedAnnotations.Add(ann);
            }
        }

        private void ImportAnnotationsWithOffset(IReadOnlyList<AnnotationSaveData> annotations, Size offset) {
            foreach (AnnotationSaveData data in annotations) {
                AnnotationElement? ann = null;
                try {
                    if (!TryCreateAnnotationFromSave(data, dpiScale: 1f, out ann))
                        continue;

                    ann.X += offset.Width;
                    ann.Y += offset.Height;
                    ann.IsSelected = true;
                    AddAnnotationElement(ann);
                    selectedAnnotations.Add(ann);
                    ann = null;
                } finally {
                    ann?.Dispose();
                }
            }
        }

        private bool Annotation_OnMouseDown(MouseEventArgs e, Point graph_location, ref GraphElement? clickedElement) {
            if (e.Clicks >= 2)
                return false;

            clickedElement ??= GetAnnotationAtPoint(graph_location);
            clickedElement?.MouseDown(graph_location, e.Button);
            return false;
        }

        private bool Annotation_OnMouseDownDoubleClick(MouseEventArgs e, GraphElement? clickedElement) {
            if (e.Clicks != 2 || e.Button != MouseButtons.Left || clickedElement is not AnnotationElement ann)
                return false;

            CancelAnnotationMouseCapture(ann);
            ann.ShowPropertiesDialog();
            CancelAnnotationMouseCapture(ann);
            return true;
        }

        private void CancelAnnotationMouseCapture(AnnotationElement ann) {
            ann.CancelMouseCapture();
            MouseDownElement = null;
            currentDragOperation = DragOperation.None;
            viewBeingDragged = false;
            downButtons &= ~MouseButtons.Left;
        }

        private bool Annotation_FinishDrawShape() {
            if (currentDragOperation != DragOperation.DrawShape)
                return false;

            const int minDrawSize = 30;
            if (SelectionZone.Width >= minDrawSize || SelectionZone.Height >= minDrawSize) {
                int w = Math.Max(SelectionZone.Width, minDrawSize);
                int h = Math.Max(SelectionZone.Height, minDrawSize);
                Point center = new(SelectionZone.Left + w / 2, SelectionZone.Top + h / 2);
                AddAnnotationElement(new ShapeAnnotationElement(this, center, w, h));
            } else {
                AddAnnotationElement(new ShapeAnnotationElement(this, SelectionZoneOriginPoint));
            }

            inDrawShapeMode = false;
            Cursor = Cursors.Default;
            SelectionZone = new Rectangle();
            return true;
        }

        private void Annotation_OnMouseUpLeft(GraphElement? element, bool viewBeingDragged) {
            if (currentDragOperation == DragOperation.None && MouseDownElement is AnnotationElement clickedAnnotation) {
                HandleMouseUpOnTrackedAnnotation(clickedAnnotation, viewBeingDragged);
                return;
            }

            if (currentDragOperation == DragOperation.None && MouseDownElement is null
                && element is AnnotationElement unselectedAnnotation && !viewBeingDragged) {
                SelectSingleAnnotation(unselectedAnnotation);
            }
        }

        private void HandleMouseUpOnTrackedAnnotation(AnnotationElement clickedAnnotation, bool viewBeingDragged) {
            if (AnnotationSelectionModifiers.IsRemoveFromSelection) {
                selectedAnnotations.Remove(clickedAnnotation);
                clickedAnnotation.IsSelected = false;
            } else if (AnnotationSelectionModifiers.IsAddToSelection) {
                if (clickedAnnotation.IsSelected)
                    selectedAnnotations.Remove(clickedAnnotation);
                else
                    selectedAnnotations.Add(clickedAnnotation);
                clickedAnnotation.IsSelected = !clickedAnnotation.IsSelected;
            } else if (!viewBeingDragged) {
                SelectSingleAnnotation(clickedAnnotation);
            }

            MouseDownElement = null;
            Invalidate();
        }

        private void SelectSingleAnnotation(AnnotationElement annotation) {
            if (AnnotationSelectionModifiers.IsAddToSelection) {
                selectedAnnotations.Add(annotation);
                annotation.IsSelected = true;
                return;
            }

            if (AnnotationSelectionModifiers.IsRemoveFromSelection)
                return;

            ClearNodeAndAnnotationSelection();
            selectedAnnotations.Add(annotation);
            annotation.IsSelected = true;
        }

        private void Annotation_AppendContextMenuItems(Point graph_location) {
            rightClickMenu.Items.Add(new ToolStripSeparator());
            rightClickMenu.Items.Add(new ToolStripMenuItem("Add Text", null, (_, _) => {
                rightClickMenu.Close();
                AddTextAnnotation(graph_location);
            }));
            rightClickMenu.Items.Add(new ToolStripMenuItem("Add Shape", null, (_, _) => {
                rightClickMenu.Close();
                AddShapeAnnotation(graph_location);
            }));
        }

        private DragOperation Annotation_ResolveDragOperation(DragOperation proposed) {
            return inDrawShapeMode && proposed == DragOperation.Selection ? DragOperation.DrawShape : proposed;
        }

        private void Annotation_OnItemDrag(Point graph_location) {
            if (MouseDownElement is not AnnotationElement draggedAnn)
                return;

            if (selectedAnnotations.Contains(draggedAnn)) {
                if (draggedAnn.IsResizing) {
                    draggedAnn.Dragged(graph_location);
                    return;
                }

                Point startPoint = draggedAnn.Location;
                draggedAnn.Dragged(graph_location);
                Point endPoint = draggedAnn.Location;
                if (startPoint == endPoint)
                    return;

                int dx = endPoint.X - startPoint.X;
                int dy = endPoint.Y - startPoint.Y;
                foreach (AnnotationElement ann in selectedAnnotations.Where(a => a != draggedAnn)) {
                    ann.X += dx;
                    ann.Y += dy;
                }
                foreach (BaseNodeElement node in selectedNodes)
                    node.SetLocation(new Point(node.X + dx, node.Y + dy));
                Invalidate();
                return;
            }

            draggedAnn.Dragged(graph_location);
        }

        private void Annotation_UpdateCursor(Point graph_location) {
            if (currentDragOperation == DragOperation.DrawShape
                || (inDrawShapeMode && currentDragOperation == DragOperation.None)) {
                Cursor = Cursors.Cross;
                return;
            }

            if (currentDragOperation != DragOperation.None || viewBeingDragged)
                return;

            Cursor = Cursors.Default;
            for (int i = annotationElements.Count - 1; i >= 0; i--) {
                Cursor? annCursor = annotationElements[i].GetCursorForPoint(graph_location);
                if (annCursor is not null) {
                    Cursor = annCursor;
                    return;
                }
            }
        }

        private void Annotation_OnKeyDown(KeyEventArgs e) {
            if (inDrawShapeMode && e.KeyCode == Keys.Escape) {
                inDrawShapeMode = false;
                currentDragOperation = DragOperation.None;
                Cursor = Cursors.Default;
                SelectionZone = new Rectangle();
                e.Handled = true;
            }
        }

        private void Annotation_OnDeleteKey() {
            if (selectedAnnotations.Count > 0)
                TryDeleteSelection();
        }

        private void Annotation_MoveSelection(int dx, int dy) {
            foreach (AnnotationElement ann in selectedAnnotations) {
                ann.X += dx;
                ann.Y += dy;
            }
        }
    }
}
