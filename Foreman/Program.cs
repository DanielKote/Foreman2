using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace Foreman
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            //test2(); return;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}


        static void test2() //zip file image read test
        {
            string zipLocation = Path.Combine(Application.StartupPath, "reskins-bobs_2.0.2.zip");
            string tempLocation = Path.Combine(new string[] { Application.StartupPath, "zip-temp" });

            Stopwatch stopwatch = new Stopwatch();

            if (Directory.Exists(tempLocation))
                Directory.Delete(tempLocation, true);
            if (!Directory.Exists(tempLocation))
                Directory.CreateDirectory(tempLocation);

            stopwatch.Start();
            ZipFile.ExtractToDirectory(zipLocation, tempLocation);
            string[] iconFiles = Directory.GetFiles(tempLocation, "*.png", SearchOption.AllDirectories);
            List<Bitmap> bitmaps = new List<Bitmap>();
            foreach (string iconFile in iconFiles)
            {
                try
                {
                    using (Bitmap image = new Bitmap(iconFile)) //If you don't do this, the file is locked for the lifetime of the bitmap
                    {
                        Bitmap bmp = new Bitmap(64, 64, PixelFormat.Format32bppPArgb);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(image, new Rectangle(0, 0, (64 * image.Width / image.Height), 64));
                        }
                        bitmaps.Add(bmp);
                    }
                }
                catch (Exception) { bitmaps.Add(null); }
            }
            if (Directory.Exists(tempLocation))
                Directory.Delete(tempLocation, true);
            stopwatch.Stop();

            Console.WriteLine(bitmaps.Count);
            bitmaps.Clear();
            Console.WriteLine("Unzip and read all bitmaps (time elapsed: " + stopwatch.ElapsedMilliseconds + ")");

            stopwatch.Reset();
            stopwatch.Start();

            using(ZipArchive zfile = ZipFile.Open(zipLocation, ZipArchiveMode.Read)) //<<<--------------------------------BEST
            {
                foreach(ZipArchiveEntry zentity in zfile.Entries)
                {
                    if (Path.GetExtension(zentity.Name).ToLower() == ".png")
                    {
                        try
                        {
                            using (Bitmap image = new Bitmap(zentity.Open())) //If you don't do this, the file is locked for the lifetime of the bitmap
                            {
                                Bitmap bmp = new Bitmap(64, 64, PixelFormat.Format32bppPArgb);
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    g.DrawImage(image, new Rectangle(0, 0, (64 * image.Width / image.Height), 64));
                                }
                                bitmaps.Add(bmp);
                            }
                        }
                        catch (Exception) { bitmaps.Add(null); }
                    }
                }
            }

            stopwatch.Stop();

            Console.WriteLine(bitmaps.Count);
            bitmaps.Clear();
            Console.WriteLine("read all bitmaps from zipfile directly (time elapsed: " + stopwatch.ElapsedMilliseconds + ")");



        }

        static void test() //factorio console launch & data loading
        {
            string factorioPath = Path.Combine(new string[] { "D:\\", "software", "Factorios", "foreman", "Factorio_1.1 - vanilla - foreman" });
            string modPath = Path.Combine(factorioPath, "mods");

            Directory.CreateDirectory(Path.Combine(modPath, "foremanexport_0.1.1"));
            File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_0.1.1", "info.json" }), Path.Combine(new string[] { modPath, "foremanexport_0.1.1", "info.json" }), true);
            File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_0.1.1", "instrument-after-data.lua" }), Path.Combine(new string[] { modPath, "foremanexport_0.1.1", "instrument-after-data.lua" }), true);
            File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_0.1.1", "instrument-control - nn.lua" }), Path.Combine(new string[] { modPath, "foremanexport_0.1.1", "instrument-control.lua" }), true);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "D:\\software\\Factorios\\foreman\\Factorio_1.1 - vanilla - foreman\\bin\\x64\\factorio.exe";

            process.StartInfo.Arguments = "--create temp-save.zip";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();
            while (!process.HasExited) { }
            process.StartInfo.Arguments = "--instrument-mod foremanexport --benchmark temp-save.zip";
            process.Start();
            string resultString = "";
            while (!process.HasExited)
            {
                resultString += process.StandardOutput.ReadToEnd();
            }
            if (File.Exists("temp-save.zip"))
                File.Delete("temp-save.zip");
            if (Directory.Exists(Path.Combine(modPath, "foremanexport_0.1.1")))
                Directory.Delete(Path.Combine(modPath, "foremanexport_0.1.1"), true);

            //Console.WriteLine(resultString);
            //return;

            string iconString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P1>>>") + 23);
            iconString = iconString.Substring(0, iconString.IndexOf("<<<END-EXPORT-P1>>>") - 2);

            string dataString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P2>>>") + 23);
            dataString = dataString.Substring(0, dataString.IndexOf("<<<END-EXPORT-P2>>>") - 2);

            JObject iconJObject = JObject.Parse(iconString);
            JObject dataJObject = JObject.Parse(dataString);

            Console.WriteLine(dataString);

            //JObject resut = JObject.Parse(q);
            //Console.Write(resut.ToString(Newtonsoft.Json.Formatting.Indented));

        }
    }
}
