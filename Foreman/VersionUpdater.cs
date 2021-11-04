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
			if (original["Version"] == null || original["Object"] == null || (string)original["Object"] != "ProductionGraphViewer")
			{
				//this is most likely the 'original' foreman graph. At the moment there isnt a conversion in place to bring it up to current standard (Feature will be added later)
				MessageBox.Show("Attempt to update save to current foreman version failed.\nSorry.", "Cant load save", MessageBoxButtons.OK);
				return null;
			}

			if ((int)original["Version"] == 1)
			{
				//Version update 1 -> 2:
				//	Graph now has the extra productivity for non-miners value 
				original["ExtraProdForNonMiners"] = false;
				original["Version"] = 2;
			}

			return original;
		}

		public static JToken UpdateGraph(JToken original)
		{
			if (original["Version"] == null || original["Object"] == null || (string)original["Object"] != "ProductionGraph")
			{
				//this is most likely the 'original' foreman graph. At the moment there isnt a conversion in place to bring it up to current standard (Feature will be added later)
				MessageBox.Show("Imported graph could not be updated to current foreman version.\nSorry.", "Cant process import", MessageBoxButtons.OK);
				return null;
			}

			if((int)original["Version"] == 1)
			{
				//Version update 1 -> 2:
				//	recipe node now has "ExtraPoductivity" value added
				original["Version"] = 2;

				foreach (JToken nodeJToken in original["Nodes"].Where(jt => (NodeType)(int)jt["NodeType"] == NodeType.Recipe).ToList())
					nodeJToken["ExtraProductivity"] = 0;
			}

			return original;
		}
	}
}
