using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using Foreman.ProductionGraphView;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.Serialization {
    /// <summary>Maps live graph / viewer state to <see cref="ProductionGraphSaveDocument"/> (inverse of <see cref="GraphSaveLoader"/>).</summary>
    public static class GraphSaveWriter {
        public static ProductionGraphSaveDocument WriteProductionGraph(ProductionGraph graph) {
            (IReadOnlyCollection<BaseNode> includedNodes, IReadOnlyCollection<NodeLink> includedLinks) = graph.GetFragmentForSerialization();
            GraphIncludedSetCollector included = new();
            included.CollectFromNodes(includedNodes, graph.DefaultAssemblerQuality);

            return new ProductionGraphSaveDocument {
                Version = GraphSaveFormat.SaveFormatVersion,
                IncludedItems = Sort(included.Items),
                IncludedQualities = SortQualities(included.Qualities),
                IncludedRecipes = included.BuildRecipeShortList(),
                IncludedPlantProcesses = included.BuildPlantShortList(),
                IncludedAssemblers = Sort(included.Assemblers),
                IncludedModules = Sort(included.Modules),
                IncludedBeacons = Sort(included.Beacons),
                Solver = WriteSolver(graph),
                Nodes = [.. includedNodes.Select(WriteNode)],
                Links = [.. includedLinks.Select(WriteLink)]
            };
        }

        public static GraphViewerSaveDocument WriteViewer(ProductionGraphViewer viewer, DataCache cache) {
            return new GraphViewerSaveDocument {
                Version = GraphSaveFormat.SaveFormatVersion,
                SavedPresetName = cache.PresetName,
                IncludedMods = new Dictionary<string, string>(cache.IncludedMods),
                ProductionGraph = WriteProductionGraph(viewer.Graph),
                Ui = WriteViewerUi(viewer, cache),
                Annotations = viewer.GetAnnotationSaveData(),
                AnnotationDpi = viewer.DeviceDpi
            };
        }

        public static GraphViewerUiSaveData WriteViewerUi(ProductionGraphViewer viewer, DataCache cache) => new() {
            Unit = viewer.Graph.SelectedRateUnit,
            ViewOffset = viewer.ViewOffset,
            ViewScale = viewer.ViewScale,
            ExtraProdForNonMiners = viewer.Graph.EnableExtraProductivityForNonMiners,
            AssemblerSelectorStyle = viewer.Graph.AssemblerSelector.DefaultSelectionStyle,
            ModuleSelectorStyle = viewer.Graph.ModuleSelector.DefaultSelectionStyle,
            FuelPriorityList = [.. viewer.Graph.FuelSelector.FuelPriority.Select(i => i.Name)],
            EnabledRecipes = SortEnabled(cache.Recipes.Values),
            EnabledAssemblers = SortEnabled(cache.Assemblers.Values),
            EnabledModules = SortEnabled(cache.Modules.Values),
            EnabledBeacons = SortEnabled(cache.Beacons.Values),
            OldImport = false
        };

        public static NodeCopyOptionsSaveDocument WriteNodeCopyOptions(NodeCopyOptions options) => new() {
            Version = GraphSaveFormat.SaveFormatVersion,
            AssemblerName = options.Assembler.Assembler.Name,
            AssemblerQualityName = options.Assembler.Quality.Name,
            NeighbourCount = options.NeighbourCount,
            ExtraProductivityBonus = options.ExtraProductivityBonus,
            AssemblerModules = WriteModules(options.AssemblerModules),
            BeaconModules = WriteModules(options.BeaconModules),
            FuelName = options.Fuel?.Name,
            BeaconName = options.Beacon.Beacon?.Name,
            BeaconQualityName = options.Beacon.Quality?.Name,
            BeaconCount = options.BeaconCount,
            BeaconsPerAssembler = options.BeaconsPerAssembler,
            BeaconsConst = options.BeaconsConst
        };

        public static KeyNodeClipboardSaveData WriteKeyNodeClipboard(bool keyNode, string title) =>
            new(keyNode, title);

        private static ProductionGraphSolverSaveData WriteSolver(ProductionGraph graph) => new() {
            EnableExtraProductivityForNonMiners = graph.EnableExtraProductivityForNonMiners,
            DefaultNodeDirection = graph.DefaultNodeDirection,
            PullOutputNodes = graph.PullOutputNodes,
            PullOutputNodesPower = graph.PullOutputNodesPower,
            LowPriorityPower = graph.LowPriorityPower,
            MaxQualitySteps = graph.MaxQualitySteps,
            DefaultQualityName = graph.DefaultAssemblerQuality?.Name ?? "normal"
        };

        private static GraphLinkSaveData WriteLink(NodeLink link) => new(
            link.SupplierNode.NodeID,
            link.ConsumerNode.NodeID,
            link.Item.Item?.Name ?? "ItemNameError",
            link.Item.Quality?.Name ?? "QualityError");

        private static GraphNodeSaveData WriteNode(BaseNode node) {
            var baseFields = new NodeFields(
                node.NodeID,
                node.Location,
                node.RateType,
                node.RateType == RateType.Manual ? node.DesiredSetValue : null,
                node.NodeDirection,
                node.KeyNode ? node.KeyNodeTitle : null);

            return node switch {
                RecipeNode rnode => WriteRecipeNode(baseFields, rnode),
                PlantNode pnode => new PlantNodeSaveData {
                    NodeId = baseFields.NodeId,
                    Location = baseFields.Location,
                    RateType = baseFields.RateType,
                    DesiredSetValue = baseFields.DesiredSetValue,
                    Direction = baseFields.Direction,
                    KeyNodeTitle = baseFields.KeyNodeTitle,
                    PlantProcessId = pnode.BasePlantProcess.PlantID,
                    QualityName = pnode.Seed.Quality?.Name ?? "QualityError"
                },
                ConsumerNode cnode => new ConsumerNodeSaveData {
                    NodeId = baseFields.NodeId,
                    Location = baseFields.Location,
                    RateType = baseFields.RateType,
                    DesiredSetValue = baseFields.DesiredSetValue,
                    Direction = baseFields.Direction,
                    KeyNodeTitle = baseFields.KeyNodeTitle,
                    ItemName = cnode.ConsumedItem.Item?.Name ?? "ItemNameError",
                    QualityName = cnode.ConsumedItem.Quality?.Name ?? "QualityError"
                },
                SupplierNode snode => new SupplierNodeSaveData {
                    NodeId = baseFields.NodeId,
                    Location = baseFields.Location,
                    RateType = baseFields.RateType,
                    DesiredSetValue = baseFields.DesiredSetValue,
                    Direction = baseFields.Direction,
                    KeyNodeTitle = baseFields.KeyNodeTitle,
                    ItemName = snode.SuppliedItem.Item?.Name ?? "ItemNameError",
                    QualityName = snode.SuppliedItem.Quality?.Name ?? "QualityError",
                    DesiredRatePerSec = snode.RateType == RateType.Manual ? snode.DesiredRatePerSec : null
                },
                PassthroughNode passnode => new PassthroughNodeSaveData {
                    NodeId = baseFields.NodeId,
                    Location = baseFields.Location,
                    RateType = baseFields.RateType,
                    DesiredSetValue = baseFields.DesiredSetValue,
                    Direction = baseFields.Direction,
                    KeyNodeTitle = baseFields.KeyNodeTitle,
                    ItemName = passnode.PassthroughItem.Item?.Name ?? "ItemNameError",
                    QualityName = passnode.PassthroughItem.Quality?.Name ?? "QualityError",
                    DesiredRatePerSec = passnode.RateType == RateType.Manual ? passnode.DesiredRatePerSec : null,
                    SimpleDraw = passnode.SimpleDraw
                },
                SpoilNode spoilnode => new SpoilNodeSaveData {
                    NodeId = baseFields.NodeId,
                    Location = baseFields.Location,
                    RateType = baseFields.RateType,
                    DesiredSetValue = baseFields.DesiredSetValue,
                    Direction = baseFields.Direction,
                    KeyNodeTitle = baseFields.KeyNodeTitle,
                    InputItemName = spoilnode.InputItem.Item?.Name ?? "ItemNameError",
                    OutputItemName = spoilnode.OutputItem.Item?.Name ?? "ItemNameError",
                    QualityName = spoilnode.InputItem.Quality?.Name ?? "QualityError"
                },
                _ => throw new NotSupportedException($"Unsupported node type: {node.GetType().Name}")
            };
        }

        private static RecipeNodeSaveData WriteRecipeNode(NodeFields baseFields, RecipeNode rnode) {
            BeaconQualityPair beaconPair = rnode.SelectedBeacon;
            bool hasBeacon = beaconPair.Beacon is not null && beaconPair.Quality is not null;

            return new RecipeNodeSaveData {
                NodeId = baseFields.NodeId,
                Location = baseFields.Location,
                RateType = baseFields.RateType,
                DesiredSetValue = baseFields.DesiredSetValue,
                Direction = baseFields.Direction,
                KeyNodeTitle = baseFields.KeyNodeTitle,
                RecipeId = rnode.BaseRecipe.Recipe?.RecipeID ?? 0,
                RecipeQualityName = rnode.BaseRecipe.Quality?.Name ?? "normal",
                NeighbourCount = rnode.NeighbourCount,
                ExtraProductivityBonus = rnode.ExtraProductivityBonus,
                LowPriority = rnode.LowPriority,
                AssemblerName = rnode.SelectedAssembler.Assembler.Name,
                AssemblerQualityName = rnode.SelectedAssembler.Quality.Name,
                AssemblerModules = WriteModules(rnode.AssemblerModules),
                FuelName = rnode.Fuel?.Name,
                BurntResultName = rnode.FuelRemains?.Name,
                BeaconName = beaconPair.Beacon is IBeacon beacon ? beacon.Name : null,
                BeaconQualityName = beaconPair.Quality is IQuality beaconQuality ? beaconQuality.Name : null,
                BeaconModules = hasBeacon ? WriteModules(rnode.BeaconModules) : [],
                BeaconCount = hasBeacon ? rnode.BeaconCount : 0,
                BeaconsPerAssembler = hasBeacon ? rnode.BeaconsPerAssembler : 0,
                BeaconsConst = hasBeacon ? rnode.BeaconsConst : 0
            };
        }

        private static List<ModuleQualitySaveData> WriteModules(IEnumerable<ModuleQualityPair> modules) =>
            [.. modules.Select(m => new ModuleQualitySaveData(m.Module.Name, m.Quality.Name))];

        private static List<string> Sort(IEnumerable<string> names) =>
            [.. names.OrderBy(n => n, StringComparer.Ordinal)];

        private static List<KeyValuePair<string, int>> SortQualities(IEnumerable<KeyValuePair<string, int>> qualities) =>
            [.. qualities.OrderBy(q => q.Key, StringComparer.Ordinal)];

        private static List<string> SortEnabled<T>(IEnumerable<T> entities) where T : IDataObjectBase =>
            [.. entities.Where(e => e.Enabled).Select(e => e.Name).OrderBy(n => n, StringComparer.Ordinal)];

        private readonly record struct NodeFields(
            int NodeId,
            System.Drawing.Point Location,
            RateType RateType,
            double? DesiredSetValue,
            NodeDirection Direction,
            string? KeyNodeTitle);
    }
}
