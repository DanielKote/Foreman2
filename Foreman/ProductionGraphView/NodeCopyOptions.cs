using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using Foreman.Serialization;
using System;
using System.Collections.Generic;

namespace Foreman.ProductionGraphView {
    public class NodeCopyOptions {
        public AssemblerQualityPair Assembler { get; }
        public IReadOnlyList<ModuleQualityPair> AssemblerModules { get; }
        public IItem? Fuel { get; }
        public double NeighbourCount { get; }
        public double ExtraProductivityBonus { get; }
        public BeaconQualityPair Beacon { get; }
        public IReadOnlyList<ModuleQualityPair> BeaconModules { get; }
        public double BeaconCount { get; }
        public double BeaconsPerAssembler { get; }
        public double BeaconsConst { get; }
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
            IItem? fuel,
            BeaconQualityPair beacon,
            IReadOnlyList<ModuleQualityPair> beaconModules,
            double beaconCount,
            double beaconsPerAssembler,
            double beaconsConst,
            double neighbourCount,
            double extraProductivityBonus) {
            Assembler = assembler;
            AssemblerModules = [.. assemblerModules];
            Fuel = fuel;
            Beacon = beacon;
            BeaconModules = [.. beaconModules];
            BeaconCount = beaconCount;
            BeaconsPerAssembler = beaconsPerAssembler;
            BeaconsConst = beaconsConst;
            NeighbourCount = neighbourCount;
            ExtraProductivityBonus = extraProductivityBonus;
        }

        internal static NodeCopyOptions? FromSaveDocument(NodeCopyOptionsSaveDocument document, DataCache cache) {
            IQuality? defaultQuality = cache.DefaultQuality;

            if (!cache.Assemblers.TryGetValue(document.AssemblerName, out IAssembler? assembler) || assembler is null)
                return null;

            IQuality? assemblerQuality = ResolveQuality(cache, document.AssemblerQualityName, defaultQuality);
            if (assemblerQuality is null)
                return null;

            BeaconQualityPair beaconPair;
            if (document.BeaconName is not null) {
                if (!cache.Beacons.TryGetValue(document.BeaconName, out IBeacon? beacon) || beacon is null)
                    return null;
                IQuality? beaconQuality = ResolveQuality(cache, document.BeaconQualityName ?? "", defaultQuality);
                if (beaconQuality is null)
                    return null;
                beaconPair = new BeaconQualityPair(beacon, beaconQuality);
            } else
                beaconPair = new BeaconQualityPair(/*"no beacon"*/);

            IItem? fuel = null;
            if (document.FuelName is not null && cache.Items.TryGetValue(document.FuelName, out IItem? fuelItem))
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

        private static IQuality? ResolveQuality(DataCache cache, string qualityName, IQuality? defaultQuality) {
            return cache.Qualities.TryGetValue(qualityName, out IQuality? quality)
                ? quality
                : cache.MissingQualities.TryGetValue(qualityName, out quality) ? quality : defaultQuality;
        }

        private static List<ModuleQualityPair> ResolveModules(
            DataCache cache,
            IReadOnlyList<ModuleQualitySaveData> modules,
            IQuality? defaultQuality) {
            List<ModuleQualityPair> result = [];
            foreach (ModuleQualitySaveData moduleData in modules) {
                if (!cache.Modules.TryGetValue(moduleData.ModuleName, out IModule? module) || module is null)
                    continue;
                IQuality? quality = ResolveQuality(cache, moduleData.QualityName, defaultQuality);
                if (quality is null)
                    continue;
                result.Add(new ModuleQualityPair(module, quality));
            }
            return result;
        }

        private NodeCopyOptions(AssemblerQualityPair assembler, List<ModuleQualityPair> assemblerModules, double neighbourCount, double extraProductivityBonus, IItem? fuel, BeaconQualityPair beacon, List<ModuleQualityPair> beaconModules, double beaconCount, double beaconsPerA, double beaconsCont) {
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
