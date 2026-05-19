using Foreman.ProductionGraphView;
using System;

namespace Foreman {
    public partial class EditFlowPanel {
        private EventHandler? viewerResizeHandler;

        public void ApplyViewportBounds() {
            EditPanelViewportLayout.Apply(this, RateOptionsTable, myGraphViewer);
            AttachViewerResizeHandler();
        }

        private void AttachViewerResizeHandler() {
            if (viewerResizeHandler != null)
                return;
            viewerResizeHandler = (_, _) => {
                if (!IsDisposed && Visible)
                    EditPanelViewportLayout.Apply(this, RateOptionsTable, myGraphViewer);
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
