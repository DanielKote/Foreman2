using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Foreman {
    [Serializable]
    public class NodeCopyOptions : ISerializable {
        public readonly AssemblerQualityPair Assembler;
        public readonly IReadOnlyList<ModuleQualityPair> AssemblerModules;
        public readonly Item Fuel;
        public readonly double NeighbourCount;
        public readonly double ExtraProductivityBonus;

        public readonly BeaconQualityPair Beacon;
        public readonly IReadOnlyList<ModuleQualityPair> BeaconModules;
        public readonly double BeaconCount;
        public readonly double BeaconsPerAssembler;
        public readonly double BeaconsConst;

        public NodeCopyOptions(ReadOnlyRecipeNode node) {
            Assembler = node.SelectedAssembler;
            AssemblerModules = new List<ModuleQualityPair>(node.AssemblerModules);
            Fuel = node.Fuel;
            Beacon = node.SelectedBeacon;
            BeaconModules = new List<ModuleQualityPair>(node.BeaconModules);
            BeaconCount = node.BeaconCount;
            BeaconsPerAssembler = node.BeaconsPerAssembler;
            BeaconsConst = node.BeaconsConst;
            NeighbourCount = node.NeighbourCount;
            ExtraProductivityBonus = node.ExtraProductivity;
        }

        private NodeCopyOptions(AssemblerQualityPair assembler, List<ModuleQualityPair> assemblerModules, double neighbourCount, double extraProductivityBonus,
            Item fuel, BeaconQualityPair beacon, List<ModuleQualityPair> beaconModules, double beaconCount, double beaconsPerA, double beaconsCont) {
            Assembler = assembler;
            AssemblerModules = assemblerModules;
            Fuel = fuel;
            Beacon = beacon;
            BeaconModules = beaconModules;
            BeaconCount = beaconCount;
            BeaconsPerAssembler = beaconsPerA;
            BeaconsConst = beaconsCont;
            NeighbourCount = neighbourCount;
            ExtraProductivityBonus = extraProductivityBonus;
        }

        public static NodeCopyOptions GetNodeCopyOptions(string serialized, DataCache cache) {
            try {
                return GetNodeCopyOptions(JObject.Parse(serialized), cache);
            } catch {
                return null;
            }
        }

        public static NodeCopyOptions GetNodeCopyOptions(JToken json, DataCache cache) {
            if (json["Version"] == null || (int) json["Version"] != Properties.Settings.Default.ForemanVersion || json["Object"] == null ||
                (string) json["Object"] != "NodeCopyOptions")
                return null;

            var beacons = json["Beacon"] != null;
            var assembler = cache.Assemblers.ContainsKey((string) json["Assembler"]) ? cache.Assemblers[(string) json["Assembler"]] : null;
            var assemblerQuality = cache.Qualities.ContainsKey((string) json["AssemblerQuality"])
                ? cache.Qualities[(string) json["AssemblerQuality"]]
                : null;
            var assemblerQp = new AssemblerQualityPair(assembler, assemblerQuality ?? cache.DefaultQuality);

            var beacon = beacons && cache.Beacons.ContainsKey((string) json["Beacon"]) ? cache.Beacons[(string) json["Beacon"]] : null;
            var beaconQuality = beacons && cache.Qualities.ContainsKey((string) json["BeaconQuality"])
                ? cache.Qualities[(string) json["BeaconQuality"]]
                : null;
            var beaconQp =
                beacon != null ? new BeaconQualityPair(beacon, beaconQuality ?? cache.DefaultQuality) : new BeaconQualityPair("no beacon");

            var aModules = new List<ModuleQualityPair>();
            foreach (var moduleToken in json["AModules"]) {
                var moduleName = (string) moduleToken["Name"];
                var moduleQuality = (string) moduleToken["Quality"];
                var module = cache.Modules.TryGetValue(moduleName, out var cacheModule) ? cacheModule : null;
                var quality = cache.Qualities.TryGetValue(moduleQuality, out var cacheQuality) ? cacheQuality : cache.DefaultQuality;
                if (module != null)
                    aModules.Add(new ModuleQualityPair(module, quality));
            }

            var bModules = new List<ModuleQualityPair>();
            foreach (var moduleToken in json["BModules"]) {
                var moduleName = (string) moduleToken["Name"];
                var moduleQuality = (string) moduleToken["Quality"];
                var module = cache.Modules.TryGetValue(moduleName, out var cacheModule) ? cacheModule : null;
                var quality = cache.Qualities.TryGetValue(moduleQuality, out var cacheQuality) ? cacheQuality : cache.DefaultQuality;
                if (module != null)
                    bModules.Add(new ModuleQualityPair(module, quality));
            }

            var fuel = json["Fuel"] != null && cache.Items.ContainsKey((string) json["Fuel"]) ? cache.Items[(string) json["Fuel"]] : null;

            var nco = new NodeCopyOptions(
                assemblerQp,
                aModules,
                (double) json["Neighbours"],
                (double) json["ExtraProductivity"],
                fuel,
                beaconQp,
                bModules,
                beacons ? (double) json["BeaconCount"] : 0,
                beacons ? (double) json["BeaconsPA"] : 0,
                beacons ? (double) json["BeaconsC"] : 0);
            return nco;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Version", Properties.Settings.Default.ForemanVersion);
            info.AddValue("Object", "NodeCopyOptions");
            info.AddValue("Assembler", Assembler.Assembler.Name);
            info.AddValue("AssemblerQuality", Assembler.Quality.Name);

            info.AddValue("Neighbours", NeighbourCount);
            info.AddValue("ExtraProductivity", ExtraProductivityBonus);
            info.AddValue("AModules", AssemblerModules);
            info.AddValue("BModules", BeaconModules);

            if (Fuel != null)
                info.AddValue("Fuel", Fuel.Name);

            if (Beacon) {
                info.AddValue("Beacon", Beacon.Beacon.Name);
                info.AddValue("BeaconQuality", Beacon.Quality.Name);
                info.AddValue("BeaconCount", BeaconCount);
                info.AddValue("BeaconsPA", BeaconsPerAssembler);
                info.AddValue("BeaconsC", BeaconsConst);
            }
        }
    }
}