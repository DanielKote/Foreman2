using Foreman.ProductionGraphView.Annotations;
using Foreman.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ForemanTest.Annotations {
    [TestClass]
    public class AnnotationClipboardCodecTests {
        [TestMethod]
        public void ReadAnnotations_RoundTripsWirePayload() {
            var original = new TextAnnotationSaveData {
                X = 10,
                Y = 20,
                Width = 120,
                Height = 40,
                Text = "Note",
                FontFamily = "Segoe UI",
                FontSize = 12f
            };
            string json = AnnotationClipboardCodec.MergeAnnotationsIntoFragment("{}", [original]);

            IReadOnlyList<AnnotationSaveData>? restored = AnnotationClipboardCodec.ReadAnnotations(json);

            Assert.IsNotNull(restored);
            var text = restored.OfType<TextAnnotationSaveData>().Single();
            Assert.AreEqual("Note", text.Text);
            Assert.AreEqual(10, text.X);
        }

        [TestMethod]
        public void ReadAnnotations_MissingArray_ReturnsNull() {
            Assert.IsNull(AnnotationClipboardCodec.ReadAnnotations("{\"Object\":\"ProductionGraph\"}"));
        }

        [TestMethod]
        public void MergeAnnotationsIntoFragment_AddsAnnotationsArray() {
            string fragment = "{\"Object\":\"ProductionGraph\",\"Version\":8,\"Nodes\":[]}";
            var annotation = new ShapeAnnotationSaveData {
                X = 1,
                Y = 2,
                Width = 30,
                Height = 40,
                ShapeType = "Rectangle"
            };

            string merged = AnnotationClipboardCodec.MergeAnnotationsIntoFragment(
                fragment,
                [annotation]);

            IReadOnlyList<AnnotationSaveData>? restored = AnnotationClipboardCodec.ReadAnnotations(merged);
            Assert.IsNotNull(restored);
            Assert.HasCount(1, restored);
            Assert.IsInstanceOfType<ShapeAnnotationSaveData>(restored[0]);
        }

        [TestMethod]
        public void MergeAnnotationsIntoFragment_InvalidJson_ReturnsOriginal() {
            const string invalid = "not json";
            string merged = AnnotationClipboardCodec.MergeAnnotationsIntoFragment(
                invalid,
                [new TextAnnotationSaveData { X = 0, Y = 0, Width = 1, Height = 1 }]);

            Assert.AreEqual(invalid, merged);
        }
    }
}
