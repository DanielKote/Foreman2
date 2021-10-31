local function Export()
	exporttable = {}
	exporttable['mods'] = {}
	exporttable['technologies'] = {}
	exporttable['recipes'] = {}


	for name, version in pairs(game.active_mods) do
		ntable = {}
		ntable['name'] = name
		ntable['version'] = version
		table.insert(exporttable['mods'], ntable)
	end

	if game.forces['player'] ~= nil then
		for _, tech in pairs(game.forces['player'].technologies) do
				ttech = {}
				ttech['name'] = tech.name
				ttech['enabled'] = tech.researched
				table.insert(exporttable['technologies'], ttech)
		end
		for _, recipe in pairs(game.forces['player'].recipes) do
				trecipe = {}
				trecipe['name'] = recipe.name
				trecipe['enabled'] = recipe.enabled
				table.insert(exporttable['recipes'], trecipe)
		end
	else --couldnt find the 'player' force, so just read all forces and assume a complete amalgamation of all researched tech and recipes works.
		for _, force in pairs(game.forces) do
			for _, tech in pairs(force.technologies) do
				ttech = {}
				ttech['name'] = tech.name
				ttech['enabled'] = tech.researched
				table.insert(exporttable['technologies'], ttech)
			end
			for _, recipe in pairs(force.recipes) do
				trecipe = {}
				trecipe['name'] = recipe.name
				trecipe['enabled'] = recipe.enabled
				table.insert(exporttable['recipes'], trecipe)
			end
		end
	end

	localised_print('<<<START-EXPORT-P0>>>')
	localised_print(game.table_to_json(exporttable))
	localised_print('<<<END-EXPORT-P0>>>')

end

script.on_nth_tick(1, 	
	function()

		Export()	

		ENDEXPORTANDJUSTDIE() -- just the most safe way of ensuring that we export once and quit. Basically... there is no ENDEXPORTANDJUSTDIE function. so lua will throw an expection and the run will end here.
	end
)