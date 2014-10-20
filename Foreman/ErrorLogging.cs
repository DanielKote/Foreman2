using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Foreman
{
	public static class ErrorLogging
	{
		public static void LogLine(String message)
		{
			try
			{
				File.AppendAllText(Path.Combine(Application.StartupPath, "errorlog.txt"), message + "\n");
			}
			catch { } //Not good.
		}
	}
}
