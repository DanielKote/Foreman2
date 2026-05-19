using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Foreman.DataCaching.Loading {
    internal sealed class DataCachePostLoadProcessor(DataCache owner, DataCacheStore store) {
        private readonly DataCache _owner = owner;
        private readonly DataCacheStore _store = store;

        public void RunAfterPresetParsed() {
            SortGroupsAndSubgroups();
            RemoveRecipesWithoutAssemblers();
            ProcessAvailableStatuses();
            ProcessSciencePacks();
            CleanupGroups();
            UpdateFluidTemperatureDependencies();
        }

        //------------------------------------------------------Post-load finalization (after preset JSON is parsed)

        internal void SortGroupsAndSubgroups() {
            foreach (GroupPrototype g in _store.Groups.Values.Cast<GroupPrototype>())
                g.SortSubgroups();
            foreach (SubgroupPrototype sg in _store.Subgroups.Values.Cast<SubgroupPrototype>())
                sg.SortIRs();
        }

        internal void RemoveRecipesWithoutAssemblers() {
            // Drop only recipes that cannot be crafted in-game (no machine for any exported crafting category).
            // Hidden recipes with machines stay in cache so the user can enable them via "Show Hidden".
            foreach (RecipePrototype recipe in _store.Recipes.Values.OfType<RecipePrototype>().Where(r => r.AssemblersInternal.Count == 0)) {
                if (recipe.HasCraftingMachineInPreset)
                    continue;
                foreach (ItemPrototype ingredient in recipe.IngredientListInternal)
                    ingredient.ConsumptionRecipesInternal.Remove(recipe);
                foreach (ItemPrototype product in recipe.ProductListInternal)
                    product.ProductionRecipesInternal.Remove(recipe);
                foreach (TechnologyPrototype tech in recipe.MyUnlockTechnologiesInternal)
                    tech.UnlockedRecipesInternal.Remove(recipe);
                foreach (ModulePrototype module in recipe.AssemblerModulesInternal)
                    module.RecipesInternal.Remove(recipe);
                recipe.MySubgroupInternal.RecipesInternal.Remove(recipe);

                _store.Recipes.Remove(recipe.Name);
                ErrorLogging.LogLine(string.Format(CultureInfo.InvariantCulture, "Removal of {0} due to having no _store.Assemblers associated with it.", recipe));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Removal of {0} due to having no _store.Assemblers associated with it.", recipe));
            }
        }

        internal void ProcessSciencePacks() {
            //DFS for processing the required sci packs of each technology. Basically some research only requires 1 sci pack, but to unlock it requires researching tech with many sci packs. Need to account for that
            var techRequirements = new Dictionary<TechnologyPrototype, HashSet<IItem>>();
            var sciPacks = new HashSet<IItem>();
            HashSet<IItem> TechRequiredSciPacks(TechnologyPrototype tech) {
                if (techRequirements.TryGetValue(tech, out var value))
                    return value;

                var requiredItems = new HashSet<IItem>(tech.SciPackListInternal);
                foreach (TechnologyPrototype prereq in tech.PrerequisitesInternal)
                    foreach (IItem sciPack in TechRequiredSciPacks(prereq))
                        requiredItems.Add(sciPack);

                sciPacks.UnionWith(requiredItems);
                techRequirements.Add(tech, requiredItems);

                return requiredItems;
            }

            //tech ordering - set each technology's 'tier' to be its furthest distance from the 'starting tech' node
            HashSet<TechnologyPrototype> visitedTech = [];
            if (_store.StartingTech is not null)
                visitedTech.Add(_store.StartingTech); //tier 0, everything starts from here.
            int GetTechnologyTier(TechnologyPrototype tech) {
                if (!visitedTech.Contains(tech)) {
                    int maxPrerequisiteTier = 0;
                    foreach (TechnologyPrototype prereq in tech.PrerequisitesInternal)
                        maxPrerequisiteTier = Math.Max(maxPrerequisiteTier, GetTechnologyTier(prereq));
                    tech.Tier = maxPrerequisiteTier + 1;
                    visitedTech.Add(tech);
                }
                return tech.Tier;
            }

            //science pack processing - DF again where we want to calculate which science packs are required to get to the given science pack
            var visitedPacks = new HashSet<IItem>();
            void UpdateSciencePackPrerequisites(IItem sciPack) {
                if (visitedPacks.Contains(sciPack))
                    return;

                //for simplicities sake we will only account for prerequisites of the first available production recipe (or first non-available if no available production _store.Recipes exist). This means that if (for who knows what reason) there are multiple valid production _store.Recipes only the first one will count!
                var prerequisites = new HashSet<IItem>(sciPack.ProductionRecipes.OrderByDescending(r => r.Available).FirstOrDefault()?.MyUnlockTechnologies.OrderByDescending(t => t.Available).FirstOrDefault()?.SciPackList ?? []);
                foreach (IRecipe r in sciPack.ProductionRecipes)
                    foreach (ITechnology t in r.MyUnlockTechnologies)
                        prerequisites.IntersectWith(t.SciPackList);

                //prerequisites now contains all the immediate required sci packs. we will now Update their prerequisites via this function, then add their prerequisites to our own set before finalizing it.
                foreach (IItem prereq in prerequisites.ToList()) {
                    UpdateSciencePackPrerequisites(prereq);
                    prerequisites.UnionWith(_store.SciencePackPrerequisites[prereq]);
                }
                _store.SciencePackPrerequisites.Add(sciPack, prerequisites);
                visitedPacks.Add(sciPack);
            }

            //step 1: update tech unlock status & science packs (add a 0 cost pack to the tech if it has no such requirement but its prerequisites do), set tech tier
            foreach (TechnologyPrototype tech in _store.Technologies.Values.Cast<TechnologyPrototype>()) {
                TechRequiredSciPacks(tech);
                GetTechnologyTier(tech);
                foreach (ItemPrototype sciPack in techRequirements[tech].Cast<ItemPrototype>())
                    tech.InternalOneWayAddSciPack(sciPack, 0);
            }

            //step 2: further sci pack processing -> for every available science pack we want to build a list of science packs necessary to aquire it. In a situation with multiple (non-equal) research paths (ex: 3 can be aquired through either pack 1&2 or pack 1 alone), take the intersect (1 in this case). These will be added to the sci pack requirement lists
            foreach (IItem sciPack in sciPacks)
                UpdateSciencePackPrerequisites(sciPack);


            //step 2.5: update the technology science packs to account for the science pack prerequisites
            foreach (TechnologyPrototype tech in _store.Technologies.Values.Cast<TechnologyPrototype>())
                foreach (IItem sciPack in tech.SciPackList.ToList())
                    foreach (ItemPrototype reqSciPack in _store.SciencePackPrerequisites[sciPack].Cast<ItemPrototype>())
                        tech.InternalOneWayAddSciPack(reqSciPack, 0);

            //step 3: calculate science pack tier (minimum tier of technology that unlocks the recipe for the given science pack). also make the _store.SciencePacks list.
            var sciencePackTiers = new Dictionary<IItem, int>();
            foreach (ItemPrototype sciPack in sciPacks.Cast<ItemPrototype>()) {
                int minTier = int.MaxValue;
                foreach (IRecipe recipe in sciPack.ProductionRecipesInternal)
                    foreach (ITechnology tech in recipe.MyUnlockTechnologies)
                        minTier = Math.Min(minTier, tech.Tier);
                if (minTier == int.MaxValue) //there are no _store.Recipes for this sci pack. EX: space science pack. We will grant it the same tier as the first tech to require this sci pack. This should sort them relatively correctly (ex - placing space sci pack last, and placing seablock starting tech first)
                    minTier = techRequirements.Where(kvp => kvp.Value.Contains(sciPack)).Select(kvp => kvp.Key).Min(t => t.Tier);
                sciencePackTiers.Add(sciPack, minTier);
                _store.SciencePacks.Add(sciPack);
            }

            //step 4: update all science pack lists (main _store.SciencePacks list, plus SciPackList of every technology). Sorting is done by A: if science pack B has science pack A as a prerequisite (in sciPackRequiredPacks), then B goes after A. If neither has the other as a prerequisite, then compare by sciencePack tiers
            _store.SciencePacks.Sort((s1, s2) => sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) + (_store.SciencePackPrerequisites[s1].Contains(s2) ? 1000 : _store.SciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));
            foreach (TechnologyPrototype tech in _store.Technologies.Values.Cast<TechnologyPrototype>())
                tech.SciPackListInternal.Sort((s1, s2) => sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) + (_store.SciencePackPrerequisites[s1].Contains(s2) ? 1000 : _store.SciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));

            //step 5: create science pack lists for each recipe (list of distinct min-pack sets -> ex: if recipe can be aquired through 4 techs with [ A+B, A+B, A+C, A+B+C ] science pack requirements, we will only include A+B and A+C
            foreach (RecipePrototype recipe in _store.Recipes.Values.Cast<RecipePrototype>()) {
                var sciPackLists = new List<List<IItem>>();
                foreach (TechnologyPrototype tech in recipe.MyUnlockTechnologiesInternal) {
                    bool exists = false;
                    foreach (List<IItem> sciPackList in sciPackLists.ToList()) {
                        if (!sciPackList.Except(tech.SciPackListInternal).Any()) // sci pack lists already includes a list that is a subset of the _store.Technologies sci pack list (ex: already have A+B while tech's is A+B+C)
                            exists = true;
                        else if (!tech.SciPackListInternal.Except(sciPackList).Any()) //technology sci pack list is a subset of an already included sci pack list. we will add thi to the list and delete the existing one (ex: have A+B while tech's is A -> need to remove A+B and include A)
                            sciPackLists.Remove(sciPackList);
                    }
                    if (!exists)
                        sciPackLists.Add(tech.SciPackListInternal);
                }
                recipe.MyUnlockSciencePacks = sciPackLists;
            }
        }

        internal void ProcessAvailableStatuses() {
            //quick function to depth-first search the tech tree to calculate the availability of the technology. Hashset used to keep track of visited tech and not have to re-check them.
            //NOTE: factorio ensures no cyclic, so we are guaranteed to have a directed acyclic graph (may be disconnected)
            var unlockableTechSet = new HashSet<TechnologyPrototype>();
            bool IsUnlockable(TechnologyPrototype tech) {
                if (!tech.Available)
                    return false;
                else if (unlockableTechSet.Contains(tech))
                    return true;
                else if (tech.PrerequisitesInternal.Count == 0)
                    return true;
                else {
                    bool available = true;
                    foreach (TechnologyPrototype preTech in tech.PrerequisitesInternal)
                        available = available && IsUnlockable(preTech);
                    tech.Available = available;

                    if (available)
                        unlockableTechSet.Add(tech);
                    return available;
                }
            }

            //step 0: check availability of _store.Technologies
            foreach (TechnologyPrototype tech in _store.Technologies.Values)
                IsUnlockable(tech);

            //step 1: update recipe unlock status
            foreach (RecipePrototype recipe in _store.Recipes.Values)
                recipe.Available = recipe.MyUnlockTechnologiesInternal.Any(t => t.Available);

            //step 2: mark any recipe for barelling / crating as unavailable
            if (_store.UseRecipeBWLists) {
                foreach (RecipePrototype recipe in _store.Recipes.Values) {
                    //part 1: make unavailable if recipe fits the black & doesnt fit the white recipe black lists (these should be the 'barelling' and 'unbarelling' _store.Recipes)
                    if (!DataCacheRecipeFilters.WhiteList.Any(white => white.IsMatch(recipe.Name)) && DataCacheRecipeFilters.BlackList.Any(black => black.IsMatch(recipe.Name))) //if we dont match a whitelist and match a blacklist...
                        recipe.Available = false;
                    //part 2: make unavailable if recipe fits the DataCacheRecipeFilters.RecyclingItemNameBlackList (should remove any of the barel recycling _store.Recipes added by 2.0 SA)
                    foreach (KeyValuePair<string, Regex> recycleBL in DataCacheRecipeFilters.RecyclingItemNameBlackList)
                        if (recipe.ProductListInternal.Count == 1 && (IItem)recipe.ProductListInternal[0] == _store.Items[recycleBL.Key] && recipe.IngredientListInternal.Count == 1 && recycleBL.Value.IsMatch(recipe.IngredientListInternal[0].Name))
                            recipe.Available = false;
                }
            }


            //step 3: mark any recipe with no unlocks, or 0->0 _store.Recipes (industrial revolution... what are those aetheric glow _store.Recipes?) as unavailable.
            foreach (RecipePrototype recipe in _store.Recipes.Values)
                if (recipe.MyUnlockTechnologiesInternal.Count == 0 || (recipe.ProductListInternal.Count == 0 && recipe.IngredientListInternal.Count == 0 && !recipe.Name.StartsWith("§§", StringComparison.Ordinal))) //§§ denotes foreman added _store.Recipes. ignored during this pass (but not during the assembler check pass)
                    recipe.Available = false;

            //step 4 (loop): recipe/assembler propagation, useless items, spoil/plant results — see ItemAvailabilityFixpoint
            ItemAvailabilityFixpoint.Run(_store);

            //step 5: set the 'default' enabled statuses of _store.Recipes,_store.Assemblers,_store.Modules & _store.Beacons to their available status.
            foreach (RecipePrototype recipe in _store.Recipes.Values)
                recipe.Enabled = recipe.Available && !recipe.HiddenInGame;
            foreach (AssemblerPrototype assembler in _store.Assemblers.Values)
                assembler.Enabled = assembler.Available;
            foreach (ModulePrototype module in _store.Modules.Values)
                module.Enabled = module.Available;
            foreach (BeaconPrototype beacon in _store.Beacons.Values)
                beacon.Enabled = beacon.Available;
            _store.PlayerAssembler?.Enabled = true; //its enabled, so it can theoretically be used, but it is set as 'unavailable' so a warning will be issued if you use it.

            _store.RocketAssembler?.Enabled = _store.Assemblers["rocket-silo"]?.Enabled ?? false; //rocket assembler is set to enabled if rocket silo is enabled
            _store.RocketAssembler?.Available = _store.Assemblers["rocket-silo"] is not null; //override
        }

        internal void CleanupGroups() {
            //step 6: clean up _store.Groups and _store.Subgroups (delete any _store.Subgroups that have no _store.Items/_store.Recipes, then delete any _store.Groups that have no _store.Subgroups)
            foreach (SubgroupPrototype subgroup in _store.Subgroups.Values.ToList()) {
                if (subgroup.ItemsInternal.Count == 0 && subgroup.RecipesInternal.Count == 0 && subgroup.MyGroup is GroupPrototype gp) {
                    gp.SubgroupsInternal.Remove(subgroup);
                    _store.Subgroups.Remove(subgroup.Name);
                }
            }
            foreach (GroupPrototype group in _store.Groups.Values.ToList())
                if (group.SubgroupsInternal.Count == 0)
                    _store.Groups.Remove(group.Name);

            //step 7: update _store.Subgroups and _store.Groups to set them to unavailable if they only contain unavailable _store.Items/_store.Recipes
            foreach (SubgroupPrototype subgroup in _store.Subgroups.Values)
                if (!subgroup.ItemsInternal.Any(i => i.Available) && !subgroup.RecipesInternal.Any(r => r.Available))
                    subgroup.Available = false;
            foreach (GroupPrototype group in _store.Groups.Values)
                if (!group.SubgroupsInternal.Any(sg => sg.Available))
                    group.Available = false;

            //step 8: sort _store.Groups/_store.Subgroups
            foreach (GroupPrototype group in _store.Groups.Values)
                group.SortSubgroups();
            foreach (SubgroupPrototype sgroup in _store.Subgroups.Values)
                sgroup.SortIRs();

        }

        internal void UpdateFluidTemperatureDependencies() {
            //step 9: update the temperature dependent status of _store.Items (fluids)
            foreach (FluidPrototype fluid in _store.Items.Values.Where(i => i is IFluid)) {
                var productionRange = new FRange(double.MaxValue, double.MinValue);
                var consumptionRange = new FRange(double.MinValue, double.MaxValue); //a bit different -> the min value is the LARGEST minimum of each consumption recipe, and the max value is the SMALLEST max of each consumption recipe

                foreach (IRecipe recipe in fluid.ProductionRecipesInternal) {
                    productionRange.Min = Math.Min(productionRange.Min, recipe.ProductTemperatureMap[fluid]);
                    productionRange.Max = Math.Max(productionRange.Max, recipe.ProductTemperatureMap[fluid]);
                }
                foreach (IRecipe recipe in fluid.ConsumptionRecipesInternal) {
                    consumptionRange.Min = Math.Max(consumptionRange.Min, recipe.IngredientTemperatureMap[fluid].Min);
                    consumptionRange.Max = Math.Min(consumptionRange.Max, recipe.IngredientTemperatureMap[fluid].Max);
                }
                fluid.IsTemperatureDependent = !(consumptionRange.Contains(productionRange));
            }
        }
    }
}
