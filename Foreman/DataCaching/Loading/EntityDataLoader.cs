using Foreman.DataCaching.DataTypes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;

namespace Foreman.DataCaching.Loading {
    internal sealed class EntityDataLoader(DataCache owner, DataCacheStore store, PresetLoadSession session) {
        private readonly DataCache _owner = owner;
        private readonly DataCacheStore _store = store;
        private readonly PresetLoadSession _session = session;

        public void LoadEntities(JsonObject jsonData, Dictionary<string, IconColorPair> iconCache) {
            foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "entities"))
                ProcessEntity(objJsonNode, iconCache);
        }

        internal void LoadCharacter(JsonNode? objJtoken) {
            if (objJtoken is null || _store.PlayerAssembler is null)
                return;
            AssemblerAdditionalProcessing(objJtoken, _store.PlayerAssembler, _session.CraftingCategories);
            _store.Assemblers.Add(_store.PlayerAssembler.Name, _store.PlayerAssembler);
        }
        internal void ProcessEntity(JsonNode objJsonNode, Dictionary<string, IconColorPair> iconCache) {
            var type = PresetJson.GetString(objJsonNode, "type");
            //character is processed later
            if (type == "character" ||
                PresetJson.GetString(objJsonNode, "name") is not string name ||
                PresetJson.GetString(objJsonNode, "localised_name") is not string localisedName)
                return;

            EntityObjectBasePrototype entity;
            EnergySource esource =
                (PresetJson.GetString(objJsonNode, "fuel_type") == "item") ? EnergySource.Burner :
                (PresetJson.GetString(objJsonNode, "fuel_type") == "fluid") ? EnergySource.FluidBurner :
                (PresetJson.GetString(objJsonNode, "fuel_type") == "electricity") ? EnergySource.Electric :
                (PresetJson.GetString(objJsonNode, "fuel_type") == "heat") ? EnergySource.Heat : EnergySource.Void;
            var etype = type switch {
                "beacon" => EntityType.Beacon,
                "mining-drill" => EntityType.Miner,
                "offshore-pump" => EntityType.OffshorePump,
                "furnace" or "assembling-machine" or "rocket-silo" => EntityType.Assembler,
                "boiler" => EntityType.Boiler,
                "generator" => EntityType.Generator,
                "burner-generator" => EntityType.BurnerGenerator,
                "reactor" => EntityType.Reactor,
                _ => EntityType.ERROR,
            };
            if (etype == EntityType.ERROR)
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, "Unexpected type of entity ({0} in json data!", type));


            entity = etype == EntityType.Beacon
                ? new BeaconPrototype(_owner, name, localisedName, esource, isMissing: false)
                : new AssemblerPrototype(_owner, name, localisedName, etype, esource, isMissing: false);

            //icons
            if (PresetJson.GetString(objJsonNode, "icon_name") is string iconName && iconCache.TryGetValue(iconName, out var icp))
                entity.SetIconAndColor(icp);
            else if (PresetJson.GetString(objJsonNode, "icon_alt_name") is string iconAlt && iconCache.TryGetValue(iconAlt, out var icpAlt))
                entity.SetIconAndColor(icpAlt);

            //associated _store.Items
            foreach (var item in PresetJson.EnumerateStrings(objJsonNode, "items_to_place_this"))
                if (_store.Items.TryGetValue(item, out var itemProto))
                    entity.AssociatedItemsInternal.Add((ItemPrototype)itemProto);

            //base parameters
            if (PresetJson.GetNode(objJsonNode, "q_speed") is JsonNode qSpeed) {
                foreach (JsonNode speedToken in PresetJson.EnumerateArray(qSpeed))
                    if (PresetJson.GetString(speedToken, "quality") is string quality && PresetJson.GetDouble(speedToken, "value") is double value)
                        entity.SpeedInternal.Add(_store.Qualities[quality], value);
            } else if (PresetJson.GetDouble(objJsonNode, "speed") is double speed) {
                foreach (IQuality quality in _store.Qualities.Values)
                    entity.SpeedInternal.Add(quality, speed);
            }

            entity.ModuleSlots = PresetJson.GetInt32(objJsonNode, "module_inventory_size") ?? 0;

            //_store.Modules
            if (entity.EntityType == EntityType.Assembler || entity.EntityType == EntityType.Miner || entity.EntityType == EntityType.Rocket || entity.EntityType == EntityType.Beacon) {
                if (entity is AssemblerPrototype prototype) {
                    prototype.BaseConsumptionBonus = PresetJson.GetDouble(objJsonNode, "base_module_effects", "consumption") ?? default;
                    prototype.BaseSpeedBonus = PresetJson.GetDouble(objJsonNode, "base_module_effects", "speed") ?? default;
                    prototype.BaseProductivityBonus = PresetJson.GetDouble(objJsonNode, "base_module_effects", "productivity") ?? default;
                    prototype.BasePollutionBonus = PresetJson.GetDouble(objJsonNode, "base_module_effects", "pollution") ?? default;
                    prototype.BaseQualityBonus = PresetJson.GetDouble(objJsonNode, "base_module_effects", "quality") ?? default;
                    prototype.AllowModules = PresetJson.GetBool(objJsonNode, "uses_module_effects") is true;
                    prototype.AllowBeacons = PresetJson.GetBool(objJsonNode, "uses_beacon_effects") is true;
                }

                if (objJsonNode["allowed_effects"] is JsonNode allowedEffects) {
                    bool allow_consumption = PresetJson.GetBool(allowedEffects, "consumption") is true;
                    bool allow_speed = PresetJson.GetBool(allowedEffects, "speed") is true;
                    bool alllow_productivity = PresetJson.GetBool(allowedEffects, "productivity") is true;
                    bool allow_pollution = PresetJson.GetBool(allowedEffects, "pollution") is true;
                    bool allow_quality = PresetJson.GetBool(allowedEffects, "quality") is true;

                    if (objJsonNode["allowed_module_categories"] is not JsonObject allowedModuleCats || allowedModuleCats.Count == 0) {
                        foreach (ModulePrototype module in _store.Modules.Values.Cast<ModulePrototype>()) {
                            bool validModule = (allow_consumption || module.ConsumptionBonus >= 0) &&
                                                (allow_speed || module.SpeedBonus <= 0) &&
                                                (alllow_productivity || module.ProductivityBonus <= 0) &&
                                                (allow_pollution || module.PollutionBonus >= 0) &&
                                                (allow_quality || module.QualityBonus <= 0);
                            if (validModule) {
                                entity.ModulesInternal.Add(module);
                                if (entity is AssemblerPrototype aEntity)
                                    module.AssemblersInternal.Add(aEntity);
                                else if (entity is BeaconPrototype bEntity)
                                    module.BeaconsInternal.Add(bEntity);
                            }
                        }
                    } else {
                        foreach (string moduleCategory in PresetJson.GetObjectPropertyNames(allowedModuleCats)) {
                            if (_session.ModuleCategories.TryGetValue(moduleCategory, out var categoryModules)) {
                                foreach (ModulePrototype module in categoryModules) {
                                    bool validModule = (allow_consumption || module.ConsumptionBonus >= 0) &&
                                                        (allow_speed || module.SpeedBonus <= 0) &&
                                                        (alllow_productivity || module.ProductivityBonus <= 0) &&
                                                        (allow_pollution || module.PollutionBonus >= 0) &&
                                                        (allow_quality || module.QualityBonus <= 0);
                                    if (validModule) {
                                        entity.ModulesInternal.Add(module);
                                        if (entity is AssemblerPrototype aEntity)
                                            module.AssemblersInternal.Add(aEntity);
                                        else if (entity is BeaconPrototype bEntity)
                                            module.BeaconsInternal.Add(bEntity);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //energy types
            EntityEnergyFurtherProcessing(objJsonNode, entity);

            //assembler / beacon specific parameters
            if (etype == EntityType.Beacon) {
                var bEntity = (BeaconPrototype)entity;

                if (BeaconAdditionalProcessing(objJsonNode, bEntity))
                    _store.Beacons.Add(bEntity.Name, bEntity);
            } else {
                var aEntity = (AssemblerPrototype)entity;

                bool success = false;
                switch (etype) {
                    case EntityType.Assembler:
                        AssemblerAdditionalProcessing(objJsonNode, aEntity, _session.CraftingCategories);
                        success = true;
                        break;
                    case EntityType.Boiler:
                        success = BoilerAdditionalProcessing(objJsonNode, aEntity);
                        break;
                    case EntityType.BurnerGenerator:
                        success = BurnerGeneratorAdditionalProcessing(aEntity);
                        break;
                    case EntityType.Generator:
                        success = GeneratorAdditionalProcessing(objJsonNode, aEntity);
                        break;
                    case EntityType.Miner:
                        MinerAdditionalProcessing(objJsonNode, aEntity);
                        success = true;
                        break;
                    case EntityType.OffshorePump:
                        success = OffshorePumpAdditionalProcessing(objJsonNode, aEntity, _session.ResourceCategories["<<foreman_resource_category_water_tile>>"]);
                        break;
                    case EntityType.Reactor:
                        success = ReactorAdditionalProcessing(objJsonNode, aEntity);
                        break;
                }
                if (success)
                    _store.Assemblers.Add(aEntity.Name, aEntity);
            }
        }

        internal void EntityEnergyFurtherProcessing(JsonNode objJsonNode, EntityObjectBasePrototype entity) {
            entity.ConsumptionEffectivity = PresetJson.GetDouble(objJsonNode, "fuel_effectivity") ?? 0;

            //pollution
            var pollutions = PresetJson.GetStringDoubleDictionary(objJsonNode, "pollution");
            if (pollutions is not null) {
                foreach (var pollution in pollutions ?? [])
                    entity.PollutionInternal.Add(pollution.Key, pollution.Value);
            }

            //energy production
            foreach (JsonNode speedToken in PresetJson.EnumerateArray(objJsonNode, "q_energy_production"))
                if (PresetJson.GetString(speedToken, "quality") is string quality && PresetJson.GetDouble(speedToken, "value") is double value)
                    entity.EnergyProductionInternal.Add(_store.Qualities[quality], value);

            //energy consumption
            entity.EnergyDrainInternal = PresetJson.GetDouble(objJsonNode, "drain") ?? 0; //seconds
            foreach (JsonNode speedToken in PresetJson.EnumerateArray(objJsonNode, "q_max_energy_usage"))
                if (PresetJson.GetString(speedToken, "quality") is string quality && PresetJson.GetDouble(speedToken, "value") is double value)
                    entity.EnergyConsumptionInternal.Add(_store.Qualities[quality], value);

            //fuel processing
            switch (entity.EnergySource) {
                case EnergySource.Burner:
                    foreach (var categoryJsonNode in PresetJson.EnumerateStrings(objJsonNode, "fuel_categories")) {
                        if (_session.FuelCategories.TryGetValue(categoryJsonNode, out var fuelCategoryItems)) {
                            foreach (ItemPrototype item in fuelCategoryItems) {
                                entity.FuelsInternal.Add(item);
                                item.FuelsEntitiesInternal.Add(entity);
                            }
                        }
                    }
                    break;

                case EnergySource.FluidBurner:
                    entity.IsTemperatureFluidBurner = PresetJson.GetBool(objJsonNode, "burns_fluid") is not true;
                    entity.FluidFuelTemperatureRange = new FRange(PresetJson.GetDouble(objJsonNode, "minimum_fuel_temperature") ?? double.NegativeInfinity, PresetJson.GetDouble(objJsonNode, "maximum_fuel_temperature") ?? double.PositiveInfinity);

                    if (PresetJson.GetString(objJsonNode, "fuel_filter") is string fuelFilter) {
                        var fuel = (ItemPrototype)_store.Items[fuelFilter];
                        if (entity.IsTemperatureFluidBurner || _session.FuelCategories["§§fc:liquids"].Contains(fuel)) {
                            entity.FuelsInternal.Add(fuel);
                            fuel.FuelsEntitiesInternal.Add(entity);
                        }
                        //else
                        //	; //there is no valid fuel for this entity. Realistically this means it cant be used. It will thus have an error when placed (no fuel selected -> due to no fuel existing)
                    } else if (!entity.IsTemperatureFluidBurner) {
                        //add in all liquid fuels
                        foreach (ItemPrototype fluid in _session.FuelCategories["§§fc:liquids"]) {
                            entity.FuelsInternal.Add(fluid);
                            fluid.FuelsEntitiesInternal.Add(entity);
                        }
                    } else //ok, this is a bit of a FK U, but this basically means this entity can burn any fluid, and burns it as a temperature range. This is how the old steam generators worked (where you could feed in hot sulfuric acid and it would just burn through it no problem). If you want to use it, fine. Here you go.
                      {
                        foreach (FluidPrototype fluid in _store.Items.Values.Where(i => i is IFluid).Cast<FluidPrototype>()) {
                            entity.FuelsInternal.Add(fluid);
                            fluid.FuelsEntitiesInternal.Add(entity);
                        }
                    }
                    break;

                case EnergySource.Heat:
                    if (_store.HeatItem is not null) {
                        entity.FuelsInternal.Add(_store.HeatItem);
                        _store.HeatItem.FuelsEntitiesInternal.Add(entity);
                    }
                    break;

                case EnergySource.Electric:
                    break;

                case EnergySource.Void:
                default:
                    break;
            }
        }

        internal static bool BeaconAdditionalProcessing(JsonNode objJsonNode, BeaconPrototype bEntity) {
            bEntity.DistributionEffectivity = PresetJson.GetDouble(objJsonNode, "distribution_effectivity") ?? 0.5;
            bEntity.DistributionEffectivityQualityBoost = PresetJson.GetDouble(objJsonNode, "distribution_effectivity_bonus_per_quality_level") ?? 0;

            if (objJsonNode["profile"] != null) {
                int quantity = 1;
                double lastProfile = 0.5;
                foreach (JsonNode profileJsonNode in PresetJson.EnumerateArray(objJsonNode, "profile")) {
                    lastProfile = PresetJson.GetDoubleValue(profileJsonNode) ?? 0;
                    bEntity.profile[quantity] = lastProfile;

                    quantity++;
                    if (quantity >= bEntity.profile.Length)
                        break;
                }
                while (quantity < bEntity.profile.Length) {
                    bEntity.profile[quantity] = lastProfile;
                    quantity++;
                }
                bEntity.profile[0] = bEntity.profile[1]; //helps with calculating partial beacon values (ex: 0.5 _store.Beacons)
            }

            return true;
        }

        internal void AssemblerAdditionalProcessing(JsonNode objJsonNode, AssemblerPrototype aEntity, Dictionary<string, List<RecipePrototype>> craftingCategories) //recipe user
        {
            LinkCraftableRecipesFromJson(objJsonNode, aEntity);
            foreach (string machineCategory in PresetJson.EnumerateStrings(objJsonNode, "crafting_categories"))
                LinkAssemblerRecipesForCategory(craftingCategories, machineCategory, aEntity);
        }

        private void LinkCraftableRecipesFromJson(JsonNode objJsonNode, AssemblerPrototype assembler) {
            foreach (string recipeName in PresetJson.EnumerateStrings(objJsonNode, "craftable_recipes")) {
                if (_store.Recipes.TryGetValue(recipeName, out IRecipe? recipe) && recipe is RecipePrototype prototype)
                    LinkRecipeToAssembler(prototype, assembler);
            }
        }

        private static void LinkAssemblerRecipesForCategory(
            Dictionary<string, List<RecipePrototype>> craftingCategories,
            string recipeCategory,
            AssemblerPrototype assembler) {
            if (!craftingCategories.TryGetValue(recipeCategory, out List<RecipePrototype>? recipes))
                return;

            foreach (RecipePrototype recipe in recipes)
                LinkRecipeToAssembler(recipe, assembler);
        }

        internal void LinkRecipesToCraftingCategoryMachines() {
            foreach (RecipePrototype recipe in _store.Recipes.Values.OfType<RecipePrototype>().Where(r => r.AssemblersInternal.Count == 0)) {
                foreach (string category in recipe.CraftingCategoryKeysInternal) {
                    if (!_session.CraftingCategoryMachines.TryGetValue(category, out HashSet<string>? entityNames))
                        continue;
                    foreach (string entityName in entityNames) {
                        if (_store.Assemblers.TryGetValue(entityName, out IAssembler? assembler) && assembler is AssemblerPrototype assemblerPrototype)
                            LinkRecipeToAssembler(recipe, assemblerPrototype);
                    }
                }
            }
        }

        private static void LinkRecipeToAssembler(RecipePrototype recipe, AssemblerPrototype assembler) {
            if (recipe.AssemblersInternal.Contains(assembler))
                return;
            recipe.AssemblersInternal.Add(assembler);
            assembler.RecipesInternal.Add(recipe);
        }

        internal void MinerAdditionalProcessing(JsonNode objJsonNode, AssemblerPrototype aEntity) //resource provider
        {
            foreach (var recipe in PresetJson.EnumerateStrings(objJsonNode, "resource_categories")
                .SelectMany(s => _session.ResourceCategories.TryGetValue(s, out var list) ? list : [])
                .Where(recipe => TestRecipeEntityPipeFit(recipe, objJsonNode))) {
                ProcessEntityRecipeTechlink(aEntity, recipe);

                recipe.AssemblersInternal.Add(aEntity);
                aEntity.RecipesInternal.Add(recipe);
            }
        }

        internal bool OffshorePumpAdditionalProcessing(JsonNode objJsonNode, AssemblerPrototype aEntity, List<RecipePrototype> waterPumpRecipes) {
            //check if the pump has a specified 'output' fluid preset. if yes then only that recipe is added to it; if not then all water tile resource _store.Recipes are added
            var outPipeFilters = PresetJson.EnumerateStrings(objJsonNode, "out_pipe_filters").ToList();

            if (outPipeFilters.Count != 0) {
                if (_store.Recipes.TryGetValue(GetExtractionRecipeName(outPipeFilters[0]), out var extractionRecipe)) {
                    ProcessEntityRecipeTechlink(aEntity, (RecipePrototype)extractionRecipe);
                    ((RecipePrototype)extractionRecipe).AssemblersInternal.Add(aEntity);
                    aEntity.RecipesInternal.Add((RecipePrototype)extractionRecipe);
                } else {
                    //add new recipe
                    if (!_store.Items.TryGetValue(outPipeFilters[0], out var extractionFluid) || _store.ExtractionSubgroupFluids is null)
                        return false;

                    var recipe = new RecipePrototype(
                        _owner,
                        GetExtractionRecipeName(outPipeFilters[0]),
                        extractionFluid.FriendlyName + " Extraction",
                        _store.ExtractionSubgroupFluids,
                        extractionFluid.Name) {
                        Time = 1
                    };

                    recipe.InternalOneWayAddProduct((ItemPrototype)extractionFluid, 60, 60);
                    ((ItemPrototype)extractionFluid).ProductionRecipesInternal.Add(recipe);

                    recipe.SetIconAndColor(new IconColorPair(recipe.ProductListInternal[0].Icon, recipe.ProductListInternal[0].AverageColor));

                    _store.Recipes.Add(recipe.Name, recipe);
                }
            } else {
                foreach (RecipePrototype recipe in waterPumpRecipes) {
                    ProcessEntityRecipeTechlink(aEntity, recipe);
                    recipe.AssemblersInternal.Add(aEntity);
                    aEntity.RecipesInternal.Add(recipe);
                }
            }

            return true;
        }

        internal bool BoilerAdditionalProcessing(JsonNode objJsonNode, AssemblerPrototype aEntity) //Uses whatever the default energy source of it is to convert water into steam of a given temperature
        {
            if (PresetJson.GetString(objJsonNode, "fluid_ingredient") is not string fluidIng || PresetJson.GetString(objJsonNode, "fluid_product") is not string fluidProduct)
                return false;
            var ingredient = (FluidPrototype)_store.Items[fluidIng];
            var product = (FluidPrototype)_store.Items[fluidProduct];

            //boiler is a ingredient to product conversion with product coming out at the  target_temperature *C at a rate based on energy efficiency & energy use to bring the INGREDIENT to the given temperature (basically ingredient goes from default temp to target temp, then shifts to product). we will add an extra recipe for this
            double temp = PresetJson.GetDouble(objJsonNode, "target_temperature") ?? default;

            //I will be honest here. Testing has shown that the actual 'speed' is dependent on the incoming temperature (not the default temperature), as could have likely been expected.
            //this means that if you put in 65* water instead of 15* water to boil it to 165* steam it will result in 1.5x the 'maximum' output as listed in the factorio info menu and calculated below.
            //so if some mod does some wonky things like water pre-heating, or uses boiler to heat other fluids at non-default temperatures (I havent found any such mods, but testing shows it is possible to make such a mod)
            //then the values calculated here will be wrong.
            //Still, for now I will leave it as is.
            if (ingredient.SpecificHeatCapacity == 0) {
                foreach (IQuality quality in _store.Qualities.Values)
                    aEntity.SpeedInternal.Add(quality, 0);
            } else {
                foreach (IQuality quality in _store.Qualities.Values)
                    aEntity.SpeedInternal.Add(quality, (double)(aEntity.GetEnergyConsumption(quality) / ((temp - ingredient.DefaultTemperature) * ingredient.SpecificHeatCapacity * 60))); //by placing this here we can keep the recipe as a 1 sec -> 60 production, simplifying recipe comparing for presets.
            }

            RecipePrototype recipe;
            string boilRecipeName = string.Format(CultureInfo.InvariantCulture, "§§r:b:{0}:{1}:{2}", ingredient.Name, product.Name, temp.ToString(CultureInfo.InvariantCulture));
            if (!_store.Recipes.ContainsKey(boilRecipeName) && _store.EnergySubgroupBoiling is not null) {
                recipe = new RecipePrototype(
                    _owner,
                    boilRecipeName,
                    ingredient == product ? string.Format(DisplayCulture.Format, "{0} boiling to {1}°c", ingredient.FriendlyName, temp.ToString("0", DisplayCulture.Format)) : string.Format(DisplayCulture.Format, "{0} boiling to {1}°c {2}", ingredient.FriendlyName, temp.ToString("0", DisplayCulture.Format), product.FriendlyName),
                    _store.EnergySubgroupBoiling,
                    boilRecipeName);

                var icon = IconCache.CombineIcons(ingredient.Icon, product.Icon, ingredient.Icon.Height);
                try {
                    recipe.SetIconAndColor(new IconColorPair(icon, product.AverageColor));
                    icon = null;
                } finally {
                    icon?.Dispose();
                }

                recipe.Time = 1;

                recipe.InternalOneWayAddIngredient(ingredient, 60);
                ingredient.ConsumptionRecipesInternal.Add(recipe);

                double productQuantity = 60 * ingredient.SpecificHeatCapacity / product.SpecificHeatCapacity;
                recipe.InternalOneWayAddProduct(product, productQuantity, productQuantity, temp);
                product.ProductionRecipesInternal.Add(recipe);


                foreach (ModulePrototype module in _store.Modules.Values.Cast<ModulePrototype>()) //we will let the assembler sort out which module can be used with this recipe
                {
                    module.RecipesInternal.Add(recipe);
                    recipe.AssemblerModulesInternal.Add(module);
                }

                _store.Recipes.Add(recipe.Name, recipe);
            } else
                recipe = (RecipePrototype)_store.Recipes[boilRecipeName];

            ProcessEntityRecipeTechlink(aEntity, recipe);
            recipe.AssemblersInternal.Add(aEntity);
            aEntity.RecipesInternal.Add(recipe);

            return true;
        }

        internal bool GeneratorAdditionalProcessing(JsonNode objJsonNode, AssemblerPrototype aEntity) //consumes steam (at the provided temperature up to the given maximum) to generate electricity
        {
            if (PresetJson.GetString(objJsonNode, "fluid_ingredient") is not string fluidIng)
                return false;
            var ingredient = (FluidPrototype)_store.Items[fluidIng];

            double baseSpeed = (PresetJson.GetDouble(objJsonNode, "fluid_usage_per_sec") ?? default) / 60; //use 60 multiplier to make _store.Recipes easier
            double baseEnergyProduction = PresetJson.GetDouble(objJsonNode, "max_power_output") ?? default; //in seconds

            foreach (IQuality quality in _store.Qualities.Values)
                aEntity.SpeedInternal.Add(quality, baseSpeed * aEntity.GetEnergyProduction(quality) / baseEnergyProduction);

            aEntity.OperationTemperature = PresetJson.GetDouble(objJsonNode, "full_power_temperature") ?? default;
            double minTemp = PresetJson.GetDouble(objJsonNode, "minimum_temperature") ?? double.NaN;
            double maxTemp = PresetJson.GetDouble(objJsonNode, "maximum_temperature") ?? double.NaN;
            if (!double.IsNaN(minTemp) && minTemp < ingredient.DefaultTemperature)
                minTemp = ingredient.DefaultTemperature;
            if (!double.IsNaN(maxTemp) && maxTemp > DataCacheFluidLimits.MaxTemp)
                maxTemp = double.NaN;

            //actual energy production is a bit more complicated here (as it involves actual temperatures), but we will have to handle it in the graph (after all values have been calculated and we know the amounts and temperatures getting passed here, we can calc the energy produced)

            RecipePrototype recipe;
            string generationRecipeName = string.Format(CultureInfo.InvariantCulture, "§§r:g:{0}:{1}>{2}", ingredient.Name, minTemp, maxTemp);
            if (!_store.Recipes.ContainsKey(generationRecipeName) && _store.EnergySubgroupEnergy is not null) {
                recipe = new RecipePrototype(
                    _owner,
                    generationRecipeName,
                    string.Format(DisplayCulture.Format, "{0} to Electricity", ingredient.FriendlyName),
                    _store.EnergySubgroupEnergy,
                    generationRecipeName);

                var icon = IconCache.CombineIcons(ingredient.Icon, _store.ElectricityIcon ?? IconCache.UnknownIcon, ingredient.Icon.Height, false);
                try {
                    recipe.SetIconAndColor(new IconColorPair(icon, ingredient.AverageColor));
                    icon = null;
                } finally {
                    icon?.Dispose();
                }

                recipe.Time = 1;

                recipe.InternalOneWayAddIngredient(ingredient, 60, double.IsNaN(minTemp) ? double.NegativeInfinity : minTemp, double.IsNaN(maxTemp) ? double.PositiveInfinity : maxTemp);

                ingredient.ConsumptionRecipesInternal.Add(recipe);

                foreach (ModulePrototype module in _store.Modules.Values.Cast<ModulePrototype>()) //we will let the assembler sort out which module can be used with this recipe
                {
                    module.RecipesInternal.Add(recipe);
                    recipe.AssemblerModulesInternal.Add(module);
                }

                _store.Recipes.Add(recipe.Name, recipe);
            } else
                recipe = (RecipePrototype)_store.Recipes[generationRecipeName];

            ProcessEntityRecipeTechlink(aEntity, recipe);
            recipe.AssemblersInternal.Add(aEntity);
            aEntity.RecipesInternal.Add(recipe);

            return true;
        }

        internal bool BurnerGeneratorAdditionalProcessing(AssemblerPrototype aEntity) //consumes fuel to generate electricity
        {
            if (_store.BurnerRecipe is null)
                return false;
            aEntity.RecipesInternal.Add(_store.BurnerRecipe);
            _store.BurnerRecipe.AssemblersInternal.Add(aEntity);
            ProcessEntityRecipeTechlink(aEntity, _store.BurnerRecipe);

            foreach (IQuality quality in _store.Qualities.Values)
                aEntity.SpeedInternal.Add(quality, 1f); //doesnt matter - recipe is empty

            return true;
        }

        internal bool ReactorAdditionalProcessing(JsonNode objJsonNode, AssemblerPrototype aEntity) {
            if (_store.HeatRecipe is null || _store.HeatItem is null)
                return false;
            aEntity.NeighbourBonus = PresetJson.GetDouble(objJsonNode, "neighbour_bonus") ?? 0;
            aEntity.RecipesInternal.Add(_store.HeatRecipe);
            _store.HeatRecipe.AssemblersInternal.Add(aEntity);
            ProcessEntityRecipeTechlink(aEntity, _store.HeatRecipe);

            foreach (IQuality quality in _store.Qualities.Values)
                aEntity.SpeedInternal.Add(quality, (aEntity.GetEnergyConsumption(quality)) / _store.HeatItem.FuelValue); //the speed of producing 1MJ of energy as heat for this reactor based on quality

            return true;
        }

        internal void ProcessEntityRecipeTechlink(EntityObjectBasePrototype entity, RecipePrototype recipe) {
            if (entity.AssociatedItemsInternal.Count == 0 && _store.StartingTech is not null) {
                recipe.MyUnlockTechnologiesInternal.Add(_store.StartingTech);
                _store.StartingTech.UnlockedRecipesInternal.Add(recipe);
            } else {
                foreach (IItem placeItem in entity.AssociatedItemsInternal) {
                    foreach (IRecipe placeItemRecipe in placeItem.ProductionRecipes) {
                        foreach (TechnologyPrototype tech in placeItemRecipe.MyUnlockTechnologies.Cast<TechnologyPrototype>()) {
                            recipe.MyUnlockTechnologiesInternal.Add(tech);
                            tech.UnlockedRecipesInternal.Add(recipe);
                        }
                    }
                }
            }
        }

        internal static bool TestRecipeEntityPipeFit(RecipePrototype recipe, JsonNode objJsonNode) //returns true if the fluid boxes of the entity (assembler or miner) can accept the provided recipe (with its in/out fluids)
                {
            int inPipes = PresetJson.GetInt32(objJsonNode, "in_pipes") ?? default;
            var inPipeFilters = PresetJson.EnumerateStrings(objJsonNode, "in_pipe_filters").ToHashSet();
            int outPipes = PresetJson.GetInt32(objJsonNode, "out_pipes") ?? default;
            var outPipeFilters = PresetJson.EnumerateStrings(objJsonNode, "out_pipe_filters").ToHashSet();
            int ioPipes = PresetJson.GetInt32(objJsonNode, "io_pipes") ?? default;
            var ioPipeFilters = PresetJson.EnumerateStrings(objJsonNode, "io_pipe_filters").ToHashSet();

            int inCount = 0; //unfiltered
            int outCount = 0; //unfiltered
            foreach (ItemPrototype inFluid in recipe.IngredientListInternal.Where(i => i is IFluid)) {
                if (inPipeFilters?.Contains(inFluid.Name) is true) {
                    inPipes--;
                    inPipeFilters.Remove(inFluid.Name);
                } else if (ioPipeFilters?.Contains(inFluid.Name) is true) {
                    ioPipes--;
                    ioPipeFilters.Remove(inFluid.Name);
                } else
                    inCount++;
            }
            foreach (ItemPrototype outFluid in recipe.ProductListInternal.Where(i => i is IFluid)) {
                if (outPipeFilters?.Contains(outFluid.Name) is true) {
                    outPipes--;
                    outPipeFilters.Remove(outFluid.Name);
                } else if (ioPipeFilters?.Contains(outFluid.Name) is true) {
                    ioPipes--;
                    ioPipeFilters.Remove(outFluid.Name);
                } else
                    outCount++;
            }
            //remove any unused filtered pipes from the equation - they cant be used due to the filters.
            inPipes -= inPipeFilters?.Count ?? 0;
            ioPipes -= ioPipeFilters?.Count ?? 0;
            outPipes -= outPipeFilters?.Count ?? 0;

            //return true if the remaining unfiltered ingredients & products (fluids) can fit into the remaining unfiltered pipes
            return (inCount - inPipes <= ioPipes && outCount - outPipes <= ioPipes && inCount + outCount <= inPipes + outPipes + ioPipes);
        }

        private static string GetExtractionRecipeName(string itemName) => "§§r:e:" + itemName;
    }
}
