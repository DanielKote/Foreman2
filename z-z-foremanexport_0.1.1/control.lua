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

local IconsTech = {}
local IconsRecipes = {}
local IconsItems = {}
local IconsFluids = {}
--local IconsModules = {} --modules go into items
local IconsAMachines = {}
local IconsMiners = {}
local IconsGroups = {}

--translations for localization names & descriptions.
local Translations = {}
--counter for translations - we have to first queue up the translation requests then wait for them to finish. Once counters fall back to 0, we know we are done
local TranslationCounter = 0

--just for debug
local FailedIcons = {}

local function FlushFileBuffer()
	game.write_file('ForemanFactorioSetup.txt', FileBuffer, true)
	FileBuffer = ''
end

local function LOG(text)
	if DebugLog then
		FileBuffer = FileBuffer..text..'\n'
	
		if string.len(FileBuffer) > 100000 then
			FlushFileBuffer()
		end
	end
end

local function ExportRaw(text)
	FileBuffer = FileBuffer..text
	if string.len(FileBuffer) > 100000 then
		FlushFileBuffer()
	end
end

local function ExportLine(text, indentSize)
	if ExportSetup then
		indents = ''
		while indentSize > 0 do
			indentSize = indentSize - 1
			indents = indents..'	'
		end
		ExportRaw(indents..text..'\n')
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
	elseif game.technology_prototypes[techName].enabled then
		AvailableTech[techName] = game.technology_prototypes[techName]
		return true
	elseif game.technology_prototypes[techName].hidden then
		BlockedTech[techName] = game.technology_prototypes[techName]
		return false
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

--deep test for two tables to see if their values are equal (not references)
local function TablesEqualityCheck(o1, o2)
    if o1 == o2 then return true end
    local o1Type = type(o1)
    local o2Type = type(o2)
    if o1Type ~= o2Type then return false end
    if o1Type ~= 'table' then return false end

    local keySet = {}

    for key1, value1 in pairs(o1) do
        local value2 = o2[key1]
        if value2 == nil or TablesEqualityCheck(value1, value2) == false then
            return false
        end
        keySet[key1] = true
    end

    for key2, _ in pairs(o2) do
        if not keySet[key2] then return false end
    end
    return true
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

--get all the simple_entities that we used to transfer icon data from data.lua phase, and parse them into their respective dictionaries
local function ReadIconData()
	unsortedIconData = {}
	
	lastValue = 0
	for name, entity in pairs(game.entity_prototypes) do
		if string.sub(name,1,10) == 'foreman-x-' then
			currentValue = tonumber(string.sub(name,11,15))
			lastValue = (lastValue > currentValue) and lastValue or currentValue
			unsortedIconData[currentValue] = entity.order
		end
	end
	LOG('-------------------------ICON DATA TRANSFER----------------------')
	LOG('IconDataDummyEntities:'..lastValue,0)
	
	processString = ''
	currentDictionary = nil
	i = 1
	while i <= lastValue do
		partString = unsortedIconData[i]
		
		processString = processString..partString
		if string.len(processString) > 10000 or i == lastValue then --just grab a nice 8000-10000 char buffer for processing
			progressMade = true
			while progressMade do
				progressMade = false
				if string.len(processString) < 8000 and i ~= lastValue then
					break
				end
			
				--process dictionary type
				if string.sub(processString,1,6) == '<!><!>' then
					if string.sub(processString,7,7) == 'T' then
						currentDictionary = IconsTech
					elseif string.sub(processString,7,7) == 'R' then
						currentDictionary = IconsRecipes
					elseif string.sub(processString,7,7) == 'I' then
						currentDictionary = IconsItems
					elseif string.sub(processString,7,7) == 'F' then
						currentDictionary = IconsFluids
					elseif string.sub(processString,7,7) == 'A' then
						currentDictionary = IconsAMachines
					elseif string.sub(processString,7,7) == 'D' then
						currentDictionary = IconsMiners
					elseif string.sub(processString,7,7) == 'G' then
						currentDictionary = IconsGroups
					else
						currentDictionary = nil --basically, a fail state
					end
					processString = string.sub(processString, 8)
					progressMade = true
				end
				
				--process icon entity
				if string.sub(processString,1,4) == '<><>' then
					nameEndIndex, _ = string.find(processString, '<><>', 5)
					dataEndIndex, _ = string.find(processString, '<.><.>', 5)
					name = string.sub(processString, 5, nameEndIndex - 1)
					data = string.sub(processString, nameEndIndex + 4, dataEndIndex - 1)
					processString = string.sub(processString, dataEndIndex + 6)
					progressMade = true
					
					currentDictionary[name] = data
					LOG(name)
				end
				
			end		
		end
		i = i+1
	end
	
	--if(processString ~= "") then EXPORT(EXPORT) end --ERROR
end

--prepares the translation table (by requesting translations - completion will happen in the on_string_translated event)
local function ProcessTranslations()
	LOG('-------------------------TRANSLATION PROCESSING----------------------')
	for _, _ in pairs(AvailableTech) do
		TranslationCounter = TranslationCounter + 1 -- one for localized name, X one for localized description
	end
	for _, _ in pairs(AvailableRecipes) do
		TranslationCounter = TranslationCounter + 1
	end
	for _, _ in pairs(AvailableItems) do
		TranslationCounter = TranslationCounter + 1
	end
	for _, _ in pairs(AvailableFluids) do
		TranslationCounter = TranslationCounter + 1
	end
	for _, _ in pairs(AvailableModules) do
		TranslationCounter = TranslationCounter + 1
	end
	for _, _ in pairs(AvailableAMachines) do
		TranslationCounter = TranslationCounter + 1
	end	
	for _, _ in pairs(AvailableMiners) do
		TranslationCounter = TranslationCounter + 1
	end
	for _, _ in pairs(game.resource_category_prototypes) do
		TranslationCounter = TranslationCounter + 1
	end
	
	for _, _ in pairs(game.item_group_prototypes) do
		TranslationCounter = TranslationCounter + 1
		--player.request_translation(obj.localised_description)
	end
	
	for _, obj in pairs(AvailableTech) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	for _, obj in pairs(AvailableRecipes) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	for _, obj in pairs(AvailableItems) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	for _, obj in pairs(AvailableFluids) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	for _, obj in pairs(AvailableModules) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	for _, obj in pairs(AvailableAMachines) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end	
	for _, obj in pairs(AvailableMiners) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	for _, obj in pairs(game.resource_category_prototypes) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	
	for _, obj in pairs(game.item_group_prototypes) do
		player.request_translation(obj.localised_name)
		--player.request_translation(obj.localised_description)
	end
	
	LOG('Translations:'..TranslationCounter,0)
	FlushFileBuffer()
end

local function ExportModList()
	ExportLine('"mods": [', 1)
	ExportLine('"core",', 2)
	counter = 0
	for _, _ in pairs(game.active_mods) do
		counter = counter + 1
	end
	
	for name, _ in pairs(game.active_mods) do
		counter = counter - 1
		if counter > 0 then
			ExportLine('"'..name..'",', 2)
		else
			ExportLine('"'..name..'"', 2)
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

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,tech.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,tech.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
			
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
		ExportLine('],',  3)
		
		if IconsTech[name] then
			ExportRaw(IconsTech[name]..'\n')
		else
			table.insert(FailedIcons, 'Tech: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end

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

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,recipe.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,recipe.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
		ExportParameter('category', recipe.category, false, ',', 3)
		ExportParameter('order', recipe.order, false, ',', 3)
		--ExportParameter('group', recipe.group.name, false, ',', 3) --Redundant - each group has a list of subgroups
		ExportParameter('subgroup', recipe.subgroup.name, false, ',', 3)
		
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
		
		if IconsRecipes[name] then
			--need to check for null icons (very likely here since recipes with 1 result will usually not have an icon and just use the one from the result)
			if string.find(IconsRecipes[name], '"icon_info": null') then
				if recipe.products[1] then
					if IconsItems[recipe.products[1].name] then
						ExportRaw(IconsItems[recipe.products[1].name]..'\n')
					elseif IconsFluids[recipe.products[1].name] then
						ExportRaw(IconsFluids[recipe.products[1].name]..'\n')
					else
						table.insert(FailedIcons, 'Recipe: '..name)
						ExportParameter('icon_info', 'null', true, '', 3)
					end
				else
					table.insert(FailedIcons, 'Recipe: '..name)
					ExportParameter('icon_info', 'null', true, '', 3)
				end				
			else
				ExportRaw(IconsRecipes[name]..'\n')
			end
		else
			table.insert(FailedIcons, 'Recipe: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end

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

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,item.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,item.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
		--ExportParameter('category', item.category, false, ',', 3)
		ExportParameter('order', item.order, false, ',', 3)
		--ExportParameter('group', item.group.name, false, ',', 3) --reduntant: groups contain set of subgroups
		ExportParameter('subgroup', item.subgroup.name, false, ',', 3)

		
		if item.fuel_category ~= nil then
			ExportParameter('fuel_category', item.fuel_category, false, ',', 3)
			ExportParameter('fuel_value', item.fuel_value, false, ',', 3)
		end
		
		if IconsItems[name] then
			ExportRaw(IconsItems[name]..'\n')
		else
			table.insert(FailedIcons, 'Item: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end
		
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

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,fluid.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,fluid.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
		ExportParameter('order', fluid.order, false, ',', 3)
		--ExportParameter('group', fluid.group.name, false, ',', 3) --reduntant
		ExportParameter('subgroup', fluid.subgroup.name, false, ',', 3)

		ExportParameter('default_temperature', fluid.default_temperature, true, ',', 3)
		ExportParameter('max_temperature', fluid.max_temperature, true, ',', 3)
		
		if fluid.fuel_value ~= nil then
			ExportParameter('fuel_value', fluid.fuel_value, false, ',', 3)
		end

		if IconsFluids[name] then
			ExportRaw(IconsFluids[name]..'\n')
		else
			table.insert(FailedIcons, 'Fluid: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end
		
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
	for name, modle in pairs(AvailableModules) do
		ExportLine('{', 2)
		
		ExportParameter('name', modle.name, false, ',', 3)

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,modle.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,modle.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
		ExportParameter('order', modle.order, false, ',', 3)

		ExportParameter('category', modle.category, false, ',', 3)
		ExportParameter('tier', modle.tier, true, ',', 3)
		
		ExportParameter('module_effects_consumption', (modle.module_effects.consumption == nil) and 0 or modle.module_effects.consumption.bonus, true, ',', 3)
		ExportParameter('module_effects_speed', (modle.module_effects.speed == nil) and 0 or modle.module_effects.speed.bonus, true, ',', 3)
		ExportParameter('module_effects_productivity', (modle.module_effects.productivity == nil) and 0 or modle.module_effects.productivity.bonus, true, ',', 3)
		ExportParameter('module_effects_pollution', (modle.module_effects.pollution == nil) and 0 or modle.module_effects.pollution.bonus, true, ',', 3)
		
		ExportLine('"limitations": [', 3)
		pcounter = 0
		for _, _ in pairs(modle.limitations) do
			pcounter = pcounter + 1
		end
		for _, recipe in pairs(modle.limitations) do
			pcounter = pcounter - 1
			if pcounter > 0 then
				ExportLine('"'..recipe..'",', 4)
			else
				ExportLine('"'..recipe..'"', 4)
			end
		end
		ExportLine('],', 3)
		
		if IconsItems[name] then
			ExportRaw(IconsItems[name]..'\n')
		else
			table.insert(FailedIcons, 'Module: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end

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
	ExportLine('"crafting_machines": [', 1)
	counter = 0
	for _, _ in pairs(AvailableAMachines) do
		counter = counter + 1
	end
	for name, machine in pairs(AvailableAMachines) do
		ExportLine('{', 2)
		
		ExportParameter('name', machine.name, false, ',', 3)

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,machine.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,machine.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
		ExportParameter('order', machine.order, false, ',', 3)

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
		
		if IconsAMachines[name] then
			ExportRaw(IconsAMachines[name]..'\n')
		else
			table.insert(FailedIcons, 'AssemblyMachine: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end
		
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

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,miner.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,miner.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
		ExportParameter('order', miner.order, false, ',', 3)

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
		
		if IconsMiners[name] then
			ExportRaw(IconsMiners[name]..'\n')
		else
			table.insert(FailedIcons, 'Miner: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end
		
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

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,resource.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,resource.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
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

		lname = ''
		--ldescription = ''
		for ln, trans in pairs(Translations) do
			if TablesEqualityCheck(ln,group.localised_name) then
				lname = trans
			end
			--if TablesEqualityCheck(ln,item.localised_description) then
			--	ldescription = trans
			--end
		end
		ExportParameter('localised_name', lname, false, ',', 3)
		--ExportParameter('localised_description', ldescription, false, ',', 3)
		
		--ExportParameter('category', item.category, false, ',', 3)
		ExportParameter('order', group.order, false, ',', 3)
		--ExportParameter('order_in_recipe', group.order_in_recipe, false, ',', 3) --redundant; or more to the point too much work. Just sort groups based on 'order' all the time

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
		
		if IconsGroups[name] then
			ExportRaw(IconsGroups[name]..'\n')
		else
			table.insert(FailedIcons, 'Group: '..name)
			ExportParameter('icon_info', 'null', true, '', 3)
		end


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

script.on_event(defines.events.on_player_joined_game,
	function(e)
		--this is called when the game first starts (single player at least)
		game.write_file('ForemanFactorioSetup.txt', '', false)
		player = game.players[e.player_index]
		
		ProcessPrototypes()
		FlushFileBuffer()
		ReadIconData()
		FlushFileBuffer()
		
		ProcessTranslations()
		--after this things continue in the on_translated event
	end
)

script.on_event(defines.events.on_string_translated,
	function(e)
		TranslationCounter = TranslationCounter - 1
	
		if e.translated then
			Translations[e.localised_string] = e.result
		end
		
		if TranslationCounter == 0 then
			--we have finished all the translations/localizations. can proceed
			LOG('-------------------------END OF PROCESSING. BEGIN EXPORT----------------------')
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
			FlushFileBuffer()
			
			LOG('-------------------------END OF EXPORT.----------------------')
			for _,failedIcon in ipairs(FailedIcons) do
				LOG(failedIcon)
			end
			
			game.print('Foreman export complete.')
		end
	end
)