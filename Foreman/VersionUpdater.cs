using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public static class VersionUpdater
	{
		//at some point we need to come back here and actuall fill in the version updater from the base foreman to the current version.


		public static JObject UpdateSave(JObject original)
		{
			MessageBox.Show("Attempt to update save to current foreman version failed.\nSorry.", "Cant load save", MessageBoxButtons.OK);
			return null;
		}

		public static JToken UpdateGraph(JToken original)
		{
			MessageBox.Show("Imported graph could not be updated to current foreman version.\nSorry.", "Cant process import", MessageBoxButtons.OK);
			return null;
		}
	}
}
