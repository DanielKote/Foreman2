using Foreman.ProductionGraphView.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman.ProductionGraphView.Annotations {
    /// <summary>Bounds and pixel sizing for full-graph PNG export (nodes, links, and annotations).</summary>
    public static class GraphExportBounds {
        /// <summary>Padding around annotation-only exports (graph bounds already include node borders).</summary>
        public const int AnnotationOnlyPadding = 50;

        public static bool IsExportable(Rectangle bounds) =>
            bounds.Width > 0 && bounds.Height > 0;

        public static Rectangle GetGraphBounds(AnnotationElement annotation) =>
            new(
                annotation.X - annotation.Width / 2,
                annotation.Y - annotation.Height / 2,
                annotation.Width,
                annotation.Height);

        public static Rectangle Compute(Rectangle graphBounds, IEnumerable<Rectangle> annotationBounds) {
            var annotations = annotationBounds
                .Where(r => r.Width > 0 && r.Height > 0)
                .ToList();

            bool hasGraph = graphBounds.Width > 0 && graphBounds.Height > 0;
            bool hasAnnotations = annotations.Count > 0;

            if (!hasGraph && !hasAnnotations)
                return Rectangle.Empty;

            if (hasGraph && !hasAnnotations)
                return graphBounds;

            int xMin = int.MaxValue;
            int yMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMax = int.MinValue;

            if (hasGraph)
                Include(graphBounds, ref xMin, ref yMin, ref xMax, ref yMax);

            foreach (Rectangle annotation in annotations)
                Include(annotation, ref xMin, ref yMin, ref xMax, ref yMax);

            int pad = hasGraph ? 0 : AnnotationOnlyPadding;
            return new Rectangle(
                xMin - pad,
                yMin - pad,
                xMax - xMin + (2 * pad),
                yMax - yMin + (2 * pad));
        }

        public static int ScaledWidth(Rectangle bounds, float scale) =>
            Math.Max(1, (int)Math.Ceiling(bounds.Width * scale));

        public static int ScaledHeight(Rectangle bounds, float scale) =>
            Math.Max(1, (int)Math.Ceiling(bounds.Height * scale));

        private static void Include(Rectangle r, ref int xMin, ref int yMin, ref int xMax, ref int yMax) {
            xMin = Math.Min(xMin, r.Left);
            yMin = Math.Min(yMin, r.Top);
            xMax = Math.Max(xMax, r.Right);
            yMax = Math.Max(yMax, r.Bottom);
        }
    }
}
