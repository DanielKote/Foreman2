using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Foreman.Properties;
using System.Threading;

namespace Foreman
{
    class FactorioLuaProcessor
    {
        private class MissingPrototypeValueException : Exception
        {
            public LuaTable Table;
            public String Key;

            public MissingPrototypeValueException(LuaTable table, String key, String message = "")
                : base(message)
            {
                Table = table;
                Key = key;
            }
        }

        private Dictionary<string, Item> Items;
        private Dictionary<string, Recipe> Recipes;
        private Dictionary<string, Assembler> Assemblers;
        private Dictionary<string, Miner> Miners;
        private Dictionary<string, Resource> Resources;
        private Dictionary<string, Module> Modules;

        private Dictionary<string, Exception> FailedFiles;
        private Dictionary<string, Exception> FailedPaths;

        private List<Mod> Mods = DataCache.Mods;
        private static string DataPath { get { return Path.Combine(Settings.Default.FactorioPath, "data"); } }
        private static string ModPath { get { return Path.Combine(Settings.Default.FactorioUserDataPath, "mods"); } }
        private string Difficulty { get { return Settings.Default.FactorioNormalDifficulty ? "normal" : "expensive"; } }

        private const float defaultRecipeTime = 0.5f;

        public Dictionary<string, Exception> GetFileExceptions() { return FailedFiles; }
        public Dictionary<string, Exception> GetPathExceptions() { return FailedPaths; }

        public FactorioLuaProcessor()
        {
            FailedFiles = new Dictionary<string, Exception>();
            FailedPaths = new Dictionary<string, Exception>();
        }

        public JObject LoadData(IProgress<KeyValuePair<int, string>> progress, CancellationToken ctoken, int startingPercent, int endingPercent)
        {
            int modPercent = (int)((endingPercent - startingPercent) * 0.6 + startingPercent);

            progress.Report(new KeyValuePair<int, string>(startingPercent, "Running Factorio LUA code"));
            using (Lua lua = new Lua())
            {
                AddLuaPackagePath(lua, Path.Combine(DataPath, "core", "lualib")); //Core lua functions
                AddLuaPackagePath(lua, Path.Combine(DataPath, "base")); // Base mod
                String basePackagePath = lua["package.path"] as String;

                String dataloaderFile = Path.Combine(DataPath, "core", "lualib", "dataloader.lua");
                try
                {
                    lua.DoFile(dataloaderFile);
                }
                catch (Exception e)
                {
                    FailedFiles[dataloaderFile] = e;
                    ErrorLogging.LogLine(String.Format("Error loading dataloader.lua. This file is required to load any values from the prototypes. Message: '{0}'", e.Message));
                    return null;
                }

                // Added a custom require function to support relative paths (angels refining was using this and a few others)
                // Defines is defined here: https://lua-api.factorio.com/latest/defines.html 
                // Also contains a special case for "__base__" that can resolve to the base mod
                lua.DoString(@"
                    local oldrequire = require

                    function table_size(t)
                      local count = 0
                      for k,v in pairs(t) do
                        count = count + 1
                      end
                        return count
                    end

                    function relative_require(modname)
                      if string.match(modname, '__.+__[/%.]') then
                        return oldrequire(string.gsub(modname, '__.+__[/%.]', ''))
                      end
                      local regular_loader = package.searchers[2]
                      local loader = function(inner)
                        if string.match(modname, '(.*)%.') then
                          return regular_loader(string.match(modname, '(.*)%.') .. '.' .. inner)
                        end
                      end

                      table.insert(package.searchers, 1, loader)
                      local retval = oldrequire(modname)
                      table.remove(package.searchers, 1)

                      return retval
                    end
                    _G.require = relative_require

                    function module(modname,...)
                    end
    
                    require ""util""
                    util = {}
                    util.table = {}
                    util.table.deepcopy = table.deepcopy
                    util.multiplystripes = multiplystripes
                    util.by_pixel = by_pixel
                    util.format_number = format_number
                    util.increment = increment

                    _G.unpack = table.unpack;

                    function log(...)
                    end

                    
                    defines = {}
                    defines.alert_type = {}
                    defines.behavior_result = {}
                    defines.build_check_type = {}
                    defines.chain_signal_state = {}
                    defines.chunk_generated_status = {}
                    defines.circuit_condition_index = {}
                    defines.circuit_connector_id = {}
                    defines.command = {}
                    defines.compound_command = {}
                    defines.control_behavior = {}
                    defines.control_behavior.inserter = {}
                    defines.control_behavior.inserter.circuit_mode_of_operation = {}
                    defines.control_behavior.inserter.hand_read_mode = {}
                    defines.control_behavior.logistics_container = {}
                    defines.control_behavior.logistics_container.circuit_mode_of_operation = {}
                    defines.control_behavior.lamp = {}
                    defines.control_behavior.lamp.circuit_mode_of_operation = {}
                    defines.control_behavior.mining_drill = {}
                    defines.control_behavior.mining_drill.resource_read_mode = {}
                    defines.control_behavior.transport_belt = {}
                    defines.control_behavior.transport_belt.content_read_mode = {}
                    defines.control_behavior.type = {}
                    defines.controllers = {}
                    defines.deconstruction_item = {}
                    defines.deconstruction_item.entity_filter_mode = {}
                    defines.deconstruction_item.tile_filter_mode = {}
                    defines.deconstruction_item.tile_selection_mode = {}
                    defines.difficulty = {}
                    defines.difficulty_settings = {}
                    defines.difficulty_settings.recipe_difficulty = {}
                    defines.difficulty_settings.technology_difficulty = {}
                    defines.difficulty_settings.recipe_difficulty.normal = 1
                    defines.difficulty_settings.technology_difficulty.normal = 1
                    defines.distraction = {}
                    defines.direction = {}
                    defines.direction.north = 1
                    defines.direction.east = 2
                    defines.direction.south = 3
                    defines.direction.west = 4
                    defines.direction.northeast = 5
                    defines.direction.southeast = 6
                    defines.direction.southwest = 7
                    defines.direction.northwest = 8
                    defines.disconnect_reason = {}
                    defines.entity_status = {}
                    defines.events = {}
                    defines.flow_precision_index = {}
                    defines.group_state = {}
                    defines.gui_type = {}
                    defines.input_action = {}
                    defines.inventory = {}
                    defines.logistic_member_index = {}
                    defines.logistic_mode = {}
                    defines.mouse_button_type = {}
                    defines.prototypes = {}
                    defines.rail_connection_direction = {}
                    defines.rail_direction = {}
                    defines.relative_gui_position = {}
                    defines.relative_gui_type = {}
                    defines.render_mode	= {}
                    defines.rich_text_setting = {}
                    defines.riding = {}
                    defines.riding.acceleration = {}
                    defines.riding.direction = {}
                    defines.shooting = {}
                    defines.signal_state = {}
                    defines.train_state	 = {}
                    defines.transport_line = {}
                    defines.wire_connection_id = {}
                    defines.wire_type = {}
                ");

                // Should be mod settings generated by Factorio. It looks at settings.lua, settings-updates.lua, and settings-final-fixed.lua.
                var modSettingsLua = ReadModSettings();
                modSettingsLua = modSettingsLua.Replace(" = True", " = true");
                modSettingsLua = modSettingsLua.Replace(" = False", " = false");
                lua.DoString(modSettingsLua);

                IEnumerable<Mod> newEnabledMods = Mods.Where(m => m.Enabled);

                // Create global dictionary of mods
                String luaCode = "mods = {}\n";
                foreach (Mod m in newEnabledMods)
                {
                    luaCode += $"mods['{m.Name}'] = '{m.parsedVersion.ToString()}'\n";
                }
                lua.DoString(luaCode);

                // Note: All mods "settings" file is processed first then data, then date-updates, etc.
                // This allows things such as the base mod to generate barrel recipes after other mods have defined liquids
                float total = newEnabledMods.Count() * 3;
                float current = 0;
                foreach (String filename in new String[] { "data.lua", "data-updates.lua", "data-final-fixes.lua" })
                {
                    foreach (Mod mod in newEnabledMods)
                    {
                        progress.Report(new KeyValuePair<int, string>(startingPercent + (int)((modPercent - startingPercent) * current++ / total), ""));

                        Console.WriteLine("Processing: " + filename + " in " + mod);
                        AddLuaPackagePath(lua, mod.dir);

                        //Because many mods use the same path to refer to different files, we need to clear the 'loaded' table so Lua doesn't think they're already loaded
                        lua.DoString(@"
                            for k, v in pairs(package.loaded) do
                               package.loaded[k] = false
                            end");

                        String dataFile = Path.Combine(mod.dir, filename);
                        if (File.Exists(dataFile))
                        {
                            try
                            {
                                lua.DoFile(dataFile);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                FailedFiles[dataFile] = e;
                            }
                        }
                    }
                }

                progress.Report(new KeyValuePair<int, string>(modPercent, "Processing Data.Raw from Factorio LUA"));

                //------------------------------------------------------------------------------------------
                // Lua files have all been executed, now it's time to extract their data from the lua engine
                //------------------------------------------------------------------------------------------
                total = 0;
                current = 0;
                JObject factorioData = new JObject();

                List<string> itemTypes = new List<string> {
                    "item",
                    "fluid",
                    "capsule",
                    "module",
                    "ammo",
                    "gun",
                    "armor",
                    "blueprint",
                    "deconstruction-item",
                    "mining-tool",
                    "repair-tool",
                    "tool",
                    "rail-planner",
                    "item-with-entity-data",
                    "item-with-inventory",
                    "item-with-label",
                    "item-with-tags",
                    "spider-vehicle",
                    "spidertron-remote"
                };
                foreach (string type in itemTypes)
                    total += (lua.GetTable("data.raw")[type] as LuaTable)?.Keys.Count ?? 0;

                LuaTable technologyTable = lua.GetTable("data.raw")["technology"] as LuaTable;
                LuaTable recipeTable = lua.GetTable("data.raw")["recipe"] as LuaTable;
                LuaTable assemblerTable = lua.GetTable("data.raw")["assembling-machine"] as LuaTable;
                LuaTable furnaceTable = lua.GetTable("data.raw")["furnace"] as LuaTable;
                LuaTable minerTable = lua.GetTable("data.raw")["mining-drill"] as LuaTable;
                LuaTable resourceTable = lua.GetTable("data.raw")["resource"] as LuaTable;
                LuaTable moduleTable = lua.GetTable("data.raw")["module"] as LuaTable;
                total += (recipeTable?.Keys.Count?? 0) + (assemblerTable?.Keys.Count?? 0) + (furnaceTable?.Keys.Count?? 0) + (minerTable?.Keys.Count?? 0) + (resourceTable?.Keys.Count?? 0) + (moduleTable?.Keys.Count?? 0);

                foreach (string typeName in itemTypes)
                {
                    LuaTable itemTable = lua.GetTable("data.raw")[typeName] as LuaTable;
                    if (itemTable != null)
                    {
                        var enumerator = itemTable.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            progress.Report(new KeyValuePair<int, string>(modPercent + (int)((endingPercent - modPercent) * current++ / total), ""));
                            InterpretItem(enumerator.Key as String, enumerator.Value as LuaTable);
                        }
                    }
                }
                if (recipeTable != null)
                {
                    var recipeEnumerator = recipeTable.GetEnumerator();
                    while (recipeEnumerator.MoveNext())
                    {
                        progress.Report(new KeyValuePair<int, string>(modPercent + (int)((endingPercent - modPercent) * current++ / total), ""));
                        InterpretRecipe(recipeEnumerator.Key as String, recipeEnumerator.Value as LuaTable, technologyTable);
                    }
                }
                if (assemblerTable != null)
                {
                    var assemblerEnumerator = assemblerTable.GetEnumerator();
                    while (assemblerEnumerator.MoveNext())
                    {
                        progress.Report(new KeyValuePair<int, string>(modPercent + (int)((endingPercent - modPercent) * current++ / total), ""));
                        InterpretAssemblingMachine(assemblerEnumerator.Key as String, assemblerEnumerator.Value as LuaTable);
                    }
                }

                if (furnaceTable != null)
                {
                    var furnaceEnumerator = furnaceTable.GetEnumerator();
                    while (furnaceEnumerator.MoveNext())
                    {
                        progress.Report(new KeyValuePair<int, string>(modPercent + (int)((endingPercent - modPercent) * current++ / total), ""));
                        InterpretFurnace(furnaceEnumerator.Key as String, furnaceEnumerator.Value as LuaTable);
                    }
                }

                if (minerTable != null)
                {
                    var minerEnumerator = minerTable.GetEnumerator();
                    while (minerEnumerator.MoveNext())
                    {
                        progress.Report(new KeyValuePair<int, string>(modPercent + (int)((endingPercent - modPercent) * current++ / total), ""));
                        InterpretMiner(minerEnumerator.Key as String, minerEnumerator.Value as LuaTable);
                    }
                }

                if (resourceTable != null)
                {
                    var resourceEnumerator = resourceTable.GetEnumerator();
                    while (resourceEnumerator.MoveNext())
                    {
                        progress.Report(new KeyValuePair<int, string>(modPercent + (int)((endingPercent - modPercent) * current++ / total), ""));
                        InterpretResource(resourceEnumerator.Key as String, resourceEnumerator.Value as LuaTable);
                    }
                }

                if (moduleTable != null)
                {
                    foreach (String moduleName in moduleTable.Keys)
                    {
                        progress.Report(new KeyValuePair<int, string>(modPercent + (int)((endingPercent - modPercent) * current++ / total), ""));
                        InterpretModule(moduleName, moduleTable[moduleName] as LuaTable);
                    }
                }
                progress.Report(new KeyValuePair<int, string>(endingPercent, ""));
                return factorioData;
            }
        }

        private string ReadModSettings()
        {
            var sb = new StringBuilder();
            sb.AppendLine("settings = {}");
            sb.AppendLine("settings.startup = {}");

            var settingsFile = Path.Combine(ModPath, "mod-settings.dat");

            if (!File.Exists(settingsFile))
            {
                ErrorLogging.LogLine($"Unable to find mod-settings.dat: {settingsFile}");
                return sb.ToString();
            }

            ushort major;
            ushort minor;
            ushort patch;
            ushort dev;

            FactorioLuaPropertyTree propTree;

            using (BinaryReader reader = new BinaryReader(File.Open(settingsFile, FileMode.Open)))
            {
                major = reader.ReadUInt16();
                minor = reader.ReadUInt16();
                patch = reader.ReadUInt16();
                dev = reader.ReadUInt16();

                if (major >= 1 || (major <= 0 && minor >= 17))
                {
                    var noop = reader.ReadByte();
                }

                propTree = FactorioLuaPropertyTree.Read(reader);
            }

            var startup = (Dictionary<string, FactorioLuaPropertyTree>)((Dictionary<string, FactorioLuaPropertyTree>)propTree.Content)["startup"].Content;

            const string braces = @"{}";

            foreach (var x in startup)
            {
                var format = $"settings.startup[\"{x.Key}\"]";
                sb.AppendLine($"{format} = {braces}");
                var valPropTree = ((Dictionary<string, FactorioLuaPropertyTree>)x.Value.Content)["value"].Content;
                var type = valPropTree.GetType();
                var value = "";
                if (type.Name == "String")
                {
                    value = '"' + valPropTree.ToString() + '"';
                }
                else if (type.Name == "Boolean" || type.Name == "Double" || type.Name == "Float" || type.Name == "Integer")
                {
                    value = valPropTree.ToString();
                }
                else
                {
                    throw new Exception($"Unknown type {type.Name} found while parsing settings for key {x.Key}");
                }

                var assignment = $"{format}.value = {value}";
                sb.AppendLine(assignment);
            }

            return sb.ToString();
        }

        private void InterpretItem(String name, LuaTable values)
        {
            name = name.Replace("__ENTITY__", "");
            name = name.Replace("__ITEM__", "");
            if (String.IsNullOrEmpty(name))
            {
                return;
            }
            Item newItem = new Item(name, "", false, null, "");
            newItem.SetIconAndColor(GetIconAndColor(values));

            if (!Items.ContainsKey(name))
            {
                Items.Add(name, newItem);
            }
        }

        private void InterpretRecipe(String name, LuaTable values, LuaTable techTable)
        {
            try
            {
                String subgroup = GetLuaValueOrDefault<string>(values, "subgroup", true, "");
                if (subgroup == "fill-barrel" || subgroup == "empty-barrel" || subgroup.Contains("deadlock-crates"))
                    return;

                var recipeData = values[Difficulty] == null ? values : GetLuaValueOrDefault<LuaTable>(values, Difficulty, true);
                if (recipeData == null)
                {
                    ErrorLogging.LogLine($"Error reading recipe '{name}', unable to locate data table.");
                    return;
                }

                float time = GetLuaValueOrDefault<float>(recipeData, "energy_required", true, 0.5f);

                Dictionary<Item, float> ingredients = extractIngredientsFromLuaRecipe(values);
                Dictionary<Item, float> results = extractResultsFromLuaRecipe(values);

                if (false)
                {
                    Console.Write("RECIPE < " + name + "> (" + time + "s): ");
                    foreach (Item item in ingredients.Keys)
                        Console.Write(item.Name + " x" + ingredients[item] + ", ");
                    Console.Write("   --->   ");
                    foreach (Item item in results.Keys)
                        Console.Write(item.Name + " x" + results[item] + ", ");
                    Console.WriteLine("");
                }

                if (results == null)
                    return;

                if (name == null)
                    name = results.ElementAt(0).Key.Name;

                // check if recipe even appears ingame (e.g. gets researched)
                if (!IsRecipeAvailable(name, recipeData, techTable))
                {
                    return;
                }

                Recipe newRecipe = new Recipe(name, "", null, "");
                newRecipe.Time = (time == 0.0f) ? defaultRecipeTime : time;
                foreach (KeyValuePair<Item, float> kvp in ingredients)
                    newRecipe.AddIngredient(kvp.Key, kvp.Value);
                foreach (KeyValuePair<Item, float> kvp in results)
                    newRecipe.AddResult(kvp.Key, kvp.Value);

                newRecipe.Category = GetLuaValueOrDefault<string>(values, "category", true, "crafting");
                // Skip barreling recipes from Bobs/Angels
                if (newRecipe.Category == "barreling-pump" || newRecipe.Category == "air-pump")
                {
                    if (newRecipe.Name.StartsWith("empty-") || newRecipe.Name.StartsWith("fill-"))
                        return;
                }
                newRecipe.SetIconAndColor(GetIconAndColor(values));

                Recipes.Add(newRecipe.Name, newRecipe);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(String.Format("Error reading value '{0}' from recipe prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
            }
        }

        private bool IsRecipeAvailable(String name, LuaTable recipeData, LuaTable techTable)
        {
            bool? recipeEnabled = GetLuaValueOrDefault<bool?>(recipeData, "enabled", true);
            if (recipeEnabled.GetValueOrDefault(true)) // recipe is already available at gamestart - if there is no value, factorio defaults to true
            {
                return true;
            }
            System.Collections.IDictionaryEnumerator techTableEnum = techTable.GetEnumerator();
            while (techTableEnum.MoveNext())
            {
                bool? techEnabled = GetLuaValueOrDefault<bool?>(techTableEnum.Value as LuaTable, "enabled", true);
                if (!techEnabled.GetValueOrDefault(true)) // technology is disabled - if there is no value, factorio defaults to true
                {
                    continue; // next tech
                }
                LuaTable effects = GetLuaValueOrDefault<LuaTable>(techTableEnum.Value as LuaTable, "effects", true);
                if (effects == null) // technology doesn't unlock anything O_o
                {
                    continue; // next tech
                }
                System.Collections.IDictionaryEnumerator effectsEnum = effects.GetEnumerator();
                while (effectsEnum.MoveNext())
                {
                    LuaTable effect = effectsEnum.Value as LuaTable;
                    if (effect != null && GetLuaValueOrDefault<string>(effect, "recipe", true) == name)
                    {

                        return true; // recipe is unlocked via technology
                    }
                }
            }
            return false; // recipe is useless garbage!
        }

        private Dictionary<Item, float> extractIngredientsFromLuaRecipe(LuaTable values)
        {
            Dictionary<Item, float> ingredients = new Dictionary<Item, float>();

            LuaTable ingredientsTable = GetLuaValueOrDefault<LuaTable>(values, "ingredients", true) ??
                                        GetLuaValueOrDefault<LuaTable>(GetLuaValueOrDefault<LuaTable>(values, Difficulty), "ingredients");

            var ingredientEnumerator = ingredientsTable.GetEnumerator();
            while (ingredientEnumerator.MoveNext())
            {
                LuaTable ingredientTable = ingredientEnumerator.Value as LuaTable;
                String name;
                float amount;
                if (ingredientTable["name"] != null)
                {
                    name = ingredientTable["name"] as String;
                }
                else
                {
                    name = ingredientTable[1] as String; //Name and amount often have no key in the prototype
                }
                if (ingredientTable["amount"] != null)
                {
                    amount = Convert.ToSingle(ingredientTable["amount"]);
                }
                else
                {
                    amount = Convert.ToSingle(ingredientTable[2]);
                }
                Item ingredient = FindOrCreateUnknownItem(name);
                if (!ingredients.ContainsKey(ingredient))
                {
                    ingredients.Add(ingredient, amount);
                }
                else
                {
                    ingredients[ingredient] += amount;
                }
            }

            return ingredients;
        }

        private Dictionary<Item, float> extractResultsFromLuaRecipe(LuaTable values)
        {
            Dictionary<Item, float> results = new Dictionary<Item, float>();

            LuaTable source = null;

            if (values[Difficulty] == null)
                source = values;
            else
            {
                var difficultyTable = GetLuaValueOrDefault<LuaTable>(values, Difficulty, true);
                if (difficultyTable?["result"] != null || difficultyTable?["results"] != null)
                    source = difficultyTable;
            }

            if (source == null)
            {
                ErrorLogging.LogLine($"Error reading results from {values}.");
                return null;
            }

            if (source?["result"] != null)
            {
                String resultName = GetLuaValueOrDefault<string>(source, "result");
                float resultCount = GetLuaValueOrDefault<float>(source, "result_count", true);
                if (resultCount == 0f)
                {
                    resultCount = 1f;
                }
                results.Add(FindOrCreateUnknownItem(resultName), resultCount);
            }
            else
            {
                // If we can't read results, try difficulty/results
                LuaTable resultsTable = GetLuaValueOrDefault<LuaTable>(source, "results", true);

                if (resultsTable != null)
                {
                    var resultEnumerator = resultsTable.GetEnumerator();
                    while (resultEnumerator.MoveNext())
                    {
                        LuaTable resultTable = resultEnumerator.Value as LuaTable;
                        Item result;
                        if (resultTable["name"] != null)
                        {
                            result = FindOrCreateUnknownItem(GetLuaValueOrDefault<string>(resultTable, "name"));
                        }
                        else if (resultTable[1] != null)
                        {
                            result = FindOrCreateUnknownItem((string)resultTable[1]);
                        }
                        else
                        {
                            ErrorLogging.LogLine($"Error reading results from {values}.");
                            return null;
                        }

                        float amount = 0f;
                        if (resultTable["amount"] != null)
                        {
                            amount = GetLuaValueOrDefault<float>(resultTable, "amount");
                            //Just the average yield. Maybe in the future it should show more information about the probability
                            var probability = GetLuaValueOrDefault<float>(resultTable, "probability", true, 1.0f);
                            amount *= probability;
                        }
                        else if (resultTable["amount_min"] != null)
                        {
                            float probability = GetLuaValueOrDefault<float>(resultTable, "probability", true, 1f);
                            float amount_min = GetLuaValueOrDefault<float>(resultTable, "amount_min");
                            float amount_max = GetLuaValueOrDefault<float>(resultTable, "amount_max");
                            amount = ((amount_min + amount_max) / 2f) * probability;        //Just the average yield. Maybe in the future it should show more information about the probability
                        }
                        else
                        {
                            amount = Convert.ToSingle(resultTable[2]);
                        }

                        if (results.ContainsKey(result))
                        {
                            results[result] += amount;
                        }
                        else
                        {
                            results.Add(result, amount);
                        }
                    }
                }
                else
                {
                    ErrorLogging.LogLine($"Error reading results from {values}.");
                }
            }
            return results;
        }

        //This is only if a recipe references an item that isn't in the item prototypes (which shouldn't really happen)
        private Item FindOrCreateUnknownItem(String itemName)
        {
            Item newItem;
            if (!Items.ContainsKey(itemName))
            {
                Items.Add(itemName, newItem = new Item(itemName, "", false, null, ""));
            }
            else
            {
                newItem = Items[itemName];
            }
            return newItem;
        }

        private void InterpretAssemblingMachine(String name, LuaTable values)
        {
            try
            {
                Assembler newAssembler = new Assembler(name);
                newAssembler.Icon = GetIconAndColor(values).Icon;

                // 0.17 compat, ingredient_count no longer required
                newAssembler.ModuleSlots = GetLuaValueOrDefault<int>(values, "module_slots", true, 0);
                if (newAssembler.ModuleSlots == 0)
                {
                    var moduleTable = GetLuaValueOrDefault<LuaTable>(values, "module_specification", true);
                    if (moduleTable != null)
                    {
                        newAssembler.ModuleSlots = GetLuaValueOrDefault<int>(moduleTable, "module_slots", true, 0);
                    }
                }
                newAssembler.Speed = GetLuaValueOrDefault<float>(values, "crafting_speed");

                LuaTable effects = GetLuaValueOrDefault<LuaTable>(values, "allowed_effects", true);
                if (effects != null)
                {
                    foreach (String effect in effects.Values)
                    {
                        newAssembler.AllowedEffects.Add(effect);
                    }
                }
                LuaTable categories = GetLuaValueOrDefault<LuaTable>(values, "crafting_categories");
                foreach (String category in categories.Values)
                {
                    newAssembler.Categories.Add(category);
                }

                foreach (String s in Settings.Default.EnabledAssemblers)
                {
                    if (s == name)
                    {
                        newAssembler.Enabled = true;
                    }
                }

                Assemblers.Add(newAssembler.Name, newAssembler);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(String.Format("Error reading value '{0}' from assembler prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
            }
        }

        private void InterpretFurnace(String name, LuaTable values)
        {
            try
            {
                Assembler newFurnace = new Assembler(name);

                newFurnace.Icon = GetIconAndColor(values).Icon;
                newFurnace.ModuleSlots = GetLuaValueOrDefault<int>(values, "module_slots", true, 0);
                if (newFurnace.ModuleSlots == 0)
                {
                    var moduleTable = GetLuaValueOrDefault<LuaTable>(values, "module_specification", true);
                    if (moduleTable != null)
                    {
                        newFurnace.ModuleSlots = GetLuaValueOrDefault<int>(moduleTable, "module_slots", true, 0);
                    }
                }
                newFurnace.Speed = GetLuaValueOrDefault<float>(values, "crafting_speed", true, -1f);
                if (newFurnace.Speed == -1f)
                {   //In case we're still on Factorio 0.10
                    newFurnace.Speed = GetLuaValueOrDefault<float>(values, "smelting_speed");
                }

                LuaTable categories = GetLuaValueOrDefault<LuaTable>(values, "crafting_categories", true);
                if (categories == null)
                {   //Another 0.10 compatibility thing.
                    categories = GetLuaValueOrDefault<LuaTable>(values, "smelting_categories");
                }
                foreach (String category in categories.Values)
                {
                    newFurnace.Categories.Add(category);
                }

                foreach (String s in Settings.Default.EnabledAssemblers)
                {
                    if (s == name)
                    {
                        newFurnace.Enabled = true;
                    }
                }

                Assemblers.Add(newFurnace.Name, newFurnace);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(String.Format("Error reading value '{0}' from furnace prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
            }
        }

        private void InterpretMiner(String name, LuaTable values)
        {
            try
            {
                Miner newMiner = new Miner(name);

                newMiner.Icon = GetIconAndColor(values).Icon;
                // 0.17 compat, mining_power no longer required
                newMiner.MiningPower = GetLuaValueOrDefault<float>(values, "mining_power", true, 1);
                newMiner.Speed = GetLuaValueOrDefault<float>(values, "mining_speed");
                newMiner.ModuleSlots = GetLuaValueOrDefault<int>(values, "module_slots", true, 0);
                if (newMiner.ModuleSlots == 0)
                {
                    var moduleTable = GetLuaValueOrDefault<LuaTable>(values, "module_specification", true);
                    if (moduleTable != null)
                    {
                        newMiner.ModuleSlots = GetLuaValueOrDefault<int>(moduleTable, "module_slots", true, 0);
                    }
                }

                LuaTable categories = GetLuaValueOrDefault<LuaTable>(values, "resource_categories");
                if (categories != null)
                {
                    foreach (String category in categories.Values)
                    {
                        newMiner.ResourceCategories.Add(category);
                    }
                }

                foreach (String s in Settings.Default.EnabledMiners)
                {
                    if (s == name)
                    {
                        newMiner.Enabled = true;
                    }
                }

                Miners.Add(name, newMiner);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(String.Format("Error reading value '{0}' from miner prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
            }
        }

        private void InterpretResource(String name, LuaTable values)
        {
            try
            {
                if (values["minable"] == null)
                {
                    return; //This means the resource is not usable by miners and is therefore not useful to us
                }
                Resource newResource = new Resource(name);
                newResource.Category = GetLuaValueOrDefault<string>(values, "category", true, "basic-solid");
                LuaTable minableTable = GetLuaValueOrDefault<LuaTable>(values, "minable", true);
                newResource.Hardness = GetLuaValueOrDefault<float>(minableTable, "hardness", true, 0.5f);
                newResource.Time = GetLuaValueOrDefault<float>(minableTable, "mining_time");

                if (minableTable["result"] != null)
                {
                    newResource.result = GetLuaValueOrDefault<string>(minableTable, "result");
                }
                else
                {
                    try
                    {
                        newResource.result = ((minableTable["results"] as LuaTable)[1] as LuaTable)["name"] as String;
                    }
                    catch (Exception e)
                    {
                        throw new MissingPrototypeValueException(minableTable, "results", e.Message);
                    }
                }

                Resources.Add(name, newResource);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(String.Format("Error reading value '{0}' from resource prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
            }
        }

        private void InterpretModule(String name, LuaTable values)
        {
            try
            {
                float speedBonus = 0f;
                float productivityBonus = 0f;

                LuaTable effectTable = GetLuaValueOrDefault<LuaTable>(values, "effect");
                LuaTable speed = GetLuaValueOrDefault<LuaTable>(effectTable, "speed", true);
                if (speed != null)
                {
                    speedBonus = GetLuaValueOrDefault<float>(speed, "bonus", true, -1f);
                }

                LuaTable productivity = GetLuaValueOrDefault<LuaTable>(effectTable, "productivity", true);
                if (productivity != null)
                {
                    productivityBonus = GetLuaValueOrDefault<float>(productivity, "bonus", true, -1f);
                }

                var limitations = GetLuaValueOrDefault<LuaTable>(values, "limitation", true);
                HashSet<String> allowedIn = null;
                if (limitations != null)
                {
                    allowedIn = new HashSet<string>();
                    foreach (var recipe in limitations.Values)
                    {
                        allowedIn.Add((string)recipe);
                    }
                }

                Module newModule = new Module(name, speedBonus, productivityBonus, allowedIn);

                foreach (String s in Settings.Default.EnabledModules)
                {
                    if (s == name)
                    {
                        newModule.Enabled = true;
                    }
                }

                Modules.Add(name, newModule);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(String.Format("Error reading value '{0}' from module prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
            }
        }

        /*
        private void InterpretInserter(String name, LuaTable values)
        {
            try
            {
                float rotationSpeed = GetLuaValueOrDefault<float>(values, "rotation_speed");
                Inserter newInserter = new Inserter(name);
                newInserter.RotationSpeed = rotationSpeed;
                newInserter.Icon = GetIcon(values);

                Inserters.Add(name, newInserter);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(String.Format("Error reading value '{0}' from inserter prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
            }
        }
        */

        private void AddLuaPackagePath(Lua lua, string dir)
        {
            try
            {
                string escapedDir = (dir + Path.DirectorySeparatorChar).Replace(@"\", @"\\").Replace("'", @"\'");
                // Inserting the given path at the front of package.path. Without this mods might import files from other mods.
                string luaCommand = String.Format("package.path = '{0}?.lua;' .. package.path", escapedDir);
                lua.DoString(luaCommand);
            }
            catch (Exception e)
            {
                FailedPaths[dir] = e;
            }
        }

        private static T GetLuaValueOrDefault<T>(LuaTable table, String key, Boolean canBeMissing = false, T defaultValue = default(T))
        {
            object value = table[key];
            Type actualType = typeof(T);
            if (value == null)
            {
                if (canBeMissing)
                {
                    return defaultValue;
                }
                else
                {
                    throw new MissingPrototypeValueException(table, key, "Key is missing");
                }
            }
            if (actualType.IsGenericType && actualType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                actualType = Nullable.GetUnderlyingType(actualType);
            }
            try
            {
                return (T)Convert.ChangeType(value, actualType);
            }
            catch (FormatException)
            {
                throw new MissingPrototypeValueException(table, key, String.Format("Generics cast failed for ('{0}')", value));
            }
        }

        private static T GetLuaValue<T>(LuaTable table, double key, Boolean canBeMissing = false, T defaultValue = default(T))
        {
            object value = table[key];
            Type actualType = typeof(T);
            if (value == null)
            {
                if (canBeMissing)
                {
                    return defaultValue;
                }
                else
                {
                    throw new MissingPrototypeValueException(table, key.ToString(), "Key is missing");
                }
            }
            if (actualType.IsGenericType && actualType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                actualType = Nullable.GetUnderlyingType(actualType);
            }
            try
            {
                return (T)Convert.ChangeType(value, actualType);
            }
            catch (FormatException)
            {
                throw new MissingPrototypeValueException(table, key.ToString(), String.Format("Generics cast failed for ('{0}')", value));
            }
        }

        private IconColorPair GetIconAndColor(LuaTable values)
        {
            string fileName = GetLuaValueOrDefault<string>(values, "icon", true);
            int defaultIconSize = GetLuaValueOrDefault<int>(values, "icon_size", true, 0);
            string mipmaps = GetLuaValueOrDefault<string>(values, "icon_mipmaps", true);
            if (mipmaps != null)
                defaultIconSize = 4 * Convert.ToInt32(Math.Pow(2, Convert.ToDouble(mipmaps)));

            IconInfo mIconInfo = new IconInfo(fileName, defaultIconSize);
            List<IconInfo> iconInfos = new List<IconInfo>();

            LuaTable icons = GetLuaValueOrDefault<LuaTable>(values, "icons", true);
            if (icons != null)
            {
                List<double> iconKeys = new List<double>();
                foreach (var s in icons.Keys)
                {
                    iconKeys.Add(Convert.ToDouble(s));
                }
                foreach (double iconKey in iconKeys)
                {
                    LuaTable iconTable = GetLuaValue<LuaTable>(icons, iconKey, true);
                    if (iconTable != null)
                    {
                        List<string> iconTableKeys = new List<string>();
                        foreach (var s in iconTable.Keys)
                        {
                            iconTableKeys.Add(Convert.ToString(s));
                        }
                        // icon size
                        int iconSize = iconTableKeys.Contains("icon_size") ? Convert.ToInt32(iconTable["icon_size"]) : (defaultIconSize > 0 ? defaultIconSize : 32);
                        double iconScale = iconTableKeys.Contains("scale") ? Convert.ToDouble(iconTable["scale"]) : 0;
                        if(iconScale == 0) iconScale = iconTableKeys.Contains("icon_scale") ? Convert.ToDouble(iconTable["icon_scale"]) : 0; //old version naming convention

                        if (!iconTableKeys.Contains("icon"))
                            continue;

                        IconInfo iconInfo = new IconInfo(Convert.ToString(iconTable["icon"]), iconSize);
                        iconInfo.iconScale = iconScale;

                        // tint
                        LuaTable tintTable = null;
                        if (iconTableKeys.Contains("tint"))
                            tintTable = (LuaTable)iconTable["tint"];
                        if (tintTable != null)
                        {
                            iconInfo.SetIconTint(
                                Convert.ToDouble(tintTable["a"]),
                                Convert.ToDouble(tintTable["r"]),
                                Convert.ToDouble(tintTable["g"]),
                                Convert.ToDouble(tintTable["b"]));
                        }

                        // offset
                        if (iconTableKeys.Contains("shift"))
                        {
                            LuaTable offsetTable = (LuaTable)iconTable["shift"];
                            if (offsetTable != null)
                            {
                                iconInfo.iconOffset = new Point(
                                (int)Math.Round(Convert.ToDouble(GetLuaValue<string>(offsetTable, 1, true))),
                                (int)Math.Round(Convert.ToDouble(GetLuaValue<string>(offsetTable, 2, true))));
                            }
                        }

                        iconInfos.Add(iconInfo);
                    }
                }
            }
            return IconProcessor.GetIconAndColor(mIconInfo, iconInfos, 32);
        }

    }
}
