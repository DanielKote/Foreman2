using Foreman.ProductionGraphView;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace ForemanTest {
    [TestClass]
    public class EditPanelScreenLayoutTests : ForemanTestBase {
        private const int ViewerW = 1200;
        private const int ViewerH = 800;
        private const int Margin = EditPanelScreenLayout.DefaultMargin;

        [TestMethod]
        public void ClampRectToViewer_KeepsBoundsInsideViewer() {
            var offBottom = new Rectangle(100, 900, 472, 689);
            Rectangle clamped = EditPanelScreenLayout.ClampRectToViewer(offBottom, ViewerW, ViewerH, Margin);

            Assert.IsTrue(EditPanelScreenLayout.FitsViewer(clamped, ViewerW, ViewerH, Margin));
            Assert.AreEqual(offBottom.Width, clamped.Width);
            Assert.AreEqual(offBottom.Height, clamped.Height);
        }

        [TestMethod]
        public void ClampRectToViewer_ShiftsUpWhenPanelExtendsBelowViewer() {
            var offBottom = new Rectangle(200, 700, 400, 200);
            Rectangle clamped = EditPanelScreenLayout.ClampRectToViewer(offBottom, ViewerW, ViewerH, Margin);

            Assert.IsLessThan(offBottom.Top, clamped.Top);
            Assert.AreEqual(ViewerH - Margin - offBottom.Height, clamped.Top);
        }

        [TestMethod]
        public void ClampRectToViewer_ShiftsDownWhenPanelExtendsAboveViewer() {
            var offTop = new Rectangle(200, -50, 300, 150);
            Rectangle clamped = EditPanelScreenLayout.ClampRectToViewer(offTop, ViewerW, ViewerH, Margin);

            Assert.AreEqual(Margin, clamped.Top);
        }

        [TestMethod]
        public void GetShiftToFit_UnionOfTwoPanels_UsesSingleDeltaForBoth() {
            var left = new Rectangle(50, 750, 472, 689);
            var right = new Rectangle(left.Right + 5, 750, 300, 400);
            var union = Rectangle.Union(left, right);
            Point shift = EditPanelScreenLayout.GetShiftToFit(union, ViewerW, ViewerH, Margin);

            var shiftedUnion = new Rectangle(union.X + shift.X, union.Y + shift.Y, union.Width, union.Height);
            Assert.IsTrue(EditPanelScreenLayout.FitsViewer(shiftedUnion, ViewerW, ViewerH, Margin));
            Assert.IsLessThan(0, shift.Y, "Tall union below the viewer should shift upward.");
        }

        [TestMethod]
        public void FitsViewer_ReturnsTrueForBoundsAlreadyInside() {
            var inside = new Rectangle(Margin, Margin, 200, 100);
            Assert.IsTrue(EditPanelScreenLayout.FitsViewer(inside, ViewerW, ViewerH, Margin));
        }
    }

}
