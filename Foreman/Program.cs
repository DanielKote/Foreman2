using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
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
			//test10(); return;

			ErrorLogging.ClearLog();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		public static void test10()
		{
			List<List<int>> technologies = new List<List<int>>();
			technologies.Add(new List<int>(new int[] { 1, 2, 3 }));
			technologies.Add(new List<int>(new int[] { 1, 2 }));
			technologies.Add(new List<int>(new int[] { 1, 4 }));

			List<List<int>> sciPackLists = new List<List<int>>();
			foreach (List<int> tech in technologies)
			{
				bool exists = false;
				foreach (List<int> sciPackList in sciPackLists.ToList())
				{
					if (!sciPackList.Except(tech).Any()) // sci pack lists already includes a list that is a subset of the technologies sci pack list (ex: already have A+B while tech's is A+B+C)
						exists = true;
					else if (!tech.Except(sciPackList).Any()) //technology sci pack list is a subset of an already included sci pack list. we will add thi to the list and delete the existing one (ex: have A+B while tech's is A -> need to remove A+B and include A)
						sciPackLists.Remove(sciPackList);
				}
				if (!exists)
					sciPackLists.Add(tech);
			}

			foreach(List<int> sciPackList in sciPackLists)
			{
				foreach (int sciPack in sciPackList)
					Console.Write(sciPack + ", ");
				Console.WriteLine();
			}
		}

		public static void test9()
		{
			Console.WriteLine(double.PositiveInfinity);
			Console.WriteLine(double.NegativeInfinity);
		}

		public static void test8()
		{
			Point a = new Point(0, 0);
			Point b = new Point(100, 20);
			int c = 25;
			bool horizontal = true;

			if (horizontal)
				Console.WriteLine(new Point(a.X + ((b.X - a.X) * (c - a.Y) / (b.Y - a.Y)), c));
			else
				Console.WriteLine(new Point(c, a.Y + ((b.Y - a.Y) * (c - a.X) / (b.X - a.X))));
		}

		public static void test7()
		{
			double ActualAssemblerCount = 59.999;
			double rateMultiplier = 60;
			double partialAssemblers = ActualAssemblerCount % rateMultiplier;
			double fullAssemblers = ActualAssemblerCount - partialAssemblers;
			Console.WriteLine(fullAssemblers);
			Console.WriteLine(partialAssemblers);

			Console.WriteLine(12.123 % 1);
		}

		public static void test6()
		{
			string[] efficincies = new string[] { "e1", "e2", "e3", "s1", "s2", "s3", "s1", "s2", "s3" };
			string[] speeds = new string[] { "s1", "s2", "s3", "s1", "s2", "s3", "s1", "s2", "s3" };
			string[] orderedOptions = efficincies.Concat(speeds).ToArray();
			int moduleSize = 16;

			double expected = nCr(orderedOptions.Length + moduleSize - 1, moduleSize);

			for (int i = 0; i < 20; i++)
				Console.WriteLine(nCr(12 + i - 1, i));

			if (expected < 1000000)
			{
				List<string[]> permutations = new List<string[]>();
				string[] permu = new string[moduleSize];
				AddItem(permu, 0, 0);

				void AddItem(string[] permutation, int itemIndex, int startingIndex)
				{
					if (itemIndex == permutation.Length)
						permutations.Add(permutation.ToArray());
					else
					{
						for (int i = startingIndex; i < orderedOptions.Length; i++)
						{
							permutation[itemIndex] = orderedOptions[i];
							AddItem(permutation, itemIndex + 1, i);
						}
					}
				}
				Console.WriteLine(permutations.Count);
			}
			Console.WriteLine(expected);



			double nCr(int n, int r)
			{
				// naive: return Factorial(n) / (Factorial(r) * Factorial(n - r));
				return nPr(n, r) / Factorial(r);
			}

			double nPr(int n, int r)
			{
				// naive: return Factorial(n) / Factorial(n - r);
				return FactorialDivision(n, n - r);
			}

			double FactorialDivision(int topFactorial, int divisorFactorial)
			{
				double result = 1;
				for (int i = topFactorial; i > divisorFactorial; i--)
					result *= i;
				return result;
			}

			double Factorial(double i)
			{
				if (i <= 1)
					return 1;
				return i * Factorial(i - 1);
			}

		}

		public struct testStruct
		{
			public bool available;
			public bool enabled;
			public string name;

			public testStruct(bool av, bool en, string na) { available = av; enabled = en; name = na; }
		}

		static void test5()
		{


			List<testStruct> testList = new List<testStruct>();
			testList.Add(new testStruct(true, true, "a1"));
			testList.Add(new testStruct(false, true, "b1"));
			testList.Add(new testStruct(true, false, "c1"));
			testList.Add(new testStruct(false, false, "d1"));
			testList.Add(new testStruct(true, true, "a2"));
			testList.Add(new testStruct(false, true, "b2"));
			testList.Add(new testStruct(true, false, "c2"));
			testList.Add(new testStruct(false, false, "d2"));
			foreach (testStruct ts in testList.OrderByDescending(t => t.available).ThenByDescending(t => t.enabled).ThenBy(t => t.name))
				Console.WriteLine(ts.available + ", " + ts.enabled + ", " + ts.name);
		}

		static void test4()
		{
			SubgroupPrototype sg = new SubgroupPrototype(null, "sg", "-");

			ItemPrototype i1 = new ItemPrototype(null, "i1", "i1", sg, "-");
			ItemPrototype i2 = new ItemPrototype(null, "i2", "i2", sg, "-");
			ItemPrototype i3 = new ItemPrototype(null, "i3", "i3", sg, "-");
			ItemPrototype i4 = new ItemPrototype(null, "i4", "i4", sg, "-");
			ItemPrototype i5 = new ItemPrototype(null, "i5", "i5", sg, "-");


			RecipePrototype a = new RecipePrototype(null, "a", "a", sg, "-");
			a.InternalOneWayAddIngredient(i1, 1);
			a.InternalOneWayAddIngredient(i2, 1);
			a.InternalOneWayAddProduct(i5, 1, 1);

			RecipePrototype b = new RecipePrototype(null, "a", "a", sg, "-");
			b.InternalOneWayAddIngredient(i1, 1);
			b.InternalOneWayAddIngredient(i2, 1);
			b.InternalOneWayAddProduct(i5, 1, 1);

			RecipePrototype c = new RecipePrototype(null, "a", "a", sg, "-");
			c.InternalOneWayAddIngredient(i1, 10);
			c.InternalOneWayAddIngredient(i2, 10);
			c.InternalOneWayAddProduct(i5, 1, 1);

			RecipePrototype d = new RecipePrototype(null, "a", "a", sg, "-");
			d.InternalOneWayAddIngredient(i3, 1);
			d.InternalOneWayAddIngredient(i4, 1);
			d.InternalOneWayAddProduct(i5, 1, 1);

			HashSet<Recipe> recipes = new HashSet<Recipe>();
			HashSet<Recipe> missingRecipes = new HashSet<Recipe>(new RecipeNaInPrComparer());

			recipes.Add(a);
			recipes.Add(b);
			recipes.Add(c);
			recipes.Add(d);

			missingRecipes.Add(a);
			missingRecipes.Add(b);
			missingRecipes.Add(c);
			missingRecipes.Add(d);

			Console.WriteLine("Regular hashset:");
			foreach (Recipe r in recipes)
				Console.WriteLine(r);
			Console.WriteLine("Deep hashset:");
			foreach (Recipe r in missingRecipes)
				Console.WriteLine(r);

		}

		static void test3()
		{
			string path = "D:/software/Factorios/foreman";
			string folder = "test";
			Console.WriteLine(Path.Combine(path, folder));
			Console.WriteLine(Path.GetDirectoryName(Path.Combine(path, folder)));
			Console.WriteLine(Path.GetFullPath(Path.Combine(path, folder)));

			string fileName = "__base__/graphics/technology/speed-module-1.png";
			string origin = fileName.Substring(0, fileName.IndexOf("__", 2) + 2);
			string file = fileName.Substring(fileName.IndexOf("__", 2) + 3);
			Console.WriteLine(origin);
			Console.WriteLine(file);

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

			using (ZipArchive zfile = ZipFile.Open(zipLocation, ZipArchiveMode.Read)) //<<<--------------------------------BEST
			{
				foreach (ZipArchiveEntry zentity in zfile.Entries)
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
			string factorioPath = Path.GetFullPath("D:\\software\\Factorios\\Factorio_1.0.0 - krastorio");
			string modPath = Path.Combine(factorioPath, "mods");

			Directory.CreateDirectory(Path.Combine(modPath, "foremanexport_0.1.1"));
			File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_0.1.1", "info.json" }), Path.Combine(new string[] { modPath, "foremanexport_0.1.1", "info.json" }), true);
			File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_0.1.1", "instrument-after-data.lua" }), Path.Combine(new string[] { modPath, "foremanexport_0.1.1", "instrument-after-data.lua" }), true);
			File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_0.1.1", "instrument-control - nn.lua" }), Path.Combine(new string[] { modPath, "foremanexport_0.1.1", "instrument-control.lua" }), true);

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			process.StartInfo.FileName = Path.Combine(new string[] { factorioPath, "bin", "x64", "factorio.exe" });

			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;

			string resultString = "";
			process.StartInfo.Arguments = "--create temp-save.zip";
			process.Start();
			while (!process.HasExited)
			{
				resultString += process.StandardOutput.ReadToEnd();
				Thread.Sleep(100);
			}
			resultString += process.StandardOutput.ReadToEnd();
			Console.WriteLine(resultString);


			resultString = "";
			process.StartInfo.Arguments = "--instrument-mod foremanexport --benchmark temp-save.zip";
			process.Start();
			while (!process.HasExited)
			{
				resultString += process.StandardOutput.ReadToEnd();
				Thread.Sleep(100);
			}
			resultString += process.StandardOutput.ReadToEnd();

			if (File.Exists("temp-save.zip"))
				File.Delete("temp-save.zip");
			if (Directory.Exists(Path.Combine(modPath, "foremanexport_0.1.1")))
				Directory.Delete(Path.Combine(modPath, "foremanexport_0.1.1"), true);

			Console.WriteLine(resultString);
			return;
			/*
			string iconString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P1>>>") + 23);
			iconString = iconString.Substring(0, iconString.IndexOf("<<<END-EXPORT-P1>>>") - 2);

			string dataString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P2>>>") + 23);
			dataString = dataString.Substring(0, dataString.IndexOf("<<<END-EXPORT-P2>>>") - 2);

			JObject iconJObject = JObject.Parse(iconString);
			JObject dataJObject = JObject.Parse(dataString);
			Console.WriteLine(dataString);
			*/

			//JObject resut = JObject.Parse(q);
			//Console.Write(resut.ToString(Newtonsoft.Json.Formatting.Indented));

		}
	}
}
