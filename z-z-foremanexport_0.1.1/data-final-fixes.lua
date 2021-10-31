IncludeBarrelsAndCrates = false
DebugLog = false

function LOG(text)
	if DebugLog then
		log(text)
	end
end

--need to transfer icon information from the raw cycle (data-final-fixes.lua) to the runtime cycle (control.lua)
--so we make the json like entries for everything we need, then break it into 200char strings, and chuck them into simple_entity prototypes (under order) to read in control

--process the entire icons/icon/iconsize property set, and add the corresponding dummy item
outputSet = {}
function ProcessIcon(obj, defaultSize)

	output =         '<><>'..obj.name..'<><>'
	if obj.icon or obj.icons then --if both are missing, then icon_info is set to null
		output = output..'			"icon_info":\n			{\n'
		output = output..'				"icon_dsize": '..defaultSize..',\n'
		output = output..ProcessIconData(obj, 4)..',\n'
		output = output..'				"icons":\n				[\n'

		if obj.icons then
			pcounter = 0
			for _,_ in ipairs(obj.icons) do
				pcounter = pcounter + 1
			end
		
			for _,icon in ipairs(obj.icons) do
				output = output..'					{\n'
				output = output..ProcessIconData(icon,6)
				output = output..'\n					}'
				
				pcounter = pcounter - 1
				if pcounter > 0 then
					output = output..',\n'
				else
					output = output..'\n'
				end
			end
		end
		output = output..'				]\n'
		output = output..'			}<.><.>'

	else
		output = output..'			"icon_info": null<.><.>'
	end

	LOG(output)
	table.insert(outputSet,output)
end

--reads and prepares icon data (icon,icon_size,icon_mipmaps)
function ProcessIconData(obj, indentSize)
	indents = ''
	while indentSize > 0 do
		indentSize = indentSize - 1
		indents = indents..'	'
	end

	output = ''
	if obj.icon then
		output = output..indents..'"icon": "'..obj.icon..'",\n'
	else
		output = output..indents..'"icon": "",\n'
	end
	if obj.icon_size then
		output = output..indents..'"icon_size": '..obj.icon_size..',\n'
	else
		output = output..indents..'"icon_size": 0,\n'
	end
	if obj.icon_mipmaps then
		output = output..indents..'"icon_mipmaps": '..obj.icon_mipmaps..',\n'
	else
		output = output..indents..'"icon_mipmaps": 0,\n'
	end
	if obj.scale then
		output = output..indents..'"scale": '..obj.scale..',\n'
	else
		output = output..indents..'"scale": 0,\n'
	end
	if obj.shift then
		output = output..indents..'"shift": ['..obj.shift[1]..', '..obj.shift[2]..'],\n'
	else
		output = output..indents..'"shift": [0, 0],\n'
	end
	if obj.tint then
		if obj.tint['r'] ~= nil then
			a = (obj.tint['a'] == nil) and 1 or obj.tint['a']
			output = output..indents..'"tint": ['..obj.tint['r']..', '..obj.tint['g']..', '..obj.tint['b']..', '..a..']'
		else
			a = (obj.tint[4] == nil) and 1 or obj.tint[4]
			output = output..indents..'"tint": ['..obj.tint[1]..', '..obj.tint[2]..', '..obj.tint[3]..', '..a..']'
		end
	else
		output = output..indents..'"tint": [255,255,255,255]'
	end
	return output
end

--function to add the entire output set into dummy objects (entities)
placeholders = {}
maxLength = 200
function AddDummies()
	splitStringSet = {}
	leftover = ''
	
	for _, str in ipairs(outputSet) do
		leftover = leftover..str
		while string.len(leftover) > maxLength do
			table.insert(splitStringSet, string.sub(leftover, 1, maxLength))
			leftover = string.sub(leftover, maxLength + 1)
		end
	end
	if leftover and string.len(leftover) > 0 then
		table.insert(splitStringSet, leftover)
	end
	
	for i, ss in ipairs(splitStringSet) do
		table.insert(placeholders,
		{
			type = 'simple-entity',
			name = 'foreman-x-'..string.format("%05d",i),
			order = ss,
			
			icon = '__z-z-foremanexport__/placeholder.png',
			icon_size = 1,
			picture = { filename='__z-z-foremanexport__/placeholder.png', width=1, height=1 },
		})
		--LOG(i)
		--LOG(ss)
	end
end

LOG('---------------------ICONS FOUND FOR:-------------------')
--process all icons
LOG('---------------------TECHNOLOGY:-------------------')
table.insert(outputSet,'<!><!>T')
for name, obj in pairs(data.raw.technology) do
	LOG(name)
	ProcessIcon(obj, 256)
end
LOG('---------------------RECIPIES:-------------------')
table.insert(outputSet,'<!><!>R')
for name, obj in pairs(data.raw.recipe) do
	LOG(name)
	ProcessIcon(obj, 32)
end
LOG('---------------------ITEMS:-------------------')
table.insert(outputSet,'<!><!>I')
for name, obj in pairs(data.raw.item) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw.capsule) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw.module) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw.ammo) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw.gun) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw.armor) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw.tool) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['repair-tool']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['rail-planner']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['item-with-entity-data']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['item-with-inventory']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['item-with-label']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['item-with-tags']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['spider-vehicle']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['spidertron-remote']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
LOG('---------------------FLUIDS:-------------------')
table.insert(outputSet,'<!><!>F')
for name, obj in pairs(data.raw.fluid) do
	LOG(name)
	ProcessIcon(obj, 32)
end
--[[
--modules go into items
LOG('---------------------MODULES:-------------------')
table.insert(outputSet,'<!><!>M')
for name, obj in pairs(data.raw.module) do
	LOG(name)
	ProcessIcon(obj, 32)
end
--]]
LOG('---------------------ASSEMBLY MACHINES:-------------------')
table.insert(outputSet,'<!><!>A')
for name, obj in pairs(data.raw['assembling-machine']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['rocket-silo']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
for name, obj in pairs(data.raw['furnace']) do
	LOG(name)
	ProcessIcon(obj, 32)
end
LOG('---------------------MINERS:-------------------')
table.insert(outputSet,'<!><!>D')
for name, obj in pairs(data.raw['mining-drill']) do
	LOG(name)
	ProcessIcon(obj, 32)
end

LOG('---------------------ITEM GROUPS:-------------------')
table.insert(outputSet,'<!><!>G')
for name, obj in pairs(data.raw['item-group']) do
	LOG(name)
	ProcessIcon(obj, 64)
end


--add in the dummy simple_entities to transfer icon data to control.lua
AddDummies()
data:extend(placeholders)


