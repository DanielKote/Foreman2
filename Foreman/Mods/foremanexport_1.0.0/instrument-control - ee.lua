local FileBuffer = ''
local IncludeBarrelsAndCrates = false
local DebugLog = false
local ExportSetup = true
		
local BlockedTech = {}

--these will once filled up contain all available X with key=name & value=prototype
local AvailableTech = {}
local AvailableRecipes = {}
local AvailableItems = {}
local AvailableFluids = {}
local AvailableModules = {}
local AvailableAMachines = {}
local AvailableBeacons = {}
local AvailableMiners = {}
local AvailableResources = {}
local AvailableOffshorePumps = {}

local etable = {}

local function LOG(text)
	if DebugLog then
		log(text)
	end
end

--this function attempts to recursively test the given tech (based on name)
--if the tech is already checked as researchable	-> return true
--if the tech is already checked as blocked			-> return false
--if the tech doesnt exist in tech prototypes		-> add to blocked tech, return false
--if the tech is enabled by default					-> add to available tech, return true
--if the tech is hidden								-> add to blocked tech, return false
--otherwise it calls itself on each prerequisite, and compiles a net AND
--if each prerequisite returns true					-> add to available tech, return true
--otherwise (any prerequisite returns false)		-> add to blocked tech, return false
local function attemptTechResearch(techName)
	if AvailableTech[techName] then
		return true
	elseif BlockedTech[techName] then
		return false
	elseif not game.technology_prototypes[techName] then
		BlockedTech[techName] = game.technology_prototypes[techName]
		return false
	elseif game.technology_prototypes[techName].hidden then
		BlockedTech[techName] = game.technology_prototypes[techName]
		return false
	elseif game.technology_prototypes[techName].enabled then
		AvailableTech[techName] = game.technology_prototypes[techName]
		return true
	end
	
	allPTAvailable = true
	for _, pTech in pairs(game.technology_prototypes[techName].prerequisites) do
		allPTAvailable = allPTAvailable and attemptTechResearch(pTech)
	end
	
	if allPTAvailable then
		AvailableTech[techName] = game.technology_prototypes[techName]
		return true
	else
		BlockedTech[techName] = game.technology_prototypes[techName]
		return false
	end
end

--this function will take in a result or a product and process it -> item or fluid
local function ProcessIF(resOrProd)
	if resOrProd.type == 'item' then
		AvailableItems[resOrProd.name] = game.item_prototypes[resOrProd.name]
	elseif resOrProd.type == 'fluid' then
		AvailableFluids[resOrProd.name] = game.fluid_prototypes[resOrProd.name]
	end
end

--prepares the 'available' tables
local function ProcessPrototypes()
	--1.1.0 start off by populating the available and blocked techs
	for name, tech in pairs(game.technology_prototypes) do
		attemptTechResearch(name)
	end
	LOG('-------------------------AVAILABLE TECH----------------------')
	for name, tech in pairs(AvailableTech) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED TECH----------------------')
	for name, tech in pairs(BlockedTech) do
		LOG(name)
	end
	
	--1.2.0 populate the available recipes (only those that are either open at start or can be unlocked with available tech tree)

	--1.2.1 start with adding all recipes, with only the ones enabled at start being 'true' (or those that dont have an enabled value)
	for name, recipe in pairs(game.recipe_prototypes) do
		if recipe.enabled then
			AvailableRecipes[name] = game.recipe_prototypes[name]
		end
	end

	--1.2.2 then add all recipes that can be unlocked through the available technologies
	for name , tech in pairs(AvailableTech) do
		if(tech.effects) then
			for _, effect in ipairs(tech.effects) do
				if(effect.type == 'unlock-recipe' and game.recipe_prototypes[effect.recipe]) then
					AvailableRecipes[effect.recipe] = game.recipe_prototypes[effect.recipe]
				end
			end
		end
	end
	
	--1.2.3 remove any barrels / crates
	LOG('-------------------------REMOVED BARRELING & CRATING RECIPES----------------------')
	if not IncludeBarrelsAndCrates then
		for name, _ in pairs(AvailableRecipes) do
			if name ~= 'empty-barrel' then
				if (string.match(name, '-barrel$') or string.match(name, '^deadlock-')) then
					LOG(name)
					AvailableRecipes[name] = nil
				end
			end
		end
	end

	LOG('-------------------------AVAILABLE RECIPES----------------------')
	for name, _ in pairs(AvailableRecipes) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED RECIPES----------------------')
	for name, _ in pairs(game.recipe_prototypes) do
		if not AvailableRecipes[name] then
			LOG(name)
		end
	end

	--1.3.1: process all available recipes, taking their ingredients and results, and passing each through the ProcessIF function to get all the items & fluids that are in use via the accessible recipes.
	--note that this means that if some item is never used in a recipe or is produced via recipe (ex:fish?), it will not be available
	for name, _ in pairs(AvailableRecipes) do
		for _ , ingredient in ipairs(game.recipe_prototypes[name].ingredients) do
			ProcessIF(ingredient)
		end
		for _, result in pairs(game.recipe_prototypes[name].products) do
			ProcessIF(result)
		end
	end
	
	LOG('-------------------------AVAILABLE ITEMS----------------------')
	for name, _ in pairs(AvailableItems) do
		LOG(name)
	end
	LOG('-------------------------AVAILABLE FLUIDS----------------------')
	for name, _ in pairs(AvailableFluids) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED ITEMS----------------------')
	for name, _ in pairs(game.item_prototypes) do
		if not AvailableItems[name] then
			LOG(name)
		end
	end
	LOG('-------------------------BLOCKED FLUIDS----------------------')
	for name, _ in pairs(game.fluid_prototypes) do
		if not AvailableFluids[name] then
			LOG(name)
		end
	end
	
	--1.4.0: process available modules
	for name, item in pairs(game.item_prototypes) do
		if AvailableItems[name] and item.module_effects then
			AvailableModules[name] = item
		end
	end

	LOG('-------------------------AVAILABLE MODULES----------------------')
	for name, _ in pairs(AvailableModules) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED MODULES----------------------')
	for name, item in pairs(game.item_prototypes) do
		if not AvailableModules[name] and item.module_effects then
			LOG(name)
		end
	end

	--1.5.0: process available assembly/crafting machines
	for name, entity in pairs(game.entity_prototypes) do
		if AvailableItems[name] and (entity.ingredient_count ~= nil) then
			AvailableAMachines[name] = entity
		end
	end

	LOG('-------------------------AVAILABLE ASSEMBLY MACHINES----------------------')
	for name, _ in pairs(AvailableAMachines) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED ASSEMBLY MACHINES----------------------')
	for name, entity in pairs(game.entity_prototypes) do
		if (entity.ingredient_count ~= nil ) and (not AvailableAMachines[name]) then
			LOG(name)
		end
	end

	--1.6.0: process available beacons
	for name, entity in pairs(game.entity_prototypes) do
		if AvailableItems[name] and (entity.distribution_effectivity ~= nil) then
			AvailableBeacons[name] = entity
		end
	end

	LOG('-------------------------AVAILABLE BEACONS----------------------')
	for name, _ in pairs(AvailableBeacons) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED BEACONS----------------------')
	for name, entity in pairs(game.entity_prototypes) do
		if (entity.ingredient_count ~= nil ) and (not AvailableBeacons[name]) then
			LOG(name)
		end
	end

	--1.7.0: process available raw miners
	for name, entity in pairs(game.entity_prototypes) do
		if AvailableItems[name] and entity.resource_categories then
			AvailableMiners[name] = entity
		end
	end

	LOG('-------------------------AVAILABLE RAW MINERS----------------------')
	for name, _ in pairs(AvailableMiners) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED RAW MINERS----------------------')
	for name, entity in pairs(game.entity_prototypes) do
		if (entity.resource_categories) and (not AvailableMiners[name]) then
			LOG(name)
		end
	end
	
	--1.8.0: process available(all) resources : these are the items/fluids that can be mined
	for name, entity in pairs(game.entity_prototypes) do
		if entity.resource_category then
			AvailableResources[name] = entity
		end
	end
	LOG('-------------------------AVAILABLE RAW RESOURCES----------------------')
	for name, _ in pairs(AvailableResources) do
		LOG(name)
	end

	--1.9.0: process available offshore pumps (will be placed as miners)
	for name, entity in pairs(game.entity_prototypes) do
		if AvailableItems[name] and entity.fluid then
			AvailableOffshorePumps[name] = entity
		end
	end

	LOG('-------------------------AVAILABLE RAW OFFSHORE PUMPS----------------------')
	for name, _ in pairs(AvailableOffshorePumps) do
		LOG(name)
	end
	LOG('-------------------------BLOCKED RAW OFFSHORE PUMPS----------------------')
	for name, entity in pairs(game.entity_prototypes) do
		if (entity.fluid) and (not AvailableOffshorePumps[name]) then
			LOG(name)
		end
	end

	--1.10: just list all the groups and subgroups for debugging if necessary
	LOG('-------------------------GROUPS----------------------')
	for name, _ in pairs(game.item_group_prototypes) do
		LOG(name)
	end
	LOG('-------------------------SUBGROUPS----------------------')
	for name, _ in pairs(game.item_subgroup_prototypes) do
		LOG(name)
	end
end

localindex = 0
local function ExportLocalisedString(lstring, index)
	-- as could be expected if lstring doesnt have a working translation then we get a beauty of a mess... so that needs to be cleaned up outside of json table
	localised_print('<#~#>')
	localised_print(lstring)
	localised_print('<#~#>')
end

local function ExportMyBurnerSourcePrototype(entity, ttable)
	ttable["max_energy_usage"] = entity.max_energy_usage
	if entity.burner_prototype ~= null then
		ttable['fuel_type'] = 'item'
		ttable['fuel_effectivity'] = entity.burner_prototype.effectivity

		ttable['fuel_categories'] = {}
		for fname, _ in pairs(entity.burner_prototype.fuel_categories) do
			table.insert(ttable['fuel_categories'], fname)
		end
	elseif entity.fluid_energy_source_prototype then
		ttable['fuel_type'] = 'fluid'
		ttable['fuel_effectivity'] = entity.fluid_energy_source_prototype.effectivity
		ttable['burns_fluid'] = entity.fluid_energy_source_prototype.burns_fluid
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
	for _, tech in pairs(AvailableTech) do
		ttech = {}
		ttech['name'] = tech.name
		ttech['icon_name'] = 'icon.t.'..tech.name
		ttech['enabled'] = tech.enabled
		
		ttech['prerequisites'] = {}
		for pname, _ in pairs(tech.prerequisites) do
			table.insert(ttech['prerequisites'], pname)
		end
		
		ttech['recipes'] = {}
		for _, effect in pairs(tech.effects) do
			if(effect.type == 'unlock-recipe' and AvailableRecipes[effect.recipe]) then
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
	for _, recipe in pairs(AvailableRecipes) do
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
	for _, item in pairs(AvailableItems) do
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
	for _, fluid in pairs(AvailableFluids) do
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
	for _, module in pairs(AvailableModules) do
		tmodule = {}
		tmodule['name'] = module.name
		tmodule['icon_name'] = 'icon.i.'..module.name
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
	etable['modules'] = tmodules
end

local function ExportCraftingMachines()
	tassemblers = {}
	for _, machine in pairs(AvailableAMachines) do
		tmachine = {}
		tmachine['name'] = machine.name
		tmachine['icon_name'] = 'icon.i.'..machine.name
		tmachine['order'] = machine.order
		tmachine['crafting_speed'] = machine.crafting_speed
		tmachine['base_productivity'] = machine.base_productivity
		tmachine['module_inventory_size'] =  (machine.module_inventory_size ~= nil) and machine.module_inventory_size or 0

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

		ExportMyBurnerSourcePrototype(machine, tmachine)

		tmachine['lid'] = '$'..localindex
		ExportLocalisedString(machine.localised_name, localindex)
		localindex = localindex + 1

		table.insert(tassemblers, tmachine)
	end
	etable['assemblers'] = tassemblers
end

local function ExportBeacons()
	tbeacons = {}
	for _, beacon in pairs(AvailableBeacons) do
		tbeacon = {}
		tbeacon['name'] = beacon.name
		tbeacon['icon_name'] = 'icon.i.'..beacon.name
		tbeacon['order'] = beacon.order
		tbeacon['module_inventory_size'] = (beacon.module_inventory_size ~= nil) and beacon.module_inventory_size or 0
		tbeacon['distribution_effectivity'] = beacon.distribution_effectivity

		tbeacon['allowed_effects'] = {}
		if beacon.allowed_effects then
			for effect, allowed in pairs(beacon.allowed_effects) do
				if allowed then
					table.insert(tbeacon['allowed_effects'], effect)
				end
			end
		end

		
		tbeacon['lid'] = '$'..localindex
		ExportLocalisedString(beacon.localised_name, localindex)
		localindex = localindex + 1

		table.insert(tbeacons, tbeacon)
	end
	etable['beacons'] = tbeacons
end

local function ExportMiners()
	tminers = {}
	for _, miner in pairs(AvailableMiners) do
		tminer = {}
		tminer['name'] = miner.name
		tminer['icon_name'] = 'icon.i.'..miner.name
		tminer['order'] = miner.order
		tminer['mining_speed'] = miner.mining_speed
		tminer['base_productivity'] = miner.base_productivity
		tminer['module_inventory_size'] = (miner.module_inventory_size ~= nil) and miner.module_inventory_size or 0

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


		ExportMyBurnerSourcePrototype(miner, tminer)

		tminer['lid'] = '$'..localindex
		ExportLocalisedString(miner.localised_name, localindex)
		localindex = localindex + 1

		table.insert(tminers, tminer)
	end
	etable['miners'] = tminers
end

local function ExportResources()
	tresources = {}
	for _, resource in pairs(AvailableResources) do
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
	etable['resources'] = tresources
end


local function ExportOffshorePumps()
	topumps = {}
	for _, opump in pairs(AvailableOffshorePumps) do
		topump = {}
		topump['name'] = opump.name
		topump['icon_name'] = 'icon.i.'..opump.name
		topump['order'] = opump.order
		topump['pumping_speed'] = opump.pumping_speed
		topump['fluid'] = opump.fluid.name
		topump['module_inventory_size'] = (opump.module_inventory_size ~= nil) and opump.module_inventory_size or 0

		topump['allowed_effects'] = {}
		if opump.allowed_effects then
			for effect, allowed in pairs(opump.allowed_effects) do
				if allowed then
					table.insert(topump['allowed_effects'], effect)
				end
			end
		end


		ExportMyBurnerSourcePrototype(opump, topump)

		topump['lid'] = '$'..localindex
		ExportLocalisedString(opump.localised_name, localindex)
		localindex = localindex + 1

		table.insert(topumps, topump)
	end
	etable['offshorepumps'] = topumps
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
		game.difficulty_settings.technology_difficulty = defines.difficulty_settings.technology_difficulty.expensive
		etable['difficulty'] = {1,1}

		ProcessPrototypes()	

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