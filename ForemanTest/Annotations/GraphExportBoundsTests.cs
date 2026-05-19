using Foreman;
using Foreman.ProductionGraphView.Annotations;
using Foreman.ProductionGraphView.Elements;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ForemanTest.Annotations {
    [TestClass]
    public class GraphExportBoundsTests {
        [TestMethod]
        public void Compute_EmptyGraphAndNoAnnotations_ReturnsEmpty() {
            Rectangle result = GraphExportBounds.Compute(Rectangle.Empty, []);

            Assert.AreEqual(Rectangle.Empty, result);
            Assert.IsFalse(GraphExportBounds.IsExportable(result));
        }

        [TestMethod]
        public void Compute_GraphOnly_ReturnsGraphBounds() {
            Rectangle graph = new(10, 20, 400, 300);

            Rectangle result = GraphExportBounds.Compute(graph, []);

            Assert.AreEqual(graph, result);
        }

        [TestMethod]
        public void Compute_AnnotationsOnly_IncludesPadding() {
            Rectangle annotation = new(100, 50, 80, 40);

            Rectangle result = GraphExportBounds.Compute(Rectangle.Empty, [annotation]);

            Assert.AreEqual(annotation.Left - GraphExportBounds.AnnotationOnlyPadding, result.Left);
            Assert.AreEqual(annotation.Top - GraphExportBounds.AnnotationOnlyPadding, result.Top);
            Assert.IsTrue(GraphExportBounds.IsExportable(result));
        }

        [TestMethod]
        public void Compute_AnnotationOutsideGraph_ExpandsBounds() {
            Rectangle graph = new(0, 0, 200, 200);
            Rectangle annotation = new(300, 0, 50, 50);

            Rectangle result = GraphExportBounds.Compute(graph, [annotation]);

            Assert.AreEqual(0, result.Left);
            Assert.AreEqual(0, result.Top);
            Assert.AreEqual(350, result.Width);
            Assert.AreEqual(200, result.Height);
        }

        [TestMethod]
        public void ScaledDimensions_NeverReturnZero() {
            Assert.AreEqual(1, GraphExportBounds.ScaledWidth(Rectangle.Empty, 1f));
            Assert.AreEqual(1, GraphExportBounds.ScaledHeight(Rectangle.Empty, 1f));
            Assert.AreEqual(1, GraphExportBounds.ScaledWidth(new Rectangle(0, 0, 3, 3), 0.05f));
        }

        [TestMethod]
        public void FullGraphExport_AnnotationsOnly_DoesNotThrow() =>
            StaTest.Run(FullGraphExport_AnnotationsOnly_DoesNotThrow_Impl);

        private static void FullGraphExport_AnnotationsOnly_DoesNotThrow_Impl() {
            using var viewer = new ProductionGraphViewer {
                Size = new Size(800, 600),
            };
            ShapeAnnotationElement? sae = new(viewer, new Point(120, 80), 100, 60);
            try {
                viewer.AddAnnotationElement(sae);
                sae = null;
            } finally {
                sae?.Dispose();
            }

            Rectangle exportBounds = viewer.GetExportBounds();
            Assert.IsTrue(GraphExportBounds.IsExportable(exportBounds));

            using var image = new Bitmap(
                GraphExportBounds.ScaledWidth(exportBounds, 1f),
                GraphExportBounds.ScaledHeight(exportBounds, 1f));
            using var graphics = Graphics.FromImage(image);
            graphics.ScaleTransform(1f, 1f);
            graphics.TranslateTransform(-exportBounds.X, -exportBounds.Y);
            viewer.Paint(graphics, true);

            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            Assert.IsGreaterThan(0, stream.Length);
        }
    }
}
