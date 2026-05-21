using Foreman.Controls;
using Foreman.ProductionGraphView;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class IRChooserPanel {
        private IReadOnlyCollection<IReadOnlyCollection<NFButton>> IRButtons => iconGrid.Buttons;
        private VScrollBar IRScrollBar => iconGrid.ScrollBar;

        private ScaledChooserMetrics scaledMetrics;
        private bool applyingViewerBounds;
        private bool refreshingViewerBounds;
        private Font? scaledFooterButtonFont;

        private readonly record struct ScaledChooserMetrics {
            public int DesignCell { get; init; }
            public int MinCell { get; init; }
            public int MinGroup { get; init; }
            public int DesignGroup { get; init; }
            public int DesignWidth { get; init; }
            public int MinGridHeight { get; init; }
            public int DesignGridHeight { get; init; }
            public int DesignFooterButton { get; init; }
            public int MinFooterButton { get; init; }
            public float DesignFooterFont { get; init; }
            public float MinFooterFont { get; init; }

            public int GroupSizeForCell(int cellSize) =>
                ChooserLayout.GroupIconSizeForCell(cellSize, DesignGroup, MinGroup);

            public int FooterButtonHeightForCell(int cellSize) =>
                ChooserLayout.FooterButtonHeightForCell(cellSize, DesignFooterButton, MinFooterButton);

            public float FooterButtonFontSizeForCell(int cellSize) =>
                ChooserLayout.FooterButtonFontSizeForCell(cellSize, DesignCell, DesignFooterFont, MinFooterFont);
        }

        private void ApplyDpiScaling() {
            scaledMetrics = new ScaledChooserMetrics {
                DesignCell = ChooserLayout.Scale(this, ChooserLayout.DesignCellPixels),
                MinCell = ChooserLayout.Scale(this, ChooserLayout.DesignMinCellPixels),
                MinGroup = ChooserLayout.Scale(this, ChooserLayout.DesignMinGroupIconPixels),
                DesignGroup = ChooserLayout.Scale(this, ChooserLayout.DesignGroupIconPixels),
                DesignWidth = ChooserLayout.Scale(this, ChooserLayout.DesignChooserWidth),
                MinGridHeight = ChooserLayout.Scale(this, ChooserLayout.DesignMinCellPixels) * ChooserIconGrid.VisibleRowCount,
                DesignGridHeight = ChooserLayout.Scale(this, ChooserLayout.DesignCellPixels) * ChooserIconGrid.VisibleRowCount,
                DesignFooterButton = ChooserLayout.Scale(this, ChooserLayout.DesignFooterButtonHeightPixels),
                MinFooterButton = ChooserLayout.Scale(this, ChooserLayout.DesignMinFooterButtonHeightPixels),
                DesignFooterFont = AddPassthroughButton.Font.Size,
                MinFooterFont = AddPassthroughButton.Font.Size * ChooserLayout.DesignMinFooterButtonFontSizePoints
                    / ChooserLayout.DesignFooterButtonFontSizePoints,
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

        private static int GetScrollbarWidth() => ChooserLayout.GetVerticalScrollbarWidth();

        private void ApplyGroupLayout(int groupButtonSize) {
            foreach (Control control in groupsPanel.Controls) {
                if (control is NFButton groupButton)
                    groupButton.Size = new Size(groupButtonSize, groupButtonSize);
            }
            groupsPanel.PerformLayout();
        }

        private int MeasureHeaderFooterHeight(int rowWidth) {
            if (!headerStack.Visible)
                return 0;
            SetFlowRowSize(headerStack, rowWidth);
            return headerStack.Height;
        }

        private void LayoutGroupsPanelSize(int width, int maxHeight = int.MaxValue) {
            groupsPanel.AutoSize = false;
            groupsPanel.WrapContents = true;
            if (!groupsPanel.Visible || groupsPanel.Controls.Count == 0) {
                groupsPanel.AutoScroll = false;
                groupsPanel.Size = new Size(Math.Max(1, width), 0);
                return;
            }
            groupsPanel.Width = Math.Max(1, width);
            groupsPanel.PerformLayout();

            int contentBottom = groupsPanel.Padding.Top;
            foreach (Control control in groupsPanel.Controls) {
                if (control.Visible)
                    contentBottom = Math.Max(contentBottom, control.Bottom);
            }

            int height = Math.Max(1, contentBottom + groupsPanel.Padding.Bottom);
            if (height > maxHeight) {
                groupsPanel.AutoScroll = true;
                height = Math.Max(1, maxHeight);
            } else {
                groupsPanel.AutoScroll = false;
            }
            groupsPanel.Size = new Size(Math.Max(1, width), height);
        }

        private int ResolveFooterButtonHeight(in ScaledChooserMetrics metrics) {
            if (iconGrid.Visible && iconGrid.TargetCellSize > 0)
                return metrics.FooterButtonHeightForCell(iconGrid.TargetCellSize);
            return metrics.DesignFooterButton;
        }

        private float ResolveFooterButtonFontSize(in ScaledChooserMetrics metrics) {
            if (iconGrid.Visible && iconGrid.TargetCellSize > 0)
                return metrics.FooterButtonFontSizeForCell(iconGrid.TargetCellSize);
            return metrics.DesignFooterFont;
        }

        private Font GetOrCreateFooterButtonFont(float fontSize) {
            if (scaledFooterButtonFont != null && Math.Abs(scaledFooterButtonFont.Size - fontSize) < 0.01f)
                return scaledFooterButtonFont;
            Font designFont = AddPassthroughButton.Font;
            scaledFooterButtonFont?.Dispose();
            scaledFooterButtonFont = new Font(designFont.FontFamily, fontSize, designFont.Style, GraphicsUnit.Point);
            return scaledFooterButtonFont;
        }

        private void DisposeScaledFooterButtonFont() {
            scaledFooterButtonFont?.Dispose();
            scaledFooterButtonFont = null;
        }

        private void ApplyFooterChromeLayout(int panelWidth, in ScaledChooserMetrics metrics) {
            int footerButtonHeight = ResolveFooterButtonHeight(metrics);
            if (footerButtonHeight <= 0)
                return;
            float footerFont = ResolveFooterButtonFontSize(metrics);
            int buttonLayoutWidth = ResolveFooterButtonLayoutWidth(panelWidth, footerButtonHeight, footerFont);
            LayoutFooterRows(panelWidth, buttonLayoutWidth, footerButtonHeight, footerFont);
        }

        /// <summary>Re-apply footer and icon-grid band after WinForms perform-layout.</summary>
        private void SyncFooterButtonsToGridCell() {
            if (PGViewer == null || iconGrid.TargetCellSize < 1)
                return;
            const int margin = EditPanelScreenLayout.DefaultMargin;
            int maxWidth = Math.Max(1, PGViewer.ClientSize.Width - margin * 2);
            ScaledChooserMetrics metrics = GetScaledMetrics();
            // Use TargetCellSize directly; ResolveFooter* can still reflect design metrics mid-layout.
            int cell = iconGrid.TargetCellSize;
            int footerHeight = metrics.FooterButtonHeightForCell(cell);
            float footerFont = metrics.FooterButtonFontSizeForCell(cell);
            int panelWidth = ResolvePanelContentWidth(maxWidth, metrics);
            int buttonLayoutWidth = ResolveFooterButtonLayoutWidth(panelWidth, footerHeight, footerFont);
            SuspendLayout();
            contentStack.SuspendLayout();
            try {
                LayoutFooterRows(panelWidth, buttonLayoutWidth, footerHeight, footerFont);
                LayoutIconGridBand(panelWidth);
            } finally {
                contentStack.ResumeLayout(performLayout: false);
                ResumeLayout(performLayout: false);
            }
        }

        private static int MeasureFooterButtonNaturalWidth(Control button, Font font, int buttonHeight) {
            if (button is not Button textButton)
                return Math.Max(1, button.Width);
            const int horizontalChrome = 16;
            Size text = TextRenderer.MeasureText(
                textButton.Text,
                font,
                new Size(int.MaxValue, Math.Max(1, buttonHeight)),
                TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding);
            return Math.Max(1, text.Width + horizontalChrome + button.Margin.Horizontal);
        }

        private int MeasureFooterButtonsNaturalWidth(int buttonHeight, float fontSize) {
            Font font = GetOrCreateFooterButtonFont(fontSize);
            int width = 0;
            foreach (FlowLayoutPanel row in new[] { nodeOptionsRowA, nodeOptionsRowB }) {
                if (!row.Visible)
                    continue;
                int rowWidth = row.Padding.Horizontal;
                foreach (Control button in row.Controls.Cast<Control>().Where(c => c.Visible))
                    rowWidth += MeasureFooterButtonNaturalWidth(button, font, buttonHeight);
                width = Math.Max(width, rowWidth);
            }
            return width;
        }

        private int ResolveFooterButtonLayoutWidth(int panelWidth, int buttonHeight, float fontSize) {
            int natural = MeasureFooterButtonsNaturalWidth(buttonHeight, fontSize);
            if (!iconGrid.Visible)
                return Math.Min(panelWidth, natural);
            return Math.Min(panelWidth, Math.Max(iconGrid.Width, natural));
        }

        private void LayoutNodeOptionsRow(
            FlowLayoutPanel row, int panelRowWidth, int buttonLayoutWidth, int buttonHeight, float fontSize) {
            row.AutoSize = false;
            row.AutoSizeMode = AutoSizeMode.GrowOnly;
            row.WrapContents = false;
            if (!row.Visible) {
                row.Size = new Size(Math.Max(1, panelRowWidth), 0);
                return;
            }

            var buttons = row.Controls.Cast<Control>().Where(c => c.Visible).ToList();
            if (buttons.Count == 0) {
                row.Size = new Size(Math.Max(1, panelRowWidth), 0);
                return;
            }

            Font font = GetOrCreateFooterButtonFont(fontSize);
            var naturalWidths = buttons.Select(b => MeasureFooterButtonNaturalWidth(b, font, buttonHeight)).ToList();
            int totalMargin = buttons.Sum(b => b.Margin.Horizontal);
            int naturalTotal = naturalWidths.Sum() + totalMargin;
            int innerWidth = Math.Max(1, panelRowWidth - row.Padding.Horizontal);
            int clusterWidth = Math.Min(innerWidth, Math.Max(naturalTotal, buttonLayoutWidth));
            int extra = Math.Max(0, innerWidth - clusterWidth);
            int x = row.Padding.Left + extra / 2;

            row.SuspendLayout();
            try {
                for (int i = 0; i < buttons.Count; i++) {
                    Control button = buttons[i];
                    int width = naturalTotal > innerWidth
                        ? Math.Max(1, (innerWidth - totalMargin) / buttons.Count)
                        : naturalWidths[i];
                    button.AutoSize = false;
                    button.Font = font;
                    var buttonSize = new Size(width, buttonHeight);
                    button.MinimumSize = buttonSize;
                    button.MaximumSize = buttonSize;
                    button.Size = buttonSize;
                    button.Location = new Point(x, row.Padding.Top);
                    x += width + button.Margin.Horizontal;
                }

                int rowHeight = Math.Max(1, buttonHeight + row.Padding.Vertical);
                row.Size = new Size(Math.Max(1, panelRowWidth), rowHeight);
            } finally {
                row.ResumeLayout(performLayout: false);
            }
        }

        private void LayoutFooterRows(int panelRowWidth, int buttonLayoutWidth, int buttonHeight, float fontSize) {
            if (nodeOptionsRowA.Visible)
                LayoutNodeOptionsRow(nodeOptionsRowA, panelRowWidth, buttonLayoutWidth, buttonHeight, fontSize);
            if (nodeOptionsRowB.Visible)
                LayoutNodeOptionsRow(nodeOptionsRowB, panelRowWidth, buttonLayoutWidth, buttonHeight, fontSize);
        }

        private void LayoutIconGridBand(int panelWidth) {
            if (!iconGrid.Visible) {
                iconGridBand.Visible = false;
                iconGridBand.Size = Size.Empty;
                return;
            }
            iconGridBand.Visible = true;
            int bandWidth = Math.Max(iconGrid.Width, panelWidth);
            iconGridBand.Size = new Size(bandWidth, iconGrid.Height);
            iconGrid.Location = new Point(Math.Max(0, (bandWidth - iconGrid.Width) / 2), 0);
        }

        private int MeasureFooterChromeHeight(int panelRowWidth, int buttonHeight, float fontSize) {
            int buttonLayoutWidth = ResolveFooterButtonLayoutWidth(panelRowWidth, buttonHeight, fontSize);
            LayoutFooterRows(panelRowWidth, buttonLayoutWidth, buttonHeight, fontSize);
            int height = 0;
            if (nodeOptionsRowA.Visible)
                height += nodeOptionsRowA.Height;
            if (nodeOptionsRowB.Visible)
                height += nodeOptionsRowB.Height;
            return height;
        }

        private int ResolveGroupsPanelMaxHeight(int maxPanelHeight, int rowWidth, in ScaledChooserMetrics metrics) {
            if (!groupsPanel.Visible)
                return int.MaxValue;
            if (headerStack.Visible)
                SetFlowRowSize(headerStack, rowWidth);
            int footerButtonHeight = ResolveFooterButtonHeight(metrics);
            float footerFont = ResolveFooterButtonFontSize(metrics);
            int chrome = MeasureHeaderFooterHeight(rowWidth) + MeasureFooterChromeHeight(rowWidth, footerButtonHeight, footerFont);
            return Math.Max(metrics.MinGroup, maxPanelHeight - chrome - metrics.MinGridHeight);
        }

        private int MeasureGroupsPanelHeight(int layoutWidth, int groupSize) {
            if (!groupsPanel.Visible || groupsPanel.Controls.Count == 0)
                return 0;
            ApplyGroupLayout(groupSize);
            LayoutGroupsPanelSize(layoutWidth);
            return groupsPanel.Height;
        }

        private int MeasureLaidOutChromeHeight(int maxWidth, int groupSize, int maxPanelHeight, in ScaledChooserMetrics metrics) {
            ApplyGroupLayout(groupSize);
            SyncChromeRowWidths(ResolvePanelContentWidth(maxWidth, metrics), groupSize, maxPanelHeight, metrics);
            return SumVisibleHeights(headerStack, groupsPanel, nodeOptionsRowA, nodeOptionsRowB);
        }

        private int MeasureMinimumPanelHeight(int layoutWidth, int restoreGroupSize, in ScaledChooserMetrics metrics) {
            ApplyGroupLayout(metrics.MinGroup);
            try {
                return MeasureHeaderFooterHeight(layoutWidth) + MeasureGroupsPanelHeight(layoutWidth, metrics.MinGroup);
            } finally {
                ApplyGroupLayout(restoreGroupSize);
            }
        }

        private static int MeasureFlowRowNaturalWidth(FlowLayoutPanel row) {
            if (!row.Visible)
                return 0;
            row.PerformLayout();
            int width = row.Padding.Horizontal;
            foreach (Control control in row.Controls.Cast<Control>().Where(c => c.Visible)) {
                Size pref = control.AutoSize ? control.PreferredSize : control.Size;
                width += pref.Width + control.Margin.Horizontal;
            }
            return width;
        }

        private int MeasureHeaderIntrinsicMinWidth() {
            if (!headerStack.Visible)
                return 0;
            int width = headerStack.Padding.Horizontal;
            foreach (Control child in headerStack.Controls) {
                if (!child.Visible)
                    continue;
                if (child is FlowLayoutPanel row)
                    width = Math.Max(width, MeasureFlowRowNaturalWidth(row));
                else
                    width = Math.Max(width, child.Width + child.Margin.Horizontal);
            }
            return width;
        }

        private int MeasureLaidOutPanelWidth() {
            int width = 0;
            if (headerStack.Visible) {
                foreach (Control row in headerStack.Controls) {
                    if (!row.Visible)
                        continue;
                    foreach (Control control in row.Controls) {
                        if (!control.Visible)
                            continue;
                        width = Math.Max(width, control.Right + row.Padding.Right);
                    }
                }
                width += headerStack.Padding.Horizontal;
            }
            if (iconGrid.Visible)
                width = Math.Max(width, iconGridBand.Width);
            foreach (FlowLayoutPanel row in new[] { nodeOptionsRowA, nodeOptionsRowB }) {
                if (!row.Visible)
                    continue;
                width = Math.Max(width, row.Width);
            }
            return width;
        }

        private int MeasureContentWidth(int maxWidth = int.MaxValue, in ScaledChooserMetrics metrics = default) {
            int width = iconGrid.Visible ? iconGrid.Width : 0;
            if (metrics.DesignCell > 0) {
                int footerHeight = ResolveFooterButtonHeight(metrics);
                float footerFont = ResolveFooterButtonFontSize(metrics);
                width = Math.Max(width, MeasureFooterButtonsNaturalWidth(footerHeight, footerFont));
            }
            width = Math.Max(width, MeasureHeaderIntrinsicMinWidth());
            return Math.Min(maxWidth, width);
        }

        private int ResolvePanelContentWidth(int maxWidth, in ScaledChooserMetrics metrics) {
            int width = MeasureContentWidth(maxWidth, metrics);
            if (IsHandleCreated && headerStack.Visible)
                width = Math.Max(width, MeasureLaidOutPanelWidth());
            return Math.Min(maxWidth, width);
        }

        private static void SetFlowRowSize(FlowLayoutPanel row, int width) {
            row.AutoSize = false;
            Size pref = row.GetPreferredSize(new Size(Math.Max(1, width), 0));
            row.Size = new Size(width, Math.Max(1, pref.Height));
        }

        private void SyncChromeRowWidths(int contentWidth, int groupSize, int maxPanelHeight = int.MaxValue, in ScaledChooserMetrics metrics = default) {
            if (contentWidth < 1)
                return;
            ApplyGroupLayout(groupSize);
            if (headerStack.Visible)
                SetFlowRowSize(headerStack, contentWidth);
            if (groupsPanel.Visible) {
                int maxGroupsHeight = metrics.DesignCell > 0
                    ? ResolveGroupsPanelMaxHeight(maxPanelHeight, contentWidth, metrics)
                    : int.MaxValue;
                LayoutGroupsPanelSize(contentWidth, maxGroupsHeight);
            }
            if (iconGrid.Visible)
                LayoutIconGridBand(contentWidth);
            ApplyFooterChromeLayout(contentWidth, metrics);
        }

        private static int SumVisibleHeights(params Control[] rows) {
            int height = 0;
            foreach (Control row in rows) {
                if (row.Visible)
                    height += row.Height;
            }
            return height;
        }

        private Size MeasureContentSize(ref int groupSize, int maxWidth, int maxPanelHeight, in ScaledChooserMetrics metrics) {
            ApplyGroupLayout(groupSize);
            SyncChromeRowWidths(ResolvePanelContentWidth(maxWidth, metrics), groupSize, maxPanelHeight, metrics);
            int height = SumVisibleHeights(headerStack, groupsPanel, iconGridBand, nodeOptionsRowA, nodeOptionsRowB);
            return new Size(ResolvePanelContentWidth(maxWidth, metrics), height);
        }

        private void WidenGridToMinimumWidthIfPossible(int maxHeight, int maxWidth, ref int groupSize, in ScaledChooserMetrics metrics) {
            if (!iconGrid.Visible)
                return;
            int minGridOuter = ChooserLayout.DesignMinVisibleRows * metrics.MinCell + GetScrollbarWidth();
            if (minGridOuter <= iconGrid.Width || minGridOuter > maxWidth)
                return;
            int gridHeight = AvailableGridHeight(maxHeight, minGridOuter, groupSize, metrics);
            int outer = FitGridAndWidth(maxWidth, gridHeight, minGridOuter, metrics);
            if (outer > iconGrid.Width)
                groupSize = metrics.GroupSizeForCell(iconGrid.TargetCellSize);
        }

        private int AvailableGridHeight(int maxPanelHeight, int maxWidth, int groupSize, in ScaledChooserMetrics metrics) {
            int chrome = MeasureLaidOutChromeHeight(maxWidth, groupSize, maxPanelHeight, metrics);
            return Math.Max(1, Math.Min(metrics.DesignGridHeight, Math.Max(1, maxPanelHeight - chrome)));
        }

        /// <summary>Largest group icon size that still leaves room for at least a minimal recipe grid.</summary>
        private int ResolveMaxGroupSizeForHeight(int maxHeight, int maxWidth, in ScaledChooserMetrics metrics) {
            for (int size = metrics.DesignGroup; size >= metrics.MinGroup; size--) {
                if (MeasureLaidOutChromeHeight(maxWidth, size, maxHeight, metrics) + metrics.MinGridHeight <= maxHeight)
                    return size;
            }
            return metrics.MinGroup;
        }

        private static int CapGroupSize(int groupSize, int maxGroupSize) => Math.Min(groupSize, maxGroupSize);

        private int FitGridAndWidth(int maxWidth, int gridHeight, int minOuterWidth, in ScaledChooserMetrics metrics) =>
            iconGrid.ApplyLayout(
                gridHeight, maxWidth, metrics.DesignCell, metrics.MinCell, GetScrollbarWidth(), minOuterWidth);

        private (int width, int groupSize) ReflowGridAndTieGroupSize(
            int maxHeight, int maxWidth, int width, int groupSize, in ScaledChooserMetrics metrics) {
            int gridHeight = AvailableGridHeight(maxHeight, width, groupSize, metrics);
            width = FitGridAndWidth(maxWidth, gridHeight, width, metrics);
            return (width, metrics.GroupSizeForCell(iconGrid.TargetCellSize));
        }

        /// <summary>Binary-search grid height so total panel height fits <paramref name="maxHeight"/> (no viewport scroll).</summary>
        private Size FitPanelHeightToViewer(
            int maxHeight, int maxWidth, ref int groupSize, in ScaledChooserMetrics metrics) {
            if (!iconGrid.Visible)
                return MeasureContentSize(ref groupSize, maxWidth, maxHeight, metrics);

            int layoutWidth = ResolvePanelContentWidth(maxWidth, metrics);
            int maxGroupSize = ResolveMaxGroupSizeForHeight(maxHeight, maxWidth, metrics);
            groupSize = CapGroupSize(groupSize, maxGroupSize);
            ApplyGroupLayout(groupSize);

            int lo = 1;
            int hi = AvailableGridHeight(maxHeight, maxWidth, groupSize, metrics);
            int bestGrid = Math.Min(hi, metrics.MinGridHeight);
            Size bestSize = MeasureContentSize(ref groupSize, maxWidth, maxHeight, metrics);

            while (lo <= hi) {
                int mid = lo + (hi - lo + 1) / 2;
                FitGridAndWidth(maxWidth, mid, layoutWidth, metrics);
                int nextGroup = CapGroupSize(metrics.GroupSizeForCell(iconGrid.TargetCellSize), maxGroupSize);
                groupSize = nextGroup;
                ApplyGroupLayout(groupSize);
                layoutWidth = ResolvePanelContentWidth(maxWidth, metrics);
                Size candidate = MeasureContentSize(ref groupSize, maxWidth, maxHeight, metrics);
                if (candidate.Height <= maxHeight) {
                    bestGrid = mid;
                    bestSize = candidate;
                    lo = mid + 1;
                } else {
                    hi = mid - 1;
                }
            }

            FitGridAndWidth(maxWidth, bestGrid, layoutWidth, metrics);
            groupSize = CapGroupSize(metrics.GroupSizeForCell(iconGrid.TargetCellSize), maxGroupSize);
            ApplyGroupLayout(groupSize);
            layoutWidth = ResolvePanelContentWidth(maxWidth, metrics);
            bestSize = MeasureContentSize(ref groupSize, maxWidth, maxHeight, metrics);

            for (int trim = 0; trim < 32 && bestSize.Height > maxHeight; trim++) {
                int chromeHeight = MeasureLaidOutChromeHeight(maxWidth, groupSize, maxHeight, metrics);
                if (iconGrid.Height > 1) {
                    layoutWidth = ResolvePanelContentWidth(maxWidth, metrics);
                    int gridHeight = Math.Max(1, maxHeight - chromeHeight - trim);
                    FitGridAndWidth(maxWidth, gridHeight, layoutWidth, metrics);
                    groupSize = CapGroupSize(metrics.GroupSizeForCell(iconGrid.TargetCellSize), maxGroupSize);
                } else if (groupSize > metrics.MinGroup) {
                    maxGroupSize = Math.Max(metrics.MinGroup, maxGroupSize - 1);
                    groupSize = maxGroupSize;
                    ApplyGroupLayout(groupSize);
                } else {
                    break;
                }
                Size next = MeasureContentSize(ref groupSize, maxWidth, maxHeight, metrics);
                if (next.Height >= bestSize.Height)
                    break;
                bestSize = next;
            }

            if (bestSize.Height > maxHeight) {
                int chrome = MeasureLaidOutChromeHeight(maxWidth, groupSize, maxHeight, metrics);
                FitGridAndWidth(maxWidth, Math.Max(1, maxHeight - chrome), iconGrid.Width, metrics);
                groupSize = CapGroupSize(metrics.GroupSizeForCell(iconGrid.TargetCellSize), ResolveMaxGroupSizeForHeight(maxHeight, maxWidth, metrics));
                ApplyGroupLayout(groupSize);
                bestSize = MeasureContentSize(ref groupSize, maxWidth, maxHeight, metrics);
            }

            return bestSize;
        }

        private void UnwrapViewportScrollHost() {
            Panel? scrollHost = Controls.Find(EditPanelViewportLayout.ScrollHostName, false).OfType<Panel>().FirstOrDefault();
            if (scrollHost == null)
                return;
            scrollHost.Controls.Remove(contentStack);
            Controls.Remove(scrollHost);
            contentStack.Dock = DockStyle.None;
            contentStack.Location = Point.Empty;
            contentStack.Margin = Padding.Empty;
            Controls.Add(contentStack);
            contentStack.BringToFront();
            scrollHost.Dispose();
        }

        private int MeasureChromeUsedWidth() {
            int width = 0;
            FlowLayoutPanel[] rows = [headerStack, groupsPanel, nodeOptionsRowA, nodeOptionsRowB];
            foreach (FlowLayoutPanel row in rows) {
                if (!row.Visible)
                    continue;
                row.PerformLayout();
                width = Math.Max(width, row.GetPreferredSize(Size.Empty).Width);
            }
            return width;
        }

        /// <summary>Single sizing pipeline: fit content to the viewport and return the measured stack size.</summary>
        private Size LayoutContentForViewport(
            int maxWidth,
            int maxHeight,
            ref int groupSize,
            in ScaledChooserMetrics metrics) {
            UnwrapViewportScrollHost();

            Size contentSize = FitPanelHeightToViewer(maxHeight, maxWidth, ref groupSize, metrics);
            CommitContentStackLayout(ref contentSize, ref groupSize, maxWidth, maxHeight, metrics);

            if (contentSize.Height > maxHeight) {
                contentSize = FitPanelHeightToViewer(maxHeight, maxWidth, ref groupSize, metrics);
                CommitContentStackLayout(ref contentSize, ref groupSize, maxWidth, maxHeight, metrics);
            }

            FinalizePanelChromeLayout(ref contentSize, ref groupSize, maxWidth, maxHeight, metrics);
            ShrinkPanelToFitViewport(ref contentSize, maxHeight, maxWidth, ref groupSize, metrics);
            AlignPanelWidthToChrome(ref contentSize, maxHeight, maxWidth, ref groupSize, metrics);
            if (contentSize.Height > maxHeight)
                ShrinkPanelToFitViewport(ref contentSize, maxHeight, maxWidth, ref groupSize, metrics);

            return MeasureContentSizeFromLayout(ref groupSize, maxWidth, maxHeight, metrics);
        }

        private void ApplyContentStackSize(Size contentSize) {
            contentStack.Size = contentSize;
            Size = contentSize;
            MaximumSize = contentSize;
        }

        private void ApplyPanelDimensions(Size contentSize, int minWidth, int minHeight, int maxHeight) {
            int cappedMinWidth = Math.Min(minWidth, contentSize.Width);
            int cappedMinHeight = Math.Min(minHeight, Math.Min(contentSize.Height, maxHeight));
            AutoSize = false;
            MinimumSize = new Size(cappedMinWidth, cappedMinHeight);
            ApplyContentStackSize(contentSize);
        }

        private Rectangle ComputeChooserScreenBounds(Size panelSize, int margin) {
            int viewerWidth = PGViewer!.ClientSize.Width;
            int viewerHeight = PGViewer.ClientSize.Height;
            Point topLeft = EditPanelScreenLayout.GetChooserTopLeft(
                desiredScreenOrigin, panelSize, viewerWidth, viewerHeight, margin);
            return EditPanelScreenLayout.ClampRectToViewer(
                new Rectangle(topLeft, panelSize), viewerWidth, viewerHeight, margin);
        }

        private void ApplyChooserScreenBounds(Rectangle bounds) {
            SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.All);
        }

        /// <summary>WinForms can report taller children after PerformLayout than the pre-layout sum.</summary>
        private void ReconcileLayoutAfterPerformLayout(
            int minWidth,
            int minHeight,
            int maxWidth,
            int maxHeight,
            int margin,
            ref int groupSize,
            in ScaledChooserMetrics metrics) {
            int actualHeight = SumLaidOutContentHeight();
            Size contentSize = Size;
            if (actualHeight > maxHeight) {
                contentSize = LayoutContentForViewport(maxWidth, maxHeight, ref groupSize, metrics);
                ApplyPanelDimensions(contentSize, minWidth, minHeight, maxHeight);
            } else if (actualHeight > 0 && Math.Abs(actualHeight - Size.Height) > 1) {
                contentSize = new Size(Size.Width, actualHeight);
                ApplyPanelDimensions(contentSize, minWidth, minHeight, maxHeight);
                SyncChromeRowWidths(ResolvePanelContentWidth(maxWidth, metrics), groupSize, maxHeight, metrics);
            }

            Rectangle bounds = ComputeChooserScreenBounds(contentSize, margin);
            if (Bounds != bounds)
                ApplyChooserScreenBounds(bounds);

            int viewerWidth = PGViewer!.ClientSize.Width;
            int viewerHeight = PGViewer.ClientSize.Height;
            if (!EditPanelScreenLayout.FitsViewer(Bounds, viewerWidth, viewerHeight, margin))
                ApplyChooserScreenBounds(ComputeChooserScreenBounds(Size, margin));

            SyncFooterButtonsToGridCell();
        }

        private void ApplyViewerBounds() {
            if (applyingViewerBounds || PGViewer == null)
                return;
            if (!PGViewer.IsHandleCreated)
                PGViewer.CreateControl();
            applyingViewerBounds = true;
            const int margin = EditPanelScreenLayout.DefaultMargin;
            int maxHeight = Math.Max(1, PGViewer.ClientSize.Height - margin * 2);
            int maxWidth = Math.Max(1, PGViewer.ClientSize.Width - margin * 2);
            ScaledChooserMetrics metrics = GetScaledMetrics();
            int groupSize = metrics.DesignGroup;
            int minWidth = 0;
            int minHeight = 0;
            SuspendLayout();
            contentStack.SuspendLayout();
            try {
                int width = Math.Min(metrics.DesignWidth, maxWidth);

                for (int pass = 0; pass < 8; pass++) {
                    int prevGroup = groupSize;
                    int prevWidth = width;
                    (width, groupSize) = ReflowGridAndTieGroupSize(maxHeight, maxWidth, width, groupSize, metrics);
                    if (pass > 0 && groupSize == prevGroup && width == prevWidth)
                        break;
                }

                minWidth = ComputeMinimumWidth(maxWidth, metrics);
                WidenGridToMinimumWidthIfPossible(maxHeight, maxWidth, ref groupSize, metrics);

                groupSize = Math.Min(groupSize, ResolveMaxGroupSizeForHeight(maxHeight, maxWidth, metrics));
                ApplyGroupLayout(groupSize);

                minHeight = MeasureMinimumPanelHeight(ResolvePanelContentWidth(maxWidth, metrics), groupSize, metrics) + metrics.MinGridHeight;
                Size contentSize = LayoutContentForViewport(maxWidth, maxHeight, ref groupSize, metrics);
                ApplyPanelDimensions(contentSize, minWidth, minHeight, maxHeight);
                ApplyChooserScreenBounds(ComputeChooserScreenBounds(contentSize, margin));
            } finally {
                contentStack.ResumeLayout(performLayout: false);
                ResumeLayout(performLayout: false);
                applyingViewerBounds = false;
            }

            PerformLayout();
            SyncFooterButtonsToGridCell();
            ReconcileLayoutAfterPerformLayout(minWidth, minHeight, maxWidth, maxHeight, margin, ref groupSize, metrics);
        }

        private int ComputeMinimumWidth(int maxWidth, in ScaledChooserMetrics metrics) {
            int minGridOuter = ChooserLayout.DesignMinVisibleRows * metrics.MinCell + GetScrollbarWidth();
            int headerAtMinGrid = MeasureHeaderIntrinsicMinWidth();
            int footerHeight = ResolveFooterButtonHeight(metrics);
            float footerFont = ResolveFooterButtonFontSize(metrics);
            int footerMin = MeasureFooterButtonsNaturalWidth(footerHeight, footerFont);
            return Math.Min(maxWidth, Math.Min(metrics.DesignWidth, Math.Max(minGridOuter, Math.Max(headerAtMinGrid, footerMin))));
        }

        private void AlignPanelWidthToChrome(ref Size contentSize, int maxHeight, int maxWidth, ref int groupSize, in ScaledChooserMetrics metrics) {
            SyncChromeRowWidths(ResolvePanelContentWidth(maxWidth, metrics), groupSize, maxHeight, metrics);
            contentSize = MeasureContentSizeFromLayout(ref groupSize, maxWidth, maxHeight, metrics);
            ApplyContentStackSize(contentSize);
        }

        /// <summary>Iteratively shrink chrome + grid until measured height fits the viewer (short windows).</summary>
        private void ShrinkPanelToFitViewport(ref Size contentSize, int maxHeight, int maxWidth, ref int groupSize, in ScaledChooserMetrics metrics) {
            int previousHeight = int.MaxValue;
            for (int attempt = 0; attempt < 32 && contentSize.Height > maxHeight; attempt++) {
                int rowWidth = ResolvePanelContentWidth(maxWidth, metrics);
                int maxGroup = ResolveMaxGroupSizeForHeight(maxHeight, maxWidth, metrics);
                groupSize = Math.Max(metrics.MinGroup, maxGroup - attempt / 4);
                ApplyGroupLayout(groupSize);

                int footerButtonHeight = ResolveFooterButtonHeight(metrics);
                float footerFont = ResolveFooterButtonFontSize(metrics);
                int chrome = MeasureHeaderFooterHeight(rowWidth) + MeasureFooterChromeHeight(rowWidth, footerButtonHeight, footerFont);
                if (groupsPanel.Visible) {
                    int groupsCap = Math.Max(metrics.MinGroup, maxHeight - chrome - metrics.MinGridHeight - attempt);
                    LayoutGroupsPanelSize(rowWidth, groupsCap);
                }

                chrome = MeasureLaidOutChromeHeight(maxWidth, groupSize, maxHeight, metrics);
                int gridBudget = Math.Max(1, maxHeight - chrome);
                FitGridAndWidth(maxWidth, gridBudget, iconGrid.Width, metrics);
                groupSize = CapGroupSize(metrics.GroupSizeForCell(iconGrid.TargetCellSize), maxGroup);
                ApplyGroupLayout(groupSize);

                SyncChromeRowWidths(rowWidth, groupSize, maxHeight, metrics);
                contentSize = MeasureContentSizeFromLayout(ref groupSize, maxWidth, maxHeight, metrics);

                if (contentSize.Height >= previousHeight)
                    break;
                previousHeight = contentSize.Height;
            }
        }

        /// <summary>Re-apply row heights after assigning panel Size (parent layout resets AutoSize chrome).</summary>
        private void FinalizePanelChromeLayout(ref Size contentSize, ref int groupSize, int maxWidth, int maxHeight, in ScaledChooserMetrics metrics) {
            SyncChromeRowWidths(ResolvePanelContentWidth(maxWidth, metrics), groupSize, maxHeight, metrics);
            contentSize = MeasureContentSizeFromLayout(ref groupSize, maxWidth, maxHeight, metrics);
            int rowWidth = Math.Max(contentSize.Width, ResolvePanelContentWidth(maxWidth, metrics));
            if (rowWidth > contentSize.Width) {
                SyncChromeRowWidths(rowWidth, groupSize, maxHeight, metrics);
                contentSize = new Size(rowWidth, contentSize.Height);
            }
            ApplyContentStackSize(contentSize);
        }

        /// <summary>Apply explicit row heights; contentStack.PerformLayout can reset AutoSize children to design metrics.</summary>
        private void CommitContentStackLayout(
            ref Size contentSize, ref int groupSize, int maxWidth, int maxHeight, in ScaledChooserMetrics metrics) {
            SyncChromeRowWidths(ResolvePanelContentWidth(maxWidth, metrics), groupSize, maxHeight, metrics);
            ApplyContentStackSize(contentSize);
            contentSize = MeasureContentSizeFromLayout(ref groupSize, maxWidth, maxHeight, metrics);
        }

        private int SumLaidOutContentHeight() =>
            SumVisibleHeights(headerStack, groupsPanel, iconGridBand, nodeOptionsRowA, nodeOptionsRowB);

        private Size MeasureContentSizeFromLayout(ref int groupSize, int maxWidth, int maxHeight, in ScaledChooserMetrics metrics) {
            int width = ResolvePanelContentWidth(maxWidth, metrics);
            SyncChromeRowWidths(width, groupSize, maxHeight, metrics);
            return new Size(width, SumLaidOutContentHeight());
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

            int designGroup = metrics.DesignGroup;
            Size contentSize = MeasureContentSize(ref designGroup, metrics.DesignWidth, int.MaxValue, metrics);
            ApplyFooterChromeLayout(contentSize.Width, metrics);
            contentSize = MeasureContentSizeFromLayout(ref designGroup, metrics.DesignWidth, int.MaxValue, metrics);
            ApplyContentStackSize(contentSize);
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
