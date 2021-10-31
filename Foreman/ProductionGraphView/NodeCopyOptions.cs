using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Foreman
{
	[Serializable]
	public class NodeCopyOptions : ISerializable
	{
		public readonly Assembler Assembler;
		public readonly IReadOnlyList<Module> AssemblerModules;
		public readonly Item Fuel;
		public readonly double NeighbourCount;

		public readonly Beacon Beacon;
		public readonly IReadOnlyList<Module> BeaconModules;
		public readonly double BeaconCount;
		public readonly double BeaconsPerAssembler;
		public readonly double BeaconsConst;

		public NodeCopyOptions(ReadOnlyRecipeNode node)
		{
			Assembler = node.SelectedAssembler;
			AssemblerModules = new List<Module>(node.AssemblerModules);
			Fuel = node.Fuel;
			Beacon = node.SelectedBeacon;
			BeaconModules = new List<Module>(node.BeaconModules);
			BeaconCount = node.BeaconCount;
			BeaconsPerAssembler = node.BeaconsPerAssembler;
			BeaconsConst = node.BeaconsConst;
			NeighbourCount = node.NeighbourCount;
		}

		private NodeCopyOptions(Assembler assembler, List<Module> assemblerModules, double neighbourCount, Item fuel, Beacon beacon, List<Module> beaconModules, double beaconCount, double beaconsPerA, double beaconsCont)
		{
			Assembler = assembler;
			AssemblerModules = assemblerModules;
			Fuel = fuel;
			Beacon = beacon;
			BeaconModules = beaconModules;
			BeaconCount = beaconCount;
			BeaconsPerAssembler = beaconsPerA;
			BeaconsConst = beaconsCont;
			NeighbourCount = neighbourCount;
		}

		public static NodeCopyOptions GetNodeCopyOptions(string serialized, DataCache cache)
		{
			try { return GetNodeCopyOptions(JObject.Parse(serialized), cache); }
			catch { return null; }
		}

		public static NodeCopyOptions GetNodeCopyOptions(JToken json, DataCache cache)
		{
			if (json["Version"] == null || (int)json["Version"] != Properties.Settings.Default.ForemanVersion || json["Object"] == null || (string)json["Object"] != "NodeCopyOptions")
				return null;

			try
			{
				bool beacons = json["Beacon"] != null;
				NodeCopyOptions nco = new NodeCopyOptions(
					cache.Assemblers.ContainsKey((string)json["Assembler"]) ? cache.Assemblers[(string)json["Assembler"]] : null,
					new List<Module>(json["AModules"].Where(j => cache.Modules.ContainsKey((string)j)).Select(j => cache.Modules[(string)j])),
					(double)json["Neighbours"],
					(json["Fuel"] != null && cache.Items.ContainsKey((string)json["Fuel"])) ? cache.Items[(string)json["fuel"]] : null,
					(beacons && cache.Beacons.ContainsKey((string)json["Beacon"])) ? cache.Beacons[(string)json["Beacon"]] : null,
					new List<Module>(json["BModules"].Where(j => cache.Modules.ContainsKey((string)j)).Select(j => cache.Modules[(string)j])),
					beacons ? (double)json["BeaconCount"] : -1,
					beacons ? (double)json["BeaconsPA"] : -1,
					beacons ? (double)json["BeaconsC"] : -1);
				return nco;
			}
			catch { return null; }
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Version", Properties.Settings.Default.ForemanVersion);
			info.AddValue("Object", "NodeCopyOptions");
			info.AddValue("Assembler", Assembler.Name);

			info.AddValue("Neighbours", NeighbourCount);
			info.AddValue("AModules", AssemblerModules.Select(m => m.Name));
			info.AddValue("BModules", BeaconModules.Select(m => m.Name));

			if (Fuel != null)
				info.AddValue("Fuel", Fuel.Name);

			if (Beacon != null)
			{
				info.AddValue("Beacon", Beacon.Name);
				info.AddValue("BeaconCount", BeaconCount);
				info.AddValue("BeaconsPA", BeaconsPerAssembler);
				info.AddValue("BeaconsC", BeaconsConst);
			}
		}
	}
}
