local function ExportLine(text, indentSize)
	indents = ''
	while indentSize > 0 do
		indentSize = indentSize - 1
		indents = indents..'	'
	end
	log(indents..text)
end

local function ExportParameter(paramName, paramValue, numerical, suffix, indentSize)
	if numerical then
		ExportLine('"'..paramName..'": '..paramValue..suffix, indentSize)
	else
		ExportLine('"'..paramName..'": "'..paramValue..'"'..suffix, indentSize)
	end
end

local function ExportIcon(icon, icon_size, icons)

	if icon == nil and icons == nil then
		ExportLine('"icon_data": null', 3)
	else
		ExportLine('"icon_data": {', 3)
		ExportParameter('icon', (icon == nil) and 'null' or '"'..icon..'"', true, ',', 4)
		ExportParameter('icon_size', (icon_size == nil) and 'null' or icon_size, true, ',', 4)
		
		if icons == nil then
			ExportParameter('icons', 'null', true, '', 4)
		else
			ExportLine('"icons": [', 4)

			ccounter = 0 for _,_ in ipairs(icons) do ccounter = ccounter + 1 end
			for _, ic in ipairs(icons) do
				ExportLine('{', 5)
				ExportParameter('icon', ic.icon, false, ',', 6)
				ExportParameter('icon_size', (ic.icon_size == nil) and 'null' or ic.icon_size, true, ',', 6)
				ExportParameter('scale', (ic.scale == nil) and 'null' or ic.scale, true, ',', 6)

				if ic.tint == nil or (ic.tint[1] == nil and ic.tint['r'] == nil and ic.tint['g'] == nil and ic.tint['b'] == nil and ic.tint['a'] == nil) then
					ExportLine('"tint": [255,255,255,255],', 6)
				elseif ic.tint[1] ~= nil then
					ExportLine('"tint": ['..ic.tint[1]..','..ic.tint[2]..','..ic.tint[3]..','..(ic.tint[4] == nil and '255' or ic.tint[4])..'],', 6)
				else --must be the r/g/b/a set
					ExportLine('"tint": ['..(ic.tint['r'] == nil and '255' or ic.tint['r'])..','..(ic.tint['g'] == nil and '255' or ic.tint['g'])..','..(ic.tint['b'] == nil and '255' or ic.tint['b'])..','..(ic.tint['a'] == nil and '255' or ic.tint['a'])..'],', 6)
				end
				if ic.shift == nil then
					ExportLine('"shift": [0,0]', 6)
				else
					ExportLine('"shift": ['..ic.shift[1]..','..ic.shift[2]..']', 6)
				end

				ccounter = ccounter - 1 if ccounter > 0 then ExportLine('},', 5) else ExportLine('}', 5) end
			end
			ExportLine(']', 4)
		end

		ExportLine('}', 3)
	end
end



log("FOREMAN INSTRUMENT AFTER DATA")

output = {}

output['technologies'] = data.raw.technology
output['recipes'] = data.raw.recipe
output['items'] = {}
output['fluids'] = data.raw.fluid
output['groups'] = data.raw['item-group']
output['entities'] = {}

for _, section in ipairs({ 'ammo', 'armor', 'capsule', 'gun', 'item', 'item-with-entity-data', 'item-with-inventory', 'item-with-label', 'item-with-tags', 'mining-tool', 'module', 'rail-planner', 'repair-tool', 'selection-tool', 'spider-vehicle', 'spidertron-remote', 'tool', 'upgrade-item' }) do
	for name, obj in pairs(data.raw[section]) do
		output['items'][name] = obj
	end
end

for _, section in ipairs({'assembling-machine', 'beacon', 'furnace', 'mining-drill', 'module', 'offshore-pump', 'rocket-silo'}) do
	for name, obj in pairs(data.raw[section]) do
		output['entities'][name] = obj
	end
end

ExportLine('<<<START-EXPORT-P1>>>', 0)
ExportLine('{', 0)

ExportLine('"technologies": [', 1)
counter = 0 for _,_ in pairs(output.technologies) do counter = counter + 1 end
for _, data in pairs(output.technologies) do
	ExportLine('{', 2)
	ExportParameter('name', data.name, false, ',', 3)
	ExportParameter('icon_name', "icon.t."..data.name, false, ',', 3)
	ExportIcon(data.icon, data.icon_size, data.icons)
	counter = counter - 1 if counter > 0 then ExportLine('},', 2) else ExportLine('}', 2) end
end
ExportLine('],', 1)

ExportLine('"recipes": [', 1)
counter = 0 for _,_ in pairs(output.recipes) do counter = counter + 1 end
for _, data in pairs(output.recipes) do
	ExportLine('{', 2)
	ExportParameter('name', data.name, false, ',', 3)
	ExportParameter('icon_name', "icon.r."..data.name, false, ',', 3)
	ExportIcon(data.icon, data.icon_size, data.icons)
	counter = counter - 1 if counter > 0 then ExportLine('},', 2) else ExportLine('}', 2) end
end
ExportLine('],', 1)

ExportLine('"items": [', 1)
counter = 0 for _,_ in pairs(output.items) do counter = counter + 1 end
for _, data in pairs(output.items) do
	ExportLine('{', 2)
	ExportParameter('name', data.name, false, ',', 3)
	ExportParameter('icon_name', "icon.i."..data.name, false, ',', 3)
	ExportIcon(data.icon, data.icon_size, data.icons)
	counter = counter - 1 if counter > 0 then ExportLine('},', 2) else ExportLine('}', 2) end
end
ExportLine('],', 1)

ExportLine('"fluids": [', 1)
counter = 0 for _,_ in pairs(output.fluids) do counter = counter + 1 end
for _, data in pairs(output.fluids) do
	ExportLine('{', 2)
	ExportParameter('name', data.name, false, ',', 3)
	ExportParameter('icon_name', "icon.i."..data.name, false, ',', 3)
	ExportIcon(data.icon, data.icon_size, data.icons)
	counter = counter - 1 if counter > 0 then ExportLine('},', 2) else ExportLine('}', 2) end
end
ExportLine('],', 1)

ExportLine('"entities": [', 1)
counter = 0 for _,_ in pairs(output.entities) do counter = counter + 1 end
for _, data in pairs(output.entities) do
	ExportLine('{', 2)
	ExportParameter('name', data.name, false, ',', 3)
	ExportParameter('icon_name', "icon.e."..data.name, false, ',', 3)
	ExportIcon(data.icon, data.icon_size, data.icons)
	counter = counter - 1 if counter > 0 then ExportLine('},', 2) else ExportLine('}', 2) end
end
ExportLine('],', 1)

ExportLine('"groups": [', 1)
counter = 0 for _,_ in pairs(output.groups) do counter = counter + 1 end
for _, data in pairs(output.groups) do
	ExportLine('{', 2)
	ExportParameter('name', data.name, false, ',', 3)
	ExportParameter('icon_name', "icon.g."..data.name, false, ',', 3)
	ExportIcon(data.icon, data.icon_size, data.icons)
	counter = counter - 1 if counter > 0 then ExportLine('},', 2) else ExportLine('}', 2) end
end
ExportLine(']', 1)

ExportLine('}', 0)
ExportLine('<<<END-EXPORT-P1>>>', 0)