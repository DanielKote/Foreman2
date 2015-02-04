using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows;

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
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			new MainForm().Show();
			Application.Run();
		}
	}
}
