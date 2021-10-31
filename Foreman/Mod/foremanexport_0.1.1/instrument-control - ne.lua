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
local AvailableMiners = {}
local AvailableResources = {}

--these are the name -> icon index dictionaries. an object with a given name will ask the correct dictionary for the icon index, which will then give him the actual icon information from the IconCollection dictionary
local IconsTech = {}
local IconsRecipes = {}
local IconsItems = {}
local IconsFluids = {}
--local IconsModules = {} --modules go into items
local IconsAMachines = {}
local IconsMiners = {}
local IconsGroups = {}

local IconIndex = 0
local IconCollection = {} --this is where the icon data is actually stored

--translations for localization names & descriptions.
local Translations = {}
--counter for translations - we have to first queue up the translation requests then wait for them to finish. Once counters fall back to 0, we know we are done
local TranslationCounter = 0

--just for debug
local FailedIcons = {}

local function LOG(text)
	if DebugLog then
		log(text)
	end
end

local function ExportLine(text, indentSize)
	if ExportSetup then
		indents = ''
		while indentSize > 0 do
			indentSize = indentSize - 1
			indents = indents..'	'
		end
		localised_print(indents..text)
	end
end

local function ExportParameter(paramName, paramValue, numerical, suffix, indentSize)
	if numerical then
		ExportLine('"'..paramName..'": '..paramValue..suffix, indentSize)
	else
		ExportLine('"'..paramName..'": "'..paramValue..'"'..suffix, indentSize)
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

	--1.6.0: process available raw miners
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
	
	--1.7.0: process available(all) resources : these are the items/fluids that can be mined
	for name, entity in pairs(game.entity_prototypes) do
		if entity.resource_category then
			AvailableResources[name] = entity
		end
	end
	LOG('-------------------------AVAILABLE RAW RESOURCES----------------------')
	for name, _ in pairs(AvailableResources) do
		LOG(name)
	end
	
	--1.8: just list all the groups and subgroups for debugging if necessary
	LOG('-------------------------GROUPS----------------------')
	for name, _ in pairs(game.item_group_prototypes) do
		LOG(name)
	end
	LOG('-------------------------SUBGROUPS----------------------')
	for name, _ in pairs(game.item_subgroup_prototypes) do
		LOG(name)
	end
end


local function ExportModList()
	ExportLine('"mods": [', 1)
	
	ExportLine('{', 2)
	ExportParameter('name', 'core', false, ',', 3)
	ExportParameter('version', '1.0', false, '', 3)
	ExportLine('},', 2)
	
	counter = 0
	for _, _ in pairs(game.active_mods) do
		counter = counter + 1
	end
	
	for name, version in pairs(game.active_mods) do
		ExportLine('{', 2)
	
		ExportParameter('name', name, false, ',', 3)
		ExportParameter('version', version, false, '', 3)
	
		counter = counter - 1
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportResearch()
	ExportLine('"technologies": [', 1)
	counter = 0
	for _, _ in pairs(AvailableTech) do
		counter = counter + 1
	end
	for name, tech in pairs(AvailableTech) do
		ExportLine('{', 2)
		
		ExportParameter('name', tech.name, false, ',', 3)
		ExportParameter('enabled', tech.enabled and 'true' or 'false', true, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(tech.localised_name)
		ExportLine('",', 0)
			
		ExportLine('"prerequisites": [', 3)
		pcounter = 0
		for _, _ in pairs(tech.prerequisites) do
			pcounter = pcounter + 1
		end
		for pname, _ in pairs(tech.prerequisites) do
			pcounter = pcounter - 1
			if pcounter > 0 then
				ExportLine('"'..pname..'",', 4)
			else
				ExportLine('"'..pname..'"', 4)
			end
		end
		ExportLine('],',  3)
		
		ExportLine('"recipes": [', 3)
		pcounter = 0
		for _, effect in pairs(tech.effects) do
			if(effect.type == 'unlock-recipe' and AvailableRecipes[effect.recipe]) then
				pcounter = pcounter + 1
			end
		end
		for _, effect in pairs(tech.effects) do
			if(effect.type == 'unlock-recipe' and AvailableRecipes[effect.recipe]) then
				pcounter = pcounter - 1
				if pcounter > 0 then
					ExportLine('"'..effect.recipe..'",', 4)
				else
					ExportLine('"'..effect.recipe..'"', 4)
				end
			end
		end
		ExportLine(']',  3)

		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportRecipes()
	ExportLine('"recipes": [', 1)
	counter = 0
	for _, _ in pairs(AvailableRecipes) do
		counter = counter + 1
	end
	for name, recipe in pairs(AvailableRecipes) do
		ExportLine('{', 2)
		
		ExportParameter('name', recipe.name, false, ',', 3)
		ExportParameter('enabled', recipe.enabled and 'true' or 'false', true, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(recipe.localised_name)
		ExportLine('",', 0)
		
		ExportParameter('category', recipe.category, false, ',', 3)		
		ExportParameter('energy', recipe.energy, true, ',', 3)
		
		ExportLine('"ingredients": [', 3)
		pcounter = 0
		for _, _ in pairs(recipe.ingredients) do
			pcounter = pcounter + 1
		end
		for _, ingredient in pairs(recipe.ingredients) do
			pcounter = pcounter - 1
			ExportLine('{', 4)
			ExportParameter('name', ingredient.name, false, ',', 5)
			ExportParameter('type', ingredient.type, false, ',', 5)
			if ingredient.type == 'fluid' then
				ExportParameter('amount', ingredient.amount, true, ',', 5)
				ExportParameter('minimum_temperature', (ingredient.minimum_temperature  == nil) and 'null' or ingredient.minimum_temperature , true, ',', 5)
				ExportParameter('maximum_temperature', (ingredient.maximum_temperature  == nil) and 'null' or ingredient.maximum_temperature , true, '', 5)
			else
				ExportParameter('amount', ingredient.amount, true, '', 5)
			end

			if pcounter > 0 then
				ExportLine('},', 4)
			else
				ExportLine('}', 4)
			end
		end
		ExportLine('],',  3)
		
		ExportLine('"products": [', 3)
		pcounter = 0
		for _, _ in pairs(recipe.products) do
			pcounter = pcounter + 1
		end
		for _, product in pairs(recipe.products) do
			pcounter = pcounter - 1
			ExportLine('{', 4)
			ExportParameter('name', product.name, false, ',', 5)
			ExportParameter('type', product.type, false, ',', 5)
			
			amount = (product.amount == nil) and ((product.amount_max + product.amount_min)/2) or product.amount
			amount = amount * ( (product.probability == nil) and 1 or product.probability)
			if product.type == 'fluid' then
				ExportParameter('amount', amount, true, ',', 5)
				ExportParameter('temperature', (product.temperature == nil) and 'null' or product.temperature, true, '', 5)
			else
				ExportParameter('amount', amount, true, '', 5)
			end

			if pcounter > 0 then
				ExportLine('},', 4)
			else
				ExportLine('}', 4)
			end
		end
		ExportLine('],',  3)

		ExportParameter('order', recipe.order, false, ',', 3)
		ExportParameter('subgroup', recipe.subgroup.name, false, '', 3)
		
		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end


local function ExportItems()
	ExportLine('"items": [', 1)
	counter = 0
	for _, _ in pairs(AvailableItems) do
		counter = counter + 1
	end
	for name, item in pairs(AvailableItems) do
		ExportLine('{', 2)
		
		ExportParameter('name', item.name, false, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(item.localised_name)
		ExportLine('",', 0)
				
		if item.fuel_category ~= nil then
			ExportParameter('fuel_category', item.fuel_category, false, ',', 3)
			ExportParameter('fuel_value', item.fuel_value, false, ',', 3)
		end

		ExportParameter('order', item.order, false, ',', 3)
		ExportParameter('subgroup', item.subgroup.name, false, '', 3)
		
		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportFluids()
	ExportLine('"fluids": [', 1)
	counter = 0
	for _, _ in pairs(AvailableFluids) do
		counter = counter + 1
	end
	for name, fluid in pairs(AvailableFluids) do
		ExportLine('{', 2)
		
		ExportParameter('name', fluid.name, false, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(fluid.localised_name)
		ExportLine('",', 0)
		
		ExportParameter('default_temperature', fluid.default_temperature, true, ',', 3)
		ExportParameter('max_temperature', fluid.max_temperature, true, ',', 3)
		
		if fluid.fuel_value ~= nil then
			ExportParameter('fuel_value', fluid.fuel_value, false, ',', 3)
		end

		ExportParameter('order', fluid.order, false, ',', 3)
		ExportParameter('subgroup', fluid.subgroup.name, false, '', 3)

		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportModules()
	ExportLine('"modules": [', 1)
	counter = 0
	for _, _ in pairs(AvailableModules) do
		counter = counter + 1
	end
	for name, module in pairs(AvailableModules) do
		ExportLine('{', 2)
		
		ExportParameter('name', module.name, false, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(module.localised_name)
		ExportLine('",', 0)
		
		ExportParameter('category', module.category, false, ',', 3)
		ExportParameter('tier', module.tier, true, ',', 3)
		
		ExportParameter('module_effects_consumption', (module.module_effects.consumption == nil) and 0 or module.module_effects.consumption.bonus, true, ',', 3)
		ExportParameter('module_effects_speed', (module.module_effects.speed == nil) and 0 or module.module_effects.speed.bonus, true, ',', 3)
		ExportParameter('module_effects_productivity', (module.module_effects.productivity == nil) and 0 or module.module_effects.productivity.bonus, true, ',', 3)
		ExportParameter('module_effects_pollution', (module.module_effects.pollution == nil) and 0 or module.module_effects.pollution.bonus, true, ',', 3)
		
		ExportLine('"limitations": [', 3)
		pcounter = 0
		for _, _ in pairs(module.limitations) do
			pcounter = pcounter + 1
		end
		for _, recipe in pairs(module.limitations) do
			pcounter = pcounter - 1
			if pcounter > 0 then
				ExportLine('"'..recipe..'",', 4)
			else
				ExportLine('"'..recipe..'"', 4)
			end
		end
		ExportLine('],', 3)

		ExportParameter('order', module.order, false, '', 3)

		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportCraftingMachines()
	ExportLine('"assemblers": [', 1)
	counter = 0
	for _, _ in pairs(AvailableAMachines) do
		counter = counter + 1
	end
	for name, machine in pairs(AvailableAMachines) do
		ExportLine('{', 2)
		
		ExportParameter('name', machine.name, false, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(machine.localised_name)
		ExportLine('",', 0)
		
		ExportParameter('crafting_speed', machine.crafting_speed, true, ',', 3)
		ExportParameter('base_productivity', machine.base_productivity, true, ',', 3)
		ExportParameter('module_inventory_size', machine.module_inventory_size, true, ',', 3)
		
		ExportLine('"crafting_categories": [', 3)
		pcounter = 0
		for _, _ in pairs(machine.crafting_categories) do
			pcounter = pcounter + 1
		end
		for category, _ in pairs(machine.crafting_categories) do
			pcounter = pcounter - 1
			if pcounter > 0 then
				ExportLine('"'..category..'",', 4)
			else
				ExportLine('"'..category..'"', 4)
			end
		end
		ExportLine('],', 3)
		
		ExportLine('"allowed_effects": [', 3)
		pcounter = 0
		for _, _ in pairs(machine.allowed_effects) do
			pcounter = pcounter + 1
		end
		for effect, _ in pairs(machine.allowed_effects) do
			pcounter = pcounter - 1
			if pcounter > 0 then
				ExportLine('"'..effect..'",', 4)
			else
				ExportLine('"'..effect..'"', 4)
			end
		end
		ExportLine('],', 3)

		ExportParameter('order', machine.order, false, '', 3)
		
		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportMiners()
	ExportLine('"miners": [', 1)
	counter = 0
	for _, _ in pairs(AvailableMiners) do
		counter = counter + 1
	end
	for name, miner in pairs(AvailableMiners) do
		ExportLine('{', 2)
		
		ExportParameter('name', miner.name, false, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(miner.localised_name)
		ExportLine('",', 0)
		
		ExportParameter('mining_speed', miner.mining_speed, true, ',', 3)
		ExportParameter('base_productivity', miner.base_productivity, true, ',', 3)
		ExportParameter('module_inventory_size', miner.module_inventory_size, true, ',', 3)

		ExportLine('"resource_categories": [', 3)
		pcounter = 0
		for _, _ in pairs(miner.resource_categories) do
			pcounter = pcounter + 1
		end
		for category, _ in pairs(miner.resource_categories) do
			pcounter = pcounter - 1
			if pcounter > 0 then
				ExportLine('"'..category..'",', 4)
			else
				ExportLine('"'..category..'"', 4)
			end
		end
		ExportLine('],', 3)
		
		ExportParameter('order', miner.order, false, '', 3)
		
		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportResources()
	ExportLine('"resources": [', 1)
	counter = 0
	for _, _ in pairs(AvailableResources) do
		counter = counter + 1
	end
	
	for name, resource in pairs(AvailableResources) do
		ExportLine('{', 2)
		
		ExportParameter('name', resource.name, false, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(resource.localised_name)
		ExportLine('",', 0)
		
		ExportParameter('resource_category', resource.resource_category, false, ',', 3)
		ExportParameter('mining_time', resource.mineable_properties.mining_time, false, ',', 3)
		if resource.mineable_properties.required_fluid then
			ExportParameter('required_fluid', resource.mineable_properties.required_fluid, false, ',', 3)
		end
		if resource.mineable_properties.fluid_amount then
			ExportParameter('fluid_amount', resource.mineable_properties.fluid_amount, false, ',', 3)
		end
		
		ExportLine('"products": [', 3)
		if resource.mineable_properties.products then
			pcounter = 0
			for _, _ in pairs(resource.mineable_properties.products) do
				pcounter = pcounter + 1
			end
			for _, product in pairs(resource.mineable_properties.products) do
				pcounter = pcounter - 1
				ExportLine('{', 4)
				ExportParameter('name', product.name, false, ',', 5)
				ExportParameter('type', product.type, false, ',', 5)
				
				amount = (product.amount == nil) and ((product.amount_max + product.amount_min)/2) or product.amount
				amount = amount * ( (product.probability == nil) and 1 or product.probability)
				ExportParameter('amount', amount, true, '', 5)

				if pcounter > 0 then
					ExportLine('},', 4)
				else
					ExportLine('}', 4)
				end
			end
		end
		ExportLine('],', 3)
		
		ExportParameter('minable', resource.mineable_properties.minable and 'true' or 'false', true, '', 3)

		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end

local function ExportGroups()
	ExportLine('"groups": [', 1)
	counter = 0
	for _, _ in pairs(game.item_group_prototypes) do
		counter = counter + 1
	end
	for name, group in pairs(game.item_group_prototypes) do
		ExportLine('{', 2)
		
		ExportParameter('name', group.name, false, ',', 3)

		ExportLine('"localised_name": "', 3)
		localised_print(group.localised_name)
		ExportLine('",', 0)
		
		ExportLine('"subgroups": [', 3)
		pcounter = 0
		for _, _ in pairs(group.subgroups) do
			pcounter = pcounter + 1
		end
		for _, subgroup in pairs(group.subgroups) do
			pcounter = pcounter - 1
			if pcounter > 0 then
				ExportLine('"'..subgroup.name..'",', 4)
			else
				ExportLine('"'..subgroup.name..'"', 4)
			end
		end
		ExportLine('],', 3)

		ExportParameter('order', group.order, false, '', 3)

		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine('],', 1)
end	

local function ExportSubGroups()
	ExportLine('"subgroups": [', 1)
	counter = 0
	for _, _ in pairs(game.item_subgroup_prototypes) do
		counter = counter + 1
	end
	for name, subgroup in pairs(game.item_subgroup_prototypes) do
		ExportLine('{', 2)
		
		ExportParameter('name', subgroup.name, false, ',', 3)

		--ExportParameter('category', item.category, false, ',', 3)
		ExportParameter('order', subgroup.order, false, '', 3)

		counter = counter - 1 
		if counter > 0 then
			ExportLine('},', 2)
		else
			ExportLine('}', 2)
		end
	end
	ExportLine(']', 1)
end	

	
exportComplete = false
script.on_nth_tick(10, 	
	function()
		if not exportComplete then
			exportComplete = true

			game.difficulty_settings.recipe_difficulty = defines.difficulty_settings.recipe_difficulty.normal
			game.difficulty_settings.technology_difficulty = defines.difficulty_settings.technology_difficulty.expensive

			ProcessPrototypes()	

			ExportLine('<<<START-EXPORT-P2>>>',0)

			ExportLine('{', 0)
			ExportModList()
			ExportResearch()
			ExportRecipes()
			ExportItems()
			ExportFluids()
			ExportModules()
			ExportCraftingMachines()
			ExportMiners()
			ExportResources()
			ExportGroups()
			ExportSubGroups()
			ExportLine('}', 0)

			ExportLine('<<<END-EXPORT-P2>>>',0)
	
		end
	end
)