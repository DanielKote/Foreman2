using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Foreman {

    public struct IconInfo {
        public string iconPath;
        public int iconSize;
        public double iconScale;
        public Point iconOffset;
        public Color iconTint;

        public IconInfo(string iconPath, int iconSize) {
            this.iconPath = iconPath;
            this.iconSize = iconSize;
            this.iconScale = 1;
            this.iconOffset = new Point(0, 0);
            iconTint = IconCacheProcessor.NoTint;
        }

        public void SetIconTint(double a, double r, double g, double b) {
            a = (a <= 1 ? a * 255 : a);
            r = (r <= 1 ? r * 255 : r);
            g = (g <= 1 ? g * 255 : g);
            b = (b <= 1 ? b * 255 : b);
            iconTint = Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }
    }

    public class IconCacheProcessor : IDisposable {
        internal static readonly Color NoTint = Color.White;

        public int TotalPathCount { get; private set; }
        public int FailedPathCount { get; private set; }

        private Dictionary<string, IconColorPair> myIconCache;

        private Dictionary<string, string> folderLinks;
        private Dictionary<string, ZipArchiveEntry>? archiveFileLinks;
        private readonly List<ZipArchive> openedArchives = [];
        private readonly Dictionary<string, Bitmap?> bitmapCache = []; //just so we dont have to load the same file multiple times

        public IconCacheProcessor() {
            TotalPathCount = 0;
            FailedPathCount = 0;

            myIconCache = [];

            folderLinks = [];
            archiveFileLinks = [];
        }

        public bool PrepareModPaths(Dictionary<string, string> modSet, string modsPath, string dataPath, CancellationToken token) {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            folderLinks.Clear();
            archiveFileLinks?.Clear();
            bitmapCache.Clear();

            //factorio checks for foldeer <name>_<version>, then folder <name> then zip <name>_<version>
            //if zip, then the actual files can either be in the root of zip, or in <name> foler, or in <name>_<version> folder
            //NOTE: versions are of type v1.v2.v3 where each number can have any amount of leading zeros
            foreach (KeyValuePair<string, string> mod in modSet) {
                if (token.IsCancellationRequested)
                    return false;

                string versionMatch = string.Join(".", mod.Value.Split('.').Select(s => "0*" + int.Parse(s).ToString()));

                string[] folders = Directory.GetDirectories(modsPath);
                string[] files = Directory.GetFiles(modsPath);

                var foundFolder = folders.FirstOrDefault(f => Regex.IsMatch(Path.GetFileName(f).ToLower(), string.Format("{0}_{1}", mod.Key, versionMatch)));
                if (foundFolder == null)
                    foundFolder = folders.FirstOrDefault(f => Path.GetFileName(f).ToLower() == mod.Key);

                if (foundFolder != null)
                    folderLinks.Add("__" + mod.Key.ToLower() + "__", foundFolder);
                else {
                    var foundFile = files.FirstOrDefault(f => Regex.IsMatch(Path.GetFileName(f).ToLower(), string.Format("{0}_{1}.zip", mod.Key, versionMatch)));
                    if (foundFile == null) {
                        if (mod.Key.ToLower() != "core" && mod.Key.ToLower() != "base" && mod.Key.ToLower() != "elevated-rails" && mod.Key.ToLower() != "quality" && mod.Key.ToLower() != "space-age")
                            return false;
                        continue;
                    }

                    //for zip files, since we have to iterate through them for each file we might as well make a full link of every possible filepath to given entry
                    ZipArchive zip = ZipFile.Open(foundFile, ZipArchiveMode.Read);
                    openedArchives.Add(zip);
                    foreach (ZipArchiveEntry zentity in zip.Entries) {
                        if (zentity.Name == "")
                            continue; //folder

                        var brokenPath = new LinkedList<string>();
                        string filePath = zentity.FullName;
                        while (filePath != "" && Path.GetFileName(filePath) is string fileName && Path.GetDirectoryName(filePath) is string dirName) {
                            brokenPath.AddFirst(fileName);
                            filePath = dirName;
                        }
                        brokenPath.First?.Value = "__" + mod.Key.ToLower() + "__";
                        archiveFileLinks?.Add(Path.Combine(brokenPath.ToArray()).ToLower(), zentity);
                    }
                }
            }
            folderLinks.Add("__core__", Path.Combine(dataPath, "core"));
            folderLinks.Add("__base__", Path.Combine(dataPath, "base"));
            folderLinks.Add("__elevated-rails__", Path.Combine(dataPath, "elevated-rails"));
            folderLinks.Add("__quality__", Path.Combine(dataPath, "quality"));
            folderLinks.Add("__space-age__", Path.Combine(dataPath, "space-age"));

            return true;
        }

        public async Task<bool> CreateIconCache(JsonObject iconJObject, string cachePath, IProgress<KeyValuePair<int, string>> progress, CancellationToken token, int startingPercent, int endingPercent) {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            TotalPathCount = 0;
            FailedPathCount = 0;

            myIconCache.Clear();
            bitmapCache.Clear();

            int totalCount =
                PresetJson.CountArray(iconJObject, "technologies") +
                PresetJson.CountArray(iconJObject, "recipes") +
                PresetJson.CountArray(iconJObject, "items") +
                PresetJson.CountArray(iconJObject, "fluids") +
                PresetJson.CountArray(iconJObject, "entities") +
                PresetJson.CountArray(iconJObject, "groups") +
                PresetJson.CountArray(iconJObject, "qualities");

            progress.Report(new(startingPercent, "Creating icons."));
            int counter = 0;
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconJObject, "technologies")) {
                if (token.IsCancellationRequested)
                    return false;
                progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 256);
            }
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconJObject, "recipes")) {
                if (token.IsCancellationRequested)
                    return false;
                progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconJObject, "items")) {
                if (token.IsCancellationRequested)
                    return false;
                progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconJObject, "fluids")) {
                if (token.IsCancellationRequested)
                    return false;
                progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconJObject, "entities")) {
                if (token.IsCancellationRequested)
                    return false;
                progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 64);
            }
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconJObject, "groups")) {
                if (token.IsCancellationRequested)
                    return false;
                progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 64);
            }
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconJObject, "qualities")) {
                if (token.IsCancellationRequested)
                    return false;
                progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }

            await IconCache.SaveIconCacheAsync(cachePath, myIconCache, token);

            return (FailedPathCount == 0);
        }

        private void ProcessIcon(JsonNode objJToken, int defaultIconSize) {
            if (PresetJson.GetNode(objJToken, "icon_data") is not JsonNode iconDataJToken)
                return;

            string? iconName = PresetJson.GetString(objJToken, "icon_name");
            if (iconName is null || myIconCache.ContainsKey(iconName))
                return;

            string? mainIconPath = PresetJson.GetString(iconDataJToken, "icon");
            int baseIconSize = PresetJson.GetInt32(iconDataJToken, "icon_size") ?? 32;

            IconInfo iicon = mainIconPath is not null
                ? new IconInfo(mainIconPath, baseIconSize)
                : new IconInfo("", baseIconSize);
            if (mainIconPath is not null)
                iicon.iconScale = defaultIconSize / iicon.iconSize;

            var layerIcons = new List<IconInfo>();
            foreach (JsonNode iconJToken in PresetJson.EnumerateArray(iconDataJToken, "icons")) {
                if (PresetJson.GetString(iconJToken, "icon") is not string icon ||
                    PresetJson.GetNode(iconJToken, "shift") is not JsonArray shift ||
                    shift.Count < 2 ||
                    PresetJson.GetNode(iconJToken, "tint") is not JsonArray tint ||
                    tint.Count < 4)
                    continue;

                IconInfo layerIcon = new IconInfo(icon, PresetJson.GetInt32(iconJToken, "icon_size") ?? baseIconSize);
                layerIcon.iconScale = PresetJson.GetDouble(iconJToken, "scale") ?? defaultIconSize / layerIcon.iconSize;
                layerIcon.iconOffset = new Point(PresetJson.GetInt32Value(shift[0]) ?? default, PresetJson.GetInt32Value(shift[1]) ?? default);
                layerIcon.SetIconTint(
                    PresetJson.GetDoubleValue(tint[3]) ?? default,
                    PresetJson.GetDoubleValue(tint[0]) ?? default,
                    PresetJson.GetDoubleValue(tint[1]) ?? default,
                    PresetJson.GetDoubleValue(tint[2]) ?? default);
                layerIcons.Add(layerIcon);
            }

            if (mainIconPath is null && layerIcons.Count == 0)
                return;

            myIconCache.Add(iconName, GetIconAndColor(iicon, layerIcons, defaultIconSize));
        }


        public IconColorPair GetIconAndColor(IconInfo iinfo, List<IconInfo> iinfos, int defaultCanvasSize) {
            if (iinfos == null)
                iinfos = new List<IconInfo>();
            double IconCanvasScale = defaultCanvasSize == 32 ? 2 : 1; //just some upscailing for icons (item icons are set at 32x32, but they look better at 64x64)
            int IconCanvasSize = (int)(defaultCanvasSize * IconCanvasScale);

            if (iinfos.Count == 0) //if there are no icons, use the single icon
                iinfos.Add(iinfo);

            //quick check to ensure it isnt a null icon
            bool empty = true;
            foreach (IconInfo ii in iinfos) {
                if (!string.IsNullOrEmpty(ii.iconPath))
                    empty = false;
            }
            if (empty)
                return new IconColorPair(null, Color.Black);

            //prepare the canvas - we will add each successive icon/layer on top of it
            Bitmap canvas = new Bitmap(IconCanvasSize, IconCanvasSize, PixelFormat.Format32bppPArgb);
            BitmapData canvasData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadWrite, canvas.PixelFormat);
            int cBPP = Bitmap.GetPixelFormatSize(canvas.PixelFormat) / 8;
            int bCount = canvasData.Stride * canvas.Height;
            byte[] canvasPixels = new byte[bCount];
            IntPtr ptrCanvasFPixel = canvasData.Scan0;
            Marshal.Copy(ptrCanvasFPixel, canvasPixels, 0, canvasPixels.Length);
            int heightInPixels = canvasData.Height;
            int widthInBytes = canvasData.Width * cBPP;

            foreach (IconInfo ii in iinfos) {
                //load the image and prep it for processing
                int iconSize = ii.iconSize > 0 ? ii.iconSize : iinfo.iconSize;
                int iconDrawSize = (int)(iconSize * (ii.iconScale > 0 ? ii.iconScale : (double)defaultCanvasSize / iconSize));
                iconDrawSize = (int)(iconDrawSize * IconCanvasScale);

                var iconImage = LoadImageFromMod(ii.iconPath, iconDrawSize);
                if (iconImage is null)
                    continue;

                //draw the icon onto a layer (that we will apply tint to and blend with canvas)
                Bitmap layerSlice = new Bitmap(canvas.Width, canvas.Height, canvas.PixelFormat);
                using (Graphics g = Graphics.FromImage(layerSlice))
                    g.DrawImageUnscaled(iconImage, (IconCanvasSize / 2) - (iconDrawSize / 2) + ii.iconOffset.X, (IconCanvasSize / 2) - (iconDrawSize / 2) + ii.iconOffset.Y);

                //grab the layer data
                BitmapData layerData = layerSlice.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadOnly, canvas.PixelFormat);
                byte[] layerPixels = new byte[bCount];
                IntPtr ptrLayerFPixel = layerData.Scan0;
                Marshal.Copy(ptrLayerFPixel, layerPixels, 0, layerPixels.Length);

                //blend -> for each value in 0->1 (so when multiplying, you have to divide by 255 if in 0->255)
                //newCanvas(A/R/G/B) = Layer(A/R/G/B) * tint(A/R/G/B)   +   oldCanvas(A/R/G/B) * (1 - tint(A) * Layer(A))
                //https://www.factorio.com/blog/post/fff-172
                for (int y = 0; y < heightInPixels; y++) {
                    int currentLine = y * canvasData.Stride;
                    for (int x = 0; x < widthInBytes; x = x + cBPP) {
                        int canvasMulti = 255 - (ii.iconTint.A * layerPixels[currentLine + x + 3] / 255);
                        canvasPixels[currentLine + x + 0] = (byte)Math.Min(255,
                            (layerPixels[currentLine + x + 0] * ii.iconTint.B / 255) +
                            (canvasPixels[currentLine + x + 0] * canvasMulti / 255));
                        canvasPixels[currentLine + x + 1] = (byte)Math.Min(255,
                            (layerPixels[currentLine + x + 1] * ii.iconTint.G / 255) +
                            (canvasPixels[currentLine + x + 1] * canvasMulti / 255));
                        canvasPixels[currentLine + x + 2] = (byte)Math.Min(255,
                            (layerPixels[currentLine + x + 2] * ii.iconTint.R / 255) +
                            (canvasPixels[currentLine + x + 2] * canvasMulti / 255));
                        canvasPixels[currentLine + x + 3] = (byte)Math.Min(255,
                            (layerPixels[currentLine + x + 3] * ii.iconTint.A / 255) +
                            (canvasPixels[currentLine + x + 3] * canvasMulti / 255));

                    }
                }
                layerSlice.UnlockBits(layerData);
            }

            //we are done adding all the layers, so copy the canvas data
            Marshal.Copy(canvasPixels, 0, ptrCanvasFPixel, canvasPixels.Length);
            canvas.UnlockBits(canvasData);

            //at this point we need to convert the canvas into a non-alpha multiplied format due to winforms having issues with it
            Bitmap result = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImageUnscaled(canvas, 0, 0);

            //finally, calculate the average color (yes, it comes out a bit different due to inclusion of transparency)
            Color averageColor = GetAverageColor(result);
            if (averageColor.GetBrightness() > 0.9)
                result = AddBorder(result); //if the image is too bright, add a border to it. Honestly, this is never done anymore - it was useful before layer blending was fixed and some icons came out... white.
            if (averageColor.GetBrightness() > 0.7)
                averageColor = Color.FromArgb(255, (int)(averageColor.R * 0.7), (int)(averageColor.G * 0.7), (int)(averageColor.B * 0.7));

            return new IconColorPair(result, averageColor);
        }

        private Bitmap? LoadImageFromMod(string fileName, int resultSize = 32) //NOTE: must make sure we use pre-multiplied alpha
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            if (string.IsNullOrEmpty(fileName))
                return null;
            fileName = fileName.ToLower().Replace("/", "\\");
            while (fileName.IndexOf("\\\\") != -1) //found this error in krastorio - apparently factorio ignores multiple slashes in file name
                fileName = fileName.Replace("\\\\", "\\");

            //if the image isnt currently in the cache, process it and add it to cache
            if (!bitmapCache.ContainsKey(fileName)) {
                TotalPathCount++;
                string origin = fileName.Substring(0, fileName.IndexOf("__", 2) + 2);
                string file = fileName.Substring(fileName.IndexOf("__", 2) + 3);

                if (folderLinks.ContainsKey(origin)) {

                    file = Path.Combine(folderLinks[origin], file);
                    try { bitmapCache.Add(fileName, new Bitmap(file)); } catch (Exception ex) {
                        bitmapCache.Add(fileName, null);
                        FailedPathCount++;
                        ErrorLogging.LogException(ex, "IconCacheProcessor: failed to load icon from file " + fileName);
                    }

                } else if (archiveFileLinks?.TryGetValue(fileName, out var entry) is true) {
                    try { bitmapCache.Add(fileName, new Bitmap(entry.Open())); } catch (Exception ex) {
                        bitmapCache.Add(fileName, null);
                        FailedPathCount++;
                        ErrorLogging.LogException(ex, "IconCacheProcessor: failed to load icon from archive " + fileName);
                    }

                } else {
                    FailedPathCount++;
                    bitmapCache.Add(fileName, null);
                    ErrorLogging.LogLine("IconCacheProcessor: given fileName not found in mod folders: " + fileName);
                }
            }

            if (bitmapCache[fileName] is not Bitmap image)
                return null;

            //draw it to correct size.
            var bmp = new Bitmap(resultSize, resultSize, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, new Rectangle(0, 0, (resultSize * image.Width / image.Height), resultSize));
            }
            return bmp;
        }

        private Color GetAverageColor(Bitmap icon) {
            if (icon == null)
                return Color.Black;

            BitmapData iconData = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.ReadOnly, icon.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(icon.PixelFormat) / 8;
            int byteCount = iconData.Stride * icon.Height;
            byte[] iconPixels = new byte[byteCount];
            IntPtr ptrFirstPixel = iconData.Scan0;
            Marshal.Copy(ptrFirstPixel, iconPixels, 0, iconPixels.Length);
            int heightInPixels = iconData.Height;
            int widthInBytes = iconData.Width * bytesPerPixel;

            int[] totalPixel = { 0, 0, 0, 0 };
            int totalCounter = 1; //just to avoid div by 0 in case of completely empty bitmap
            for (int y = 0; y < heightInPixels; y++) {
                int currentLine = y * iconData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel) {
                    if (iconPixels[currentLine + x + 3] > 10) //ignore transparent pixels
                    {
                        totalPixel[3] += iconPixels[currentLine + x];     //B
                        totalPixel[2] += iconPixels[currentLine + x + 1]; //G
                        totalPixel[1] += iconPixels[currentLine + x + 2]; //R
                        totalCounter++;
                    }
                }
            }
            for (int i = 1; i < 4; i++) {
                totalPixel[i] /= totalCounter;
                totalPixel[i] = Math.Min(totalPixel[i], 255);
            }
            icon.UnlockBits(iconData);

            return Color.FromArgb(255, totalPixel[1], totalPixel[2], totalPixel[3]);
        }

        private const int iconBorder = 1; //border is drawn on a new layer as 
        private Bitmap AddBorder(Bitmap icon) {
            Bitmap canvas = new Bitmap(icon.Width, icon.Height, icon.PixelFormat);
            BitmapData iconData = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.ReadOnly, icon.PixelFormat);
            BitmapData canvasData = canvas.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.WriteOnly, icon.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(icon.PixelFormat) / 8; //same for both
            int byteCount = iconData.Stride * icon.Height; //same for both
            byte[] iconPixels = new byte[byteCount];
            byte[] canvasPixels = new byte[byteCount];

            IntPtr ptrFirstPixel = iconData.Scan0;
            Marshal.Copy(ptrFirstPixel, iconPixels, 0, iconPixels.Length);
            int heightInPixels = iconData.Height;
            int widthInBytes = iconData.Width * bytesPerPixel;

            for (int y = iconBorder; y < heightInPixels - iconBorder; y++) {
                int currentLine = y * iconData.Stride;
                for (int x = iconBorder * bytesPerPixel; x < widthInBytes - iconBorder * bytesPerPixel; x += bytesPerPixel) {
                    if (iconPixels[currentLine + x + 3] > 11) //check if A >= 10
                    {
                        for (int iy = -iconBorder; iy <= iconBorder; iy++) {
                            for (int ix = -iconBorder * bytesPerPixel; ix <= iconBorder * bytesPerPixel; ix += bytesPerPixel) {
                                int currentCanvasIndex = currentLine + iy * iconData.Stride + x + ix;
                                canvasPixels[currentCanvasIndex] = 64;
                                canvasPixels[currentCanvasIndex + 1] = 64;
                                canvasPixels[currentCanvasIndex + 2] = 64;
                                canvasPixels[currentCanvasIndex + 3] = 64;
                            }
                        }
                    }
                }
            }
            ptrFirstPixel = canvasData.Scan0;
            Marshal.Copy(canvasPixels, 0, ptrFirstPixel, canvasPixels.Length);
            icon.UnlockBits(iconData);
            canvas.UnlockBits(canvasData);

            //draw the processed icon (singluar) onto the main canvas
            using (Graphics g = Graphics.FromImage(canvas)) {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImageUnscaled(icon, 0, 0);
            }

            return canvas;
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    archiveFileLinks?.Clear();
                    archiveFileLinks = null;

                    foreach (Bitmap? bitmap in bitmapCache.Values)
                        bitmap?.Dispose();
                    bitmapCache.Clear();

                    foreach (ZipArchive zip in openedArchives)
                        zip.Dispose();
                    openedArchives.Clear();
                }
                disposedValue = true;
            }
        }
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private static Dictionary<KeyValuePair<Bitmap, Bitmap>, Bitmap> combinedBitmapDictionary = new Dictionary<KeyValuePair<Bitmap, Bitmap>, Bitmap>();
        private const double qualitySizeMultiplier = 0.5;
        public static Bitmap? CombinedQualityIcon(Bitmap? baseIcon, Bitmap? qualityIcon) {
            if (baseIcon is null || qualityIcon is null)
                return null;

            if (combinedBitmapDictionary.TryGetValue(new KeyValuePair<Bitmap, Bitmap>(baseIcon, qualityIcon), out Bitmap? combinedBitmap))
                return combinedBitmap;

            //combine the two bitmaps
            Bitmap canvas = new Bitmap(baseIcon.Width, baseIcon.Height, baseIcon.PixelFormat);
            using (Graphics g = Graphics.FromImage(canvas)) {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(baseIcon, new Rectangle(0, 0, baseIcon.Width, baseIcon.Height));
                g.DrawImage(qualityIcon, new Rectangle((int)(baseIcon.Width * (1 - qualitySizeMultiplier)), (int)(baseIcon.Height * (1 - qualitySizeMultiplier)), (int)(baseIcon.Width * qualitySizeMultiplier), (int)(baseIcon.Height * qualitySizeMultiplier)));
            }
            combinedBitmapDictionary.Add(new KeyValuePair<Bitmap, Bitmap>(baseIcon, qualityIcon), canvas);
            return canvas;
        }
    }
}