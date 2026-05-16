using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Foreman {
    [Serializable]
    public class NodeCopyOptions : ISerializable {
        public readonly AssemblerQualityPair Assembler;
        public readonly IReadOnlyList<ModuleQualityPair> AssemblerModules;
        public readonly Item? Fuel;
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

        private NodeCopyOptions(AssemblerQualityPair assembler, List<ModuleQualityPair> assemblerModules, double neighbourCount, double extraProductivityBonus, Item? fuel, BeaconQualityPair beacon, List<ModuleQualityPair> beaconModules, double beaconCount, double beaconsPerA, double beaconsCont) {
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

        public static NodeCopyOptions? GetNodeCopyOptions(string serialized, DataCache cache) {
            try { return GetNodeCopyOptions(JObject.Parse(serialized), cache); } catch { return null; }
        }

        public static NodeCopyOptions? GetNodeCopyOptions(JToken json, DataCache cache) {
            if (JsonTokens.AsInt32(json["Version"]) != Properties.Settings.Default.ForemanVersion || JsonTokens.AsString(json["Object"]) != "NodeCopyOptions")
                return null;

            Quality? defaultQuality = cache.DefaultQuality;

            string? assemblerName = json.Value<string>("Assembler");
            if (string.IsNullOrEmpty(assemblerName) || !cache.Assemblers.TryGetValue(assemblerName, out Assembler? assembler) || assembler is null)
                return null;

            Quality? assemblerQuality = null;
            string? assemblerQualityName = json.Value<string>("AssemblerQuality");
            if (!string.IsNullOrEmpty(assemblerQualityName) && cache.Qualities.TryGetValue(assemblerQualityName, out Quality? aq))
                assemblerQuality = aq;
            if (assemblerQuality is null && defaultQuality is null)
                return null;
            Quality resolvedAssemblerQuality = assemblerQuality is not null
                ? assemblerQuality
                : defaultQuality ?? throw new InvalidOperationException("Missing default quality for assembler.");
            AssemblerQualityPair assemberQP = new AssemblerQualityPair(assembler, resolvedAssemblerQuality);

            bool beacons = json["Beacon"] != null;
            BeaconQualityPair beaconQP;
            if (beacons) {
                string? beaconName = json.Value<string>("Beacon");
                if (string.IsNullOrEmpty(beaconName) || !cache.Beacons.TryGetValue(beaconName, out Beacon? beacon) || beacon is null)
                    return null;
                Quality? beaconQuality = null;
                string? beaconQualityName = json.Value<string>("BeaconQuality");
                if (!string.IsNullOrEmpty(beaconQualityName) && cache.Qualities.TryGetValue(beaconQualityName, out Quality? bq))
                    beaconQuality = bq;
                if (beaconQuality is null && defaultQuality is null)
                    return null;
                Quality resolvedBeaconQuality = beaconQuality is not null
                    ? beaconQuality
                    : defaultQuality ?? throw new InvalidOperationException("Missing default quality for beacon.");
                beaconQP = new BeaconQualityPair(beacon, resolvedBeaconQuality);
            } else
                beaconQP = new BeaconQualityPair("no beacon");

            List<ModuleQualityPair> aModules = new List<ModuleQualityPair>();
            foreach (JToken moduleToken in json["AModules"] ?? new JArray()) {
                string? moduleName = moduleToken.Value<string>("Name");
                string? moduleQualityName = moduleToken.Value<string>("Quality");
                if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(moduleQualityName))
                    continue;
                if (!cache.Modules.TryGetValue(moduleName, out Module? module) || module is null)
                    continue;
                Quality? quality = cache.Qualities.TryGetValue(moduleQualityName, out Quality? mq) ? mq : defaultQuality;
                if (quality is null)
                    continue;
                aModules.Add(new ModuleQualityPair(module, quality));
            }

            List<ModuleQualityPair> bModules = new List<ModuleQualityPair>();
            foreach (JToken moduleToken in json["BModules"] ?? new JArray()) {
                string? moduleName = moduleToken.Value<string>("Name");
                string? moduleQualityName = moduleToken.Value<string>("Quality");
                if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(moduleQualityName))
                    continue;
                if (!cache.Modules.TryGetValue(moduleName, out Module? module) || module is null)
                    continue;
                Quality? quality = cache.Qualities.TryGetValue(moduleQualityName, out Quality? mq) ? mq : defaultQuality;
                if (quality is null)
                    continue;
                bModules.Add(new ModuleQualityPair(module, quality));
            }

            Item? fuel = null;
            string? fuelName = json.Value<string>("Fuel");
            if (!string.IsNullOrEmpty(fuelName) && cache.Items.TryGetValue(fuelName, out Item? fuelItem))
                fuel = fuelItem;

            NodeCopyOptions nco = new NodeCopyOptions(
                assemberQP,
                aModules,
                JsonTokens.AsDouble(json["Neighbours"]) ?? 0,
                JsonTokens.AsDouble(json["ExtraProductivity"]) ?? 0,
                fuel,
                beaconQP,
                bModules,
                beacons ? JsonTokens.AsDouble(json["BeaconCount"]) ?? 0 : 0,
                beacons ? JsonTokens.AsDouble(json["BeaconsPA"]) ?? 0 : 0,
                beacons ? JsonTokens.AsDouble(json["BeaconsC"]) ?? 0 : 0);
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

            if (Beacon.Beacon is Beacon beaconEntity && Beacon.Quality is Quality beaconQuality) {
                info.AddValue("Beacon", beaconEntity.Name);
                info.AddValue("BeaconQuality", beaconQuality.Name);
                info.AddValue("BeaconCount", BeaconCount);
                info.AddValue("BeaconsPA", BeaconsPerAssembler);
                info.AddValue("BeaconsC", BeaconsConst);
            }
        }
    }
}