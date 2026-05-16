using Foreman.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foreman {
    public class NodeCopyOptions {
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

        public NodeCopyOptions(IRecipeNodeViewModel node) : this(
            node.SelectedAssembler,
            node.AssemblerModules,
            node.Fuel,
            node.SelectedBeacon,
            node.BeaconModules,
            node.BeaconCount,
            node.BeaconsPerAssembler,
            node.BeaconsConst,
            node.NeighbourCount,
            node.ExtraProductivity) {
        }

        public NodeCopyOptions(RecipeNode node) : this(
            node.SelectedAssembler,
            node.AssemblerModules,
            node.Fuel,
            node.SelectedBeacon,
            node.BeaconModules,
            node.BeaconCount,
            node.BeaconsPerAssembler,
            node.BeaconsConst,
            node.NeighbourCount,
            node.ExtraProductivityBonus) {
        }

        private NodeCopyOptions(
            AssemblerQualityPair assembler,
            IReadOnlyList<ModuleQualityPair> assemblerModules,
            Item? fuel,
            BeaconQualityPair beacon,
            IReadOnlyList<ModuleQualityPair> beaconModules,
            double beaconCount,
            double beaconsPerAssembler,
            double beaconsConst,
            double neighbourCount,
            double extraProductivityBonus) {
            Assembler = assembler;
            AssemblerModules = new List<ModuleQualityPair>(assemblerModules);
            Fuel = fuel;
            Beacon = beacon;
            BeaconModules = new List<ModuleQualityPair>(beaconModules);
            BeaconCount = beaconCount;
            BeaconsPerAssembler = beaconsPerAssembler;
            BeaconsConst = beaconsConst;
            NeighbourCount = neighbourCount;
            ExtraProductivityBonus = extraProductivityBonus;
        }

        internal static NodeCopyOptions? FromSaveDocument(NodeCopyOptionsSaveDocument document, DataCache cache) {
            Quality? defaultQuality = cache.DefaultQuality;

            if (!cache.Assemblers.TryGetValue(document.AssemblerName, out Assembler? assembler) || assembler is null)
                return null;

            Quality? assemblerQuality = ResolveQuality(cache, document.AssemblerQualityName, defaultQuality);
            if (assemblerQuality is null)
                return null;

            BeaconQualityPair beaconPair;
            if (document.BeaconName is not null) {
                if (!cache.Beacons.TryGetValue(document.BeaconName, out Beacon? beacon) || beacon is null)
                    return null;
                Quality? beaconQuality = ResolveQuality(cache, document.BeaconQualityName ?? "", defaultQuality);
                if (beaconQuality is null)
                    return null;
                beaconPair = new BeaconQualityPair(beacon, beaconQuality);
            } else
                beaconPair = new BeaconQualityPair("no beacon");

            Item? fuel = null;
            if (document.FuelName is not null && cache.Items.TryGetValue(document.FuelName, out Item? fuelItem))
                fuel = fuelItem;

            return new NodeCopyOptions(
                new AssemblerQualityPair(assembler, assemblerQuality),
                ResolveModules(cache, document.AssemblerModules, defaultQuality),
                document.NeighbourCount,
                document.ExtraProductivityBonus,
                fuel,
                beaconPair,
                ResolveModules(cache, document.BeaconModules, defaultQuality),
                document.BeaconName is not null ? document.BeaconCount : 0,
                document.BeaconName is not null ? document.BeaconsPerAssembler : 0,
                document.BeaconName is not null ? document.BeaconsConst : 0);
        }

        private static Quality? ResolveQuality(DataCache cache, string qualityName, Quality? defaultQuality) {
            if (cache.Qualities.TryGetValue(qualityName, out Quality? quality))
                return quality;
            if (cache.MissingQualities.TryGetValue(qualityName, out quality))
                return quality;
            return defaultQuality;
        }

        private static List<ModuleQualityPair> ResolveModules(
            DataCache cache,
            IReadOnlyList<ModuleQualitySaveData> modules,
            Quality? defaultQuality) {
            List<ModuleQualityPair> result = [];
            foreach (ModuleQualitySaveData moduleData in modules) {
                if (!cache.Modules.TryGetValue(moduleData.ModuleName, out Module? module) || module is null)
                    continue;
                Quality? quality = ResolveQuality(cache, moduleData.QualityName, defaultQuality);
                if (quality is null)
                    continue;
                result.Add(new ModuleQualityPair(module, quality));
            }
            return result;
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
            try {
                NodeCopyOptionsSaveDocument? document = GraphSaveCodec.ReadNodeCopyOptions(serialized);
                return document is null ? null : FromSaveDocument(document, cache);
            } catch (Exception ex) {
                ErrorLogging.LogException(ex, "Failed to parse node copy options from clipboard");
                return null;
            }
        }

        public static NodeCopyOptions? GetNodeCopyOptions(NodeCopyOptionsSaveDocument document, DataCache cache) =>
            FromSaveDocument(document, cache);

    }
}