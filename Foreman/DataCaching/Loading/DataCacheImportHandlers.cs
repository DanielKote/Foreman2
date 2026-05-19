using Foreman.DataCaching.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.DataCaching.Loading {
    internal sealed class DataCacheImportHandlers(DataCache owner, DataCacheStore store) {
        private readonly DataCache _owner = owner;
        private readonly DataCacheStore _store = store;

        //------------------------------------------------------Import processing

        public void ProcessImportedItemsSet(IEnumerable<string> itemNames) //will ensure that all _store.Items are now part of the data cache -> existing ones (regular and missing) are skipped, new ones are added to MissingItems
        {
            foreach (string iItem in itemNames) {
                if (_store.MissingSubgroup is not null && !_store.Items.ContainsKey(iItem) && !_store.MissingItems.ContainsKey(iItem)) //want to check for missing _store.Items too - in this case dont want duplicates
                {
                    var missingItem = new ItemPrototype(_owner, iItem, iItem, _store.MissingSubgroup, "", true); //just assume it isnt a fluid. we dont honestly care (no temperatures)
                    _store.MissingItems.Add(missingItem.Name, missingItem);
                }
            }
        }

        public Dictionary<string, IQuality?> ProcessImportedQualitiesSet(IEnumerable<KeyValuePair<string, int>> qualityPairs) {
            //check that a quality exists in the set of _store.Qualities (missing or otherwise) that has the correct level; if not, make a new one
            Dictionary<string, IQuality?> qualityMap = [];

            foreach (var quality in qualityPairs) {
                //check quality sets for any direct matches (name & level)
                if (_store.Qualities.Values.Any(q => q.Name == quality.Key && q.Level == quality.Value)) {
                    qualityMap.Add(quality.Key, _store.Qualities[quality.Key]);
                    continue;
                } else if (_store.MissingQualities.Values.Any(q => q.Name == quality.Key && q.Level == quality.Value)) {
                    qualityMap.Add(quality.Key, _store.MissingQualities[quality.Key]);
                    continue;
                }

                //check for any matching level quality in the base chain (starting from 'normal' and going until null)
                IQuality? curQuality = _store.DefaultQuality;
                while (curQuality != null) {
                    if (curQuality.Level == quality.Value)
                        break;
                    curQuality = curQuality.NextQuality;
                }
                if (curQuality != null) {
                    qualityMap.Add(quality.Key, curQuality);
                    continue;
                }

                //step 3: check if there is a quality of the same level
                curQuality = _store.Qualities.Values.FirstOrDefault(q => q.Level == quality.Value);
                if (curQuality != null) {
                    qualityMap.Add(quality.Key, curQuality);
                    continue;
                }
                curQuality = _store.MissingQualities.Values.FirstOrDefault(q => q.Level == quality.Value);
                if (curQuality != null) {
                    qualityMap.Add(quality.Key, curQuality);
                    continue;
                }

                //step 4: no other option, make a new quality and add it to missing _store.Qualities
                string missingQualityName = quality.Key;
                while (_store.Qualities.ContainsKey(missingQualityName) || _store.MissingQualities.ContainsKey(missingQualityName))
                    missingQualityName += "_";

                var missingQuality = new QualityPrototype(_owner, missingQualityName, quality.Key, "-", true) {
                    Level = quality.Value
                };
                _store.MissingQualities.Add(missingQuality.Name, missingQuality);
                qualityMap.Add(quality.Key, missingQuality);
            }

            return qualityMap;
        }

        public void ProcessImportedAssemblersSet(IEnumerable<string> assemblerNames) {
            foreach (string iAssembler in assemblerNames) {
                if (!_store.Assemblers.ContainsKey(iAssembler) && !_store.MissingAssemblers.ContainsKey(iAssembler)) {
                    var missingAssembler = new AssemblerPrototype(_owner, iAssembler, iAssembler, EntityType.Assembler, EnergySource.Void, true); //dont know, dont care about entity type we will just treat it as a void-assembler (and let fuel io + recipe figure it out)
                    _store.MissingAssemblers.Add(missingAssembler.Name, missingAssembler);
                }
            }
        }

        public void ProcessImportedModulesSet(IEnumerable<string> moduleNames) {
            foreach (string iModule in moduleNames) {
                if (!_store.Modules.ContainsKey(iModule) && !_store.MissingModules.ContainsKey(iModule)) {
                    var missingModule = new ModulePrototype(_owner, iModule, iModule, true);
                    _store.MissingModules.Add(missingModule.Name, missingModule);
                }
            }
        }

        public void ProcessImportedBeaconsSet(IEnumerable<string> beaconNames) {
            foreach (string iBeacon in beaconNames) {
                if (!_store.Beacons.ContainsKey(iBeacon) && !_store.MissingBeacons.ContainsKey(iBeacon)) {
                    var missingBeacon = new BeaconPrototype(_owner, iBeacon, iBeacon, EnergySource.Void, true);
                    _store.MissingBeacons.Add(missingBeacon.Name, missingBeacon);
                }
            }
        }

        public Dictionary<long, IRecipe> ProcessImportedRecipesSet(IEnumerable<RecipeShort> recipeShorts) //will ensure all _store.Recipes are now part of the data cache -> each one is checked against existing _store.Recipes (regular & missing), and if it doesnt exist are added to MissingRecipes. Returns a set of links of original recipeID (NOT! the noew recipeIDs) to the recipe
        {
            Dictionary<long, IRecipe> recipeLinks = [];
            foreach (RecipeShort recipeShort in recipeShorts) {
                IRecipe? recipe = null;

                //recipe check #1 : does its name exist in database (note: we dont quite care about extra missing _store.Recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)
                bool recipeExists = _store.Recipes.ContainsKey(recipeShort.Name);
                if (recipeExists) {
                    //recipe check #2 : do the number of ingredients & products match?
                    recipe = _store.Recipes[recipeShort.Name];
                    recipeExists &= recipeShort.Ingredients.Count == recipe.IngredientList.Count;
                    recipeExists &= recipeShort.Products.Count == recipe.ProductList.Count;
                }
                if (recipeExists) {
                    //recipe check #3 : do the ingredients & products from the loaded data match the actual recipe? (names, not quantities -> this is to allow some _store.Recipes to pass; ex: normal->expensive might change the values, but importing such a recipe should just use the 'correct' quantities and soft-pass the different recipe)
                    foreach (string ingredient in recipeShort.Ingredients.Keys)
                        recipeExists &= _store.Items.ContainsKey(ingredient) && recipe is not null && recipe.IngredientSet.ContainsKey(_store.Items[ingredient]);
                    foreach (string product in recipeShort.Products.Keys)
                        recipeExists &= _store.Items.ContainsKey(product) && recipe is not null && recipe.ProductSet.ContainsKey(_store.Items[product]);
                }
                if (!recipeExists) {
                    if (_store.MissingRecipes.TryGetValue(recipeShort, out IRecipe? existingMissingRecipe)) {
                        recipe = existingMissingRecipe;
                    } else if (_store.MissingSubgroup is not null && _store.MissingAssembler is not null) {
                        RecipePrototype missingRecipe = new(_owner, recipeShort.Name, recipeShort.Name, _store.MissingSubgroup, "", true);
                        foreach (var ingredient in recipeShort.Ingredients) {
                            if (_store.Items.TryGetValue(ingredient.Key, out var ingredientItem))
                                missingRecipe.InternalOneWayAddIngredient((ItemPrototype)ingredientItem, ingredient.Value);
                            else
                                missingRecipe.InternalOneWayAddIngredient((ItemPrototype)_store.MissingItems[ingredient.Key], ingredient.Value);
                        }
                        foreach (var product in recipeShort.Products) {
                            if (_store.Items.TryGetValue(product.Key, out var productItem))
                                missingRecipe.InternalOneWayAddProduct((ItemPrototype)productItem, product.Value, 0);
                            else
                                missingRecipe.InternalOneWayAddProduct((ItemPrototype)_store.MissingItems[product.Key], product.Value, 0);
                        }
                        missingRecipe.AssemblersInternal.Add(_store.MissingAssembler);
                        _store.MissingAssembler.RecipesInternal.Add(missingRecipe);

                        _store.MissingRecipes.Add(recipeShort, missingRecipe);
                        recipe = missingRecipe;
                    }
                }
                if (!recipeLinks.ContainsKey(recipeShort.RecipeID) && recipe is not null)
                    recipeLinks.Add(recipeShort.RecipeID, recipe);
            }
            return recipeLinks;
        }

        //pretty much a copy of the above, just for plant processes (so no ingredient list, and using different data sets)
        public Dictionary<long, IPlantProcess> ProcessImportedPlantProcessesSet(IEnumerable<PlantShort> plantShorts) {
            var plantLinks = new Dictionary<long, IPlantProcess>();
            foreach (PlantShort plantShort in plantShorts) {
                IPlantProcess? pprocess = null;

                //recipe check #1 : does its name exist in database (note: we dont quite care about extra missing _store.Recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)
                bool pprocessExists = _store.PlantProcesses.ContainsKey(plantShort.Name);
                if (pprocessExists) {
                    //recipe check #2 : do the number of ingredients & products match?
                    pprocess = _store.PlantProcesses[plantShort.Name];
                    pprocessExists &= plantShort.Products.Count == pprocess.ProductList.Count;
                }
                if (pprocessExists) {
                    //recipe check #3 : do the ingredients & products from the loaded data match the actual recipe? (names, not quantities -> this is to allow some _store.Recipes to pass; ex: normal->expensive might change the values, but importing such a recipe should just use the 'correct' quantities and soft-pass the different recipe)
                    foreach (string product in plantShort.Products.Keys)
                        pprocessExists &= _store.Items.ContainsKey(product) && pprocess is not null && pprocess.ProductSet.ContainsKey(_store.Items[product]);
                }
                if (!pprocessExists) {
                    if (_store.MissingPlantProcesses.TryGetValue(plantShort, out IPlantProcess? existingMissingPlant)) {
                        pprocess = existingMissingPlant;
                    } else {
                        var missingPProcess = new PlantProcessPrototype(_owner, plantShort.Name, true);
                        foreach (var product in plantShort.Products) {
                            if (_store.Items.TryGetValue(product.Key, out var productItem))
                                missingPProcess.InternalOneWayAddProduct((ItemPrototype)productItem, product.Value);
                            else
                                missingPProcess.InternalOneWayAddProduct((ItemPrototype)_store.MissingItems[product.Key], product.Value);
                        }

                        _store.MissingPlantProcesses.Add(plantShort, missingPProcess);
                        pprocess = missingPProcess;
                    }
                }
                if (pprocess is not null && !plantLinks.ContainsKey(plantShort.PlantID))
                    plantLinks.Add(plantShort.PlantID, pprocess);
            }
            return plantLinks;
        }
    }
}
