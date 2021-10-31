//#define IgnoreIcons

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
        internal static readonly Color NoTint = Color.FromArgb(255, 0, 0, 0);
        internal static readonly Pen HiddenSlashPen = new Pen(new SolidBrush(Color.DarkRed), 4);
        internal static readonly int IconCanvasSize = 64;

        private static Bitmap unknownIcon;
        private static Bitmap unknownBWIcon;
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
#if IgnoreIcons
            return new IconColorPair(GetUnknownIcon(), Color.Black);
#endif
            if (iinfos == null)
                iinfos = new List<IconInfo>();
            int mainIconSize = iinfo.iconSize > 0 ? iinfo.iconSize : defaultCanvasSize;
            double IconCanvasScale = (double)IconCanvasSize / mainIconSize;
            //if (iinfos.Count > 0 && iinfos[0].iconSize > 0 && iinfos[0].iconScale == 0) mainIconSize = iinfos[0].iconSize;

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

            Bitmap icon = new Bitmap(IconCanvasSize, IconCanvasSize, PixelFormat.Format32bppArgb);
            //using(Graphics g = Graphics.FromImage(icon)) { g.FillRectangle(Brushes.Gray, new Rectangle(0, 0, icon.Width, icon.Height)); }
            foreach (IconInfo ii in iinfos)
            {
                //load the image and prep it for processing
                int iconSize = ii.iconSize > 0 ? ii.iconSize : iinfo.iconSize;
                int iconDrawSize = (int)(iconSize * (ii.iconScale > 0 ? ii.iconScale : (double)mainIconSize / iconSize));
                iconDrawSize = (int)(iconDrawSize * IconCanvasScale);

                Bitmap iconImage = LoadImage(ii.iconPath, iconDrawSize);

                //apply tint (if necessary)
                //NOTE: tint is applied as pre-multiplied alpha, so: A(result) = A(original); RGB(result) = RGB(tint) + RGB(original) * (255 - A(tint))
                if (ii.iconTint != NoTint)
                {
                    BitmapData iconData = iconImage.LockBits(new Rectangle(0, 0, iconImage.Width, iconImage.Height), ImageLockMode.ReadWrite, iconImage.PixelFormat);
                    int bytesPerPixel = Bitmap.GetPixelFormatSize(iconImage.PixelFormat) / 8;
                    int byteCount = iconData.Stride * iconImage.Height;
                    byte[] pixels = new byte[byteCount];
                    IntPtr ptrFirstPixel = iconData.Scan0;
                    Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
                    int heightInPixels = iconData.Height;
                    int widthInBytes = iconData.Width * bytesPerPixel;

                    for (int y = 0; y < heightInPixels; y++)
                    {
                        int currentLine = y * iconData.Stride;
                        for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            int pixelA = pixels[currentLine + x + 3];
                            if (pixelA > 0)
                            {
                                // calculate new pixel value
                                pixels[currentLine + x] = (byte)Math.Min((int)ii.iconTint.B + (pixelA * (255 - ii.iconTint.A) * pixels[currentLine + x] / 65025), 255);
                                pixels[currentLine + x + 1] = (byte)Math.Min((int)ii.iconTint.G + (pixelA * (255 - ii.iconTint.A) * pixels[currentLine + x + 1] / 65025), 255);
                                pixels[currentLine + x + 2] = (byte)Math.Min((int)ii.iconTint.R + (pixelA * (255 - ii.iconTint.A) * pixels[currentLine + x + 2] / 65025), 255);
                            }
                        }
                    }
                    // copy modified bytes back
                    Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
                    iconImage.UnlockBits(iconData);
                }

                //draw the processed icon (singluar) onto the main canvas
                using(Graphics g = Graphics.FromImage(icon))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    //prepare drawing zone
                    Rectangle iconBorder = new Rectangle(
                        (IconCanvasSize / 2) - (iconDrawSize / 2) + ii.iconOffset.X,
                        (IconCanvasSize / 2) - (iconDrawSize / 2) + ii.iconOffset.Y,
                        iconDrawSize,
                        iconDrawSize);
                    g.DrawImage(iconImage, iconBorder);
                }

            }
            Color averageColor = GetAverageColor(icon);
            if (averageColor.GetBrightness() > 0.9)
                icon = AddBorder(icon);
            if (averageColor.GetBrightness() > 0.7)
                averageColor = Color.FromArgb((int)(averageColor.A * 0.7), (int)(averageColor.R * 0.7), (int)(averageColor.G * 0.7), (int)(averageColor.B * 0.7));

            return new IconColorPair(icon, averageColor);
        }

        private static Bitmap LoadImage(String fileName, int resultSize = 32)
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

                if (DataCache.Mods.Any(m => m.Name == splitPath[0]))
                {
                    fullPath = DataCache.Mods.First(m => m.Name == splitPath[0]).dir;
                }

                if (!String.IsNullOrEmpty(fullPath))
                {
                    for (int i = 1; i < splitPath.Count(); i++) //Skip the first split section because it's the mod name, not a directory
                    {
                        fullPath = Path.Combine(fullPath, splitPath[i]);
                    }
                }
            }

            try
            {
                using (Bitmap image = new Bitmap(fullPath)) //If you don't do this, the file is locked for the lifetime of the bitmap
                {
                    Bitmap bmp = new Bitmap(resultSize, resultSize);
                    using (Graphics g = Graphics.FromImage(bmp))
                        g.DrawImage(image, new Rectangle(0, 0, (resultSize * image.Width / image.Height), resultSize));
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
            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * iconData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    totalPixel[3] += iconPixels[currentLine + x];     //B
                    totalPixel[2] += iconPixels[currentLine + x + 1]; //G
                    totalPixel[1] += iconPixels[currentLine + x + 2]; //R
                    totalPixel[0] += iconPixels[currentLine + x + 3]; //A
                }
            }
            for (int i = 0; i < 4; i++)
            {
                totalPixel[i] /= (byteCount / bytesPerPixel);
                totalPixel[i] = Math.Min(totalPixel[i], 255);
            }
            icon.UnlockBits(iconData);

            return Color.FromArgb(totalPixel[0], totalPixel[1], totalPixel[2], totalPixel[3]);
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

        //Asad Butt, Mon Oct 24 2016, 0fnt, "Convert an image to grayscale", Feb 15 '10 at 12:37, http://stackoverflow.com/a/2265990
        //not used at the moment, left behind just in case
        public static Bitmap MakeMonochrome(Bitmap src)
        {
            if (src == null) { return null; }


            Bitmap dst = new Bitmap(src.Width, src.Height);
            Graphics g = Graphics.FromImage(dst);

            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            ImageAttributes attrib = new ImageAttributes();

            attrib.SetColorMatrix(colorMatrix);

            g.DrawImage(src,
                new Rectangle(0, 0, src.Width, src.Height),
                0, 0, src.Width, src.Height,
                GraphicsUnit.Pixel, attrib
                );

            g.Dispose();
            return dst;
        }
    }
}
