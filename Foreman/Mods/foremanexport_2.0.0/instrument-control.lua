local etable = {}
local qualityEnabled = 0

-- Keep in sync with PresetExportFormat.CurrentVersion in Foreman (C#).
local FOREMAN_EXPORT_VERSION = 1


localindex = 0

local prodTechRecipes = {} --list of recipes that have technologies with infinite productivity research

local function ExportLocalisedString(lstring, index)
	-- as could be expected if lstring doesnt have a working translation then we get a beauty of a mess... so that needs to be cleaned up outside of json table
	localised_print('<#~#>')
	localised_print(lstring)
	localised_print('<#~#>')
end

local function ProcessTemperature(temperature)
	if temperature == -math.huge then
		return -1e100
	elseif temperature == math.huge then
		return 1e100
	end
	return temperature
end

-- Boilers with fluid fuel (e.g. oil-boiler-mk01) have more than two fluid boxes; pick process in/out by role, not index.
local function ExportBoilerFluids(entity, tentity)
	tentity['target_temperature'] = ProcessTemperature(entity.target_temperature)

	local fesp = entity.fluid_energy_source_prototype
	local fuelFilter = fesp and fesp.fluid_box and fesp.fluid_box.filter and fesp.fluid_box.filter.name

	local ingredient = nil
	local product = nil
	for _, fbox in pairs(entity.fluidbox_prototypes) do
		if fbox.filter ~= nil then
			local fname = fbox.filter.name
			if fbox.production_type == 'output' then
				if product == nil then
					product = fname
				end
			elseif fbox.production_type == 'input' or fbox.production_type == 'input-output' then
				if fname ~= fuelFilter and ingredient == nil then
					ingredient = fname
				end
			end
		end
	end

	if ingredient == nil and entity.fluidbox_prototypes[1] and entity.fluidbox_prototypes[1].filter then
		ingredient = entity.fluidbox_prototypes[1].filter.name
	end
	if product == nil and entity.fluidbox_prototypes[2] and entity.fluidbox_prototypes[2].filter then
		product = entity.fluidbox_prototypes[2].filter.name
	end

	if ingredient ~= nil then
		tentity['fluid_ingredient'] = ingredient
	end
	if product ~= nil then
		tentity['fluid_product'] = product
	end
end

local function ProcessQualityValue(qualityfunction, multiplier)
	qualityTable = {}
	if qualityEnabled then
		for _, quality in pairs(prototypes.quality) do
			table.insert(qualityTable, {['quality'] = quality.name, ['value'] = qualityfunction(quality) * multiplier})
		end
	else
		qualityEntity = {}
		table.insert(qualityTable, {['quality'] = 'normal', ['value'] = qualityfunction() * multiplier})
	end

	return qualityTable
end

local function ProcessIngredientList(ingredients)
	ingredientlist = {}
	for _, ingredient in pairs(ingredients) do
		tingredient = {}
		tingredient['name'] = ingredient.name
		tingredient['type'] = ingredient.type
		tingredient['amount'] = ingredient.amount
		if ingredient.type == 'fluid' and ingredient.minimum_temperature ~= nil then
			tingredient['minimum_temperature'] = ProcessTemperature(ingredient.minimum_temperature)
		end
		if ingredient.type == 'fluid' and ingredient.maximum_temperature ~= nil then
			tingredient['maximum_temperature'] = ProcessTemperature(ingredient.maximum_temperature)
		end
		table.insert(ingredientlist, tingredient)
	end
	return ingredientlist
end

local function ProcessProductList(products)
	productlist = {}
	for _, product in pairs(products) do
		tproduct = {}
		tproduct['name'] = product.name
		tproduct['type'] = product.type

		amount = (product.amount == nil) and ((product.amount_max + product.amount_min)/2) or product.amount
		amount = amount * ((product.probability == nil) and 1 or product.probability)
		amount_ignored_by_productivity = (product.ignored_by_productivity == nil) and 0 or product.ignored_by_productivity
		if amount_ignored_by_productivity > amount then amount_ignored_by_productivity = amount end
		amount_added_by_extra_fraction = (product.extra_count_fraction == nil) and 0 or product.extra_count_fraction

		tproduct['amount'] = amount + amount_added_by_extra_fraction
		tproduct['p_amount'] = amount - amount_ignored_by_productivity + amount_added_by_extra_fraction

		if product.type == 'fluid' and product.temperature ~= nil then
			tproduct['temperature'] = ProcessTemperature(product.temperature)
		end
		table.insert(productlist, tproduct)
	end
	return productlist
end

local function ExportModList()
	tmods = {}
	table.insert(tmods, {['name'] = 'core', ['version'] = '1.0'})
	
	for name, version in pairs(script.active_mods) do
		if name ~= 'foremanexport' then
			table.insert(tmods, {['name'] = name, ['version'] = version})
		end
	end
	etable['mods'] = tmods
end

local function ExportResearch()
	ttechnologies = {}
	for _, tech in pairs(prototypes.technology) do
		ttech = {}
		ttech['name'] = tech.name
		ttech['icon_name'] = 'icon.t.'..tech.name
		ttech['enabled'] = tech.enabled
		ttech['essential'] = tech.essential
		ttech['hidden'] = tech.hidden
		
		ttech['prerequisites'] = {}
		for pname, _ in pairs(tech.prerequisites) do
			table.insert(ttech['prerequisites'], pname)
		end

		ttech['successors'] = {}
		for sname, _ in pairs(tech.successors) do
			table.insert(ttech['successors'], sname)
		end
		
		ttech['recipes'] = {}
		ttech['qualities'] = {}
		for _, effect in pairs(tech.effects) do
			if effect.type == 'unlock-recipe' then
				table.insert(ttech['recipes'], effect.recipe)
			elseif effect.type == 'change-recipe-productivity' then
				prodTechRecipes[effect.recipe] = true
			elseif effect.type == 'unlock-quality' then
				table.insert(ttech['qualities'], effect.quality)
			elseif effect.type == 'mining-with-fluid' then
				ttech['unlocks-mining-with-fluid'] = true
			end
		end

		ttech['research_unit_ingredients'] = {}
		for _, ingredient in pairs(tech.research_unit_ingredients) do
			tingredient = {}
			tingredient['name'] = ingredient.name
			tingredient['amount'] = ingredient.amount
			table.insert(ttech['research_unit_ingredients'], tingredient)
		end
		ttech['research_unit_count'] = tech.research_unit_count

		ttech['lid'] = '$'..localindex
		ExportLocalisedString(tech.localised_name, localindex)
		localindex = localindex + 1

		table.insert(ttechnologies, ttech)
	end
	etable['technologies'] = ttechnologies
end

local function CollectRecipeCraftingCategories(recipe)
	local categories = {}
	local seen = {}
	local function add_category(cat)
		if type(cat) == 'string' and cat ~= '' and seen[cat] == nil then
			seen[cat] = true
			table.insert(categories, cat)
		end
	end

	-- category + additional_categories are what Factorio uses for has_category(); no need to scan every recipe-category prototype.
	add_category(recipe.category)
	if recipe.additional_categories ~= nil then
		for _, cat in ipairs(recipe.additional_categories) do
			add_category(cat)
		end
	end
	return categories
end

local function EntityCraftingCategorySet(entity)
	local machine_cats = {}
	if entity.crafting_categories == nil then
		return machine_cats
	end
	-- crafting_categories is dictionary[RecipeCategoryID -> true]; keys are category names.
	-- Do not call recipe:has_category here — some keys are not accepted as RecipeCategoryID by the API.
	for cat, _ in pairs(entity.crafting_categories) do
		if type(cat) == 'string' and cat ~= '' then
			machine_cats[cat] = true
		elseif type(cat) == 'userdata' and cat.valid and cat.name ~= nil and cat.name ~= '' then
			machine_cats[cat.name] = true
		end
	end
	return machine_cats
end

local function CollectEntityCraftableRecipes(entity)
	local craftable = {}
	local machine_cats = EntityCraftingCategorySet(entity)
	if next(machine_cats) == nil then
		return craftable
	end

	for _, recipe in pairs(prototypes.recipe) do
		for _, recipe_cat in ipairs(CollectRecipeCraftingCategories(recipe)) do
			if machine_cats[recipe_cat] then
				table.insert(craftable, recipe.name)
				break
			end
		end
	end
	return craftable
end

local function ExportCraftingCategoryMachines()
	etable['crafting_category_machines'] = {}
	local used_categories = {}

	for _, recipe in pairs(prototypes.recipe) do
		for _, cat in ipairs(CollectRecipeCraftingCategories(recipe)) do
			used_categories[cat] = true
		end
	end

	for cat, _ in pairs(used_categories) do
		-- get_entity_filtered returns a LuaCustomTable (userdata); use pairs(), not next().
		local entities = prototypes.get_entity_filtered{{filter = 'crafting-category', crafting_category = cat}}
		local names = {}
		for entity_name, _ in pairs(entities) do
			table.insert(names, entity_name)
		end
		if #names > 0 then
			table.sort(names)
			etable['crafting_category_machines'][cat] = names
		end
	end
end

local function ExportRecipes()
	trecipes = {}
	for _, recipe in pairs(prototypes.recipe) do
		trecipe = {}
		trecipe['name'] = recipe.name
		trecipe['icon_name'] = 'icon.r.'..recipe.name
		if recipe.products[1] then
			trecipe["icon_alt_name"] = 'icon.i.'..recipe.products[1].name
		else
			trecipe["icon_alt_name"] = 'icon.r.'..recipe.name
		end
		if prodTechRecipes[recipe.name] ~= nil then
			trecipe["prod_research"] = true
		end

		trecipe['enabled'] = recipe.enabled
		trecipe['category'] = recipe.category
		trecipe['additional_categories'] = {}
		if recipe.additional_categories ~= nil then
			for _, cat in ipairs(recipe.additional_categories) do
				table.insert(trecipe['additional_categories'], cat)
			end
		end
		trecipe['crafting_categories'] = CollectRecipeCraftingCategories(recipe)
		trecipe['energy'] = recipe.energy
		trecipe['order'] = recipe.order
		if recipe.subgroup ~= nil then
			trecipe['subgroup'] = recipe.subgroup.name
		end

		trecipe['maximum_productivity'] = recipe.maximum_productivity
		trecipe['hide_from_player_crafting'] = recipe.hide_from_player_crafting
		if recipe.hidden then
			trecipe['hidden'] = true
		end

		trecipe['allowed_effects'] = recipe.allowed_effects

		if recipe.allowed_module_categories ~= nil then
			trecipe['allowed_module_categories'] = {}
			for name, _ in pairs(recipe.allowed_module_categories) do
				trecipe['allowed_module_categories'][name] = true
			end
		end

		if recipe.surface_conditions ~= nil then
			trecipe['surface_conditions'] = recipe.surface_conditions
		end

		if recipe.trash ~= nil then
			trecipe['trash'] = {}
			for _, trashitem in pairs(recipe.trash) do
				ttrashitem = {}
				ttrashitem['name'] = trashitem.name
				ttrashitem['type'] = trashitem.type
				ttrashitem['amount'] = 1
				table.insert(trecipe['trash'], ttrashitem)
			end
		end

		trecipe['ingredients'] = ProcessIngredientList(recipe.ingredients)
		trecipe['products'] = ProcessProductList(recipe.products)

		trecipe['lid'] = '$'..localindex		
		ExportLocalisedString(recipe.localised_name, localindex)
		localindex = localindex + 1

		table.insert(trecipes, trecipe)
	end
	etable['recipes'] = trecipes
end

local function ExportQuality()
	tqualities = {}
	if prototypes.quality ~= nil then
		for _, quality in pairs(prototypes.quality) do

			tquality = {}
			tquality['name'] = quality.name
			tquality['icon_name'] = 'icon.q.'..quality.name
			tquality['order'] = quality.order
			tquality['hidden'] = quality.hidden

			tquality['level'] = quality.level
			tquality['beacon_power_multiplier'] = quality.beacon_power_usage_multiplier
			tquality['mining_drill_resource_drain_multiplier'] = quality.mining_drill_resource_drain_multiplier

			if quality.next_probability ~= 0 and quality.next ~= nil then
				tquality['next_probability'] = quality.next_probability
				tquality['next'] = (quality.next ~= nil) and quality.next.name or nil
			end
		
			tquality['lid'] = '$'..localindex
			ExportLocalisedString(quality.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tqualities, tquality)
		end
	end
	etable['qualities'] = tqualities
end

local function ExportItems()
	titems = {}
	for _, item in pairs(prototypes.item) do
		titem = {}
		titem['name'] = item.name
		titem['icon_name'] = 'icon.i.'..item.name
		titem['order'] = item.order
		if item.subgroup ~= nil then
			titem['subgroup'] = item.subgroup.name
		end
		titem["stack_size"] = item.stackable and item.stack_size or 1
		titem['weight'] = item.weight

		titem['ingredient_to_weight_coefficient'] = item.ingredient_to_weight_coefficient


		if item.fuel_category ~= nil then
			titem['fuel_category'] = item.fuel_category
			titem['fuel_value'] = item.fuel_value
			titem['fuel_emissions_multiplier'] = item.fuel_emissions_multiplier
		end

		if item.burnt_result ~= nil then
			titem['burnt_result'] = item.burnt_result.name
		end

		if item.spoil_result ~= nil then
			titem['spoil_result'] = item.spoil_result.name
			titem['q_spoil_time'] = ProcessQualityValue(item.get_spoil_ticks, 1/60)
		end

		if item.plant_result ~= nil and item.plant_result.mineable_properties ~= nil then
			titem['plant_results'] = ProcessProductList(item.plant_result.mineable_properties.products)
			titem['plant_growth_time'] = item.plant_result.growth_ticks / 60
		end

		if item.rocket_launch_products ~= nil and item.rocket_launch_products[1] ~= nil then
			titem['rocket_launch_products'] = {}
			for _, product in pairs(item.rocket_launch_products) do
				tproduct = {}
				tproduct['name'] = product.name
				tproduct['type'] = product.type

				amount = (product.amount == nil) and ((product.amount_max + product.amount_min)/2) or product.amount
				amount = amount * ( (product.probability == nil) and 1 or product.probability)

				tproduct['amount'] = amount

				if product.type == 'fluid' and product.temperature ~= nil then
					tproduct['temperature'] = ProcessTemperature(product.temperature)
				end
				table.insert(titem['rocket_launch_products'], tproduct)
			end
		end

		titem['lid'] = '$'..localindex
		ExportLocalisedString(item.localised_name, localindex)
		localindex = localindex + 1

		table.insert(titems, titem)
	end
	etable['items'] = titems
end

local function ExportFluids()
	tfluids = {}
	for _, fluid in pairs(prototypes.fluid) do
		tfluid = {}
		tfluid['name'] = fluid.name
		tfluid['icon_name'] = 'icon.i.'..fluid.name
		tfluid['order'] = fluid.order
		if fluid.subgroup ~= nil then
			tfluid['subgroup'] = fluid.subgroup.name
		end
		tfluid['default_temperature'] = ProcessTemperature(fluid.default_temperature)
		tfluid['max_temperature'] = ProcessTemperature(fluid.max_temperature)
		tfluid['gas_temperature'] = ProcessTemperature(fluid.gas_temperature)
		tfluid['heat_capacity'] = fluid.heat_capacity == nil and 0 or fluid.heat_capacity
		
		if fluid.fuel_value ~= 0 then
			tfluid['fuel_value'] = fluid.fuel_value
			tfluid['emissions_multiplier'] = fluid.emissions_multiplier
		end

		tfluid['lid'] = '$'..localindex
		ExportLocalisedString(fluid.localised_name, localindex)
		localindex = localindex + 1

		table.insert(tfluids, tfluid)
	end
	etable['fluids'] = tfluids
end


local function ExportModules()
	tmodules = {}
	for _, module in pairs(prototypes.item) do
		if module.module_effects ~= nil then
			tmodule = {}
			tmodule['name'] = module.name
			tmodule['icon_name'] = 'icon.e.'..module.name
			tmodule["icon_alt_name"] = 'icon.i.'..module.name
			tmodule['order'] = module.order
			tmodule['category'] = module.category
			tmodule['tier'] = module.tier

			tmodule['module_effects'] = {}
			tmodule['module_effects']['consumption'] = (module.module_effects.consumption == nil) and 0 or module.module_effects.consumption
			tmodule['module_effects']['speed'] = (module.module_effects.speed == nil) and 0 or module.module_effects.speed
			tmodule['module_effects']['productivity'] = (module.module_effects.productivity == nil) and 0 or module.module_effects.productivity
			tmodule['module_effects']['pollution'] = (module.module_effects.pollution == nil) and 0 or module.module_effects.pollution
			tmodule['module_effects']['quality'] = (module.module_effects.quality == nil) and 0 or module.module_effects.quality

			tmodule['lid'] = '$'..localindex
			ExportLocalisedString(module.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tmodules, tmodule)
		end
	end
	etable['modules'] = tmodules
end

local function ExportEntities()
	tentities = {}
	for _, entity in pairs(prototypes.entity) do --select any entity with an energy source (or fluid -> offshore pump). we will sort them out later. BONUS: also grab the 'character' entity - for those hand-crafts
		if entity.type == 'boiler' or entity.type == 'generator' or entity.type == 'reactor' or entity.type == 'mining-drill' or entity.type == 'offshore-pump' or entity.type == 'furnace' or entity.type == 'assembling-machine' or entity.type == 'beacon' or entity.type == 'rocket-silo' or entity.type == 'burner-generator' or entity.type == "character" then
			tentity = {}
			tentity['name'] = entity.name
			tentity['icon_name'] = 'icon.e.'..entity.name
			tentity["icon_alt_name"] = 'icon.i.'..entity.name
			tentity['order'] = entity.order
			tentity['type'] = entity.type

			if entity.next_upgrade ~= nil then tentity['next_upgrade'] = entity.next_upgrade.name end

			if entity.type == 'mining-drill' or entity.type == 'character' then
				tentity['speed'] = entity.mining_speed
			elseif entity.type == 'offshore-pump' then
				tentity['speed'] = entity.pumping_speed
			elseif entity.type == 'furnace' or entity.type == 'assembling-machine' or entity.type == 'rocket-silo' then
				tentity['q_speed'] = ProcessQualityValue(entity.get_crafting_speed, 1)
			end

			if entity.fluid_usage_per_tick ~= nil then tentity['fluid_usage_per_sec'] = entity.fluid_usage_per_tick * 60 end

			if entity.module_inventory_size ~= nil then tentity['module_inventory_size'] =  entity.module_inventory_size end
			if entity.distribution_effectivity ~= nil then tentity['distribution_effectivity'] = entity.distribution_effectivity end
			if entity.distribution_effectivity_bonus_per_quality_level ~= nil then tentity['distribution_effectivity_bonus_per_quality_level'] = entity.distribution_effectivity_bonus_per_quality_level end

			if entity.neighbour_bonus ~= nil then tentity['neighbour_bonus'] = entity.neighbour_bonus end
			
			if entity.type == 'mining-drill' or entity.type == 'furnace' or entity.type == 'assembling-machine' or entity.type == 'beacon' or entity.type == 'rocket-silo' then
				tentity['base_module_effects'] = {}
				if entity.effect_receiver ~= nil then
					tentity['base_module_effects']['consumption'] = entity.effect_receiver.base_effect.consumption ~= nil and entity.effect_receiver.base_effect.consumption or 0
					tentity['base_module_effects']['speed'] = entity.effect_receiver.base_effect.speed ~= nil and entity.effect_receiver.base_effect.speed or 0
					tentity['base_module_effects']['productivity'] = entity.effect_receiver.base_effect.productivity ~= nil and entity.effect_receiver.base_effect.productivity or 0
					tentity['base_module_effects']['pollution'] = entity.effect_receiver.base_effect.pollution ~= nil and entity.effect_receiver.base_effect.pollution or 0
					tentity['base_module_effects']['quality'] = entity.effect_receiver.base_effect.quality ~= nil and entity.effect_receiver.base_effect.quality or 0
					tentity['uses_module_effects'] = entity.effect_receiver.uses_module_effects
					tentity['uses_beacon_effects'] = entity.effect_receiver.uses_beacon_effects
					tentity['uses_surface_effects'] = entity.effect_receiver.uses_surface_effects
				else
					tentity['base_module_effects']['consumption'] = 0
					tentity['base_module_effects']['speed'] = 0
					tentity['base_module_effects']['productivity'] = 0
					tentity['base_module_effects']['pollution'] = 0
					tentity['base_module_effects']['quality'] = 0
					tentity['uses_module_effects'] = false
					tentity['uses_beacon_effects'] = false
					tentity['uses_surface_effects'] = false
				end
				
				tentity['allowed_effects'] = entity.allowed_effects ~= nil and entity.allowed_effects or {}

				tentity['allowed_module_categories'] = entity.allowed_module_categories
			end

			tentity['items_to_place_this'] = {}
			if entity.items_to_place_this ~= nil then
				for _, item in pairs(entity.items_to_place_this) do
					if(type(item) == 'string') then
						table.insert(tentity['items_to_place_this'], item)
					else
						table.insert(tentity['items_to_place_this'], item['name'])
					end
				end
			end

			if entity.crafting_categories ~= nil then
			tentity['crafting_categories'] = {}
				for fname, _ in pairs(entity.crafting_categories) do
					table.insert(tentity['crafting_categories'], fname)
				end
			end
			if entity.type == 'furnace' or entity.type == 'assembling-machine' or entity.type == 'rocket-silo' then
				local craftable = CollectEntityCraftableRecipes(entity)
				if #craftable > 0 then
					table.sort(craftable)
					tentity['craftable_recipes'] = craftable
				end
			end
			if entity.resource_categories ~= nil then
			tentity['resource_categories'] = {}
				for fname, _ in pairs(entity.resource_categories) do
					table.insert(tentity['resource_categories'], fname)
				end
			end
			tentity['profile'] = entity.profile --beacons

			--fluid boxes for input/output of boiler & generator need to be processed (almost guaranteed to be 'steam' and 'water', but... tests have shown that we can heat up whatever we want)
			--additinally we want count of fluid boxes in/out (for checking recipe validity)
			if entity.type == 'boiler' then
				ExportBoilerFluids(entity, tentity)
			elseif entity.type == 'generator' then
				tentity['full_power_temperature'] = ProcessTemperature(entity.maximum_temperature)
				tentity['max_power_output'] = entity.max_power_output * 60

				tentity['minimum_temperature'] = ProcessTemperature(entity.fluidbox_prototypes[1].minimum_temperature)
				tentity['maximum_temperature'] = ProcessTemperature(entity.fluidbox_prototypes[1].maximum_temperature)
				if entity.fluidbox_prototypes[1].filter ~= nil then
					tentity['fluid_ingredient'] = entity.fluidbox_prototypes[1].filter.name
				end
			else
				inPipes = 0
				inPipeFilters = {}
				ioPipes = 0
				ioPipeFilters = {}
				outPipes = 0
				outPipeFilters = {}
				-- i will ignore temperature limitations for this. (this is for recipe checks)

				for _, fbox in pairs(entity.fluidbox_prototypes) do
					if fbox.production_type == 'input' then
						inPipes = inPipes + 1
						if fbox.filter ~= nil then table.insert(inPipeFilters, fbox.filter.name) end
					elseif fbox.production_type == 'output' then
						outPipes = outPipes + 1
						if fbox.filter ~= nil then table.insert(outPipeFilters, fbox.filter.name) end
					elseif fbox.production_type == 'input-output' or fbox.production_type == 'none' then --il be honest - no idea what 'none' means, but its used for miners and represents a two way connection - so chuck it under in-out
						ioPipes = ioPipes + 1
						if fbox.filter ~= nil then table.insert(ioPipeFilters, fbox.filter.name) end
					end
				end
				tentity['in_pipes'] = inPipes
				tentity['in_pipe_filters'] = inPipeFilters
				tentity['out_pipes'] = outPipes
				tentity['out_pipe_filters'] = outPipeFilters
				tentity['io_pipes'] = ioPipes
				tentity['io_pipe_filters'] = ioPipeFilters
			end

			tentity['energy_usage'] = (entity.energy_usage == nil) and 0 or entity.energy_usage
			tentity['q_max_energy_usage'] = ProcessQualityValue(entity.get_max_energy_usage, 60)
			tentity['q_energy_production'] = ProcessQualityValue(entity.get_max_energy_production, 60)

			if entity.burner_prototype ~= nil then
				tentity['fuel_type'] = 'item'
				tentity['fuel_effectivity'] = entity.burner_prototype.effectivity

				tentity['pollution'] = {}
				for pollutant, quantity in pairs(entity.burner_prototype.emissions_per_joule) do
					tentity['pollution'][pollutant] = quantity
				end

				tentity['fuel_categories'] = {}
				for fname, _ in pairs(entity.burner_prototype.fuel_categories) do
					table.insert(tentity['fuel_categories'], fname)
				end

			elseif entity.fluid_energy_source_prototype then
				tentity['fuel_type'] = 'fluid'
				tentity['fuel_effectivity'] = entity.fluid_energy_source_prototype.effectivity

				tentity['pollution'] = {}
				for pollutant, quantity in pairs(entity.fluid_energy_source_prototype.emissions_per_joule) do
					tentity['pollution'][pollutant] = quantity
				end
				tentity['burns_fluid'] = entity.fluid_energy_source_prototype.burns_fluid

				--fluid limitations from fluid box:
				if entity.fluid_energy_source_prototype.fluid_box.filter ~= nil then
					tentity['fuel_filter'] = entity.fluid_energy_source_prototype.fluid_box.filter.name
				end
				tentity['minimum_fuel_temperature'] = ProcessTemperature(entity.fluid_energy_source_prototype.fluid_box.minimum_temperature) -- nil is accepted
				tentity['maximum_fuel_temperature'] = ProcessTemperature(entity.fluid_energy_source_prototype.fluid_box.maximum_temperature) --nil is accepted

			elseif entity.electric_energy_source_prototype then
				tentity['fuel_type'] = 'electricity'
				tentity['fuel_effectivity'] = 1
				tentity['drain'] = entity.electric_energy_source_prototype.drain * 60

				tentity['pollution'] = {}
				for pollutant, quantity in pairs(entity.electric_energy_source_prototype.emissions_per_joule) do
					tentity['pollution'][pollutant] = quantity
				end

			elseif entity.heat_energy_source_prototype  then
				tentity['fuel_type'] = 'heat'
				tentity['fuel_effectivity'] = 1

				tentity['pollution'] = {}
				for pollutant, quantity in pairs(entity.heat_energy_source_prototype.emissions_per_joule) do
					tentity['pollution'][pollutant] = quantity
				end
			elseif entity.void_energy_source_prototype  then
				tentity['fuel_type'] = 'void'
				tentity['fuel_effectivity'] = 1

				tentity['pollution'] = {}
				for pollutant, quantity in pairs(entity.void_energy_source_prototype.emissions_per_joule) do
					tentity['pollution'][pollutant] = quantity
				end
			else
				tentity['fuel_type'] = 'void'
				tentity['fuel_effectivity'] = 1

				tentity['pollution'] = {}
			end

			tentity['lid'] = '$'..localindex
			ExportLocalisedString(entity.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tentities, tentity)
		end
	end
	etable['entities'] = tentities
end

local function ExportResources()
	tresources = {}
	for _, resource in pairs(prototypes.entity) do
		if resource.resource_category ~= nil and resource.mineable_properties.products ~= nil and resource.mineable_properties.minable then
			tresource = {}
			tresource['name'] = resource.name
			tresource['resource_category'] = resource.resource_category
			tresource['mining_time'] = resource.mineable_properties.mining_time
			if resource.mineable_properties.required_fluid and resource.mineable_properties.fluid_amount then
				local req_fluid = resource.mineable_properties.required_fluid
				tresource['required_fluid'] = type(req_fluid) == "string" and req_fluid or req_fluid.name
				tresource['fluid_amount'] = resource.mineable_properties.fluid_amount / 10;
			end

			tresource['products'] = ProcessProductList(resource.mineable_properties.products)

			tresource['lid'] = '$'..localindex
			ExportLocalisedString(resource.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tresources, tresource)
		end
	end
	etable['resources'] = tresources
end

valid_water_resources = {}
local function ExportWaterResources()
	twresources = {}
	for _, wresource in pairs(prototypes.tile) do
		if wresource.fluid ~= nil then
			found = 0
			for _, existing_fluid in ipairs(valid_water_resources) do
				if wresource.fluid.name == existing_fluid then found = 1 end
			end
			if found == 0 then
				twresource = {}
				twresource['name'] = wresource.fluid.name
				twresource['resource_category'] = '<<foreman_resource_category_water_tile>>'
				twresource['mining_time'] = 1
				twresource['products'] = {}
				tproduct = {}
				tproduct['name'] = wresource.fluid.name
				tproduct['type'] = 'fluid'
				tproduct['amount'] = 60
				tproduct['temperature'] = ProcessTemperature(wresource.fluid.default_temperature)
				table.insert(twresource['products'], tproduct)

				twresource['lid'] = '$'..localindex
				ExportLocalisedString(wresource.fluid.localised_name, localindex)
				localindex = localindex + 1

				table.insert(twresources, twresource)
				table.insert(valid_water_resources, wresource.fluid.name)
			end
		end
	end

	etable['water_resources'] = twresources
end

local function ExportGroups()
	tgroups = {}
	for _, group in pairs(prototypes['item_group']) do
		tgroup = {}
		tgroup['name'] = group.name
		tgroup['icon_name'] = 'icon.g.'..group.name
		tgroup['order'] = group.order

		tgroup['subgroups'] = {}
		for _, subgroup in pairs(group.subgroups) do
			table.insert(tgroup['subgroups'], subgroup.name)
		end

		tgroup['lid'] = '$'..localindex
		ExportLocalisedString(group.localised_name, localindex)
		localindex = localindex + 1

		table.insert(tgroups, tgroup)
	end
	etable['groups'] = tgroups
end

local function ExportSubGroups()
	tsgroups = {}
	for _, sgroup in pairs(prototypes['item_subgroup']) do
		tsgroup = {}
		tsgroup['name'] = sgroup.name
		tsgroup['order'] = sgroup.order

		table.insert(tsgroups, tsgroup)
	end
	etable['subgroups'] = tsgroups
end

script.on_nth_tick(1, 	
	function()

		etable['difficulty'] = {0,0}
		etable['foreman_export_version'] = FOREMAN_EXPORT_VERSION

		localised_print('<<<START-EXPORT-LN>>>')

		ExportModList()
		ExportResearch()
		ExportRecipes()

		ExportQuality()

		ExportItems()
		ExportFluids()
		ExportModules()
		ExportEntities()
		ExportResources()
		ExportWaterResources()
		ExportGroups()
		ExportSubGroups()
		ExportCraftingCategoryMachines()

		localised_print('<<<END-EXPORT-LN>>>')

		localised_print('<<<START-EXPORT-P2>>>')
		localised_print(helpers.table_to_json(etable))
		localised_print('<<<END-EXPORT-P2>>>')

		ENDEXPORTANDJUSTDIE() -- just the most safe way of ensuring that we export once and quit. Basically... there is no ENDEXPORTANDJUSTDIE function. so lua will throw an exception and the run will end here.
	end
)