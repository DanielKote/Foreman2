using Foreman.ProductionGraphView;
using System;

namespace Foreman {
    public partial class EditRecipePanel {
        private EventHandler? viewerResizeHandler;

        public void ApplyViewportBounds() {
            EditPanelViewportLayout.Apply(this, MainTable, myGraphViewer);
            AttachViewerResizeHandler();
        }

        private void RefreshViewportLayout() => ApplyViewportBounds();

        private void AttachViewerResizeHandler() {
            if (viewerResizeHandler != null)
                return;
            viewerResizeHandler = (_, _) => {
                if (!IsDisposed && Visible)
                    EditPanelViewportLayout.Apply(this, MainTable, myGraphViewer);
            };
            myGraphViewer.Resize += viewerResizeHandler;
            Disposed += DetachViewerResizeHandler;
        }

        private void DetachViewerResizeHandler(object? sender, EventArgs e) {
            if (viewerResizeHandler != null && myGraphViewer != null) {
                myGraphViewer.Resize -= viewerResizeHandler;
                viewerResizeHandler = null;
            }
        }
    }
}
