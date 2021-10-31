using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Foreman
{
    [Serializable]
    public struct IconColorPair
    {
        public Bitmap Icon;
        public Color Color;
        public IconColorPair(Bitmap icon, Color color)
        {
            this.Icon = icon;
            this.Color = color;
        }
    }
    [Serializable]
    public class IconBitmapCollection
    {
        public Dictionary<int, IconColorPair> Icons;
        public IconBitmapCollection() { Icons = new Dictionary<int, IconColorPair>(); }
    }

    public struct IconInfo
    {
        public string iconPath;
        public int iconSize;
        public double iconScale;
        public Point iconOffset;
        public Color iconTint;

        public IconInfo(string iconPath, int iconSize)
        {
            this.iconPath = iconPath;
            this.iconSize = iconSize;
            this.iconScale = iconSize > 0? 32/iconSize : 1;
            this.iconOffset = new Point(0, 0);
            iconTint = IconProcessor.NoTint;
        }

        public void SetIconTint(double a, double r, double g, double b)
        {
            a = (a <= 1 ? a * 255 : a);
            r = (r <= 1 ? r * 255 : r);
            g = (g <= 1 ? g * 255 : g);
            b = (b <= 1 ? b * 255 : b);
            iconTint = Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }
    }

    public static class IconProcessor
    {
        internal static readonly Color NoTint = Color.FromArgb(255, 255, 255, 255);
        internal static readonly Pen HiddenSlashPen = new Pen(new SolidBrush(Color.DarkRed), 4);

        private static Bitmap unknownIcon;
        public static Bitmap GetUnknownIcon()
        {
            if (unknownIcon == null)
            {
                unknownIcon = LoadImage("UnknownIcon.png");
                if (unknownIcon == null)
                {
                    unknownIcon = new Bitmap(32, 32);
                    using (Graphics g = Graphics.FromImage(unknownIcon))
                    {
                        g.FillRectangle(Brushes.White, 0, 0, 32, 32);
                    }
                }
            }
            return unknownIcon;
        }

        public static IconColorPair GetIconAndColor(IconInfo iinfo, List<IconInfo> iinfos, int defaultCanvasSize)
        {
            if (iinfos == null)
                iinfos = new List<IconInfo>();
            double IconCanvasScale = defaultCanvasSize == 32 ? 2 : 1; //just some upscailing for icons (64x64 look better)
            int IconCanvasSize = (int)(defaultCanvasSize * IconCanvasScale);

            if (iinfo.iconPath.Contains("wind_turbine_icon"))
                Console.WriteLine("!!");

            if(iinfos.Count == 0) //if there are no icons, use the single icon
                iinfos.Add(iinfo);

            //quick check to ensure it isnt a null icon
            bool empty = true;
            foreach(IconInfo ii in iinfos)
            {
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

            foreach (IconInfo ii in iinfos)
            {
                //load the image and prep it for processing
                int iconSize = ii.iconSize > 0 ? ii.iconSize : iinfo.iconSize;
                int iconDrawSize = (int)(iconSize * (ii.iconScale > 0 ? ii.iconScale : (double)defaultCanvasSize / iconSize));
                iconDrawSize = (int)(iconDrawSize * IconCanvasScale);

                Bitmap iconImage = LoadImage(ii.iconPath, iconDrawSize);
                if (iconImage == null)
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
                for (int y = 0; y < heightInPixels; y++)
                {
                    int currentLine = y * canvasData.Stride;
                    for (int x = 0; x < widthInBytes; x = x + cBPP)
                    {
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

        private static Bitmap LoadImage(string fileName, int resultSize = 32) //NOTE: must make sure we use pre-multiplied alpha
        {
            if (String.IsNullOrEmpty(fileName))
            { return null; }

            string fullPath = "";
            if (File.Exists(fileName))
            {
                fullPath = fileName;
            }
            else if (File.Exists(Application.StartupPath + "\\" + fileName))
            {
                fullPath = Application.StartupPath + "\\" + fileName;
            }
            else
            {
                string[] splitPath = fileName.Split('/');
                splitPath[0] = splitPath[0].Trim('_');

                if (FactorioModsProcessor.Mods.Any(m => m.Name == splitPath[0]))
                {
                    fullPath = FactorioModsProcessor.Mods.First(m => m.Name == splitPath[0]).dir;
                }

                if (!String.IsNullOrEmpty(fullPath))
                {
                    for (int i = 1; i < splitPath.Count(); i++) //Skip the first split section because it's the mod name, not a directory
                    {
                        fullPath = Path.Combine(fullPath, splitPath[i]);
                    }
                }
            }

            if (!File.Exists(fullPath))
                return null;

            try
            {
                using (Bitmap image = new Bitmap(fullPath)) //If you don't do this, the file is locked for the lifetime of the bitmap
                {
                    Bitmap bmp = new Bitmap(resultSize, resultSize, PixelFormat.Format32bppPArgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(image, new Rectangle(0, 0, (resultSize * image.Width / image.Height), resultSize));
                    }
                    return bmp;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Color GetAverageColor(Bitmap icon)
        {
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
            int totalCounter = 0;
            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * iconData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    if (iconPixels[currentLine + x + 3] > 10) //ignore transparent pixels
                    {
                        totalPixel[3] += iconPixels[currentLine + x];     //B
                        totalPixel[2] += iconPixels[currentLine + x + 1]; //G
                        totalPixel[1] += iconPixels[currentLine + x + 2]; //R
                        totalCounter++;
                    }
                }
            }
            for (int i = 1; i < 4; i++)
            {
                totalPixel[i] /= totalCounter;
                totalPixel[i] = Math.Min(totalPixel[i], 255);
            }
            icon.UnlockBits(iconData);

            return Color.FromArgb(255, totalPixel[1], totalPixel[2], totalPixel[3]);
        }

        private const int iconBorder = 1; //border is drawn on a new layer as 
        public static Bitmap AddBorder(Bitmap icon)
        {
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

            for (int y = iconBorder; y < heightInPixels - iconBorder; y++)
            {
                int currentLine = y * iconData.Stride;
                for (int x = iconBorder * bytesPerPixel; x < widthInBytes - iconBorder * bytesPerPixel; x += bytesPerPixel)
                {
                    if(iconPixels[currentLine + x + 3] > 11) //check if A >= 10
                    {
                        for (int iy = -iconBorder; iy <= iconBorder; iy++)
                        {
                            for (int ix = -iconBorder * bytesPerPixel; ix <= iconBorder * bytesPerPixel; ix += bytesPerPixel)
                            {
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
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImageUnscaled(icon, 0, 0);
            }

            return canvas;
        }
    }
}
