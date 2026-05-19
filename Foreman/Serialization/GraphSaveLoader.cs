using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using Foreman.ProductionGraphView;
using System;
using System.Collections.Generic;

namespace Foreman.Serialization {
    /// <summary>Applies parsed save documents to <see cref="ProductionGraph"/> and related UI state.</summary>
    public static class GraphSaveLoader {
        private sealed class GraphImportContext(
            Dictionary<string, IQuality?> qualityLinks,
            Dictionary<long, IRecipe> recipeLinks,
            Dictionary<long, IPlantProcess> plantProcessLinks) {
            public Dictionary<string, IQuality?> QualityLinks { get; } = qualityLinks;
            public Dictionary<long, IRecipe> RecipeLinks { get; } = recipeLinks;
            public Dictionary<long, IPlantProcess> PlantProcessLinks { get; } = plantProcessLinks;
        }

        public static ProductionGraph.NewNodeBatch LoadProductionGraph(
            ProductionGraph graph,
            DataCache cache,
            ProductionGraphSaveDocument document,
            bool applySolverSettings) {
            var newNodeCollection = new ProductionGraph.NewNodeBatch();
            var oldNodeIndices = new Dictionary<int, BaseNode>();

            try {
                GraphImportContext import = ImportIncludedEntities(cache, document);

                if (applySolverSettings && document.Solver is not null)
                    ApplySolverSettings(graph, document.Solver, import);

                foreach (GraphNodeSaveData nodeData in document.Nodes) {
                    BaseNode? newNode = CreateNode(graph, cache, import, nodeData, newNodeCollection);
                    if (newNode is not null)
                        oldNodeIndices[nodeData.NodeId] = newNode;
                }

                foreach (GraphLinkSaveData link in document.Links) {
                    if (!oldNodeIndices.TryGetValue(link.SupplierId, out BaseNode? supplier)
                        || !oldNodeIndices.TryGetValue(link.ConsumerId, out BaseNode? consumer))
                        continue;

                    if (!import.QualityLinks.TryGetValue(link.QualityName, out IQuality? quality) || quality is null)
                        continue;

                    ItemQualityPair item = cache.Items.ContainsKey(link.ItemName)
                        ? new ItemQualityPair(cache.Items[link.ItemName], quality)
                        : new ItemQualityPair(cache.MissingItems[link.ItemName], quality);

                    if (LinkChecker.IsPossibleConnection(item, supplier, consumer))
                        newNodeCollection.NewLinks.Add(graph.CreateLink(supplier, consumer, item));
                }
            } catch (Exception e) {
                ErrorLogging.LogException(e, "Error loading nodes into production graph");
                graph.DeleteNodes(newNodeCollection.NewNodes);
                return new ProductionGraph.NewNodeBatch();
            }

            return newNodeCollection;
        }

        public static NodeCopyOptions? ToNodeCopyOptions(NodeCopyOptionsSaveDocument document, DataCache cache) =>
            NodeCopyOptions.FromSaveDocument(document, cache);

        private static GraphImportContext ImportIncludedEntities(DataCache cache, ProductionGraphSaveDocument document) {
            cache.ProcessImportedItemsSet(document.IncludedItems);
            Dictionary<string, IQuality?> qualityLinks = cache.ProcessImportedQualitiesSet(document.IncludedQualities);
            cache.ProcessImportedAssemblersSet(document.IncludedAssemblers);
            cache.ProcessImportedModulesSet(document.IncludedModules);
            cache.ProcessImportedBeaconsSet(document.IncludedBeacons);
            Dictionary<long, IRecipe> recipeLinks = cache.ProcessImportedRecipesSet(document.IncludedRecipes);
            Dictionary<long, IPlantProcess> plantProcessLinks = cache.ProcessImportedPlantProcessesSet(document.IncludedPlantProcesses);
            return new GraphImportContext(qualityLinks, recipeLinks, plantProcessLinks);
        }

        private static void ApplySolverSettings(
            ProductionGraph graph,
            ProductionGraphSolverSaveData solver,
            GraphImportContext import) {
            graph.EnableExtraProductivityForNonMiners = solver.EnableExtraProductivityForNonMiners;
            graph.DefaultNodeDirection = solver.DefaultNodeDirection;
            graph.PullOutputNodes = solver.PullOutputNodes;
            graph.PullOutputNodesPower = solver.PullOutputNodesPower;
            graph.LowPriorityPower = solver.LowPriorityPower;
            graph.MaxQualitySteps = solver.MaxQualitySteps;
            if (import.QualityLinks.TryGetValue(solver.DefaultQualityName, out IQuality? defaultQuality))
                graph.DefaultAssemblerQuality = defaultQuality;
        }

        private static BaseNode? CreateNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            GraphNodeSaveData nodeData,
            ProductionGraph.NewNodeBatch newNodeCollection) {
            BaseNode? newNode = nodeData switch {
                ConsumerNodeSaveData consumer => CreateConsumerNode(graph, cache, import, consumer, newNodeCollection),
                SupplierNodeSaveData supplier => CreateSupplierNode(graph, cache, import, supplier, newNodeCollection),
                PassthroughNodeSaveData passthrough => CreatePassthroughNode(graph, cache, import, passthrough, newNodeCollection),
                SpoilNodeSaveData spoil => CreateSpoilNode(graph, cache, import, spoil, newNodeCollection),
                PlantNodeSaveData plant => CreatePlantNode(graph, import, plant, newNodeCollection),
                RecipeNodeSaveData recipe => CreateRecipeNode(graph, cache, import, recipe, newNodeCollection),
                _ => null
            };

            if (newNode is null)
                return null;

            newNode.RateType = nodeData.RateType;
            if (newNode.RateType == RateType.Manual) {
                double manualValue = nodeData.DesiredSetValue
                    ?? (nodeData as SupplierNodeSaveData)?.DesiredRatePerSec
                    ?? (nodeData as PassthroughNodeSaveData)?.DesiredRatePerSec
                    ?? 0;
                newNode.DesiredSetValue = manualValue;
            }

            newNode.NodeDirection = nodeData.Direction;
            if (nodeData.KeyNodeTitle is not null) {
                newNode.KeyNode = true;
                newNode.KeyNodeTitle = nodeData.KeyNodeTitle;
            }

            return newNode;
        }

        private static BaseNode? CreateConsumerNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            ConsumerNodeSaveData data,
            ProductionGraph.NewNodeBatch newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out IQuality? quality) || quality is null)
                return null;

            IItem item = ResolveItem(cache, data.ItemName);
            return TrackCreatedNode(newNodeCollection, graph.CreateConsumerNode(new ItemQualityPair(item, quality), data.Location));
        }

        private static BaseNode? CreateSupplierNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            SupplierNodeSaveData data,
            ProductionGraph.NewNodeBatch newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out IQuality? quality) || quality is null)
                return null;

            IItem item = ResolveItem(cache, data.ItemName);
            return TrackCreatedNode(newNodeCollection, graph.CreateSupplierNode(new ItemQualityPair(item, quality), data.Location));
        }

        private static BaseNode? CreatePassthroughNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            PassthroughNodeSaveData data,
            ProductionGraph.NewNodeBatch newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out IQuality? quality) || quality is null)
                return null;

            IItem item = ResolveItem(cache, data.ItemName);
            BaseNode newNode = graph.CreatePassthroughNode(new ItemQualityPair(item, quality), data.Location);
            if (newNode is PassthroughNode passthrough)
                passthrough.SimpleDraw = data.SimpleDraw;
            return TrackCreatedNode(newNodeCollection, newNode);
        }

        private static BaseNode? CreateSpoilNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            SpoilNodeSaveData data,
            ProductionGraph.NewNodeBatch newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out IQuality? quality) || quality is null)
                return null;

            IItem inputItem = ResolveItem(cache, data.InputItemName);
            IItem outputItem = ResolveItem(cache, data.OutputItemName);
            return TrackCreatedNode(newNodeCollection, graph.CreateSpoilNode(new ItemQualityPair(inputItem, quality), outputItem, data.Location));
        }

        private static BaseNode? CreatePlantNode(
            ProductionGraph graph,
            GraphImportContext import,
            PlantNodeSaveData data,
            ProductionGraph.NewNodeBatch newNodeCollection) {
            return !import.QualityLinks.TryGetValue(data.QualityName, out IQuality? quality) || quality is null
                ? null
                : !import.PlantProcessLinks.TryGetValue(data.PlantProcessId, out IPlantProcess? plantProcess)
                ? null
                : TrackCreatedNode(newNodeCollection, graph.CreatePlantNode(plantProcess, quality, data.Location));
        }

        private static BaseNode? CreateRecipeNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            RecipeNodeSaveData data,
            ProductionGraph.NewNodeBatch newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.RecipeQualityName, out IQuality? recipeQuality) || recipeQuality is null)
                return null;
            if (!import.RecipeLinks.TryGetValue(data.RecipeId, out IRecipe? recipe))
                return null;

            BaseNode? newNode = null;
            graph.CreateRecipeNodeWithSetup(new RecipeQualityPair(recipe, recipeQuality), data.Location, rNode => {
                var rNodeController = (RecipeNodeController)rNode.Controller;
                rNode.LowPriority = data.LowPriority;
                rNode.NeighbourCount = data.NeighbourCount;
                rNode.ExtraProductivityBonus = data.ExtraProductivityBonus;

                if (import.QualityLinks.TryGetValue(data.AssemblerQualityName, out IQuality? assemblerQuality) && assemblerQuality is not null) {
                    if (cache.Assemblers.TryGetValue(data.AssemblerName, out IAssembler? assembler))
                        rNodeController.SetAssembler(new AssemblerQualityPair(assembler, assemblerQuality));
                    else if (cache.MissingAssemblers.TryGetValue(data.AssemblerName, out IAssembler? missingAssembler))
                        rNodeController.SetAssembler(new AssemblerQualityPair(missingAssembler, assemblerQuality));
                }

                foreach (ModuleQualitySaveData module in data.AssemblerModules)
                    AddModule(cache, import, rNodeController.AddAssemblerModule, module);

                if (data.FuelName is not null) {
                    if (cache.Items.TryGetValue(data.FuelName, out IItem? fuel))
                        rNodeController.SetFuel(fuel);
                    else if (cache.MissingItems.TryGetValue(data.FuelName, out IItem? missingFuel))
                        rNodeController.SetFuel(missingFuel);
                } else if (rNode.SelectedAssembler.Assembler.IsBurner)
                    rNodeController.SetFuel(null);

                if (data.BurntResultName is not null) {
                    IItem? burntItem = cache.Items.TryGetValue(data.BurntResultName, out IItem? known) ? known : null;
                    if (burntItem is null)
                        cache.MissingItems.TryGetValue(data.BurntResultName, out burntItem);
                    if (rNode.FuelRemains != burntItem)
                        rNode.SetBurntOverride(burntItem);
                } else if (rNode.Fuel?.BurnResult is not null)
                    rNode.SetBurntOverride(null);

                if (data.BeaconName is not null
                    && import.QualityLinks.TryGetValue(data.BeaconQualityName ?? "", out IQuality? beaconQuality)
                    && beaconQuality is not null) {
                    if (cache.Beacons.ContainsKey(data.BeaconName))
                        rNodeController.SetBeacon(new BeaconQualityPair(cache.Beacons[data.BeaconName], beaconQuality));
                    else
                        rNodeController.SetBeacon(new BeaconQualityPair(cache.MissingBeacons[data.BeaconName], beaconQuality));

                    foreach (ModuleQualitySaveData module in data.BeaconModules)
                        AddModule(cache, import, rNodeController.AddBeaconModule, module);

                    rNode.BeaconCount = data.BeaconCount;
                    rNode.BeaconsPerAssembler = data.BeaconsPerAssembler;
                    rNode.BeaconsConst = data.BeaconsConst;
                }

                newNodeCollection.NewNodes.Add(rNode);
                newNode = rNode;
            });

            return newNode;
        }

        private static BaseNode? TrackCreatedNode(ProductionGraph.NewNodeBatch newNodeCollection, BaseNode node) {
            newNodeCollection.NewNodes.Add(node);
            return node;
        }

        private static IItem ResolveItem(DataCache cache, string itemName) =>
            cache.Items.TryGetValue(itemName, out IItem? known) && known is not null
                ? known
                : cache.MissingItems[itemName];

        private static void AddModule(
            DataCache cache,
            GraphImportContext import,
            Action<ModuleQualityPair> add,
            ModuleQualitySaveData moduleData) {
            if (!import.QualityLinks.TryGetValue(moduleData.QualityName, out IQuality? moduleQuality) || moduleQuality is null)
                return;
            if (cache.Modules.TryGetValue(moduleData.ModuleName, out IModule? module))
                add(new ModuleQualityPair(module, moduleQuality));
            else if (cache.MissingModules.TryGetValue(moduleData.ModuleName, out module))
                add(new ModuleQualityPair(module, moduleQuality));
        }

    }
}
