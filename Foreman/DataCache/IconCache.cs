using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    [Serializable]
    public struct IconColorPair(Bitmap icon, Color color) {
        public Bitmap Icon = icon;
        public Color Color = color;
    }

    [Serializable]
    public class IconBitmapCollection {
        public Dictionary<string, IconColorPair> Icons = new();
    }


    public static class IconCache {
        private static Bitmap _unknownIcon;

        public static Bitmap GetUnknownIcon() {
            _unknownIcon ??= GetIcon(Path.Combine("Graphics", "UnknownIcon.png"), 32);
            return _unknownIcon;
        }

        private static Bitmap _spoilageIcon;

        public static Bitmap GetSpoilageIcon() {
            _spoilageIcon ??= GetIcon(Path.Combine("Graphics", "SpoilAssembler.png"), 96);
            return _spoilageIcon;
        }

        private static Bitmap _plantingIcon;

        public static Bitmap GetPlantingIcon() {
            _plantingIcon ??= GetIcon(Path.Combine("Graphics", "PlantAssembler.png"), 96);
            return _plantingIcon;
        }

        public static Bitmap GetIcon(string path, int size) {
            try {
                using var image = new Bitmap(path);
                var bmp = new Bitmap(size, size);

                using var g = Graphics.FromImage(bmp);
                g.DrawImage(image, new Rectangle(0, 0, size * image.Width / image.Height, size));

                return bmp;
            } catch (Exception) {
                return new Bitmap(size, size);
            }
        }

        public static Bitmap CombineIcons(Bitmap aIcon, Bitmap bIcon, int size, bool diagonalSlice = true) {
            var result = new Bitmap(size, size);
            using var g = Graphics.FromImage(result);

            using (var tlPath = new GraphicsPath()) {
                tlPath.AddLine(0, 0, 0, size);
                tlPath.AddLine(0, size, size, 0);
                tlPath.AddLine(size, 0, 0, 0);
                if (diagonalSlice)
                    g.Clip = new Region(tlPath);
                if (aIcon != null)
                    g.DrawImage(aIcon, 0, 0, size, size);
            }

            using (var trPath = new GraphicsPath()) {
                trPath.AddLine(size, size, 0, size);
                trPath.AddLine(0, size, size, 0);
                trPath.AddLine(size, 0, size, size);
                if (diagonalSlice)
                    g.Clip = new Region(trPath);
                if (bIcon != null)
                    g.DrawImage(bIcon, 0, 0, size, size);
            }

            return result;
        }


        public static void SaveIconCache(string path, Dictionary<string, IconColorPair> iconCache) {
            var iCollection = new IconBitmapCollection();

            foreach (var iconKvp in iconCache)
                iCollection.Icons.Add(iconKvp.Key, iconKvp.Value);

            if (File.Exists(path))
                File.Delete(path);

            using Stream stream = File.Open(path, FileMode.Create, FileAccess.Write);

            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, iCollection);
        }

        public static async Task<Dictionary<string, IconColorPair>> LoadIconCache(string path, IProgress<KeyValuePair<int, string>> progress,
            int startingPercent, int endingPercent) {
            var iconCache = new Dictionary<string, IconColorPair>();
            await Task.Run(() => {
                try {
                    using Stream stream = File.Open(path, FileMode.Open);

                    var binaryFormatter = new BinaryFormatter();
                    var iCollection = (IconBitmapCollection) binaryFormatter.Deserialize(stream);

                    var totalCount = iCollection.Icons.Count;
                    var counter = 0;
                    foreach (var iconKvp in iCollection.Icons) {
                        progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount,
                            "Loading Icons..."));
                        iconCache.Add(iconKvp.Key, iconKvp.Value);
                    }
                } catch { // there was an error reading the cache. Just ignore it and continue (we will have to load the icons from the files directly)
                    iconCache.Clear();
                    MessageBox.Show("Icon cache was corrupted. All icons will be empty.\nRecommendation: delete preset and import new one?");
                }
            });
            return iconCache;
        }
    }
}