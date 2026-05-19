using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Foreman {
    internal sealed class PresetDataLoader {
        private readonly DataCache _owner;
        private readonly DataCacheStore _store;
        private readonly PresetLoadSession _session;
        public PresetDataLoader(DataCache owner, DataCacheStore store, PresetLoadSession session) {
            _owner = owner;
            _store = store;
            _session = session;
        }

        public void LoadFromJson(JsonObject jsonData, Dictionary<string, IconColorPair> iconCache) {
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "mods"))
                ProcessMod(objJsonNode);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "subgroups"))
                ProcessSubgroup(objJsonNode);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "groups"))
                ProcessGroup(objJsonNode, iconCache);
            foreach (JsonNode objToken in PresetJson.EnumerateArray(jsonData, "qualities"))
                ProcessQuality(objToken, iconCache, _session.NextQualities);
            foreach (QualityPrototype quality in _store.Qualities.Values.Cast<QualityPrototype>())
                ProcessQualityLink(quality, _session.NextQualities);
            PostProcessQuality();
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "fluids"))
                ProcessFluid(objJsonNode, iconCache, _session.FuelCategories);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "items"))
                ProcessItem(objJsonNode, iconCache, _session.FuelCategories, _session.BurnResults, _session.SpoilResults);
            foreach (ItemPrototype item in _store.Items.Values.Cast<ItemPrototype>())
                ProcessBurnItem(item, _session.BurnResults);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "items"))
                ProcessPlantProcess(objJsonNode);
            foreach (ItemPrototype item in _store.Items.Values.Cast<ItemPrototype>())
                ProcessSpoilItem(item, _session.SpoilResults);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "modules"))
                ProcessModule(objJsonNode, iconCache, _session.ModuleCategories);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "recipes"))
                ProcessRecipe(objJsonNode, iconCache, _session.CraftingCategories, _session.ModuleCategories);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "resources"))
                ProcessResource(objJsonNode, _session.ResourceCategories, _session.MiningWithFluidRecipes);
            foreach (JsonNode objToken in PresetJson.EnumerateArray(jsonData, "water_resources"))
                ProcessResource(objToken, _session.ResourceCategories, _session.MiningWithFluidRecipes);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "technologies"))
                ProcessTechnology(objJsonNode, iconCache, _session.MiningWithFluidRecipes);
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "technologies"))
                ProcessTechnologyP2(objJsonNode);
        }

        public void LoadRocketLaunches(JsonObject jsonData) {
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "items").Where(t => PresetJson.GetNode(t, "rocket_launch_products") is not null))
                ProcessRocketLaunch(objJsonNode);
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "fluids").Where(t => PresetJson.GetNode(t, "rocket_launch_products") is not null))
                ProcessRocketLaunch(objJsonNode);
        }

        internal void ProcessMod(JsonNode objJsonNode) {
            if (PresetJson.GetString(objJsonNode, "name") is string name && PresetJson.GetString(objJsonNode, "version") is string version)
                _store.IncludedMods.Add(name, version);
        }

        internal void ProcessSubgroup(JsonNode objJsonNode) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name || PresetJson.GetString(objJsonNode, "order") is not string order)
                return;
            SubgroupPrototype subgroup = new SubgroupPrototype(
                _owner,
                name,
                order);

            _store.Subgroups.Add(subgroup.Name, subgroup);
        }

        internal void ProcessGroup(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName ||
                PresetJson.GetString(objJsonNode, "order") is not string order ||
                PresetJson.GetString(objJsonNode, "icon_name") is not string iconName ||
                objJsonNode["subgroups"] is not JsonNode subgroupsArr)
                return;
            GroupPrototype group = new(_owner, name, localisedName, order);

            if (iconCache.TryGetValue(iconName, out var icp))
                group.SetIconAndColor(icp);

            foreach (JsonNode subgroupJsonNode in PresetJson.EnumerateArray(subgroupsArr)) {
                if (PresetJson.GetStringValue(subgroupJsonNode) is not string s || _store.Subgroups[s] is not SubgroupPrototype sgp)
                    continue;
                sgp.myGroup = group;
                group.subgroups.Add(sgp);
            }
            _store.Groups.Add(group.Name, group);
        }

        internal void ProcessQuality(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache, Dictionary<Quality, string> nextQualities) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName ||
                PresetJson.GetString(objJsonNode, "order") is not string order ||
                PresetJson.GetString(objJsonNode, "icon_name") is not string iconName ||
                PresetJson.GetBool(objJsonNode, "hidden") is not bool hidden ||
                PresetJson.GetInt32(objJsonNode, "level") is not int level ||
                PresetJson.GetDouble(objJsonNode, "beacon_power_multiplier") is not double beaconPowerMult ||
                PresetJson.GetDouble(objJsonNode, "mining_drill_resource_drain_multiplier") is not double miningDrillResDrainMult)
                return;
            QualityPrototype quality = new(_owner, name, localisedName, order);

            if (iconCache.TryGetValue(iconName, out var icp))
                quality.SetIconAndColor(icp);

            quality.Available = !hidden;
            quality.Enabled = quality.Available; //can be set via science packs, but this requires modifying datacache... so later

            quality.Level = level;
            quality.BeaconPowerMultiplier = beaconPowerMult;
            quality.MiningDrillResourceDrainMultiplier = miningDrillResDrainMult;
            quality.NextProbability = PresetJson.GetDouble(objJsonNode, "next_probability") ?? 0;

            if (quality.NextProbability != 0 && PresetJson.GetString(objJsonNode, "next") is string next)
                _session.NextQualities.Add(quality, next);

            _store.Qualities.Add(quality.Name, quality);
        }

        internal void ProcessQualityLink(QualityPrototype quality, Dictionary<Quality, string> nextQualities) {

            if (_session.NextQualities.ContainsKey(quality) && _store.Qualities.ContainsKey(_session.NextQualities[quality])) {
                quality.NextQuality = _store.Qualities[_session.NextQualities[quality]];
                ((QualityPrototype)_store.Qualities[_session.NextQualities[quality]]).PrevQuality = quality;
            }
        }

        internal void PostProcessQuality() {
            //make sure that the default quality is always enabled & available
            _store.DefaultQuality = _store.Qualities.ContainsKey("normal") ? _store.Qualities["normal"] : _store.ErrorQuality;
            _store.DefaultQuality?.Enabled = true;
            (_store.DefaultQuality as QualityPrototype)?.Available = true;

            //make available all _store.Qualities that are within the defaultquality chain
            Quality? cQuality = _store.DefaultQuality;
            var defaultChainVisited = new HashSet<Quality>();
            while (cQuality is not null) {
                if (!defaultChainVisited.Add(cQuality)) {
                    ErrorLogging.LogLine(
                        "Cyclic quality chain detected while applying default-quality availability at \"" +
                        cQuality.Name + "\"; stopping chain walk.");
                    break;
                }
                ((QualityPrototype)cQuality).Available = cQuality.Enabled;
                cQuality = cQuality.NextQuality;
            }

            Quality currentQuality;
            uint currentChain;
            foreach (Quality quality in _store.Qualities.Values) {
                currentChain = 1;
                currentQuality = quality;
                var chainVisited = new HashSet<Quality> { currentQuality };
                while (currentQuality.NextQuality != null && currentQuality.NextProbability != 0) {
                    Quality next = currentQuality.NextQuality;
                    if (!chainVisited.Add(next)) {
                        ErrorLogging.LogLine(
                            "Cyclic quality chain detected while measuring chain length at \"" +
                            next.Name + "\"; chain length truncated.");
                        break;
                    }
                    currentChain++;
                    currentQuality = next;
                }
                _store.QualityMaxChainLength = Math.Max(_store.QualityMaxChainLength, currentChain);
            }
        }

        internal void ProcessFluid(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName ||
                PresetJson.GetString(objJsonNode, "subgroup") is not string subgroupStr ||
                PresetJson.GetString(objJsonNode, "order") is not string order ||
                PresetJson.GetString(objJsonNode, "icon_name") is not string iconName ||
                PresetJson.GetDouble(objJsonNode, "default_temperature") is not double defaultTemp ||
                PresetJson.GetDouble(objJsonNode, "heat_capacity") is not double heatCapacity ||
                PresetJson.GetDouble(objJsonNode, "gas_temperature") is not double gasTemp ||
                PresetJson.GetDouble(objJsonNode, "max_temperature") is not double maxTemp ||
                _store.Subgroups[subgroupStr] is not SubgroupPrototype sgProto)
                return;
            FluidPrototype item = new(_owner, name, localisedName, sgProto, order);

            if (iconCache.TryGetValue(iconName, out var icp))
                item.SetIconAndColor(icp);

            item.DefaultTemperature = defaultTemp;
            item.SpecificHeatCapacity = heatCapacity;
            item.GasTemperature = gasTemp;
            item.MaxTemperature = maxTemp;

            if (PresetJson.GetDouble(objJsonNode, "fuel_value") is double fuelValue && fuelValue > 0) {
                item.FuelValue = fuelValue;
                item.PollutionMultiplier = PresetJson.GetDouble(objJsonNode, "emissions_multiplier") ?? 1;
                _session.FuelCategories["§§fc:liquids"].Add(item);
            }

            _store.Items.Add(item.Name, item);
        }

        internal void ProcessItem(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories, Dictionary<Item, string> burnResults, Dictionary<Item, string> spoilResults) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name ||
                _store.Items.ContainsKey(name) ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName ||
                PresetJson.GetString(objJsonNode, "subgroup") is not string subgroupStr ||
                PresetJson.GetString(objJsonNode, "order") is not string order ||
                PresetJson.GetInt32(objJsonNode, "stack_size") is not int stackSize ||
                PresetJson.GetDouble(objJsonNode, "weight") is not double weight ||
                PresetJson.GetDouble(objJsonNode, "ingredient_to_weight_coefficient") is not double ingredientToWeightCoeff ||
                _store.Subgroups[subgroupStr] is not SubgroupPrototype sgProto
                ) //special handling for fluids which appear in both _store.Items & fluid lists (ex: fluid-unknown)
                return;

            ItemPrototype item = new(_owner, name, localisedName, sgProto, order);

            if (PresetJson.GetString(objJsonNode, "icon_name") is string iconName && iconCache.TryGetValue(iconName, out var icp))
                item.SetIconAndColor(icp);
            else if (PresetJson.GetString(objJsonNode, "icon_alt_name") is string iconAltName && iconCache.TryGetValue(iconAltName, out var icpAlt))
                item.SetIconAndColor(icpAlt);

            item.StackSize = stackSize;
            item.Weight = weight;
            item.IngredientToWeightCoefficient = ingredientToWeightCoeff;

            if (PresetJson.GetString(objJsonNode, "fuel_category") is string fuelCategory &&
                PresetJson.GetDouble(objJsonNode, "fuel_value") is double fuelValue &&
                fuelValue > 0) //factorio eliminates any 0fuel value fuel from the list (checked)
            {
                item.FuelValue = fuelValue;
                item.PollutionMultiplier = PresetJson.GetDouble(objJsonNode, "fuel_emissions_multiplier") ?? 1;

                if (!_session.FuelCategories.ContainsKey(fuelCategory))
                    _session.FuelCategories.Add(fuelCategory, new());
                _session.FuelCategories[fuelCategory].Add(item);
            }
            if (PresetJson.GetString(objJsonNode, "burnt_result") is string burntResult)
                _session.BurnResults.Add(item, burntResult);
            if (PresetJson.GetString(objJsonNode, "spoil_result") is string spoilResult && objJsonNode["q_spoil_time"] is JsonNode qSpoilTime) {
                _session.SpoilResults.Add(item, spoilResult);
                foreach (JsonNode spoilToken in PresetJson.EnumerateArray(qSpoilTime))
                    if (PresetJson.GetString(spoilToken, "quality") is string quality && PresetJson.GetDouble(spoilToken, "value") is double value)
                        item.spoilageTimes.Add(_store.Qualities[quality], value);
            }

            _store.Items.Add(item.Name, item);
        }

        internal void ProcessBurnItem(ItemPrototype item, Dictionary<Item, string> burnResults) {
            if (_session.BurnResults.ContainsKey(item)) {
                item.BurnResult = _store.Items[_session.BurnResults[item]];
                ((ItemPrototype)_store.Items[_session.BurnResults[item]]).FuelOrigin = item;
            }
        }
        internal void ProcessPlantProcess(JsonNode objJsonNode) {
            if (objJsonNode["plant_results"] is JsonNode plantResults &&
                PresetJson.GetString(objJsonNode, "name") is string name &&
                PresetJson.GetDouble(objJsonNode, "plant_growth_time") is double growTime) {
                ItemPrototype seed = (ItemPrototype)_store.Items[name];
                PlantProcessPrototype plantProcess = new PlantProcessPrototype(
                    _owner,
                    seed.Name);

                plantProcess.Seed = seed;
                plantProcess.GrowTime = growTime;

                foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(plantResults)) {
                    if (PresetJson.GetString(productJsonNode, "name") is not string prodName)
                        continue;
                    ItemPrototype product = (ItemPrototype)_store.Items[prodName];
                    double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? 0;
                    if (amount != 0) {
                        plantProcess.InternalOneWayAddProduct(product, amount);
                        product.plantOrigins.Add(seed);
                        seed.PlantResult = plantProcess;
                    }
                }

                _store.PlantProcesses.Add(seed.Name, plantProcess); //seed.Name = plantProcess.name, but for clarity: any searches will be done via seed's name
            }
        }
        internal void ProcessSpoilItem(ItemPrototype item, Dictionary<Item, string> spoilResults) {
            if (_session.SpoilResults.ContainsKey(item)) {
                item.SpoilResult = _store.Items[_session.SpoilResults[item]];
                ((ItemPrototype)_store.Items[_session.SpoilResults[item]]).spoilOrigins.Add(item);
            }
        }

        internal void ProcessModule(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ModulePrototype>> moduleCategories) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName ||
                PresetJson.GetInt32(objJsonNode, "tier") is not int tier ||
                PresetJson.GetString(objJsonNode, "category") is not string category)
                return;
            ModulePrototype module = new ModulePrototype(_owner, name, localisedName);

            if (PresetJson.GetString(objJsonNode, "icon_name") is string iconName && iconCache.TryGetValue(iconName, out var icp))
                module.SetIconAndColor(icp);
            else if (PresetJson.GetString(objJsonNode, "icon_alt_name") is string iconAltName && iconCache.TryGetValue(iconAltName, out var icpAlt))
                module.SetIconAndColor(icpAlt);

            module.SpeedBonus = Math.Round((PresetJson.GetDouble(objJsonNode, "module_effects", "speed") ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.ProductivityBonus = Math.Round((PresetJson.GetDouble(objJsonNode, "module_effects", "productivity") ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.ConsumptionBonus = Math.Round((PresetJson.GetDouble(objJsonNode, "module_effects", "consumption") ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.PollutionBonus = Math.Round((PresetJson.GetDouble(objJsonNode, "module_effects", "pollution") ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.QualityBonus = Math.Round((PresetJson.GetDouble(objJsonNode, "module_effects", "quality") ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;

            module.Tier = tier;

            module.Category = category;
            if (_session.ModuleCategories.TryGetValue(category, out var list))
                list.Add(module);
            else
                _session.ModuleCategories.Add(category, [module]);

            _store.Modules.Add(module.Name, module);
        }

        internal void ProcessRecipe(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories, Dictionary<string, List<ModulePrototype>> moduleCategories) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName ||
                PresetJson.GetString(objJsonNode, "subgroup") is not string subgroupStr ||
                PresetJson.GetString(objJsonNode, "order") is not string order ||
                PresetJson.GetString(objJsonNode, "category") is not string category)
                return;
            RecipePrototype recipe = new(_owner, name, localisedName, (SubgroupPrototype)_store.Subgroups[subgroupStr], order);

            recipe.Time = PresetJson.GetDouble(objJsonNode, "energy") ?? default;
            if (PresetJson.GetBool(objJsonNode, "enabled") is true && _store.StartingTech is not null) //due to the way the import of presets happens, enabled at this stage means the recipe is available without any research necessary (aka: available at start)
            {
                recipe.myUnlockTechnologies.Add(_store.StartingTech);
                _store.StartingTech.unlockedRecipes.Add(recipe);
            }

            if (_session.CraftingCategories.TryGetValue(category, out var list))
                list.Add(recipe);
            else
                _session.CraftingCategories.Add(category, [recipe]);

            if (PresetJson.GetString(objJsonNode, "icon_name") is string iconName && iconCache.TryGetValue(iconName, out var icp))
                recipe.SetIconAndColor(icp);
            else if (PresetJson.GetString(objJsonNode, "icon_alt_name") is string iconAltName && iconCache.TryGetValue(iconAltName, out var icpAlt))
                recipe.SetIconAndColor(icpAlt);

            recipe.HasProductivityResearch = PresetJson.GetBool(objJsonNode, "prod_research") is true;
            recipe.MaxProductivityBonus = PresetJson.GetDouble(objJsonNode, "maximum_productivity") ?? 1000;

            foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "products")) {
                if (PresetJson.GetString(productJsonNode, "name") is not string prodName)
                    continue;
                ItemPrototype product = (ItemPrototype)_store.Items[prodName];
                double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? 0;
                if (amount != 0 && PresetJson.GetDouble(productJsonNode, "p_amount") is double pAmount) {
                    if (PresetJson.GetString(productJsonNode, "type") == "fluid")
                        recipe.InternalOneWayAddProduct(product, amount, pAmount, PresetJson.GetDouble(productJsonNode, "temperature") ?? ((FluidPrototype)product).DefaultTemperature);
                    else
                        recipe.InternalOneWayAddProduct(product, amount, pAmount);

                    product.productionRecipes.Add(recipe);
                }
            }

            foreach (JsonNode ingredientJsonNode in PresetJson.EnumerateArray(objJsonNode, "ingredients")) {
                if (PresetJson.GetString(ingredientJsonNode, "name") is not string ingName)
                    continue;
                ItemPrototype ingredient = (ItemPrototype)_store.Items[ingName];
                double amount = PresetJson.GetDouble(ingredientJsonNode, "amount") ?? 0;
                if (amount != 0) {
                    double minTemp = (PresetJson.GetString(ingredientJsonNode, "type") == "fluid" && PresetJson.GetDouble(ingredientJsonNode, "minimum_temperature") is double min) ? min : double.NegativeInfinity;
                    double maxTemp = (PresetJson.GetString(ingredientJsonNode, "type") == "fluid" && PresetJson.GetDouble(ingredientJsonNode, "maximum_temperature") is double max) ? max : double.PositiveInfinity;
                    if (minTemp < DataCacheFluidLimits.MinTemp)
                        minTemp = double.NegativeInfinity;
                    if (maxTemp > DataCacheFluidLimits.MaxTemp)
                        maxTemp = double.PositiveInfinity;

                    recipe.InternalOneWayAddIngredient(ingredient, amount, minTemp, maxTemp);
                    ingredient.consumptionRecipes.Add(recipe);
                }
            }

            if (objJsonNode["allowed_effects"] is JsonNode allowedEffects) {
                recipe.AllowConsumptionBonus = PresetJson.GetBool(allowedEffects, "consumption") is true;
                recipe.AllowSpeedBonus = PresetJson.GetBool(allowedEffects, "speed") is true;
                recipe.AllowProductivityBonus = PresetJson.GetBool(allowedEffects, "productivity") is true;
                recipe.AllowPollutionBonus = PresetJson.GetBool(allowedEffects, "pollution") is true;
                recipe.AllowQualityBonus = PresetJson.GetBool(allowedEffects, "quality") is true;

                foreach (ModulePrototype module in _store.Modules.Values.Cast<ModulePrototype>()) {
                    bool validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                                        (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                                        (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                                        (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                                        (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                    if (validModule) {
                        recipe.beaconModules.Add(module);
                    }
                }

                if (objJsonNode["allowed_module_categories"] is not JsonObject allowedModCats || allowedModCats.Count == 0) {
                    foreach (ModulePrototype module in _store.Modules.Values.Cast<ModulePrototype>()) {
                        bool validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                                            (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                                            (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                                            (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                                            (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                        if (validModule) {
                            recipe.assemblerModules.Add(module);
                            module.recipes.Add(recipe);
                        }
                    }
                } else {
                    foreach (string moduleCategory in PresetJson.GetObjectPropertyNames(allowedModCats)) {
                        if (_session.ModuleCategories.ContainsKey(moduleCategory)) {
                            foreach (ModulePrototype module in _session.ModuleCategories[moduleCategory]) {
                                bool validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                                                    (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                                                    (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                                                    (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                                                    (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                                if (validModule) {
                                    recipe.assemblerModules.Add(module);
                                    module.recipes.Add(recipe);
                                }
                            }
                        }
                    }
                }
            }

            _store.Recipes.Add(recipe.Name, recipe);
        }

        internal string GetExtractionRecipeName(string itemName) { return "§§r:e:" + itemName; }

        internal void ProcessResource(JsonNode objJsonNode, Dictionary<string, List<RecipePrototype>> resourceCategories, List<Recipe> miningWithFluidRecipes) {
            var subgroup = PresetJson.GetString(PresetJson.EnumerateArray(objJsonNode, "products").FirstOrDefault(), "type") == "fluid" ? _store.ExtractionSubgroupFluids : _store.ExtractionSubgroupItems;
            if (!PresetJson.EnumerateArray(objJsonNode, "products").Any() ||
                subgroup is null ||
                PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName)
                return;

            RecipePrototype recipe = new(_owner, GetExtractionRecipeName(name), localisedName + " Extraction", subgroup, name);

            recipe.Time = PresetJson.GetDouble(objJsonNode, "mining_time") ?? 0;

            foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "products")) {
                if (PresetJson.GetString(productJsonNode, "name") is not string prodName || !_store.Items.ContainsKey(name) || PresetJson.GetDouble(productJsonNode, "amount") <= 0)
                    continue;
                ItemPrototype product = (ItemPrototype)_store.Items[prodName];
                var amount = PresetJson.GetDouble(productJsonNode, "amount") ?? default;
                recipe.InternalOneWayAddProduct(product, amount, amount);
                product.productionRecipes.Add(recipe);
            }

            if (recipe.productList.Count == 0) {
                recipe.mySubgroup.recipes.Remove(recipe);
                return;
            }

            if (PresetJson.GetString(objJsonNode, "required_fluid") is string requiredFluid && PresetJson.GetDouble(objJsonNode, "fluid_amount") is double fluidAmount && fluidAmount != 0) {
                ItemPrototype reqLiquid = (ItemPrototype)_store.Items[requiredFluid];
                recipe.InternalOneWayAddIngredient(reqLiquid, fluidAmount);
                reqLiquid.consumptionRecipes.Add(recipe);
                _session.MiningWithFluidRecipes.Add(recipe);
            }

            foreach (ModulePrototype module in _store.Modules.Values.Cast<ModulePrototype>()) //we will let the assembler sort out which module can be used with this recipe
            {
                module.recipes.Add(recipe);
                recipe.assemblerModules.Add(module);
            }

            recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

            if (PresetJson.GetString(objJsonNode, "resource_category") is string category) {
                if (_session.ResourceCategories.TryGetValue(category, out var list))
                    list.Add(recipe);
                else
                    _session.ResourceCategories.Add(category, [recipe]);
            }

            //resource recipe will be processed when adding to miners (each miner that can use this recipe will have its recipe's techs added to unlock tech of the resource recipe)
            //this is for any non-fluid based resource! (fluid based item mining is locked behind research and processed in research function)
            //recipe.myUnlockTechnologies.Add(_store.StartingTech);
            //_store.StartingTech.unlockedRecipes.Add(recipe);

            _store.Recipes.Add(recipe.Name, recipe);
        }

        internal void ProcessTechnology(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache, List<Recipe> miningWithFluidRecipes) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName)
                return;
            TechnologyPrototype technology = new TechnologyPrototype(
                _owner,
                name,
                localisedName);

            if (PresetJson.GetString(objJsonNode, "icon_name") is string iconName && iconCache.TryGetValue(iconName, out var icp))
                technology.SetIconAndColor(icp);

            technology.Available = PresetJson.GetBool(objJsonNode, "hidden") is not true && PresetJson.GetBool(objJsonNode, "enabled") is true; //not sure - factorio documentation states 'enabled' means 'available at start', but in this case 'enabled' being false seems to represent the technology not appearing on screen (same as hidden)??? I will just work with what tests show -> tech is available if it is enabled & not hidden.

            foreach (string recipe in PresetJson.EnumerateStrings(objJsonNode, "recipes")) {
                if (_store.Recipes.TryGetValue(recipe, out var proto)) {
                    ((RecipePrototype)proto).myUnlockTechnologies.Add(technology);
                    technology.unlockedRecipes.Add((RecipePrototype)proto);
                }
            }

            foreach (string qualityName in PresetJson.EnumerateStrings(objJsonNode, "qualities")) {
                if (_store.Qualities.TryGetValue(qualityName, out var quality)) {
                    ((QualityPrototype)quality).myUnlockTechnologies.Add(technology);
                    technology.unlockedQualities.Add((QualityPrototype)quality);
                }
            }

            if (objJsonNode["unlocks-mining-with-fluid"] is not null) {
                foreach (RecipePrototype recipe in _session.MiningWithFluidRecipes.Cast<RecipePrototype>()) {
                    recipe.myUnlockTechnologies.Add(technology);
                    technology.unlockedRecipes.Add(recipe);
                }
            }

            foreach (JsonNode ingredientJsonNode in PresetJson.EnumerateArray(objJsonNode, "research_unit_ingredients")) {
                var ingName = PresetJson.GetString(ingredientJsonNode, "name");
                var amount = PresetJson.GetDouble(ingredientJsonNode, "amount") ?? 0;

                if (ingName is string && amount != 0) {
                    technology.InternalOneWayAddSciPack((ItemPrototype)_store.Items[ingName], amount);
                    ((ItemPrototype)_store.Items[ingName]).consumptionTechnologies.Add(technology);
                }
            }

            _store.Technologies.Add(technology.Name, technology);
        }

        internal void ProcessTechnologyP2(JsonNode objJsonNode) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name)
                return;
            TechnologyPrototype technology = (TechnologyPrototype)_store.Technologies[name];
            foreach (string prerequisite in PresetJson.EnumerateStrings(objJsonNode, "prerequisites")) {
                if (_store.Technologies.TryGetValue(prerequisite, out var proto)) {
                    technology.prerequisites.Add((TechnologyPrototype)proto);
                    ((TechnologyPrototype)proto).postTechs.Add(technology);
                }
            }
            if (technology.prerequisites.Count == 0 && _store.StartingTech is not null) //entire tech tree will stem from teh '_store.StartingTech' node.
            {
                technology.prerequisites.Add(_store.StartingTech);
                _store.StartingTech.postTechs.Add(technology);
            }
        }
        internal void ProcessRocketLaunch(JsonNode objJsonNode) {
            if (PresetJson.GetString(objJsonNode, "name") is not string name || _store.RocketLaunchSubgroup is null || _store.RocketAssembler is null)
                return;
            if (!_store.Items.ContainsKey("rocket-part") || !_store.Recipes.ContainsKey("rocket-part") || !_store.Assemblers.ContainsKey("rocket-silo")) {
                ErrorLogging.LogLine(string.Format("No Rocket silo / rocket part found! launch product for {0} will be ignored.", PresetJson.GetString(objJsonNode, "name") ?? "<NULL JSON>"));
                return;
            }

            ItemPrototype rocketPart = (ItemPrototype)_store.Items["rocket-part"];
            RecipePrototype rocketPartRecipe = (RecipePrototype)_store.Recipes["rocket-part"];
            ItemPrototype launchItem = (ItemPrototype)_store.Items[name];

            RecipePrototype recipe = new RecipePrototype(
                _owner,
                string.Format("§§r:rl:launch-{0}", launchItem.Name),
                string.Format("Rocket Launch: {0}", launchItem.FriendlyName),
                _store.RocketLaunchSubgroup,
                launchItem.Name);

            recipe.Time = 1; //placeholder really...

            //process products - have to calculate what the maximum input size of the launch item is so as not to waste any products (ex: you can launch 2000 science packs, but you will only get 100 fish. so input size must be set to 100 -> 100 science packs to 100 fish)
            double inputSize = launchItem.StackSize;
            Dictionary<ItemPrototype, double> amountPerLaunchItem = new Dictionary<ItemPrototype, double>();
            Dictionary<ItemPrototype, double> productTemp = new Dictionary<ItemPrototype, double>();
            foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "rocket_launch_products")) {
                if (PresetJson.GetString(productJsonNode, "name") is not string prodName || !_store.Items.TryGetValue(prodName, out Item? productItem))
                    continue;
                ItemPrototype product = (ItemPrototype)productItem;
                double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? default;
                if (amount == 0)
                    continue;

                if (inputSize * amount > product.StackSize)
                    inputSize = Math.Floor(product.StackSize / amount);

                amountPerLaunchItem.Add(product, amount);

                if (PresetJson.GetString(productJsonNode, "type") == "fluid")
                    productTemp.Add(product, PresetJson.GetDouble(productJsonNode, "temperature") ?? ((FluidPrototype)product).DefaultTemperature);

                product.productionRecipes.Add(recipe);
                recipe.SetIconAndColor(new IconColorPair(product.Icon, Color.DarkGray));
            }
            foreach (var (product, amount) in amountPerLaunchItem)
                recipe.InternalOneWayAddProduct(product, inputSize * amount, 0, productTemp.TryGetValue(product, out double temp) ? temp : double.NaN);

            recipe.InternalOneWayAddIngredient(launchItem, inputSize);
            launchItem.consumptionRecipes.Add(recipe);

            recipe.InternalOneWayAddIngredient(rocketPart, 100);
            rocketPart.consumptionRecipes.Add(recipe);

            foreach (TechnologyPrototype tech in rocketPartRecipe.myUnlockTechnologies) {
                recipe.myUnlockTechnologies.Add(tech);
                tech.unlockedRecipes.Add(recipe);
            }

            recipe.assemblers.Add(_store.RocketAssembler);
            _store.RocketAssembler.recipes.Add(recipe);

            _store.Recipes.Add(recipe.Name, recipe);
        }
    }
}