using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Foreman.Serialization {
    /// <summary>STJ wire shapes for Foreman graph .fjson (property names match on-disk format).</summary>
    internal sealed class ProductionGraphWire {
        public int Version { get; set; }
        public string? Object { get; set; }
        public bool EnableExtraProductivityForNonMiners { get; set; }
        public int DefaultNodeDirection { get; set; }
        public bool Solver_PullOutputNodes { get; set; }
        public double Solver_PullOutputNodesPower { get; set; }
        public double Solver_LowPriorityPower { get; set; }
        public uint MaxQualitySteps { get; set; }
        public string? DefaultQuality { get; set; }
        public List<string>? IncludedItems { get; set; }
        public List<RecipeShortWire>? IncludedRecipes { get; set; }
        public List<PlantShortWire>? IncludedPlantProcesses { get; set; }
        public List<string>? IncludedAssemblers { get; set; }
        public List<string>? IncludedModules { get; set; }
        public List<string>? IncludedBeacons { get; set; }
        public List<QualityEntryWire>? IncludedQualities { get; set; }
        public List<NodeWire>? Nodes { get; set; }
        public List<GraphLinkWire>? NodeLinks { get; set; }
    }

    internal sealed class GraphViewerWire {
        public int Version { get; set; }
        public string? Object { get; set; }
        public string? SavedPresetName { get; set; }
        public List<string>? IncludedMods { get; set; }
        public int Unit { get; set; }
        public string? ViewOffset { get; set; }
        public float ViewScale { get; set; } = 1f;
        public bool ExtraProdForNonMiners { get; set; }
        public int AssemblerSelectorStyle { get; set; }
        public int ModuleSelectorStyle { get; set; }
        public List<string>? FuelPriorityList { get; set; }
        public List<string>? EnabledRecipes { get; set; }
        public List<string>? EnabledAssemblers { get; set; }
        public List<string>? EnabledModules { get; set; }
        public List<string>? EnabledBeacons { get; set; }
        public bool OldImport { get; set; }
        public ProductionGraphWire? ProductionGraph { get; set; }
        public List<AnnotationSaveData>? Annotations { get; set; }
        public int? AnnotationDpi { get; set; }
    }

    internal sealed class NodeCopyOptionsWire {
        public int Version { get; set; }
        public string? Object { get; set; }
        public string? Assembler { get; set; }
        public string? AssemblerQuality { get; set; }
        public double Neighbours { get; set; }
        public double ExtraProductivity { get; set; }
        public List<ModuleQualityWire>? AModules { get; set; }
        public List<ModuleQualityWire>? BModules { get; set; }
        public string? Fuel { get; set; }
        public string? Beacon { get; set; }
        public string? BeaconQuality { get; set; }
        public double BeaconCount { get; set; }
        public double BeaconsPA { get; set; }
        public double BeaconsC { get; set; }
    }

    internal sealed class KeyNodeClipboardWire {
        public bool Item1 { get; set; }
        public string? Item2 { get; set; }
    }

    internal sealed class RecipeShortWire {
        public string? Name { get; set; }
        public long RecipeID { get; set; }
        public bool isMissing { get; set; }
        public Dictionary<string, double>? Ingredients { get; set; }
        public Dictionary<string, double>? Products { get; set; }
    }

    internal sealed class PlantShortWire {
        public string? Name { get; set; }
        public long PlantID { get; set; }
        public bool isMissing { get; set; }
        public Dictionary<string, double>? Products { get; set; }
    }

    internal sealed class QualityEntryWire {
        public string? Key { get; set; }
        public int Value { get; set; }
    }

    internal sealed class ModuleQualityWire {
        public string? Name { get; set; }
        public string? Quality { get; set; }
    }

    internal sealed class GraphLinkWire {
        public int SupplierID { get; set; }
        public int ConsumerID { get; set; }
        public string? Item { get; set; }
        public string? Quality { get; set; }
    }

    /// <summary>Flat node wire (all node types share one shape; discriminated by NodeType).</summary>
    internal sealed class NodeWire {
        public int NodeID { get; set; }
        public string? Location { get; set; }
        public int NodeType { get; set; }
        public int RateType { get; set; }
        public double? DesiredSetValue { get; set; }
        public int Direction { get; set; }
        public string? KeyNode { get; set; }
        public string? Item { get; set; }
        public string? BaseQuality { get; set; }
        public double? DesiredRate { get; set; }
        public bool SDraw { get; set; }
        public string? InputItem { get; set; }
        public string? OutputItem { get; set; }
        public long PlantProcessID { get; set; }
        public long RecipeID { get; set; }
        public string? RecipeQuality { get; set; }
        public double Neighbours { get; set; }
        public double ExtraProductivity { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int LowPriority { get; set; }
        public string? Assembler { get; set; }
        public string? AssemblerQuality { get; set; }
        public List<ModuleQualityWire>? AssemblerModules { get; set; }
        public string? Fuel { get; set; }
        public string? Burnt { get; set; }
        public string? Beacon { get; set; }
        public string? BeaconQuality { get; set; }
        public List<ModuleQualityWire>? BeaconModules { get; set; }
        public double BeaconCount { get; set; }
        public double BeaconsPerAssembler { get; set; }
        public double BeaconsConst { get; set; }
    }
}
