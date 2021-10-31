using System;
using System.IO;
using System.Windows.Forms;

namespace Foreman
{
	public static class ErrorLogging
	{
		public static void ClearLog()
        {
			if (File.Exists(Path.Combine(Application.StartupPath, "errorlog.txt")));
				File.WriteAllText(Path.Combine(Application.StartupPath, "errorlog.txt"), "");
        }

		public static void LogLine(String message)
		{
			try
			{
				File.AppendAllText(Path.Combine(Application.StartupPath, "errorlog.txt"), "[" +  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]: " + message + "\n");
			}
			catch { } //Not good.
		}
	}
}
