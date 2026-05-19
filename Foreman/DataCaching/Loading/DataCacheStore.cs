using Foreman.DataCaching.DataTypes;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman.DataCaching.Loading {
    /// <summary>Mutable preset data owned by <see cref="DataCache"/>.</summary>
    internal sealed class DataCacheStore(bool filterRecipes) {
        public Dictionary<string, string> IncludedMods { get; } = [];
        public Dictionary<string, ITechnology> Technologies { get; } = [];
        public Dictionary<string, IGroup> Groups { get; } = [];
        public Dictionary<string, ISubgroup> Subgroups { get; } = [];
        public Dictionary<string, IQuality> Qualities { get; } = [];
        public Dictionary<string, IItem> Items { get; } = [];
        public Dictionary<string, IRecipe> Recipes { get; } = [];
        public Dictionary<string, IPlantProcess> PlantProcesses { get; } = [];
        public Dictionary<string, IAssembler> Assemblers { get; } = [];
        public Dictionary<string, IModule> Modules { get; } = [];
        public Dictionary<string, IBeacon> Beacons { get; } = [];
        public List<IItem> SciencePacks { get; } = [];
        public Dictionary<IItem, ICollection<IItem>> SciencePackPrerequisites { get; } = [];

        public Dictionary<string, IQuality> MissingQualities { get; } = [];
        public Dictionary<string, IItem> MissingItems { get; } = [];
        public Dictionary<string, IAssembler> MissingAssemblers { get; } = [];
        public Dictionary<string, IModule> MissingModules { get; } = [];
        public Dictionary<string, IBeacon> MissingBeacons { get; } = [];
        public Dictionary<RecipeShort, IRecipe> MissingRecipes { get; } = new(new RecipeShortNaInPrComparer());
        public Dictionary<PlantShort, IPlantProcess> MissingPlantProcesses { get; } = [];

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

        public IQuality? DefaultQuality { get; set; }
        public uint QualityMaxChainLength { get; set; }
        public IQuality? ErrorQuality { get; set; }

        public Dictionary<string, IconColorPair>? IconCache { get; set; }

        public bool UseRecipeBWLists { get; } = filterRecipes;
    }
}
