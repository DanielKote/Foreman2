using Foreman;
using Foreman.Controls;
using Foreman.ProductionGraphView;
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
        public void FooterButtonHeightForCell_MatchesDesignRatioAtFullCell() {
            Assert.AreEqual(38, ChooserLayout.FooterButtonHeightForCell(40, 38, 22));
        }

        [TestMethod]
        public void FooterButtonHeightForCell_ScalesDownWithCell() {
            Assert.AreEqual(28, ChooserLayout.FooterButtonHeightForCell(30, 38, 22));
        }

        [TestMethod]
        public void FooterButtonHeightForCell_ClampsToMinimum() {
            Assert.AreEqual(22, ChooserLayout.FooterButtonHeightForCell(10, 38, 22));
        }

        [TestMethod]
        public void FooterButtonHeightForCell_DoesNotExceedDesignHeight() {
            Assert.AreEqual(38, ChooserLayout.FooterButtonHeightForCell(100, 38, 22));
        }

        [TestMethod]
        public void FooterButtonFontSizeForCell_MatchesDesignRatioAtFullCell() {
            Assert.AreEqual(8.25f, ChooserLayout.FooterButtonFontSizeForCell(40, 40, 8.25f, 6f), 0.01f);
        }

        [TestMethod]
        public void FooterButtonFontSizeForCell_ScalesDownWithCell() {
            Assert.AreEqual(6.1875f, ChooserLayout.FooterButtonFontSizeForCell(30, 40, 8.25f, 6f), 0.01f);
        }

        [TestMethod]
        public void FooterButtonFontSizeForCell_ClampsToMinimum() {
            Assert.AreEqual(6f, ChooserLayout.FooterButtonFontSizeForCell(10, 40, 8.25f, 6f), 0.01f);
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
        public void ItemChooser_HeightFitsShortViewer() =>
            StaTest.Run(ItemChooser_HeightFitsShortViewer_Impl);

        private static void ItemChooser_HeightFitsShortViewer_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            const int margin = EditPanelScreenLayout.DefaultMargin;
            (int Width, int Height)[] viewerSizes = [(1280, 720), (1024, 600), (900, 550), (800, 500)];

            foreach ((int viewerWidth, int viewerHeight) in viewerSizes) {
                using var viewer = new ProductionGraphViewer {
                    DCache = ctx.Cache,
                    Size = new Size(viewerWidth, viewerHeight),
                };
                viewer.ApplySaveUi(new GraphViewerUiSaveData { ViewOffset = Point.Empty, ViewScale = 1f }, ctx.Cache, setEnablesFromJson: false);
                viewer.PerformLayout();
                viewer.AddItem(new Point(20, 20), new Point(200, 150));
                viewer.PerformLayout();
                ItemChooserPanel? chooser = viewer.Controls.OfType<ItemChooserPanel>().FirstOrDefault();
                Assert.IsNotNull(chooser);

                int maxPanelHeight = viewer.ClientSize.Height - margin * 2;
                Assert.IsLessThanOrEqualTo(maxPanelHeight, chooser.Height,
                    $"At viewer {viewerWidth}x{viewerHeight}, chooser height {chooser.Height} should fit within {maxPanelHeight}px.");
                Assert.IsTrue(EditPanelScreenLayout.FitsViewer(chooser.Bounds, viewer.ClientSize.Width, viewer.ClientSize.Height, margin),
                    $"Chooser at {chooser.Bounds} should be fully visible in viewer client area {viewer.ClientSize}.");
                Assert.IsNull(chooser.Controls.Find(EditPanelViewportLayout.ScrollHostName, false).FirstOrDefault(),
                    "Chooser should shrink rather than add a panel scrollbar.");
            }
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
                MethodInfo? refreshBounds = typeof(IRChooserPanel).GetMethod("RefreshViewerBounds", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(refreshBounds);
                refreshBounds.Invoke(chooser, null);

                FlowLayoutPanel contentStack = GetContentStack(chooser);
                ChooserIconGrid iconGrid = GetIconGrid(chooser);
                Panel iconGridBand = GetIconGridBand(chooser);
                int deadSpace = MeasureChooserRightDeadSpace(chooser);
                int gapBesideGrid = MeasureGapBesideIconGrid(iconGridBand, iconGrid);
                int gridOffsetInBand = iconGrid.Left;

                Assert.IsLessThanOrEqualTo(2, deadSpace,
                    $"At viewer {viewerWidth}x{viewerHeight}, chooser had {deadSpace}px unused space past content " +
                    $"(panel {chooser.Width}, stack {contentStack.Width}, grid {iconGrid.Width}).");
                Assert.IsLessThanOrEqualTo(2, Math.Abs(gapBesideGrid - gridOffsetInBand * 2),
                    $"At viewer {viewerWidth}x{viewerHeight}, icon grid should be centered in its dim-gray band " +
                    $"(band {iconGridBand.Width}px, grid {iconGrid.Width}px, left offset {gridOffsetInBand}px).");
                Assert.IsLessThanOrEqualTo(2, chooser.Width - contentStack.Width,
                    "Panel width should match the content stack.");
                Assert.AreEqual(Color.DimGray, iconGridBand.BackColor,
                    "Space beside the square grid should use the dim-gray band, not the panel black.");
                Assert.IsLessThanOrEqualTo(chooser.Width, chooser.MinimumSize.Width,
                    $"MinimumSize.Width ({chooser.MinimumSize.Width}) must not exceed actual width ({chooser.Width}).");
            }
        }

        private static int MeasureGapBesideIconGrid(Panel iconGridBand, ChooserIconGrid iconGrid) {
            if (!iconGrid.Visible || !iconGridBand.Visible)
                return 0;
            return Math.Max(0, iconGridBand.Width - iconGrid.Width);
        }

        private static Panel GetIconGridBand(IRChooserPanel chooser) {
            FieldInfo? field = typeof(IRChooserPanel).GetField("iconGridBand", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "IRChooserPanel.iconGridBand field should exist.");
            return (Panel)field.GetValue(chooser)!;
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
        public void ChooserIconGrid_ApplyLayout_RoundsUpToMinOuterWidth() =>
            StaTest.Run(ChooserIconGrid_ApplyLayout_RoundsUpToMinOuterWidth_Impl);

        private static void ChooserIconGrid_ApplyLayout_RoundsUpToMinOuterWidth_Impl() {
            using var grid = new ChooserIconGrid();
            int scrollbar = SystemInformation.VerticalScrollBarWidth;
            int outerWidth = grid.ApplyLayout(
                availableGridHeight: ChooserIconGrid.VisibleRowCount * 40,
                maxLayoutWidth: 270,
                designCellSize: 40,
                minCellSize: 18,
                scrollbarWidth: scrollbar,
                minOuterWidth: 251);

            Assert.IsTrue(outerWidth >= 251,
                $"Grid outer width {outerWidth} should meet chrome minimums that do not land on a cell boundary.");
            Assert.IsTrue(outerWidth <= 270);
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
