using Foreman;
using Foreman.ProductionGraphView.Elements;
using Foreman.Serialization;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace ForemanTest.Annotations {
    [TestClass]
    [DoNotParallelize]
    public class TextAnnotationElementTests : ForemanTestBase {
        [TestMethod]
        public void FitBoxToTextAtCenter_PreservesCenter() =>
            StaTest.Run(FitBoxToTextAtCenter_PreservesCenter_Impl);

        [TestMethod]
        public void SetFontSizeInPoints_GrowsBoxWhenFitted() =>
            StaTest.Run(SetFontSizeInPoints_GrowsBoxWhenFitted_Impl);

        private static void FitBoxToTextAtCenter_PreservesCenter_Impl() {
            using var viewer = CreateViewer();
            using var annotation = new TextAnnotationElement(viewer, new Point(100, 200));
            annotation.Text = "Hello";

            int centerX = annotation.X;
            int centerY = annotation.Y;
            int widthBefore = annotation.Width;
            int heightBefore = annotation.Height;

            annotation.SetFontSizeInPoints(24f);
            annotation.FitBoxToTextAtCenter();

            Assert.AreEqual(centerX, annotation.X);
            Assert.AreEqual(centerY, annotation.Y);
            Assert.IsGreaterThanOrEqualTo(widthBefore, annotation.Width);
            Assert.IsGreaterThanOrEqualTo(heightBefore, annotation.Height);
        }

        private static void SetFontSizeInPoints_GrowsBoxWhenFitted_Impl() {
            using var viewer = CreateViewer();
            using var annotation = new TextAnnotationElement(viewer, new Point(50, 50));
            annotation.Text = "Scale me";
            annotation.FitBoxToTextAtCenter();

            int widthAt14 = annotation.Width;
            annotation.SetFontSizeInPoints(28f);
            annotation.FitBoxToTextAtCenter();

            Assert.IsGreaterThan(widthAt14, annotation.Width);
            Assert.AreEqual(28f, annotation.TextFont.SizeInPoints, 0.01f);
        }

        private static ProductionGraphViewer CreateViewer() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var viewer = new ProductionGraphViewer {
                DCache = ctx.Cache,
                Size = new Size(800, 600),
            };
            viewer.Graph.DefaultAssemblerQuality = ctx.Quality;
            viewer.ApplySaveUi(new GraphViewerUiSaveData {
                ViewOffset = Point.Empty,
                ViewScale = 1f,
            }, ctx.Cache, setEnablesFromJson: false);
            return viewer;
        }
    }
}
