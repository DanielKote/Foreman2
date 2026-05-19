using Foreman.ProductionGraphView.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace ForemanTest.Annotations {
    [TestClass]
    public class TextAnnotationLayoutTests {
        [TestMethod]
        public void MeasureBoxForText_EmptyText_ReturnsMinimumSize() {
            using var font = new Font("Segoe UI", 14f, FontStyle.Regular, GraphicsUnit.Point);
            Size box = TextAnnotationLayout.MeasureBoxForText("", font);

            Assert.AreEqual(TextAnnotationLayout.MinBoxWidth, box.Width);
            Assert.AreEqual(TextAnnotationLayout.MinBoxHeight, box.Height);
        }

        [TestMethod]
        public void MeasureBoxForText_LabelText_IsLargerThanMinimum() {
            using var font = new Font("Segoe UI", 14f, FontStyle.Bold, GraphicsUnit.Point);
            Size box = TextAnnotationLayout.MeasureBoxForText("Hello", font);

            Assert.IsGreaterThan(TextAnnotationLayout.MinBoxWidth, box.Width);
            Assert.IsGreaterThan(TextAnnotationLayout.MinBoxHeight, box.Height);
        }

        [TestMethod]
        public void ComputeResizeFontSize_DoublesBox_DoublesFont() {
            float result = TextAnnotationLayout.ComputeResizeFontSize(14f, 100, 50, 200, 100);

            Assert.AreEqual(28f, result, 0.01f);
        }

        [TestMethod]
        public void ComputeResizeFontSize_InvalidStartSize_ReturnsStartFont() {
            float result = TextAnnotationLayout.ComputeResizeFontSize(14f, 0, 50, 200, 100);

            Assert.AreEqual(14f, result);
        }

        [TestMethod]
        public void ComputeResizeFontSize_ClampsToMaximum() {
            float result = TextAnnotationLayout.ComputeResizeFontSize(
                200f, 10, 10, 1000, 1000);

            Assert.AreEqual(TextAnnotationLayout.MaxFontSizePt, result);
        }

        [TestMethod]
        public void NearlyEqualFontSize_WithinTolerance_ReturnsTrue() {
            Assert.IsTrue(TextAnnotationLayout.NearlyEqualFontSize(14f, 14.04f));
            Assert.IsFalse(TextAnnotationLayout.NearlyEqualFontSize(14f, 14.1f));
        }
    }
}
