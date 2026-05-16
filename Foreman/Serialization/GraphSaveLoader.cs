using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman {
    /// <summary>Applies parsed save documents to <see cref="ProductionGraph"/> and related UI state.</summary>
    public static class GraphSaveLoader {
        private sealed class GraphImportContext(
            Dictionary<string, Quality?> qualityLinks,
            Dictionary<long, Recipe> recipeLinks,
            Dictionary<long, PlantProcess> plantProcessLinks) {
            public Dictionary<string, Quality?> QualityLinks { get; } = qualityLinks;
            public Dictionary<long, Recipe> RecipeLinks { get; } = recipeLinks;
            public Dictionary<long, PlantProcess> PlantProcessLinks { get; } = plantProcessLinks;
        }

        public static ProductionGraph.NewNodeCollection LoadProductionGraph(
            ProductionGraph graph,
            DataCache cache,
            ProductionGraphSaveDocument document,
            bool applySolverSettings) {
            var newNodeCollection = new ProductionGraph.NewNodeCollection();
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

                    if (!import.QualityLinks.TryGetValue(link.QualityName, out Quality? quality) || quality is null)
                        continue;

                    ItemQualityPair item = cache.Items.ContainsKey(link.ItemName)
                        ? new ItemQualityPair(cache.Items[link.ItemName], quality)
                        : new ItemQualityPair(cache.MissingItems[link.ItemName], quality);

                    if (LinkChecker.IsPossibleConnection(item, supplier, consumer))
                        newNodeCollection.newLinks.Add(graph.CreateLink(supplier, consumer, item));
                }
            } catch (Exception e) {
                ErrorLogging.LogException(e, "Error loading nodes into production graph");
                graph.DeleteNodes(newNodeCollection.newNodes);
                return new ProductionGraph.NewNodeCollection();
            }

            return newNodeCollection;
        }

        public static NodeCopyOptions? ToNodeCopyOptions(NodeCopyOptionsSaveDocument document, DataCache cache) =>
            NodeCopyOptions.FromSaveDocument(document, cache);

        private static GraphImportContext ImportIncludedEntities(DataCache cache, ProductionGraphSaveDocument document) {
            cache.ProcessImportedItemsSet(document.IncludedItems);
            Dictionary<string, Quality?> qualityLinks = cache.ProcessImportedQualitiesSet(document.IncludedQualities);
            cache.ProcessImportedAssemblersSet(document.IncludedAssemblers);
            cache.ProcessImportedModulesSet(document.IncludedModules);
            cache.ProcessImportedBeaconsSet(document.IncludedBeacons);
            Dictionary<long, Recipe> recipeLinks = cache.ProcessImportedRecipesSet(document.IncludedRecipes);
            Dictionary<long, PlantProcess> plantProcessLinks = cache.ProcessImportedPlantProcessesSet(document.IncludedPlantProcesses);
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
            if (import.QualityLinks.TryGetValue(solver.DefaultQualityName, out Quality? defaultQuality))
                graph.DefaultAssemblerQuality = defaultQuality;
        }

        private static BaseNode? CreateNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            GraphNodeSaveData nodeData,
            ProductionGraph.NewNodeCollection newNodeCollection) {
            BaseNode? newNode = nodeData switch {
                ConsumerNodeSaveData consumer => CreateConsumerNode(graph, cache, import, consumer, newNodeCollection),
                SupplierNodeSaveData supplier => CreateSupplierNode(graph, cache, import, supplier, newNodeCollection),
                PassthroughNodeSaveData passthrough => CreatePassthroughNode(graph, cache, import, passthrough, newNodeCollection),
                SpoilNodeSaveData spoil => CreateSpoilNode(graph, cache, import, spoil, newNodeCollection),
                PlantNodeSaveData plant => CreatePlantNode(graph, cache, import, plant, newNodeCollection),
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
            ProductionGraph.NewNodeCollection newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out Quality? quality) || quality is null)
                return null;

            Item item = ResolveItem(cache, data.ItemName);
            return TrackCreatedNode(newNodeCollection, graph.CreateConsumerNode(new ItemQualityPair(item, quality), data.Location));
        }

        private static BaseNode? CreateSupplierNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            SupplierNodeSaveData data,
            ProductionGraph.NewNodeCollection newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out Quality? quality) || quality is null)
                return null;

            Item item = ResolveItem(cache, data.ItemName);
            return TrackCreatedNode(newNodeCollection, graph.CreateSupplierNode(new ItemQualityPair(item, quality), data.Location));
        }

        private static BaseNode? CreatePassthroughNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            PassthroughNodeSaveData data,
            ProductionGraph.NewNodeCollection newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out Quality? quality) || quality is null)
                return null;

            Item item = ResolveItem(cache, data.ItemName);
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
            ProductionGraph.NewNodeCollection newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out Quality? quality) || quality is null)
                return null;

            Item inputItem = ResolveItem(cache, data.InputItemName);
            Item outputItem = ResolveItem(cache, data.OutputItemName);
            return TrackCreatedNode(newNodeCollection, graph.CreateSpoilNode(new ItemQualityPair(inputItem, quality), outputItem, data.Location));
        }

        private static BaseNode? CreatePlantNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            PlantNodeSaveData data,
            ProductionGraph.NewNodeCollection newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.QualityName, out Quality? quality) || quality is null)
                return null;
            if (!import.PlantProcessLinks.TryGetValue(data.PlantProcessId, out PlantProcess? plantProcess))
                return null;

            return TrackCreatedNode(newNodeCollection, graph.CreatePlantNode(plantProcess, quality, data.Location));
        }

        private static BaseNode? CreateRecipeNode(
            ProductionGraph graph,
            DataCache cache,
            GraphImportContext import,
            RecipeNodeSaveData data,
            ProductionGraph.NewNodeCollection newNodeCollection) {
            if (!import.QualityLinks.TryGetValue(data.RecipeQualityName, out Quality? recipeQuality) || recipeQuality is null)
                return null;
            if (!import.RecipeLinks.TryGetValue(data.RecipeId, out Recipe? recipe))
                return null;

            BaseNode? newNode = null;
            graph.CreateRecipeNodeWithSetup(new RecipeQualityPair(recipe, recipeQuality), data.Location, rNode => {
                var rNodeController = (RecipeNodeController)rNode.Controller;
                rNode.LowPriority = data.LowPriority;
                rNode.NeighbourCount = data.NeighbourCount;
                rNode.ExtraProductivityBonus = data.ExtraProductivityBonus;

                if (import.QualityLinks.TryGetValue(data.AssemblerQualityName, out Quality? assemblerQuality) && assemblerQuality is not null) {
                    if (cache.Assemblers.TryGetValue(data.AssemblerName, out Assembler? assembler))
                        rNodeController.SetAssembler(new AssemblerQualityPair(assembler, assemblerQuality));
                    else if (cache.MissingAssemblers.TryGetValue(data.AssemblerName, out Assembler? missingAssembler))
                        rNodeController.SetAssembler(new AssemblerQualityPair(missingAssembler, assemblerQuality));
                }

                foreach (ModuleQualitySaveData module in data.AssemblerModules)
                    AddModule(cache, import, rNodeController.AddAssemblerModule, module);

                if (data.FuelName is not null) {
                    if (cache.Items.TryGetValue(data.FuelName, out Item? fuel))
                        rNodeController.SetFuel(fuel);
                    else if (cache.MissingItems.TryGetValue(data.FuelName, out Item? missingFuel))
                        rNodeController.SetFuel(missingFuel);
                } else if (rNode.SelectedAssembler.Assembler.IsBurner)
                    rNodeController.SetFuel(null);

                if (data.BurntResultName is not null) {
                    Item? burntItem = cache.Items.TryGetValue(data.BurntResultName, out Item? known) ? known : null;
                    if (burntItem is null)
                        cache.MissingItems.TryGetValue(data.BurntResultName, out burntItem);
                    if (rNode.FuelRemains != burntItem)
                        rNode.SetBurntOverride(burntItem);
                } else if (rNode.Fuel?.BurnResult is not null)
                    rNode.SetBurntOverride(null);

                if (data.BeaconName is not null
                    && import.QualityLinks.TryGetValue(data.BeaconQualityName ?? "", out Quality? beaconQuality)
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

                newNodeCollection.newNodes.Add(rNode);
                newNode = rNode;
            });

            return newNode;
        }

        private static BaseNode? TrackCreatedNode(ProductionGraph.NewNodeCollection newNodeCollection, BaseNode node) {
            newNodeCollection.newNodes.Add(node);
            return node;
        }

        private static Item ResolveItem(DataCache cache, string itemName) =>
            cache.Items.TryGetValue(itemName, out Item? known) && known is not null
                ? known
                : cache.MissingItems[itemName];

        private static void AddModule(
            DataCache cache,
            GraphImportContext import,
            Action<ModuleQualityPair> add,
            ModuleQualitySaveData moduleData) {
            if (!import.QualityLinks.TryGetValue(moduleData.QualityName, out Quality? moduleQuality) || moduleQuality is null)
                return;
            if (cache.Modules.TryGetValue(moduleData.ModuleName, out Module? module))
                add(new ModuleQualityPair(module, moduleQuality));
            else if (cache.MissingModules.TryGetValue(moduleData.ModuleName, out module))
                add(new ModuleQualityPair(module, moduleQuality));
        }

    }
}