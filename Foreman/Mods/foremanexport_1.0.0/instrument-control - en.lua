local etable = {}

localindex = 0
local function ExportLocalisedString(lstring, index)
	-- as could be expected if lstring doesnt have a working translation then we get a beauty of a mess... so that needs to be cleaned up outside of json table
	localised_print('<#~#>')
	localised_print(lstring)
	localised_print('<#~#>')
end

local function ExportEnergySource(entity, ttable)
	ttable["min_energy_usage"] = (entity.energy_usage == nil) and 0 or entity.energy_usage
	ttable["max_energy_usage"] = entity.max_energy_usage

	if entity.burner_prototype ~= null then
		ttable['fuel_type'] = 'item'
		ttable['fuel_effectivity'] = entity.burner_prototype.effectivity
		ttable['pollution'] = entity.burner_prototype.emissions

		ttable['fuel_categories'] = {}
		for fname, _ in pairs(entity.burner_prototype.fuel_categories) do
			table.insert(ttable['fuel_categories'], fname)
		end
	elseif entity.fluid_energy_source_prototype then
		ttable['fuel_type'] = 'fluid'
		ttable['fuel_effectivity'] = entity.fluid_energy_source_prototype.effectivity
		ttable['pollution'] = entity.fluid_energy_source_prototype.emissions

		ttable['burns_fluid'] = entity.fluid_energy_source_prototype.burns_fluid
	elseif entity.electric_energy_source_prototype then
		ttable['fuel_type'] = 'electricity'
		ttable['fuel_effectivity'] = 1
		ttable['drain'] = entity.electric_energy_source_prototype.drain
		ttable['pollution'] = entity.electric_energy_source_prototype.emissions
	elseif entity.heat_energy_source_prototype  then
		ttable['fuel_type'] = 'heat'
		ttable['fuel_effectivity'] = 1
		ttable['pollution'] = entity.heat_energy_source_prototype.emissions
	elseif entity.void_energy_source_prototype  then
		ttable['fuel_type'] = 'void'
		ttable['fuel_effectivity'] = 1
		ttable['pollution'] = entity.void_energy_source_prototype.emissions
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
				tingredient['minimum_temperature'] = ingredient.minimum_temperature
			end
			if ingredient.type == 'fluid' and ingredient.maximum_temperature ~= nil then
				tingredient['maximum_temperature'] = ingredient.maximum_temperature
			end
			table.insert(trecipe['ingredients'], tingredient)
		end

		trecipe['products'] = {}
		for _, product in pairs(recipe.products) do
			tproduct = {}
			tproduct['name'] = product.name
			tproduct['type'] = product.type

			amount = (product.amount == nil) and ((product.amount_max + product.amount_min)/2) or product.amount
			amount = amount * ( (product.probability == nil) and 1 or product.probability)

			tproduct['amount'] = amount

			if product.type == 'fluid' and product.temperate ~= nil then
				tproduct['temperature'] = product.temperature
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

		if item.fuel_category ~= nil then
			titem['fuel_category'] = item.fuel_category
			titem['fuel_value'] = item.fuel_value
		end

		if item.burnt_result ~= nil then
			titem['burnt_result'] = item.burnt_result.name
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
		tfluid['default_temperature'] = fluid.default_temperature
		tfluid['max_temperature'] = fluid.max_temperature

		if fluid.fuel_value ~= nil then
			tfluid['fuel_value'] = fluid.fuel_value
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

local function ExportCraftingMachines()
	tassemblers = {}
	for _, machine in pairs(game.entity_prototypes) do
		if machine.ingredient_count ~= nil then
			tmachine = {}
			tmachine['name'] = machine.name
			tmachine['icon_name'] = 'icon.e.'..machine.name
			tmachine["icon_alt_name"] = 'icon.i.'..machine.name
			tmachine['order'] = machine.order
			tmachine['crafting_speed'] = machine.crafting_speed
			tmachine['base_productivity'] = machine.base_productivity
			tmachine['module_inventory_size'] =  (machine.module_inventory_size ~= nil) and machine.module_inventory_size or 0

			tmachine['associated_items'] = {}
			if machine.items_to_place_this ~= nil then
				for _, item in pairs(machine.items_to_place_this) do
					if(type(item) == 'string') then
						table.insert(tmachine['associated_items'], item)
					else
						table.insert(tmachine['associated_items'], item['name'])
					end
				end
			end

			tmachine['crafting_categories'] = {}
			for category, _ in pairs(machine.crafting_categories) do
				table.insert(tmachine['crafting_categories'], category)
			end

			tmachine['allowed_effects'] = {}
			if machine.allowed_effects then
				for effect, allowed in pairs(machine.allowed_effects) do
					if allowed then
						table.insert(tmachine['allowed_effects'], effect)
					end
				end
			end

			ExportEnergySource(machine, tmachine)

			tmachine['lid'] = '$'..localindex
			ExportLocalisedString(machine.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tassemblers, tmachine)
		end
	end
	etable['assemblers'] = tassemblers
end

local function ExportBeacons()
	tbeacons = {}
	for _, beacon in pairs(game.entity_prototypes) do
		if beacon.distribution_effectivity ~= nil then
			tbeacon = {}
			tbeacon['name'] = beacon.name
			tbeacon['icon_name'] = 'icon.e.'..beacon.name
			tbeacon["icon_alt_name"] = 'icon.i.'..beacon.name
			tbeacon['order'] = beacon.order
			tbeacon['module_inventory_size'] = (beacon.module_inventory_size ~= nil) and beacon.module_inventory_size or 0
			tbeacon['distribution_effectivity'] = beacon.distribution_effectivity

			tbeacon['associated_items'] = {}
			if beacon.items_to_place_this ~= nil then
				for _, item in pairs(beacon.items_to_place_this) do
					if(type(item) == 'string') then
						table.insert(tbeacon['associated_items'], item)
					else
						table.insert(tbeacon['associated_items'], item['name'])
					end
				end
			end

			tbeacon['allowed_effects'] = {}
			if beacon.allowed_effects then
				for effect, allowed in pairs(beacon.allowed_effects) do
					if allowed then
						table.insert(tbeacon['allowed_effects'], effect)
					end
				end
			end

			ExportEnergySource(beacon, tbeacon)
		
			tbeacon['lid'] = '$'..localindex
			ExportLocalisedString(beacon.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tbeacons, tbeacon)
		end
	end
	etable['beacons'] = tbeacons
end

local function ExportMiners()
	tminers = {}
	for _, miner in pairs(game.entity_prototypes) do
		if miner.resource_categories ~= nil then
			tminer = {}
			tminer['name'] = miner.name
			tminer['icon_name'] = 'icon.e.'..miner.name
			tminer['icon_alt_name'] = 'icon.i.'..miner.name
			tminer['order'] = miner.order
			tminer['mining_speed'] = miner.mining_speed
			tminer['base_productivity'] = miner.base_productivity
			tminer['module_inventory_size'] = (miner.module_inventory_size ~= nil) and miner.module_inventory_size or 0

			tminer['associated_items'] = {}
			if miner.items_to_place_this ~= nil then
				for _, item in pairs(miner.items_to_place_this) do
					if(type(item) == 'string') then
						table.insert(tminer['associated_items'], item)
					else
						table.insert(tminer['associated_items'], item['name'])
					end
				end
			end

			tminer['resource_categories'] = {}
			for category, _ in pairs(miner.resource_categories) do
				table.insert(tminer['resource_categories'], category)
			end

			tminer['allowed_effects'] = {}
			if miner.allowed_effects then
				for effect, allowed in pairs(miner.allowed_effects) do
					if allowed then
						table.insert(tminer['allowed_effects'], effect)
					end
				end
			end

			ExportEnergySource(miner, tminer)

			tminer['lid'] = '$'..localindex
			ExportLocalisedString(miner.localised_name, localindex)
			localindex = localindex + 1

			table.insert(tminers, tminer)
		end
	end
	etable['miners'] = tminers
end

local function ExportResources()
	tresources = {}
	for _, resource in pairs(game.entity_prototypes) do
		if resource.resource_category ~= nil then
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
					tproduct['temperature'] = product.temperature
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


local function ExportOffshorePumps()
	topumps = {}
	for _, opump in pairs(game.entity_prototypes) do
		if opump.fluid ~= nil then
			topump = {}
			topump['name'] = opump.name
			topump['icon_name'] = 'icon.e.'..opump.name
			topump['icon_alt_name'] = 'icon.i.'..opump.name
			topump['order'] = opump.order
			topump['pumping_speed'] = opump.pumping_speed
			topump['fluid'] = opump.fluid.name
			topump['module_inventory_size'] = (opump.module_inventory_size ~= nil) and opump.module_inventory_size or 0

			topump['associated_items'] = {}
			if opump.items_to_place_this ~= nil then
				for _, item in pairs(opump.items_to_place_this) do
					if(type(item) == 'string') then
						table.insert(topump['associated_items'], item)
					else
						table.insert(topump['associated_items'], item['name'])
					end
				end
			end

			topump['allowed_effects'] = {}
			if opump.allowed_effects then
				for effect, allowed in pairs(opump.allowed_effects) do
					if allowed then
						table.insert(topump['allowed_effects'], effect)
					end
				end
			end

			ExportEnergySource(opump, topump)

			topump['lid'] = '$'..localindex
			ExportLocalisedString(opump.localised_name, localindex)
			localindex = localindex + 1

			table.insert(topumps, topump)
		end
	end
	etable['offshorepumps'] = topumps
end

local function ExportHeatUsers()
	theatusers = {}
	for _, huser in pairs(game.entity_prototypes) do
		if huser.heat_energy_source_prototype  ~= nil then
			thuser = {}
			thuser['name'] = huser.name
			thuser['icon_name'] = 'icon.e.'..huser.name
			thuser['icon_alt_name'] = 'icon.i.'..huser.name
			thuser['order'] = huser.order

			thuser['associated_items'] = {}
			if huser.items_to_place_this ~= nil then
				for _, item in pairs(huser.items_to_place_this) do
					if(type(item) == 'string') then
						table.insert(thuser['associated_items'], item)
					else
						table.insert(thuser['associated_items'], item['name'])
					end
				end
			end

			thuser['allowed_effects'] = {}
			if huser.allowed_effects then
				for effect, allowed in pairs(huser.allowed_effects) do
					if allowed then
						table.insert(thuser['allowed_effects'], effect)
					end
				end
			end

			ExportEnergySource(huser, thuser)

			thuser['lid'] = '$'..localindex
			ExportLocalisedString(huser.localised_name, localindex)
			localindex = localindex + 1

			table.insert(theatusers, thuser)
		end
	end
	etable['heatusers'] = theatusers
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
		ExportCraftingMachines()
		ExportBeacons()
		ExportMiners()
		ExportHeatUsers()
		ExportResources()
		ExportOffshorePumps()
		ExportGroups()
		ExportSubGroups()

		localised_print('<<<END-EXPORT-LN>>>')

		localised_print('<<<START-EXPORT-P2>>>')
		localised_print(game.table_to_json(etable))
		localised_print('<<<END-EXPORT-P2>>>')

		ENDEXPORTANDJUSTDIE() -- just the most safe way of ensuring that we export once and quit. Basically... there is no ENDEXPORTANDJUSTDIE function. so lua will throw an expection and the run will end here.
	end
)