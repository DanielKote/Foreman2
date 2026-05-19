using Foreman;
using Foreman.Controls;
using Foreman.Serialization;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    public class ChooserLayoutTests : ForemanTestBase {
        [TestMethod]
        public void GroupIconSizeForCell_MatchesDesignRatioAtFullCell() {
            Assert.AreEqual(64, ChooserLayout.GroupIconSizeForCell(40, 64, 24));
        }

        [TestMethod]
        public void GroupIconSizeForCell_ScalesDownWithCell() {
            Assert.AreEqual(32, ChooserLayout.GroupIconSizeForCell(20, 64, 24));
        }

        [TestMethod]
        public void GroupIconSizeForCell_ClampsToMinimum() {
            Assert.AreEqual(24, ChooserLayout.GroupIconSizeForCell(10, 64, 24));
        }

        [TestMethod]
        public void GroupIconSizeForCell_DoesNotExceedDesignGroup() {
            Assert.AreEqual(64, ChooserLayout.GroupIconSizeForCell(100, 64, 24));
        }

        [TestMethod]
        public void ChooserIconGrid_ScrollBarWidth_MatchesSystemVerticalScrollbarWidth() =>
            StaTest.Run(ChooserIconGrid_ScrollBarWidth_MatchesSystemVerticalScrollbarWidth_Impl);

        [TestMethod]
        public void ChooserIconGrid_ScrollBarWidth_UnchangedWhenParentFlowPanelIsWider() =>
            StaTest.Run(ChooserIconGrid_ScrollBarWidth_UnchangedWhenParentFlowPanelIsWider_Impl);

        private static void ChooserIconGrid_ScrollBarWidth_MatchesSystemVerticalScrollbarWidth_Impl() {
            int systemWidth = SystemInformation.VerticalScrollBarWidth;
            using var grid = new ChooserIconGrid();
            grid.CreateControl();
            grid.ApplyLayout(
                availableGridHeight: ChooserIconGrid.VisibleRowCount * 40,
                maxLayoutWidth: ChooserIconGrid.ColumnCount * 40 + systemWidth,
                designCellSize: 40,
                minCellSize: 18,
                scrollbarWidth: systemWidth);

            Assert.AreEqual(systemWidth, grid.ScrollBar.Width,
                "Chooser scrollbar must match the standard system vertical scrollbar width.");
            Assert.AreEqual(systemWidth, grid.Width - grid.TargetCellSize * ChooserIconGrid.ColumnCount,
                "Layout must reserve exactly the system scrollbar width beside the icon grid.");
        }

        private static void ChooserIconGrid_ScrollBarWidth_UnchangedWhenParentFlowPanelIsWider_Impl() {
            int systemWidth = SystemInformation.VerticalScrollBarWidth;
            using var flow = new FlowLayoutPanel {
                FlowDirection = FlowDirection.TopDown,
                Size = new Size(600, 400),
                WrapContents = false,
            };
            using var grid = new ChooserIconGrid();
            flow.Controls.Add(grid);
            grid.CreateControl();
            const int cell = 40;
            int gridOuter = cell * ChooserIconGrid.ColumnCount + systemWidth;
            grid.ApplyLayout(
                ChooserIconGrid.VisibleRowCount * cell,
                gridOuter,
                cell,
                18,
                systemWidth);
            flow.CreateControl();
            flow.PerformLayout();
            grid.SetBounds(0, 0, 600, grid.Height, BoundsSpecified.Width);

            Assert.AreEqual(systemWidth, grid.ScrollBar.Width,
                "FlowLayoutPanel must not widen the scrollbar when the parent row is wider than the grid.");
            Assert.AreEqual(gridOuter, grid.Width,
                "Icon grid control width must stay grid plus scrollbar, not stretch to the flow panel width.");
        }

        [TestMethod]
        public void ItemChooser_ScrollBarWidth_MatchesSystemWidthAfterShow() =>
            StaTest.Run(ItemChooser_ScrollBarWidth_MatchesSystemWidthAfterShow_Impl);

        private static void ItemChooser_ScrollBarWidth_MatchesSystemWidthAfterShow_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = new ProductionGraphViewer {
                DCache = ctx.Cache,
                Size = new Size(1200, 800),
            };
            viewer.ApplySaveUi(new GraphViewerUiSaveData { ViewOffset = Point.Empty, ViewScale = 1f }, ctx.Cache, setEnablesFromJson: false);

            viewer.AddItem(new Point(10, 10), new Point(200, 150));
            ItemChooserPanel? chooser = viewer.Controls.OfType<ItemChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            ChooserIconGrid iconGrid = GetIconGrid(chooser);
            int systemWidth = SystemInformation.VerticalScrollBarWidth;
            Assert.AreEqual(systemWidth, iconGrid.ScrollBar.Width,
                "Live item chooser scrollbar must use the system vertical scrollbar width, not scaled layout slack.");
            Assert.IsLessThanOrEqualTo(systemWidth + 2, iconGrid.ScrollBar.Width,
                "Scrollbar must not be wider than the system metric (was previously DPI-scaled or stretched by flow layout).");
        }

        private static ChooserIconGrid GetIconGrid(IRChooserPanel chooser) {
            FieldInfo? field = typeof(IRChooserPanel).GetField("iconGrid", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "IRChooserPanel.iconGrid field should exist.");
            return (ChooserIconGrid)field.GetValue(chooser)!;
        }

        [TestMethod]
        public void ItemChooser_NoRightDeadSpaceWhenViewerShrinks() =>
            StaTest.Run(ItemChooser_NoRightDeadSpaceWhenViewerShrinks_Impl);

        private static void ItemChooser_NoRightDeadSpaceWhenViewerShrinks_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = new ProductionGraphViewer {
                DCache = ctx.Cache,
                Size = new Size(1200, 800),
            };
            viewer.ApplySaveUi(new GraphViewerUiSaveData { ViewOffset = Point.Empty, ViewScale = 1f }, ctx.Cache, setEnablesFromJson: false);

            viewer.AddItem(new Point(10, 10), new Point(200, 150));
            ItemChooserPanel? chooser = viewer.Controls.OfType<ItemChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            (int Width, int Height)[] viewerSizes = [(1200, 800), (700, 700), (500, 500), (400, 400), (320, 350), (280, 300), (240, 280)];
            foreach ((int viewerWidth, int viewerHeight) in viewerSizes) {
                viewer.Size = new Size(viewerWidth, viewerHeight);
                viewer.PerformLayout();
                chooser.PerformLayout();

                FlowLayoutPanel contentStack = GetContentStack(chooser);
                ChooserIconGrid iconGrid = GetIconGrid(chooser);
                int deadSpace = MeasureChooserRightDeadSpace(chooser);
                int gapBesideGrid = MeasureGapRightOfIconGrid(contentStack, iconGrid);

                Assert.IsLessThanOrEqualTo(2, deadSpace,
                    $"At viewer {viewerWidth}x{viewerHeight}, chooser had {deadSpace}px black bar past content " +
                    $"(panel {chooser.Width}, stack {contentStack.Width}, grid {iconGrid.Width}).");
                Assert.IsLessThanOrEqualTo(2, gapBesideGrid,
                    $"At viewer {viewerWidth}x{viewerHeight}, {gapBesideGrid}px black bar sat to the right of the icon grid " +
                    $"(stack {contentStack.Width}, grid right {iconGrid.Right}, header/min width mismatch).");
                Assert.IsLessThanOrEqualTo(2, chooser.Width - contentStack.Width,
                    "Panel width should match the content stack.");
                Assert.IsLessThanOrEqualTo(2, contentStack.Width - iconGrid.Width,
                    "Content stack width should match the icon grid; extra width becomes a black strip beside the cells.");
                Assert.IsLessThanOrEqualTo(chooser.Width, chooser.MinimumSize.Width,
                    $"MinimumSize.Width ({chooser.MinimumSize.Width}) must not exceed actual width ({chooser.Width}).");
            }
        }

        private static int MeasureGapRightOfIconGrid(FlowLayoutPanel contentStack, ChooserIconGrid iconGrid) {
            if (!iconGrid.Visible)
                return 0;
            int widestChromeRight = contentStack.Controls.Cast<Control>()
                .Where(c => c.Visible && c != iconGrid)
                .Select(c => c.Right)
                .DefaultIfEmpty(0)
                .Max();
            return Math.Max(0, Math.Max(widestChromeRight, contentStack.Width) - iconGrid.Right);
        }

        private static int MeasureChooserRightDeadSpace(IRChooserPanel chooser) {
            FlowLayoutPanel contentStack = GetContentStack(chooser);
            int usedRight = contentStack.Controls.Cast<Control>()
                .Where(c => c.Visible)
                .Select(c => c.Right)
                .DefaultIfEmpty(0)
                .Max();
            return Math.Max(0, chooser.ClientSize.Width - usedRight);
        }

        private static FlowLayoutPanel GetContentStack(IRChooserPanel chooser) {
            FieldInfo? field = typeof(IRChooserPanel).GetField("contentStack", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "IRChooserPanel.contentStack field should exist.");
            return (FlowLayoutPanel)field.GetValue(chooser)!;
        }

        [TestMethod]
        public void ChooserIconGrid_ApplyLayout_SizesGridToCellCount() =>
            StaTest.Run(ChooserIconGrid_ApplyLayout_SizesGridToCellCount_Impl);

        private static void ChooserIconGrid_ApplyLayout_SizesGridToCellCount_Impl() {
            using var grid = new ChooserIconGrid();
            int scrollbar = SystemInformation.VerticalScrollBarWidth;
            int outerWidth = grid.ApplyLayout(
                availableGridHeight: ChooserIconGrid.VisibleRowCount * 40,
                maxLayoutWidth: ChooserIconGrid.ColumnCount * 40 + scrollbar,
                designCellSize: 40,
                minCellSize: 18,
                scrollbarWidth: scrollbar);

            Assert.AreEqual(40, grid.TargetCellSize);
            Assert.AreEqual(40 * ChooserIconGrid.VisibleRowCount, grid.Height);
            Assert.AreEqual(40 * ChooserIconGrid.ColumnCount + scrollbar, grid.Width);
            Assert.AreEqual(outerWidth, grid.Width);
            Assert.AreEqual(40, grid.Buttons.ElementAt(0).ElementAt(0).Width);
            Assert.AreEqual(40, grid.Buttons.ElementAt(0).ElementAt(0).Height);
            Assert.IsGreaterThanOrEqualTo(grid.Width - scrollbar, grid.ScrollBar.Left);
        }

        [TestMethod]
        public void ChooserIconGrid_ApplyLayout_ShrinksWhenHeightLimited() =>
            StaTest.Run(ChooserIconGrid_ApplyLayout_ShrinksWhenHeightLimited_Impl);

        private static void ChooserIconGrid_ApplyLayout_ShrinksWhenHeightLimited_Impl() {
            using var grid = new ChooserIconGrid();
            int scrollbar = SystemInformation.VerticalScrollBarWidth;
            const int cell = 20;
            grid.ApplyLayout(
                availableGridHeight: ChooserIconGrid.VisibleRowCount * cell,
                maxLayoutWidth: 500,
                designCellSize: 40,
                minCellSize: 18,
                scrollbarWidth: scrollbar);

            Assert.AreEqual(cell, grid.TargetCellSize);
            Assert.AreEqual(cell * ChooserIconGrid.VisibleRowCount, grid.Height);
            Assert.AreEqual(cell * ChooserIconGrid.ColumnCount + scrollbar, grid.Width);
        }
    }
}
