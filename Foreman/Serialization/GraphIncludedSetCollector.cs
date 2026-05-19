using Foreman.DataCaching.DataTypes;
using Foreman.Models.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.Serialization {
    /// <summary>Collects preset entity names referenced by a graph fragment (for save/import).</summary>
    internal sealed class GraphIncludedSetCollector {
        public HashSet<string> Items { get; } = [];
        public HashSet<string> Assemblers { get; } = [];
        public HashSet<string> Modules { get; } = [];
        public HashSet<string> Beacons { get; } = [];
        public HashSet<IRecipe> Recipes { get; } = [];
        public HashSet<IRecipe> MissingRecipes { get; } = new(new RecipeNaInPrComparer());
        public HashSet<IPlantProcess> PlantProcesses { get; } = [];
        public HashSet<IPlantProcess> MissingPlantProcesses { get; } = new(new PlantNaInPrComparer());
        public HashSet<KeyValuePair<string, int>> Qualities { get; } = [];

        public void IncludeQuality(IQuality? quality) {
            if (quality is not null)
                Qualities.Add(new KeyValuePair<string, int>(quality.Name, quality.Level));
        }

        public void CollectFromNodes(IEnumerable<BaseNode> nodes, IQuality? defaultAssemblerQuality) {
            if (defaultAssemblerQuality is not null)
                IncludeQuality(defaultAssemblerQuality);

            foreach (BaseNode node in nodes) {
                switch (node) {
                    case RecipeNode rnode:
                        if (rnode.BaseRecipe.Recipe?.IsMissing is true)
                            MissingRecipes.Add(rnode.BaseRecipe.Recipe);
                        else if (rnode.BaseRecipe.Recipe is not null)
                            Recipes.Add(rnode.BaseRecipe.Recipe);

                        Assemblers.Add(rnode.SelectedAssembler.Assembler.Name);

                        if (rnode.SelectedBeacon && rnode.SelectedBeacon.Beacon is not null)
                            Beacons.Add(rnode.SelectedBeacon.Beacon.Name);

                        Modules.UnionWith(rnode.AssemblerModules.Select(m => m.Module.Name));
                        Modules.UnionWith(rnode.BeaconModules.Select(m => m.Module.Name));

                        IncludeQuality(rnode.BaseRecipe.Quality);
                        IncludeQuality(rnode.SelectedAssembler.Quality);
                        if (rnode.SelectedBeacon)
                            IncludeQuality(rnode.BaseRecipe.Quality);
                        foreach (var m in rnode.AssemblerModules)
                            IncludeQuality(m.Quality);
                        foreach (var m in rnode.BeaconModules)
                            IncludeQuality(m.Quality);
                        break;
                    case PlantNode pnode:
                        if (pnode.BasePlantProcess.IsMissing)
                            MissingPlantProcesses.Add(pnode.BasePlantProcess);
                        else
                            PlantProcesses.Add(pnode.BasePlantProcess);
                        IncludeQuality(pnode.Seed.Quality);
                        break;
                    case ConsumerNode cnode:
                        IncludeQuality(cnode.ConsumedItem.Quality);
                        break;
                    case SupplierNode snode:
                        IncludeQuality(snode.SuppliedItem.Quality);
                        break;
                    case PassthroughNode passnode:
                        IncludeQuality(passnode.PassthroughItem.Quality);
                        break;
                    case SpoilNode spoilnode:
                        IncludeQuality(spoilnode.InputItem.Quality);
                        break;
                }

                Items.UnionWith(node.Inputs.Select(i => i.Item?.Name).OfType<string>());
                Items.UnionWith(node.Outputs.Select(i => i.Item?.Name).OfType<string>());
            }
        }

        public List<RecipeShort> BuildRecipeShortList() {
            var list = Recipes.Select(recipe => new RecipeShort(recipe)).ToList();
            list.AddRange(MissingRecipes.Select(recipe => new RecipeShort(recipe)));
            return list;
        }

        public List<PlantShort> BuildPlantShortList() {
            var list = PlantProcesses.Select(p => new PlantShort(p)).ToList();
            list.AddRange(MissingPlantProcesses.Select(p => new PlantShort(p)));
            return list;
        }
    }
}
