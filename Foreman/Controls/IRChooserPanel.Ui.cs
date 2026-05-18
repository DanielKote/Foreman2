using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    partial class IRChooserPanel {
        private NFButton[,] IRButtons => iconGrid.Buttons;
        private VScrollBar IRScrollBar => iconGrid.ScrollBar;

        private ScaledChooserMetrics scaledMetrics;
        private bool applyingViewerBounds;

        private readonly struct ScaledChooserMetrics {
            public int DesignCell { get; init; }
            public int MinCell { get; init; }
            public int MinGroup { get; init; }
            public int DesignGroup { get; init; }
            public int DesignWidth { get; init; }
            public int MinGridHeight { get; init; }
            public int DesignGridHeight { get; init; }

            public int GroupSizeForCell(int cellSize) =>
                ChooserLayout.GroupIconSizeForCell(cellSize, DesignGroup, MinGroup);
        }

        private void ApplyDpiScaling() {
            scaledMetrics = new ScaledChooserMetrics {
                DesignCell = ChooserLayout.Scale(this, ChooserLayout.DesignCellPixels),
                MinCell = ChooserLayout.Scale(this, ChooserLayout.DesignMinCellPixels),
                MinGroup = ChooserLayout.Scale(this, ChooserLayout.DesignMinGroupIconPixels),
                DesignGroup = ChooserLayout.Scale(this, ChooserLayout.DesignGroupIconPixels),
                DesignWidth = ChooserLayout.Scale(this, ChooserLayout.DesignChooserWidth),
                MinGridHeight = ChooserLayout.Scale(this, ChooserLayout.DesignMinCellPixels) * ChooserLayout.DesignMinVisibleRows,
                DesignGridHeight = ChooserLayout.Scale(this, ChooserLayout.DesignCellPixels) * ChooserIconGrid.VisibleRowCount,
            };

            FilterTextBox.Width = ChooserLayout.Scale(this, ChooserLayout.DesignFilterTextWidth);
            QualitySelector.Width = ChooserLayout.Scale(this, ChooserLayout.DesignQualityComboWidth);
            int itemIcon = ChooserLayout.Scale(this, ChooserLayout.DesignItemIconPixels);
            ItemIconPanel.Size = new Size(itemIcon, itemIcon);
        }

        private ScaledChooserMetrics GetScaledMetrics() {
            if (scaledMetrics.DesignCell > 0)
                return scaledMetrics;
            ApplyDpiScaling();
            return scaledMetrics;
        }

        private int GetScrollbarWidth() => ChooserLayout.GetVerticalScrollbarWidth();

        private void ApplyGroupLayout(int groupButtonSize) {
            foreach (Control control in groupsPanel.Controls) {
                if (control is NFButton groupButton)
                    groupButton.Size = new Size(groupButtonSize, groupButtonSize);
            }
            groupsPanel.PerformLayout();
        }

        private int MeasureHeaderFooterHeight() {
            int height = 0;
            headerStack.PerformLayout();
            if (headerStack.Visible) {
                Size header = headerStack.GetPreferredSize(Size.Empty);
                height += Math.Max(header.Height, headerStack.Height);
            }
            if (nodeOptionsRowA.Visible)
                height += nodeOptionsRowA.PreferredSize.Height;
            if (nodeOptionsRowB.Visible)
                height += nodeOptionsRowB.PreferredSize.Height;
            return height;
        }

        private int MeasureGroupsPanelHeight(int layoutWidth) {
            if (!groupsPanel.Visible || groupsPanel.Controls.Count == 0)
                return 0;
            groupsPanel.PerformLayout();
            return groupsPanel.GetPreferredSize(new Size(layoutWidth, 0)).Height;
        }

        private int MeasureChromeHeight(int layoutWidth, int groupSize) {
            ApplyGroupLayout(groupSize);
            return MeasureHeaderFooterHeight() + MeasureGroupsPanelHeight(layoutWidth);
        }

        private int MeasureMinimumPanelHeight(int layoutWidth, int restoreGroupSize, in ScaledChooserMetrics metrics) {
            ApplyGroupLayout(metrics.MinGroup);
            try {
                return MeasureHeaderFooterHeight() + MeasureGroupsPanelHeight(layoutWidth);
            } finally {
                ApplyGroupLayout(restoreGroupSize);
            }
        }

        private int MeasureHeaderIntrinsicMinWidth() {
            if (!headerStack.Visible)
                return 0;
            headerStack.PerformLayout();
            return headerStack.GetPreferredSize(Size.Empty).Width;
        }

        private int MeasureHeaderMinWidth(int layoutWidth) {
            if (!headerStack.Visible)
                return 0;
            headerStack.PerformLayout();
            return headerStack.GetPreferredSize(new Size(Math.Max(1, layoutWidth), 0)).Width;
        }

        private int MeasureContentWidth() {
            if (iconGrid.Visible)
                return iconGrid.Width;
            return MeasureHeaderIntrinsicMinWidth();
        }

        private static void SetFlowRowSize(FlowLayoutPanel row, int width) {
            row.AutoSize = false;
            Size pref = row.GetPreferredSize(new Size(Math.Max(1, width), 0));
            row.Size = new Size(width, Math.Max(1, pref.Height));
        }

        private void SyncChromeRowWidths(int contentWidth) {
            if (contentWidth < 1)
                return;
            FlowLayoutPanel[] rows = { headerStack, groupsPanel, nodeOptionsRowA, nodeOptionsRowB };
            foreach (FlowLayoutPanel row in rows) {
                if (row.Visible)
                    SetFlowRowSize(row, contentWidth);
            }
        }

        private static int SumVisibleHeights(params Control[] rows) {
            int height = 0;
            foreach (Control row in rows) {
                if (row.Visible)
                    height += row.Height;
            }
            return height;
        }

        private Size MeasureContentSize(int groupSize) {
            ApplyGroupLayout(groupSize);
            int width = MeasureContentWidth();
            SyncChromeRowWidths(width);
            int height = SumVisibleHeights(headerStack, groupsPanel, iconGrid, nodeOptionsRowA, nodeOptionsRowB);
            return new Size(width, height);
        }

        private void WidenGridToMinimumWidthIfPossible(int maxHeight, int maxWidth, int minWidth, ref int groupSize, in ScaledChooserMetrics metrics) {
            if (!iconGrid.Visible || minWidth <= iconGrid.Width || minWidth > maxWidth)
                return;
            int gridHeight = AvailableGridHeight(maxHeight, minWidth, groupSize, metrics);
            int outer = FitGridAndWidth(maxWidth, gridHeight, minWidth, metrics);
            if (outer > iconGrid.Width)
                groupSize = metrics.GroupSizeForCell(iconGrid.TargetCellSize);
        }

        private int AvailableGridHeight(int maxPanelHeight, int layoutWidth, int groupSize, in ScaledChooserMetrics metrics) {
            int chrome = MeasureChromeHeight(layoutWidth, groupSize);
            return Math.Max(metrics.MinGridHeight, Math.Min(metrics.DesignGridHeight, Math.Max(1, maxPanelHeight - chrome)));
        }

        private int FitGridAndWidth(int maxWidth, int gridHeight, int width, in ScaledChooserMetrics metrics) {
            int outerWidth = iconGrid.ApplyLayout(gridHeight, width, metrics.DesignCell, metrics.MinCell, GetScrollbarWidth());
            int headerMin = MeasureHeaderIntrinsicMinWidth();
            int maxOuter = Math.Min(metrics.DesignWidth, maxWidth);
            if (headerMin > outerWidth && headerMin <= maxOuter) {
                width = headerMin;
                outerWidth = iconGrid.ApplyLayout(gridHeight, width, metrics.DesignCell, metrics.MinCell, GetScrollbarWidth());
            }
            return outerWidth;
        }

        private (int width, int groupSize) ReflowGridAndTieGroupSize(
            int maxHeight, int maxWidth, int width, int groupSize, in ScaledChooserMetrics metrics) {
            int gridHeight = AvailableGridHeight(maxHeight, width, groupSize, metrics);
            width = FitGridAndWidth(maxWidth, gridHeight, width, metrics);
            return (width, metrics.GroupSizeForCell(iconGrid.TargetCellSize));
        }

        private void ExpandGridForHeaderIfNeeded(int maxHeight, int maxWidth, ref int groupSize, in ScaledChooserMetrics metrics) {
            if (!iconGrid.Visible)
                return;
            int headerMin = MeasureHeaderIntrinsicMinWidth();
            if (headerMin <= iconGrid.Width || headerMin > maxWidth)
                return;
            int gridHeight = AvailableGridHeight(maxHeight, headerMin, groupSize, metrics);
            int outer = FitGridAndWidth(maxWidth, gridHeight, headerMin, metrics);
            if (outer >= headerMin)
                groupSize = metrics.GroupSizeForCell(iconGrid.TargetCellSize);
        }

        private void ApplyViewerBounds() {
            if (applyingViewerBounds || PGViewer == null)
                return;
            applyingViewerBounds = true;
            try {
                const int margin = EditPanelScreenLayout.DefaultMargin;
                int maxHeight = Math.Max(1, PGViewer.ClientSize.Height - margin * 2);
                int maxWidth = Math.Max(1, PGViewer.ClientSize.Width - margin * 2);
                ScaledChooserMetrics metrics = GetScaledMetrics();
                int width = Math.Min(metrics.DesignWidth, maxWidth);
                int groupSize = metrics.DesignGroup;

                for (int pass = 0; pass < 8; pass++) {
                    int prevGroup = groupSize;
                    int prevWidth = width;
                    (width, groupSize) = ReflowGridAndTieGroupSize(maxHeight, maxWidth, width, groupSize, metrics);
                    if (pass > 0 && groupSize == prevGroup && width == prevWidth)
                        break;
                }

                int minWidth = ComputeMinimumWidth(maxWidth, metrics);
                WidenGridToMinimumWidthIfPossible(maxHeight, maxWidth, minWidth, ref groupSize, metrics);
                ExpandGridForHeaderIfNeeded(maxHeight, maxWidth, ref groupSize, metrics);

                Size contentSize = MeasureContentSize(groupSize);
                int minHeight = MeasureMinimumPanelHeight(contentSize.Width, groupSize, metrics) + metrics.MinGridHeight;

                ApplyTightPanelBounds(contentSize, minWidth, minHeight);

                Rectangle bounds = EditPanelScreenLayout.ClampRectToViewer(
                    new Rectangle(Location, Size), PGViewer.ClientSize.Width, PGViewer.ClientSize.Height, margin);
                Location = bounds.Location;
            } finally {
                applyingViewerBounds = false;
            }
        }

        private int ComputeMinimumWidth(int maxWidth, in ScaledChooserMetrics metrics) {
            int minGridOuter = ChooserLayout.DesignMinVisibleRows * metrics.MinCell + GetScrollbarWidth();
            int headerAtMinGrid = MeasureHeaderIntrinsicMinWidth();
            return Math.Min(maxWidth, Math.Min(metrics.DesignWidth, Math.Max(minGridOuter, headerAtMinGrid)));
        }

        private void ApplyTightPanelBounds(Size contentSize, int minWidth, int minHeight) {
            contentStack.Size = contentSize;
            contentStack.PerformLayout();

            contentSize = MeasureContentSizeFromLayout();
            int cappedMinWidth = Math.Min(minWidth, contentSize.Width);
            int cappedMinHeight = Math.Min(minHeight, contentSize.Height);

            contentStack.Size = contentSize;
            MinimumSize = new Size(cappedMinWidth, cappedMinHeight);
            Size = contentSize;
            MaximumSize = contentSize;
        }

        private Size MeasureContentSizeFromLayout() {
            int width = MeasureContentWidth();
            SyncChromeRowWidths(width);
            int height = SumVisibleHeights(headerStack, groupsPanel, iconGrid, nodeOptionsRowA, nodeOptionsRowB);
            return new Size(width, height);
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            if (DesignMode)
                ApplyDesignTimeLayout();
        }

        /// <summary>Approximates runtime chrome + grid sizing in the WinForms designer (no viewer required).</summary>
        private void ApplyDesignTimeLayout() {
            ApplyDpiScaling();
            ScaledChooserMetrics metrics = GetScaledMetrics();

            iconGrid.ApplyLayout(
                metrics.DesignGridHeight,
                metrics.DesignWidth,
                metrics.DesignCell,
                metrics.MinCell,
                ChooserLayout.GetVerticalScrollbarWidth());

            EnsureDesignTimeGroupPreview(metrics.DesignGroup);

            Size contentSize = MeasureContentSize(metrics.DesignGroup);
            contentStack.Size = contentSize;
            Size = contentSize;
            MinimumSize = contentSize;
        }

        private void EnsureDesignTimeGroupPreview(int groupSize) {
            if (!DesignMode || groupsPanel.Controls.Count > 0)
                return;

            groupsPanel.SuspendLayout();
            foreach (string label in new[] { "log", "cont", "inter", "prod", "sci" }) {
                var button = new NFButton {
                    Size = new Size(groupSize, groupSize),
                    Text = label,
                    ForeColor = Color.Gray,
                    BackColor = Color.DimGray,
                    FlatStyle = FlatStyle.Flat,
                    UseVisualStyleBackColor = false,
                    Margin = Padding.Empty,
                    Enabled = false,
                };
                button.FlatAppearance.BorderSize = 1;
                groupsPanel.Controls.Add(button);
                GroupButtons.Add(button);
            }
            groupsPanel.ResumeLayout(true);
        }
    }
}