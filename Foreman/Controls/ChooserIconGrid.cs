using Foreman.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    /// <summary>10×8 icon grid with a vertical scrollbar; lays out cells on resize (no nested table layouts).</summary>
    public sealed partial class ChooserIconGrid : Panel {
        public const int ColumnCount = 10;
        public const int VisibleRowCount = 8;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int TargetCellSize { get; private set; } = ChooserLayout.DesignCellPixels;

        private readonly NFButton[][] buttons;
        private readonly bool iconButtonsReady;
        private int laidOutOuterWidth;
        private int laidOutOuterHeight;

        public IReadOnlyCollection<IReadOnlyCollection<NFButton>> Buttons => buttons;
        public VScrollBar ScrollBar => scrollBar;

        public ChooserIconGrid() {
            InitializeComponent();
            buttons = new NFButton[ColumnCount][];
            for (var i = 0; i < buttons.Length; ++i)
                buttons[i] = new NFButton[VisibleRowCount];

            InitializeIconButtons();
            iconButtonsReady = true;
            if (IsInDesignMode)
                ApplyDesignLayout();
        }

        private static bool IsInDesignMode =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void InitializeIconButtons() {
            for (int row = 0; row < VisibleRowCount; row++) {
                for (int column = 0; column < ColumnCount; column++) {
                    var button = new NFButton {
                        BackgroundImageLayout = ImageLayout.Zoom,
                        UseVisualStyleBackColor = false,
                        FlatStyle = FlatStyle.Flat,
                        TabStop = false,
                        ForeColor = Color.Gray,
                        BackColor = Color.DimGray,
                        Margin = Padding.Empty,
                        Enabled = false,
                    };
                    button.FlatAppearance.BorderSize = 1;
                    button.BackgroundImageChanged += (sender, e) => _ = sender switch {
                        NFButton btn => btn.FlatAppearance.BorderSize = btn.BackgroundImage switch {
                            null => 1,
                            _ => 0,
                        },
                        _ => default,
                    };
                    buttons[column][row] = button;
                    gridSurface.Controls.Add(button);
                }
            }
        }

        /// <summary>Lays out the grid at 96 DPI design metrics (design surface and default size).</summary>
        public void ApplyDesignLayout() {
            ApplyLayout(
                ChooserLayout.DesignGridOuterHeight,
                ChooserLayout.DesignGridOuterWidth,
                ChooserLayout.DesignCellPixels,
                ChooserLayout.DesignMinCellPixels,
                ChooserLayout.GetVerticalScrollbarWidth());
        }

        public void WireMouseWheel(MouseEventHandler handler) => gridSurface.MouseWheel += handler;

        /// <summary>Size the grid to fit the allotted area; returns the outer width (cells + scrollbar).</summary>
        public int ApplyLayout(int availableGridHeight, int maxLayoutWidth, int designCellSize, int minCellSize, int scrollbarWidth, int minOuterWidth = 0) {
            int minGridHeight = minCellSize * VisibleRowCount;
            int cellByHeight = Math.Max(1, availableGridHeight / VisibleRowCount);
            int cellByWidth = Math.Max(1, (maxLayoutWidth - scrollbarWidth) / ColumnCount);
            int cell = Math.Min(designCellSize, Math.Min(cellByHeight, cellByWidth));
            if (availableGridHeight >= minGridHeight)
                cell = Math.Max(minCellSize, cell);
            else
                cell = Math.Max(1, cell);

            if (minOuterWidth > 0) {
                int cellForMinOuter = (int)Math.Ceiling((minOuterWidth - scrollbarWidth) / (double)ColumnCount);
                cellForMinOuter = Math.Max(minCellSize, Math.Min(designCellSize, cellForMinOuter));
                if (cellForMinOuter * ColumnCount + scrollbarWidth <= maxLayoutWidth) {
                    int cellByHeightCap = Math.Max(1, availableGridHeight / VisibleRowCount);
                    cell = Math.Max(cell, Math.Min(cellForMinOuter, cellByHeightCap));
                }
            }

            TargetCellSize = cell;
            int gridHeight = cell * VisibleRowCount;
            int gridWidth = cell * ColumnCount;

            SuspendLayout();
            int outerWidth = gridWidth + scrollbarWidth;
            laidOutOuterWidth = outerWidth;
            laidOutOuterHeight = gridHeight;
            Height = gridHeight;
            Width = outerWidth;
            MinimumSize = new Size(outerWidth, gridHeight);
            MaximumSize = new Size(outerWidth, gridHeight);

            ApplyCellGridBounds(gridWidth, gridHeight, scrollbarWidth);
            ResumeLayout(performLayout: false);
            return outerWidth;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
            if (iconButtonsReady && laidOutOuterWidth > 0) {
                if ((specified & BoundsSpecified.Width) != 0 && width != laidOutOuterWidth)
                    width = laidOutOuterWidth;
                if ((specified & BoundsSpecified.Height) != 0 && height != laidOutOuterHeight)
                    height = laidOutOuterHeight;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            if (DesignMode && TargetCellSize == ChooserLayout.DesignCellPixels)
                ApplyDesignLayout();
        }

        protected override void OnResize(EventArgs eventargs) {
            base.OnResize(eventargs);
            if (!iconButtonsReady || TargetCellSize < 1 || laidOutOuterWidth < 1)
                return;
            int gridWidth = TargetCellSize * ColumnCount;
            int gridHeight = TargetCellSize * VisibleRowCount;
            int scrollbarWidth = ChooserLayout.GetVerticalScrollbarWidth();
            ApplyCellGridBounds(gridWidth, gridHeight, scrollbarWidth);
        }

        private void ApplyCellGridBounds(int gridWidth, int gridHeight, int scrollbarWidth) {
            gridSurface.SetBounds(0, 0, gridWidth, gridHeight);
            scrollBar.MinimumSize = new Size(scrollbarWidth, 0);
            scrollBar.MaximumSize = new Size(scrollbarWidth, int.MaxValue);
            scrollBar.SetBounds(gridWidth, 0, scrollbarWidth, gridHeight);
            LayoutCells();
        }

        private void LayoutCells() {
            int cellSize = TargetCellSize;
            if (cellSize < 1)
                return;

            for (int row = 0; row < VisibleRowCount; row++) {
                for (int column = 0; column < ColumnCount; column++) {
                    var btn = buttons[column][row];
                    btn.Bounds = new Rectangle(column * cellSize, row * cellSize, cellSize, cellSize);
                }
            }
        }
    }
}
