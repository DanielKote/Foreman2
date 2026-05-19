using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman.Serialization {
    public sealed class ModuleQualitySaveData(string moduleName, string qualityName) {
        public string ModuleName { get; } = moduleName;
        public string QualityName { get; } = qualityName;
    }

    public sealed class GraphLinkSaveData(int supplierId, int consumerId, string itemName, string qualityName) {
        public int SupplierId { get; } = supplierId;
        public int ConsumerId { get; } = consumerId;
        public string ItemName { get; } = itemName;
        public string QualityName { get; } = qualityName;
    }

    public abstract class GraphNodeSaveData {
        public required int NodeId { get; init; }
        public required Point Location { get; init; }
        public required RateType RateType { get; init; }
        public double? DesiredSetValue { get; init; }
        public required NodeDirection Direction { get; init; }
        public string? KeyNodeTitle { get; init; }
        public abstract NodeType NodeType { get; }
    }

    public sealed class ConsumerNodeSaveData : GraphNodeSaveData {
        public override NodeType NodeType => NodeType.Consumer;
        public required string ItemName { get; init; }
        public required string QualityName { get; init; }
    }

    public sealed class SupplierNodeSaveData : GraphNodeSaveData {
        public override NodeType NodeType => NodeType.Supplier;
        public required string ItemName { get; init; }
        public required string QualityName { get; init; }
        public double? DesiredRatePerSec { get; init; }
    }

    public sealed class PassthroughNodeSaveData : GraphNodeSaveData {
        public override NodeType NodeType => NodeType.Passthrough;
        public required string ItemName { get; init; }
        public required string QualityName { get; init; }
        public double? DesiredRatePerSec { get; init; }
        public bool SimpleDraw { get; init; }
    }

    public sealed class SpoilNodeSaveData : GraphNodeSaveData {
        public override NodeType NodeType => NodeType.Spoil;
        public required string InputItemName { get; init; }
        public required string OutputItemName { get; init; }
        public required string QualityName { get; init; }
    }

    public sealed class PlantNodeSaveData : GraphNodeSaveData {
        public override NodeType NodeType => NodeType.Plant;
        public required long PlantProcessId { get; init; }
        public required string QualityName { get; init; }
    }

    public sealed class RecipeNodeSaveData : GraphNodeSaveData {
        public override NodeType NodeType => NodeType.Recipe;
        public required long RecipeId { get; init; }
        public required string RecipeQualityName { get; init; }
        public double NeighbourCount { get; init; }
        public double ExtraProductivityBonus { get; init; }
        public bool LowPriority { get; init; }
        public required string AssemblerName { get; init; }
        public required string AssemblerQualityName { get; init; }
        public IReadOnlyList<ModuleQualitySaveData> AssemblerModules { get; init; } = [];
        public string? FuelName { get; init; }
        public string? BurntResultName { get; init; }
        public string? BeaconName { get; init; }
        public string? BeaconQualityName { get; init; }
        public IReadOnlyList<ModuleQualitySaveData> BeaconModules { get; init; } = [];
        public double BeaconCount { get; init; }
        public double BeaconsPerAssembler { get; init; }
        public double BeaconsConst { get; init; }
    }

    public sealed class ProductionGraphSolverSaveData {
        public bool EnableExtraProductivityForNonMiners { get; init; }
        public NodeDirection DefaultNodeDirection { get; init; }
        public bool PullOutputNodes { get; init; }
        public double PullOutputNodesPower { get; init; }
        public double LowPriorityPower { get; init; }
        public uint MaxQualitySteps { get; init; }
        public required string DefaultQualityName { get; init; }
    }

    public sealed class ProductionGraphSaveDocument {
        public required int Version { get; init; }
        public IReadOnlyList<string> IncludedItems { get; init; } = [];
        public IReadOnlyList<KeyValuePair<string, int>> IncludedQualities { get; init; } = [];
        public IReadOnlyList<RecipeShort> IncludedRecipes { get; init; } = [];
        public IReadOnlyList<PlantShort> IncludedPlantProcesses { get; init; } = [];
        public IReadOnlyList<string> IncludedAssemblers { get; init; } = [];
        public IReadOnlyList<string> IncludedModules { get; init; } = [];
        public IReadOnlyList<string> IncludedBeacons { get; init; } = [];
        public ProductionGraphSolverSaveData? Solver { get; init; }
        public IReadOnlyList<GraphNodeSaveData> Nodes { get; init; } = [];
        public IReadOnlyList<GraphLinkSaveData> Links { get; init; } = [];
    }

    public sealed class GraphViewerUiSaveData {
        public ProductionGraph.RateUnit Unit { get; init; }
        public Point ViewOffset { get; init; }
        public float ViewScale { get; init; }
        public bool ExtraProdForNonMiners { get; init; }
        public AssemblerSelector.Style AssemblerSelectorStyle { get; init; }
        public ModuleSelector.Style ModuleSelectorStyle { get; init; }
        public IReadOnlyList<string> FuelPriorityList { get; init; } = [];
        public IReadOnlyList<string> EnabledRecipes { get; init; } = [];
        public IReadOnlyList<string> EnabledAssemblers { get; init; } = [];
        public IReadOnlyList<string> EnabledModules { get; init; } = [];
        public IReadOnlyList<string> EnabledBeacons { get; init; } = [];
        public bool OldImport { get; init; }
    }

    public sealed class GraphViewerSaveDocument {
        public required int Version { get; init; }
        public string? SavedPresetName { get; init; }
        public IReadOnlyDictionary<string, string> IncludedMods { get; init; } = new Dictionary<string, string>();
        public required ProductionGraphSaveDocument ProductionGraph { get; init; }
        public GraphViewerUiSaveData? Ui { get; init; }
        public IReadOnlyList<AnnotationSaveData> Annotations { get; init; } = [];
        public int AnnotationDpi { get; init; } = 96;
    }

    public sealed class NodeCopyOptionsSaveDocument {
        public required int Version { get; init; }
        public required string AssemblerName { get; init; }
        public required string AssemblerQualityName { get; init; }
        public double NeighbourCount { get; init; }
        public double ExtraProductivityBonus { get; init; }
        public IReadOnlyList<ModuleQualitySaveData> AssemblerModules { get; init; } = [];
        public IReadOnlyList<ModuleQualitySaveData> BeaconModules { get; init; } = [];
        public string? FuelName { get; init; }
        public string? BeaconName { get; init; }
        public string? BeaconQualityName { get; init; }
        public double BeaconCount { get; init; }
        public double BeaconsPerAssembler { get; init; }
        public double BeaconsConst { get; init; }
    }

    public sealed class KeyNodeClipboardSaveData(bool keyNode, string title) {
        public bool KeyNode { get; } = keyNode;
        public string Title { get; } = title;
    }
}
