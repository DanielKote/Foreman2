using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman.Serialization {
    /// <summary>Maps STJ wire models ↔ <see cref="ProductionGraphSaveDocument"/>.</summary>
    internal static class GraphSaveWireMapper {
        public static ProductionGraphWire FromDocument(ProductionGraphSaveDocument document) => new() {
            Version = document.Version,
            Object = GraphSaveFormat.GraphObject,
            EnableExtraProductivityForNonMiners = document.Solver?.EnableExtraProductivityForNonMiners ?? false,
            DefaultNodeDirection = (int)(document.Solver?.DefaultNodeDirection ?? NodeDirection.Up),
            Solver_PullOutputNodes = document.Solver?.PullOutputNodes ?? false,
            Solver_PullOutputNodesPower = document.Solver?.PullOutputNodesPower ?? 1,
            Solver_LowPriorityPower = document.Solver?.LowPriorityPower ?? 2,
            MaxQualitySteps = document.Solver?.MaxQualitySteps ?? 0,
            DefaultQuality = document.Solver?.DefaultQualityName ?? "normal",
            IncludedItems = [.. document.IncludedItems],
            IncludedRecipes = [.. document.IncludedRecipes.Select(FromRecipeShort)],
            IncludedPlantProcesses = [.. document.IncludedPlantProcesses.Select(FromPlantShort)],
            IncludedAssemblers = [.. document.IncludedAssemblers],
            IncludedModules = [.. document.IncludedModules],
            IncludedBeacons = [.. document.IncludedBeacons],
            IncludedQualities = [.. document.IncludedQualities.Select(q => new QualityEntryWire { Key = q.Key, Value = q.Value })],
            Nodes = [.. document.Nodes.Select(FromNode)],
            NodeLinks = [.. document.Links.Select(FromLink)]
        };

        public static GraphViewerWire FromDocument(GraphViewerSaveDocument document) {
            var wire = new GraphViewerWire {
                Version = document.Version,
                Object = GraphSaveFormat.ViewerObject,
                SavedPresetName = document.SavedPresetName,
                IncludedMods = [.. document.IncludedMods.Select(m => m.Key + "|" + m.Value)],
                ProductionGraph = FromDocument(document.ProductionGraph)
            };
            if (document.Ui is not null)
                ApplyUi(wire, document.Ui);
            if (document.Annotations.Count > 0) {
                wire.Annotations = [.. document.Annotations];
                wire.AnnotationDpi = document.AnnotationDpi;
            }
            return wire;
        }

        public static NodeCopyOptionsWire FromDocument(NodeCopyOptionsSaveDocument document) {
            var wire = new NodeCopyOptionsWire {
                Version = document.Version,
                Object = GraphSaveFormat.NodeCopyObject,
                Assembler = document.AssemblerName,
                AssemblerQuality = document.AssemblerQualityName,
                Neighbours = document.NeighbourCount,
                ExtraProductivity = document.ExtraProductivityBonus,
                AModules = [.. document.AssemblerModules.Select(FromModule)],
                BModules = [.. document.BeaconModules.Select(FromModule)],
                Fuel = document.FuelName
            };
            if (document.BeaconName is not null && document.BeaconQualityName is not null) {
                wire.Beacon = document.BeaconName;
                wire.BeaconQuality = document.BeaconQualityName;
                wire.BeaconCount = document.BeaconCount;
                wire.BeaconsPA = document.BeaconsPerAssembler;
                wire.BeaconsC = document.BeaconsConst;
            }
            return wire;
        }

        public static KeyNodeClipboardWire FromDocument(KeyNodeClipboardSaveData document) => new() {
            Item1 = document.KeyNode,
            Item2 = document.Title
        };

        public static ProductionGraphSaveDocument ToProductionGraphDocument(ProductionGraphWire wire) => new() {
            Version = wire.Version,
            IncludedItems = wire.IncludedItems ?? [],
            IncludedQualities = [.. (wire.IncludedQualities ?? []).Select(q => new KeyValuePair<string, int>(q.Key ?? "", q.Value))],
            IncludedRecipes = [.. (wire.IncludedRecipes ?? []).Select(ToRecipeShort)],
            IncludedPlantProcesses = [.. (wire.IncludedPlantProcesses ?? []).Select(ToPlantShort)],
            IncludedAssemblers = wire.IncludedAssemblers ?? [],
            IncludedModules = wire.IncludedModules ?? [],
            IncludedBeacons = wire.IncludedBeacons ?? [],
            Solver = ToSolver(wire),
            Nodes = [.. (wire.Nodes ?? []).Select(ToNode).OfType<GraphNodeSaveData>()],
            Links = [.. (wire.NodeLinks ?? []).Select(ToLink)]
        };

        public static GraphViewerSaveDocument ToViewerDocument(GraphViewerWire wire) {
            return wire.ProductionGraph is null
                ? throw new InvalidOperationException("Viewer save is missing ProductionGraph.")
                : new GraphViewerSaveDocument {
                    Version = wire.Version,
                    SavedPresetName = wire.SavedPresetName,
                    IncludedMods = ParseModList(wire.IncludedMods),
                    ProductionGraph = ToProductionGraphDocument(wire.ProductionGraph),
                    Ui = ToViewerUi(wire),
                    Annotations = wire.Annotations ?? [],
                    AnnotationDpi = wire.AnnotationDpi ?? 96
                };
        }

        public static NodeCopyOptionsSaveDocument? ToNodeCopyOptionsDocument(NodeCopyOptionsWire wire) {
            return wire.Version != GraphSaveFormat.SaveFormatVersion
                || wire.Object != GraphSaveFormat.NodeCopyObject
                || string.IsNullOrEmpty(wire.Assembler)
                || string.IsNullOrEmpty(wire.AssemblerQuality)
                ? null
                : new NodeCopyOptionsSaveDocument {
                    Version = GraphSaveFormat.SaveFormatVersion,
                    AssemblerName = wire.Assembler,
                    AssemblerQualityName = wire.AssemblerQuality,
                    NeighbourCount = wire.Neighbours,
                    ExtraProductivityBonus = wire.ExtraProductivity,
                    AssemblerModules = [.. (wire.AModules ?? []).Select(ToModuleSave)],
                    BeaconModules = [.. (wire.BModules ?? []).Select(ToModuleSave)],
                    FuelName = wire.Fuel,
                    BeaconName = wire.Beacon,
                    BeaconQualityName = wire.BeaconQuality,
                    BeaconCount = wire.BeaconCount,
                    BeaconsPerAssembler = wire.BeaconsPA,
                    BeaconsConst = wire.BeaconsC
                };
        }

        public static KeyNodeClipboardSaveData? ToKeyNodeClipboard(KeyNodeClipboardWire wire) =>
            wire.Item2 is string title ? new KeyNodeClipboardSaveData(wire.Item1, title) : null;

        private static void ApplyUi(GraphViewerWire wire, GraphViewerUiSaveData ui) {
            wire.Unit = (int)ui.Unit;
            wire.ViewOffset = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", ui.ViewOffset.X, ui.ViewOffset.Y);
            wire.ViewScale = ui.ViewScale;
            wire.ExtraProdForNonMiners = ui.ExtraProdForNonMiners;
            wire.AssemblerSelectorStyle = (int)ui.AssemblerSelectorStyle;
            wire.ModuleSelectorStyle = (int)ui.ModuleSelectorStyle;
            wire.FuelPriorityList = [.. ui.FuelPriorityList];
            wire.EnabledRecipes = [.. ui.EnabledRecipes];
            wire.EnabledAssemblers = [.. ui.EnabledAssemblers];
            wire.EnabledModules = [.. ui.EnabledModules];
            wire.EnabledBeacons = [.. ui.EnabledBeacons];
            if (ui.OldImport)
                wire.OldImport = true;
        }

        private static GraphViewerUiSaveData ToViewerUi(GraphViewerWire wire) => new() {
            Unit = (ProductionGraph.RateUnit)wire.Unit,
            ViewOffset = ParsePoint(wire.ViewOffset),
            ViewScale = wire.ViewScale,
            ExtraProdForNonMiners = wire.ExtraProdForNonMiners,
            AssemblerSelectorStyle = (AssemblerSelector.Style)wire.AssemblerSelectorStyle,
            ModuleSelectorStyle = (ModuleSelector.Style)wire.ModuleSelectorStyle,
            FuelPriorityList = wire.FuelPriorityList ?? [],
            EnabledRecipes = wire.EnabledRecipes ?? [],
            EnabledAssemblers = wire.EnabledAssemblers ?? [],
            EnabledModules = wire.EnabledModules ?? [],
            EnabledBeacons = wire.EnabledBeacons ?? [],
            OldImport = wire.OldImport
        };

        private static ProductionGraphSolverSaveData ToSolver(ProductionGraphWire wire) => new() {
            EnableExtraProductivityForNonMiners = wire.EnableExtraProductivityForNonMiners,
            DefaultNodeDirection = (NodeDirection)wire.DefaultNodeDirection,
            PullOutputNodes = wire.Solver_PullOutputNodes,
            PullOutputNodesPower = wire.Solver_PullOutputNodesPower,
            LowPriorityPower = wire.Solver_LowPriorityPower,
            MaxQualitySteps = wire.MaxQualitySteps,
            DefaultQualityName = wire.DefaultQuality ?? "normal"
        };

        private static GraphNodeSaveData? ToNode(NodeWire wire) {
            Point location = ParsePoint(wire.Location);
            var rateType = (RateType)wire.RateType;
            var direction = (NodeDirection)wire.Direction;
            string? keyNode = wire.KeyNode;

            return (NodeType)wire.NodeType switch {
                NodeType.Consumer => new ConsumerNodeSaveData {
                    NodeId = wire.NodeID,
                    Location = location,
                    RateType = rateType,
                    DesiredSetValue = wire.DesiredSetValue,
                    Direction = direction,
                    KeyNodeTitle = keyNode,
                    ItemName = wire.Item ?? "ItemNameError",
                    QualityName = wire.BaseQuality ?? "QualityError"
                },
                NodeType.Supplier => new SupplierNodeSaveData {
                    NodeId = wire.NodeID,
                    Location = location,
                    RateType = rateType,
                    DesiredSetValue = wire.DesiredSetValue,
                    Direction = direction,
                    KeyNodeTitle = keyNode,
                    ItemName = wire.Item ?? "ItemNameError",
                    QualityName = wire.BaseQuality ?? "QualityError",
                    DesiredRatePerSec = wire.DesiredRate
                },
                NodeType.Passthrough => new PassthroughNodeSaveData {
                    NodeId = wire.NodeID,
                    Location = location,
                    RateType = rateType,
                    DesiredSetValue = wire.DesiredSetValue,
                    Direction = direction,
                    KeyNodeTitle = keyNode,
                    ItemName = wire.Item ?? "ItemNameError",
                    QualityName = wire.BaseQuality ?? "QualityError",
                    DesiredRatePerSec = wire.DesiredRate,
                    SimpleDraw = wire.SDraw
                },
                NodeType.Spoil => new SpoilNodeSaveData {
                    NodeId = wire.NodeID,
                    Location = location,
                    RateType = rateType,
                    DesiredSetValue = wire.DesiredSetValue,
                    Direction = direction,
                    KeyNodeTitle = keyNode,
                    InputItemName = wire.InputItem ?? "ItemNameError",
                    OutputItemName = wire.OutputItem ?? "ItemNameError",
                    QualityName = wire.BaseQuality ?? "QualityError"
                },
                NodeType.Plant => new PlantNodeSaveData {
                    NodeId = wire.NodeID,
                    Location = location,
                    RateType = rateType,
                    DesiredSetValue = wire.DesiredSetValue,
                    Direction = direction,
                    KeyNodeTitle = keyNode,
                    PlantProcessId = wire.PlantProcessID,
                    QualityName = wire.BaseQuality ?? "QualityError"
                },
                NodeType.Recipe => new RecipeNodeSaveData {
                    NodeId = wire.NodeID,
                    Location = location,
                    RateType = rateType,
                    DesiredSetValue = wire.DesiredSetValue,
                    Direction = direction,
                    KeyNodeTitle = keyNode,
                    RecipeId = wire.RecipeID,
                    RecipeQualityName = wire.RecipeQuality ?? "normal",
                    NeighbourCount = wire.Neighbours,
                    ExtraProductivityBonus = wire.ExtraProductivity,
                    LowPriority = wire.LowPriority != 0,
                    AssemblerName = wire.Assembler ?? "",
                    AssemblerQualityName = wire.AssemblerQuality ?? "normal",
                    AssemblerModules = [.. (wire.AssemblerModules ?? []).Select(ToModuleSave)],
                    FuelName = wire.Fuel,
                    BurntResultName = wire.Burnt,
                    BeaconName = wire.Beacon,
                    BeaconQualityName = wire.BeaconQuality,
                    BeaconModules = [.. (wire.BeaconModules ?? []).Select(ToModuleSave)],
                    BeaconCount = wire.BeaconCount,
                    BeaconsPerAssembler = wire.BeaconsPerAssembler,
                    BeaconsConst = wire.BeaconsConst
                },
                _ => null
            };
        }

        private static NodeWire FromNode(GraphNodeSaveData node) {
            var wire = new NodeWire {
                NodeID = node.NodeId,
                Location = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", node.Location.X, node.Location.Y),
                NodeType = (int)node.NodeType,
                RateType = (int)node.RateType,
                Direction = (int)node.Direction
            };
            if (node.RateType == RateType.Manual && node.DesiredSetValue is not null)
                wire.DesiredSetValue = node.DesiredSetValue;
            if (node.KeyNodeTitle is not null)
                wire.KeyNode = node.KeyNodeTitle;

            switch (node) {
                case RecipeNodeSaveData recipe:
                    wire.RecipeID = recipe.RecipeId;
                    wire.RecipeQuality = recipe.RecipeQualityName;
                    wire.Neighbours = recipe.NeighbourCount;
                    wire.ExtraProductivity = recipe.ExtraProductivityBonus;
                    if (recipe.LowPriority)
                        wire.LowPriority = 1;
                    wire.Assembler = recipe.AssemblerName;
                    wire.AssemblerQuality = recipe.AssemblerQualityName;
                    wire.AssemblerModules = [.. recipe.AssemblerModules.Select(FromModule)];
                    wire.Fuel = recipe.FuelName;
                    wire.Burnt = recipe.BurntResultName;
                    if (recipe.BeaconName is not null && recipe.BeaconQualityName is not null) {
                        wire.Beacon = recipe.BeaconName;
                        wire.BeaconQuality = recipe.BeaconQualityName;
                        wire.BeaconModules = [.. recipe.BeaconModules.Select(FromModule)];
                        wire.BeaconCount = recipe.BeaconCount;
                        wire.BeaconsPerAssembler = recipe.BeaconsPerAssembler;
                        wire.BeaconsConst = recipe.BeaconsConst;
                    }
                    break;
                case PlantNodeSaveData plant:
                    wire.PlantProcessID = plant.PlantProcessId;
                    wire.BaseQuality = plant.QualityName;
                    break;
                case ConsumerNodeSaveData consumer:
                    wire.Item = consumer.ItemName;
                    wire.BaseQuality = consumer.QualityName;
                    break;
                case SupplierNodeSaveData supplier:
                    wire.Item = supplier.ItemName;
                    wire.BaseQuality = supplier.QualityName;
                    if (supplier.DesiredRatePerSec is not null)
                        wire.DesiredRate = supplier.DesiredRatePerSec;
                    break;
                case PassthroughNodeSaveData passthrough:
                    wire.Item = passthrough.ItemName;
                    wire.BaseQuality = passthrough.QualityName;
                    if (passthrough.DesiredRatePerSec is not null)
                        wire.DesiredRate = passthrough.DesiredRatePerSec;
                    wire.SDraw = passthrough.SimpleDraw;
                    break;
                case SpoilNodeSaveData spoil:
                    wire.InputItem = spoil.InputItemName;
                    wire.OutputItem = spoil.OutputItemName;
                    wire.BaseQuality = spoil.QualityName;
                    break;
            }
            return wire;
        }

        private static GraphLinkWire FromLink(GraphLinkSaveData link) => new() {
            SupplierID = link.SupplierId,
            ConsumerID = link.ConsumerId,
            Item = link.ItemName,
            Quality = link.QualityName
        };

        private static GraphLinkSaveData ToLink(GraphLinkWire wire) => new(
            wire.SupplierID,
            wire.ConsumerID,
            wire.Item ?? "ItemNameError",
            wire.Quality ?? "QualityError");

        private static ModuleQualityWire FromModule(ModuleQualitySaveData module) => new() {
            Name = module.ModuleName,
            Quality = module.QualityName
        };

        private static ModuleQualitySaveData ToModuleSave(ModuleQualityWire wire) =>
            new(wire.Name ?? "", wire.Quality ?? "");

        private static RecipeShortWire FromRecipeShort(RecipeShort recipe) => new() {
            Name = recipe.Name,
            RecipeID = recipe.RecipeID,
            isMissing = recipe.isMissing,
            Ingredients = new Dictionary<string, double>(recipe.Ingredients),
            Products = new Dictionary<string, double>(recipe.Products)
        };

        private static RecipeShort ToRecipeShort(RecipeShortWire wire) => new(
            wire.Name ?? "<JSON ERROR>",
            wire.RecipeID,
            wire.isMissing,
            new Dictionary<string, double>(wire.Ingredients ?? []),
            new Dictionary<string, double>(wire.Products ?? []));

        private static PlantShort ToPlantShort(PlantShortWire wire) => new(
            wire.Name ?? "JSON ERROR",
            wire.PlantID,
            wire.isMissing,
            new Dictionary<string, double>(wire.Products ?? []));

        private static PlantShortWire FromPlantShort(PlantShort plant) => new() {
            Name = plant.Name,
            PlantID = plant.PlantID,
            isMissing = plant.isMissing,
            Products = new Dictionary<string, double>(plant.Products)
        };

        private static Dictionary<string, string> ParseModList(List<string>? entries) {
            Dictionary<string, string> modSet = [];
            foreach (string entry in entries ?? []) {
                string[] mod = entry.Split('|');
                if (mod.Length >= 2)
                    modSet[mod[0]] = mod[1];
            }
            return modSet;
        }

        private static Point ParsePoint(string? locationString) {
            string[] parts = locationString?.Split(',') ?? [];
            return parts.Length >= 2 && int.TryParse(parts[0].Trim(), out int x) && int.TryParse(parts[1].Trim(), out int y)
                ? new Point(x, y)
                : Point.Empty;
        }

    }
}
