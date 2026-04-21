using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Foreman {
    public static class GraphicsStuff {
        // returns the width of the actually drawn text
        public static int DrawText(
            Graphics graphics,
            Brush textBrush,
            StringFormat textFormat,
            string text,
            Font baseFont,
            Rectangle textbox,
            bool singleLine = false
        ) {
            float textLength = 0;
            var textFont = new Font(baseFont, baseFont.Style);
            if (singleLine) {
                textLength = graphics.MeasureString(text, textFont).Width;
                while (textLength > textbox.Width) {
                    var newNameFont = new Font(textFont.FontFamily, textFont.Size - 0.5f, textFont.Style);
                    textFont.Dispose();
                    textFont = newNameFont;
                    textLength = graphics.MeasureString(text, textFont).Width;
                }
            } else {
                var textSize = graphics.MeasureString(text, textFont, textbox.Width);
                while (textSize.Height > textbox.Height) {
                    var newNameFont = new Font(textFont.FontFamily, textFont.Size - 0.5f, textFont.Style);
                    textFont.Dispose();
                    textFont = newNameFont;
                    textSize = graphics.MeasureString(text, textFont, textbox.Width);
                }

                textLength = textSize.Width;
            }

            graphics.DrawString(text, textFont, textBrush, textbox, textFormat);
            textFont.Dispose();
            return (int) textLength;
        }


        public static void DrawRoundRect(int x, int y, int width, int height, int radius, Graphics graphics, Pen pen) {
            var radius2 = radius * 2;
            var left = x;
            var top = y;
            var bottom = y + height;
            var right = x + width;

            using (var path = new GraphicsPath()) {
                path.StartFigure();

                path.AddArc(left, top, 2 * radius, 2 * radius, 180, 90f);
                path.AddArc(right - radius2, top, radius2, radius2, 270f, 90f);
                path.AddArc(right - radius2, bottom - radius2, radius2, radius2, 0f, 90f);
                path.AddArc(left, bottom - radius2, radius2, radius2, 90f, 90f);

                path.CloseFigure();

                graphics.DrawPath(pen, path);
            }
        }

        public static void FillRoundRect(int x, int y, int width, int height, int radius, Graphics graphics, Brush brush) {
            var radius2 = radius * 2;
            var left = x;
            var top = y;
            var bottom = y + height;
            var right = x + width;

            using (var path = new GraphicsPath()) {
                path.StartFigure();

                path.AddArc(left, top, 2 * radius, 2 * radius, 180, 90f);
                path.AddArc(right - radius2, top, radius2, radius2, 270f, 90f);
                path.AddArc(right - radius2, bottom - radius2, radius2, radius2, 0f, 90f);
                path.AddArc(left, bottom - radius2, radius2, radius2, 90f, 90f);

                path.CloseFigure();

                graphics.FillPath(brush, path);
            }
        }

        public static void FillRoundRectTlFlag(int x, int y, int width, int height, int radius, Graphics graphics, Brush brush) {
            var radius2 = radius * 2;
            var left = x;
            var top = y;
            var bottom = y + height;
            var right = x + width;

            using (var path = new GraphicsPath()) {
                path.StartFigure();

                path.AddArc(left, top, 2 * radius, 2 * radius, 180f, 90f);
                path.AddLine(right, top, left, bottom);

                path.CloseFigure();

                graphics.FillPath(brush, path);
            }
        }

        public static string DoubleToString(double value) {
            if (Math.Abs(value) >= 100000)
                return value.ToString("0.00e0");
            if (Math.Abs(value) >= 10000)
                return value.ToString("0");
            if (Math.Abs(value) >= 100)
                return value.ToString("0.#");
            if (Math.Abs(value) >= 10)
                return value.ToString("0.##");
            if (Math.Abs(value) >= 0.1)
                return value.ToString("0.###");
            if (Math.Abs(value) != 0)
                return value.ToString("0.######");
            return "0";
        }

        public static string DoubleToEnergy(double value, string unit) {
            if (Math.Abs(value) >= 1000000000000)
                return (value / 1000000000000).ToString("0.##") + " P" + unit;
            if (Math.Abs(value) >= 1000000000)
                return (value / 1000000000).ToString("0.##") + " G" + unit;
            if (Math.Abs(value) >= 1000000)
                return (value / 1000000).ToString("0.##") + " M" + unit;
            if (Math.Abs(value) >= 1000)
                return (value / 1000).ToString("0.##") + " K" + unit;
            return value.ToString("0.##") + " " + unit;
        }
    }

    // As I found out the painted recipe look is rather nice. And when I needed to display it outside the tooltip, well...
    // it moved to its own class instead of me copy-pasting the code.
    public static class RecipePainter {
        private static readonly Brush BackgroundBrush = new SolidBrush(Color.FromArgb(255, 65, 65, 65));
        private static readonly Brush DarkBackgroundBrush = new SolidBrush(Color.FromArgb(255, 40, 40, 40));
        private static readonly Brush TextBrush = new SolidBrush(Color.White);
        private static readonly Pen BorderPen = new(new SolidBrush(Color.Black), 2);
        private static readonly Pen BreakerPen = new(new SolidBrush(Color.Black), 10);
        private static readonly Font QuantityFont = new(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
        private static readonly Font SectionFont = new(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
        private static readonly Pen DevPen = new(Brushes.Orange, 1);

        private static readonly Font RecipeFont = new(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);
        private static readonly Font ItemFont = new(FontFamily.GenericSansSerif, 7.8f);
        private static readonly StringFormat TextFormat = new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

        private const int SectionWidth = 200;

        public static Size GetSize(ICollection<Recipe> recipes) {
            var width = (SectionWidth + 10) * recipes.Count - 10;
            var height = 110 + 20 + recipes.Max(r => r.IngredientList.Count) * 40 + recipes.Max(r => r.ProductList.Count) * 40 +
                recipes.Max(r => r.MyUnlockSciencePacks.Count) * (Properties.Settings.Default.AbbreviateSciPacks ? 40 : 30);
            return new Size(width, height);
        }

        public static void Paint(IList<Recipe> recipes, Graphics graphics, Point offset) {
            var boundary = new Rectangle(offset, GetSize(recipes));
            graphics.FillRectangle(BackgroundBrush, boundary);

            var maxIngredientCount = 0;
            var maxProductCount = 0;
            var maxSciencePackListsCount = 0;
            foreach (var recipe in recipes) {
                maxIngredientCount = Math.Max(maxIngredientCount, recipe.IngredientList.Count);
                maxProductCount = Math.Max(maxProductCount, recipe.ProductList.Count);
                maxSciencePackListsCount = Math.Max(maxSciencePackListsCount, recipe.MyUnlockSciencePacks.Count);
            }

            var xOffset = boundary.X;
            for (var r = 0; r < recipes.Count; r++) {
                // Title

                var yOffset = boundary.Y;
                graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 40));
                graphics.DrawImage(recipes[r].Icon, 4 + xOffset, 4 + yOffset, 32, 32);

                var textbox = new Rectangle(xOffset + 42, yOffset + 4, SectionWidth - 48, 32);
                //graphics.DrawRectangle(devPen, textbox);
                GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, recipes[r].FriendlyName, RecipeFont, textbox);

                // Ingredient list:

                yOffset += 44;
                graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 20));
                yOffset += 2;
                graphics.DrawString("Ingredients:", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);
                yOffset += 20;
                for (var i = 0; i < maxIngredientCount; i++) {
                    if (i < recipes[r].IngredientList.Count) {
                        var ingredient = recipes[r].IngredientList[i];
                        graphics.DrawImage(ingredient.Icon, 14 + xOffset, 4 + yOffset, 32, 32);

                        textbox = new Rectangle(xOffset + 52, yOffset + 2, SectionWidth - 58, 20);
                        //graphics.DrawRectangle(devPen, textbox);
                        graphics.DrawString(recipes[r].IngredientSet[ingredient].ToString("0.##") + "x", QuantityFont, TextBrush,
                            new Point(52 + xOffset, 20 + yOffset));
                        GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, recipes[r].GetIngredientFriendlyName(ingredient), ItemFont, textbox);
                    }

                    yOffset += 40;
                }

                // Products list:

                graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 20));
                yOffset += 2;
                graphics.DrawString("Products:", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);
                yOffset += 20;
                for (var i = 0; i < maxProductCount; i++) {
                    if (i < recipes[r].ProductList.Count) {
                        var product = recipes[r].ProductList[i];
                        graphics.DrawImage(product.Icon, 14 + xOffset, 4 + yOffset, 32, 32);

                        textbox = new Rectangle(xOffset + 52, yOffset + 2, SectionWidth - 58, 20);
                        //graphics.DrawRectangle(devPen, textbox);
                        graphics.DrawString(recipes[r].ProductSet[product].ToString("0.##") + "x", QuantityFont, TextBrush,
                            new Point(52 + xOffset, 20 + yOffset));
                        GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, recipes[r].GetProductFriendlyName(product), ItemFont, textbox);
                    }

                    yOffset += 40;
                }

                // unlock science packs

                graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 22));
                yOffset += 2;
                if (Properties.Settings.Default.AbbreviateSciPacks)
                    graphics.DrawString(recipes[r].MyUnlockSciencePacks.Count > 1 ? "Key required science packs (any):" : "Key required science packs:",
                        SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);
                else
                    graphics.DrawString(recipes[r].MyUnlockSciencePacks.Count > 1 ? "Required science packs (any):" : "Required science packs:", SectionFont,
                        TextBrush, 4 + xOffset, 0 + yOffset);

                yOffset += 20;
                for (var i = 0; i < maxSciencePackListsCount; i++) {
                    if (i < recipes[r].MyUnlockSciencePacks.Count) {
                        var sciPacks = recipes[r].MyUnlockSciencePacks[i];
                        var sciPackSize = 24;

                        // don't show the science pack if it is a prerequisite of another science pack (that we show)

                        if (Properties.Settings.Default.AbbreviateSciPacks) {
                            var filteredSciPacks = new List<Item>(sciPacks);
                            foreach (var sciPack in sciPacks)
                            foreach (var prereq in recipes[r].Owner.SciencePackPrerequisites[sciPack])
                                filteredSciPacks.Remove(prereq);
                            sciPacks = filteredSciPacks;
                            sciPackSize = 32;
                        }

                        // ensure all science packs will fit (there should be space for 8, but knowing mods... this might not be enough)

                        var iconSize = sciPacks.Count == 0
                            ? sciPackSize
                            : Math.Min(sciPackSize, (SectionWidth - 8) / sciPacks.Count);
                        for (var j = 0; j < sciPacks.Count; j++)
                            graphics.DrawImage(sciPacks[j].Icon, xOffset + 4 + j * iconSize, 3 + yOffset, iconSize, iconSize);
                    }

                    yOffset += Properties.Settings.Default.AbbreviateSciPacks ? 40 : 30;
                }

                // time

                graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 22));
                yOffset += 2;
                graphics.DrawString("Crafting Time: " + recipes[r].Time.ToString("0.##") + " s", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);

                // breaker

                if (r < recipes.Count - 1) {
                    graphics.DrawLine(BreakerPen, xOffset + SectionWidth + 5, boundary.Y, xOffset + SectionWidth + 5, boundary.Y + boundary.Height);
                    xOffset += SectionWidth + 10;
                }
            }

            graphics.DrawRectangle(BorderPen, boundary);
        }
    }

    // https://stackoverflow.com/questions/13477689/find-number-of-decimal-places-in-decimal-value-regardless-of-culture
    public static class MathDecimals {
        public static int GetDecimals(decimal d, int i = 0) {
            while (true) {
                var multiplied = (decimal) ((double) d * Math.Pow(10, i));
                if (Math.Round(multiplied) == multiplied)
                    return i;
                i += 1;
            }
        }
    }
}