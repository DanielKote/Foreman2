using System.Collections.Generic;
using System.Drawing;

namespace Foreman {
    /// <summary>Mutable preset data owned by <see cref="DataCache"/>.</summary>
    internal sealed class DataCacheStore {
        public Dictionary<string, string> IncludedMods { get; } = new();
        public Dictionary<string, Technology> Technologies { get; } = new();
        public Dictionary<string, Group> Groups { get; } = new();
        public Dictionary<string, Subgroup> Subgroups { get; } = new();
        public Dictionary<string, Quality> Qualities { get; } = new();
        public Dictionary<string, Item> Items { get; } = new();
        public Dictionary<string, Recipe> Recipes { get; } = new();
        public Dictionary<string, PlantProcess> PlantProcesses { get; } = new();
        public Dictionary<string, Assembler> Assemblers { get; } = new();
        public Dictionary<string, Module> Modules { get; } = new();
        public Dictionary<string, Beacon> Beacons { get; } = new();
        public List<Item> SciencePacks { get; } = new();
        public Dictionary<Item, ICollection<Item>> SciencePackPrerequisites { get; } = new();

        public Dictionary<string, Quality> MissingQualities { get; } = new();
        public Dictionary<string, Item> MissingItems { get; } = new();
        public Dictionary<string, Assembler> MissingAssemblers { get; } = new();
        public Dictionary<string, Module> MissingModules { get; } = new();
        public Dictionary<string, Beacon> MissingBeacons { get; } = new();
        public Dictionary<RecipeShort, Recipe> MissingRecipes { get; } = new(new RecipeShortNaInPrComparer());
        public Dictionary<PlantShort, PlantProcess> MissingPlantProcesses { get; } = new();

        public GroupPrototype? ExtraFormanGroup { get; set; }
        public SubgroupPrototype? ExtractionSubgroupItems { get; set; }
        public SubgroupPrototype? ExtractionSubgroupFluids { get; set; }
        public SubgroupPrototype? ExtractionSubgroupFluidsOP { get; set; }
        public SubgroupPrototype? EnergySubgroupBoiling { get; set; }
        public SubgroupPrototype? EnergySubgroupEnergy { get; set; }
        public SubgroupPrototype? RocketLaunchSubgroup { get; set; }

        public ItemPrototype? HeatItem { get; set; }
        public RecipePrototype? HeatRecipe { get; set; }
        public RecipePrototype? BurnerRecipe { get; set; }
        public Bitmap? ElectricityIcon { get; set; }

        public AssemblerPrototype? PlayerAssembler { get; set; }
        public AssemblerPrototype? RocketAssembler { get; set; }
        public SubgroupPrototype? MissingSubgroup { get; set; }
        public TechnologyPrototype? StartingTech { get; set; }
        public AssemblerPrototype? MissingAssembler { get; set; }

        public Quality? DefaultQuality { get; set; }
        public uint QualityMaxChainLength { get; set; }
        public Quality? ErrorQuality { get; set; }

        public Dictionary<string, IconColorPair>? IconCache { get; set; }

        public readonly bool UseRecipeBWLists;

        public DataCacheStore(bool filterRecipes) => UseRecipeBWLists = filterRecipes;
    }
}