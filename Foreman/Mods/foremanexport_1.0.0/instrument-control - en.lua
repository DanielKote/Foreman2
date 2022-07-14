local etable = {}


localindex = 0
local function ExportLocalisedString(lstring, index)
	-- as could be expected if lstring doesnt have a working translation then we get a beauty of a mess... so that needs to be cleaned up outside of json table
	localised_print('<#~#>')
	localised_print(lstring)
	localised_print('<#~#>')
end

local function ProcessTemperature(temperature)
	if temperature == nil then
		return nil
	elseif temperature == -math.huge then
		return -1e100
	elseif temperature == math.huge then
		return 1e100
	else
		return temperature
	end
end

local function ExportModList()
	tmods = {}
	table.insert(tmods, {['name'] = 'core', ['version'] = '1.0'})
	
	for name, version in pairs(game.active_mods) do
		table.insert(tmods, {['name'] = name, ['version'] = version})
	end
	etable['mods'] = tmods
end

local function ExportResearch()
	ttechnologies = {}
	for _, tech in pairs(game.technology_prototypes) do
		ttech = {}
		ttech['name'] = tech.name
		ttech['icon_name'] = 'icon.t.'..tech.name
		ttech['enabled'] = tech.enabled
		ttech['hidden'] = tech.hidden
		
		ttech['prerequisites'] = {}
		for pname, _ in pairs(tech.prerequisites) do
			table.insert(ttech['prerequisites'], pname)
		end
		
		ttech['recipes'] = {}
		for _, effect in pairs(tech.effects) do
			if(effect.type == 'unlock-recipe') then
				table.insert(ttech['recipes'], effect.recipe)
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

local function ExportRecipes()
	trecipes = {}
	for _, recipe in pairs(game.recipe_prototypes) do
		trecipe = {}
		trecipe['name'] = recipe.name
		trecipe['icon_name'] = 'icon.r.'..recipe.name
		if recipe.products[1] then
			trecipe["icon_alt_name"] = 'icon.i.'..recipe.products[1].name
		else
			trecipe["icon_alt_name"] = 'icon.r.'..recipe.name
		end

		trecipe['enabled'] = recipe.enabled
		trecipe['category'] = recipe.category
		trecipe['energy'] = recipe.energy
		trecipe['order'] = recipe.order
		trecipe['subgroup'] = recipe.subgroup.name

		trecipe['ingredients'] = {}
		for _, ingredient in pairs(recipe.ingredients) do
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
			table.insert(trecipe['ingredients'], tingredient)
		end

		trecipe['products'] = {}
		for _, product in pairs(recipe.products) do
			tproduct = {}
			tproduct['name'] = product.name
			tproduct['type'] = product.type

			amount = (product.amount == nil) and ((product.amount_max + product.amount_min)/2) or product.amount
			amount = amount * product.probability

			tproduct['amount'] = amount
			tproduct['p_amount'] = amount

			if (product.catalyst_amount ~= nil) then

				if product.amount ~= nil then
					tproduct['p_amount'] = product.amount - math.max(0, math.min(product.amount, product.catalyst_amount))
				elseif product.catalyst_amount <= product.amount_min then
					tproduct['p_amount'] = ((product.amount_max + product.amount_min)/2) - math.max(0, product.catalyst_amount)
				else
					catalyst_amount = math.min(product.amount_max, product.catalyst_amount)
					tproduct['p_amount'] = ((product.amount_max - catalyst_amount) * (product.amount_max + 1 - catalyst_amount) / 2) / (product.amount_max + 1 - product.amount_min)
				end

				tproduct['p_amount'] = tproduct['p_amount'] * product.probability

			elseif product.amount ~= nil and product.probability == 1 then
				for _, ingredient in pairs(recipe.ingredients) do
					if ingredient.name == product.name then
						tproduct['p_amount'] = math.max(0, product.amount - ingredient.amount)
					end
				end
			end

			if product.type == 'fluid' and product.temperature ~= nil then
				tproduct['temperature'] = ProcessTemperature(product.temperature)
			end
			table.insert(trecipe['products'], tproduct)
		end

		trecipe['lid'] = '$'..localindex		
		ExportLocalisedString(recipe.localised_name, localindex)
		localindex = localindex + 1

		table.insert(trecipes, trecipe)
	end
	etable['recipes'] = trecipes
end

local function ExportItems()
	titems = {}
	for _, item in pairs(game.item_prototypes) do
		titem = {}
		titem['name'] = item.name
		titem['icon_name'] = 'icon.i.'..item.name
		titem['order'] = item.order
		titem['subgroup'] = item.subgroup.name
		titem["stack"] = item.stackable and item.stack_size or 1

		if item.fuel_category ~= nil then
			titem['fuel_category'] = item.fuel_category
			titem['fuel_value'] = item.fuel_value
			titem['pollution_multiplier'] = item.fuel_emissions_multiplier
		end

		if item.burnt_result ~= nil then
			titem['burnt_result'] = item.burnt_result.name
		end

		if item.rocket_launch_products ~= nil and item.rocket_launch_products[1] ~= nil then
			titem['launch_products'] = {}
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
				table.insert(titem['launch_products'], tproduct)
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
	for _, fluid in pairs(game.fluid_prototypes) do
		tfluid = {}
		tfluid['name'] = fluid.name
		tfluid['icon_name'] = 'icon.i.'..fluid.name
		tfluid['order'] = fluid.order
		tfluid['subgroup'] = fluid.subgroup.name
		tfluid['default_temperature'] = ProcessTemperature(fluid.default_temperature)
		tfluid['max_temperature'] = ProcessTemperature(fluid.max_temperature)
		tfluid['heat_capacity'] = fluid.heat_capacity == nil and 0 or fluid.heat_capacity
		
		if fluid.fuel_value ~= 0 then
			tfluid['fuel_value'] = fluid.fuel_value
			tfluid['pollution_multiplier'] = fluid.emissions_multiplier
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
	for _, module in pairs(game.item_prototypes) do
		if module.module_effects ~= nil then
			tmodule = {}
			tmodule['name'] = module.name
			tmodule['icon_name'] = 'icon.e.'..module.name
			tmodule["icon_alt_name"] = 'icon.i.'..module.name
			tmodule['order'] = module.order
			tmodule['category'] = module.category
			tmodule['tier'] = module.tier

			tmodule['module_effects_consumption'] = (module.module_effects.consumption == nil) and 0 or module.module_effects.consumption.bonus
			tmodule['module_effects_speed'] = (module.module_effects.speed == nil) and 0 or module.module_effects.speed.bonus
			tmodule['module_effects_productivity'] = (module.module_effects.productivity == nil) and 0 or module.module_effects.productivity.bonus
			tmodule['module_effects_pollution'] = (module.module_effects.pollution == nil) and 0 or module.module_effects.pollution.bonus

			tmodule['limitations'] = {}
			for _, recipe in pairs(module.limitations) do
				table.insert(tmodule['limitations'], recipe)
			end

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
	for _, entity in pairs(game.entity_prototypes) do --select any entity with an energy source (or fluid -> offshore pump). we will sort them out later. BONUS: also grab the 'character' entity - for those hand-crafts
		if entity.type == 'boiler' or entity.type == 'generator' or entity.type == 'reactor' or entity.type == 'mining-drill' or entity.type == 'offshore-pump' or entity.type == 'furnace' or entity.type == 'assembling-machine' or entity.type == 'beacon' or entity.type == 'rocket-silo' or entity.type == 'burner-generator' or entity.type == "character" then
			tentity = {}
			tentity['name'] = entity.name
			tentity['icon_name'] = 'icon.e.'..entity.name
			tentity["icon_alt_name"] = 'icon.i.'..entity.name
			tentity['order'] = entity.order
			tentity['type'] = entity.type

			if entity.next_upgrade ~= nil then tentity['next_upgrade'] = entity.next_upgrade.name end

			if entity.crafting_speed ~= nil then tentity['speed'] = entity.crafting_speed
			elseif entity.mining_speed ~= nil then tentity['speed'] = entity.mining_speed
			elseif entity.pumping_speed ~= nil then tentity['speed'] = entity.pumping_speed end

			if entity.fluid ~= nil then tentity['fluid_product'] = entity.fluid.name end
			if entity.fluid_usage_per_tick ~= nil then tentity['fluid_usage_per_tick'] = entity.fluid_usage_per_tick end

			if entity.module_inventory_size ~= nil then tentity['module_inventory_size'] =  entity.module_inventory_size end
			if entity.base_productivity ~= nil then tentity['base_productivity'] = entity.base_productivity end
			if entity.distribution_effectivity ~= nil then tentity['distribution_effectivity'] = entity.distribution_effectivity end
			if entity.neighbour_bonus ~= nil then tentity['neighbour_bonus'] = entity.neighbour_bonus end
			--ingredient_count is depreciated

			tentity['associated_items'] = {}
			if entity.items_to_place_this ~= nil then
				for _, item in pairs(entity.items_to_place_this) do
					if(type(item) == 'string') then
						table.insert(tentity['associated_items'], item)
					else
						table.insert(tentity['associated_items'], item['name'])
					end
				end
			end

			tentity['allowed_effects'] = {}
			if entity.allowed_effects then
				for effect, allowed in pairs(entity.allowed_effects) do
					if allowed then
						table.insert(tentity['allowed_effects'], effect)
					end
				end
			end

			if entity.crafting_categories ~= nil then
				tentity['crafting_categories'] = {}
				for category, _ in pairs(entity.crafting_categories) do
					table.insert(tentity['crafting_categories'], category)
				end
			end

			if entity.resource_categories ~= nil then
				tentity['resource_categories'] = {}
				for category, _ in pairs(entity.resource_categories) do
					table.insert(tentity['resource_categories'], category)
				end
			end

			--fluid boxes for input/output of boiler & generator need to be processed (almost guaranteed to be 'steam' and 'water', but... tests have shown that we can heat up whatever we want)
			--additinally we want count of fluid boxes in/out (for checking recipe validity)
			if entity.type == 'boiler' then
				tentity['target_temperature'] = ProcessTemperature(entity.target_temperature)

				if entity.fluidbox_prototypes[1].filter ~= nil then
					tentity['fluid_ingredient'] = entity.fluidbox_prototypes[1].filter.name
				end
				if entity.fluidbox_prototypes[2].filter ~= nil then
					tentity['fluid_product'] = entity.fluidbox_prototypes[2].filter.name
				end
			elseif entity.type == 'generator' then
				tentity['full_power_temperature'] = ProcessTemperature(entity.maximum_temperature)

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
					elseif fbox.production_type == 'input-output' then
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

			tentity['max_energy_usage'] = (entity.max_energy_usage == nil) and 0 or entity.max_energy_usage
			tentity['energy_usage'] = (entity.energy_usage == nil) and 0 or entity.energy_usage
			tentity['energy_production'] = entity.max_energy_production

			if entity.burner_prototype ~= null then
				tentity['fuel_type'] = 'item'
				tentity['fuel_effectivity'] = entity.burner_prototype.effectivity
				tentity['pollution'] = entity.burner_prototype.emissions

				tentity['fuel_categories'] = {}
				for fname, _ in pairs(entity.burner_prototype.fuel_categories) do
					table.insert(tentity['fuel_categories'], fname)
				end

			elseif entity.fluid_energy_source_prototype then
				tentity['fuel_type'] = 'fluid'
				tentity['fuel_effectivity'] = entity.fluid_energy_source_prototype.effectivity
				tentity['pollution'] = entity.fluid_energy_source_prototype.emissions
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
				tentity['drain'] = entity.electric_energy_source_prototype.drain
				tentity['pollution'] = entity.electric_energy_source_prototype.emissions

			elseif entity.heat_energy_source_prototype  then
				tentity['fuel_type'] = 'heat'
				tentity['fuel_effectivity'] = 1
				tentity['pollution'] = entity.heat_energy_source_prototype.emissions

			elseif entity.void_energy_source_prototype  then
				tentity['fuel_type'] = 'void'
				tentity['fuel_effectivity'] = 1
				tentity['pollution'] = entity.void_energy_source_prototype.emissions
			else
				tentity['fuel_type'] = 'void'
				tentity['fuel_effectivity'] = 1
				tentity['pollution'] = 0
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
	for _, resource in pairs(game.entity_prototypes) do
		if resource.resource_category ~= nil and resource.mineable_properties.products ~= nil then
			tresource = {}
			tresource['name'] = resource.name
			tresource['resource_category'] = resource.resource_category
			tresource['mining_time'] = resource.mineable_properties.mining_time
			if resource.mineable_properties.required_fluid then
				tresource['required_fluid'] = resource.mineable_properties.required_fluid
				tresource['fluid_amount'] = resource.mineable_properties.fluid_amount
			end
			tresource['name'] = resource.name

			tresource['products'] = {}
			for _, product in pairs(resource.mineable_properties.products) do
				tproduct = {}
				tproduct['name'] = product.name
				tproduct['type'] = product.type

				amount = (product.amount == nil) and ((product.amount_max + product.amount_min)/2) or product.amount
				amount = amount * ( (product.probability == nil) and 1 or product.probability)
				tproduct['amount'] = amount

				if product.type == 'fluid' and product.temperate ~= nil then
					tproduct['temperature'] = ProcessTemperature(product.temperature)
				end
				table.insert(tresource['products'], tproduct)
			end

			tresource['lid'] = '$'..localindex
			ExportLocalisedString(resource.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tresources, tresource)
		end
	end
	etable['resources'] = tresources
end

local function ExportGroups()
	tgroups = {}
	for _, group in pairs(game.item_group_prototypes) do
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
	for _, sgroup in pairs(game.item_subgroup_prototypes) do
		tsgroup = {}
		tsgroup['name'] = sgroup.name
		tsgroup['order'] = sgroup.order

		table.insert(tsgroups, tsgroup)
	end
	etable['subgroups'] = tsgroups
end

script.on_nth_tick(1, 	
	function()

		game.difficulty_settings.recipe_difficulty = defines.difficulty_settings.recipe_difficulty.expensive
		game.difficulty_settings.technology_difficulty = defines.difficulty_settings.technology_difficulty.normal
		etable['difficulty'] = {1,0}

		localised_print('<<<START-EXPORT-LN>>>')

		ExportModList()
		ExportResearch()
		ExportRecipes()
		ExportItems()
		ExportFluids()
		ExportModules()
		ExportEntities()
		ExportResources()
		ExportGroups()
		ExportSubGroups()

		localised_print('<<<END-EXPORT-LN>>>')

		localised_print('<<<START-EXPORT-P2>>>')
		localised_print(game.table_to_json(etable))
		localised_print('<<<END-EXPORT-P2>>>')

		ENDEXPORTANDJUSTDIE() -- just the most safe way of ensuring that we export once and quit. Basically... there is no ENDEXPORTANDJUSTDIE function. so lua will throw an expection and the run will end here.
	end
)