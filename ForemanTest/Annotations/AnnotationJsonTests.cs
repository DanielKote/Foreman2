using Foreman.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace ForemanTest.Annotations {
    [TestClass]
    public class AnnotationJsonTests {
        [TestMethod]
        public void SerializeDeserialize_TextAnnotation_RoundTrips() {
            var original = new TextAnnotationSaveData {
                X = 12,
                Y = 34,
                Width = 100,
                Height = 50,
                Text = "Label",
                FontFamily = "Arial",
                FontSize = 16f,
                FontStyle = 1,
                TextColor = new ColorSaveData(255, 10, 20, 30),
                BackColor = new ColorSaveData(128, 40, 50, 60),
                TextAlign = 2
            };

            JsonNode? node = AnnotationJson.SerializeToNode(original);
            Assert.IsNotNull(node);
            Assert.AreEqual("Text", node["Type"]?.GetValue<string>());

            AnnotationSaveData? restored = AnnotationJson.Deserialize(node);
            Assert.IsInstanceOfType<TextAnnotationSaveData>(restored);
            var text = (TextAnnotationSaveData)restored;
            Assert.AreEqual("Label", text.Text);
            Assert.AreEqual(12, text.X);
            Assert.AreEqual(16f, text.FontSize);
            Assert.AreEqual(30, text.TextColor.B);
        }

        [TestMethod]
        public void SerializeDeserialize_ShapeAnnotation_RoundTrips() {
            var original = new ShapeAnnotationSaveData {
                X = 1,
                Y = 2,
                Width = 80,
                Height = 90,
                ShapeType = "Ellipse",
                FillColor = new ColorSaveData(80, 1, 2, 3),
                BorderColor = new ColorSaveData(255, 4, 5, 6),
                BorderWidth = 3
            };

            AnnotationSaveData? restored = AnnotationJson.Deserialize(AnnotationJson.SerializeToNode(original)!);
            Assert.IsInstanceOfType<ShapeAnnotationSaveData>(restored);
            var shape = (ShapeAnnotationSaveData)restored;
            Assert.AreEqual("Ellipse", shape.ShapeType);
            Assert.AreEqual(3, shape.BorderWidth);
        }

        [TestMethod]
        public void DeserializeListFromRoot_UnknownType_SkipsEntry() {
            var root = JsonNode.Parse("""
                {
                  "Annotations": [
                    { "Type": "Text", "X": 0, "Y": 0, "Width": 10, "Height": 10, "Text": "ok" },
                    { "Type": "Unknown", "X": 0, "Y": 0, "Width": 1, "Height": 1 }
                  ]
                }
                """);

            IReadOnlyList<AnnotationSaveData>? list = AnnotationJson.DeserializeListFromRoot(root);
            Assert.IsNotNull(list);
            Assert.HasCount(1, list);
            Assert.IsInstanceOfType<TextAnnotationSaveData>(list[0]);
        }
    }
}
