using System.Text.Json.Serialization;

namespace Foreman.Serialization {
    public sealed class ColorSaveData(byte a, byte r, byte g, byte b) {
        public byte A { get; } = a;
        public byte R { get; } = r;
        public byte G { get; } = g;
        public byte B { get; } = b;
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    [JsonDerivedType(typeof(TextAnnotationSaveData), "Text")]
    [JsonDerivedType(typeof(ShapeAnnotationSaveData), "Shape")]
    public abstract class AnnotationSaveData {
        /// <summary>Discriminator for JSON; written by <see cref="JsonPolymorphicAttribute"/>, not duplicated as a normal property.</summary>
        [JsonIgnore]
        public string Type => this switch {
            TextAnnotationSaveData => "Text",
            ShapeAnnotationSaveData => "Shape",
            _ => ""
        };
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
    }

    public sealed class TextAnnotationSaveData : AnnotationSaveData {
        public string Text { get; init; } = "";
        public string FontFamily { get; init; } = "Segoe UI";
        public float FontSize { get; init; } = 14f;
        public int FontStyle { get; init; }
        public ColorSaveData TextColor { get; init; } = new(255, 0, 0, 0);
        public ColorSaveData BackColor { get; init; } = new(0, 255, 255, 255);
        public int TextAlign { get; init; } = 1;
    }

    public sealed class ShapeAnnotationSaveData : AnnotationSaveData {
        public string ShapeType { get; init; } = "Rectangle";
        public ColorSaveData FillColor { get; init; } = new(80, 80, 160, 255);
        public ColorSaveData BorderColor { get; init; } = new(255, 60, 120, 220);
        public int BorderWidth { get; init; } = 2;
    }
}
