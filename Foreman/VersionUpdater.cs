using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public static class VersionUpdater {
        /// <summary>Save-file schema version (distinct from application assembly version).</summary>
        public const int SaveFormatVersion = 7;

        /// <summary>Supported save versions for automatic upgrade (see <see cref="UpdateSave"/> / <see cref="UpdateGraph"/>).</summary>
        public static readonly IReadOnlySet<int> SupportedSaveVersions = new HashSet<int> { 2, 3, 4, 5, 6, 7 };


        public static JObject? UpdateSave(JObject original, DataCache cache) {
            if (original["Version"] == null || JsonTokens.AsString(original["Object"]) != "ProductionGraphViewer") {
                if (original["Nodes"] is JArray nodesOld && original["NodeLinks"] is JArray linksOld && original["ElementLocations"] is JArray locsOld) {
                    //this is most likely the 'original' foreman graph. At the moment there isnt a conversion in place to bring it up to current standard (Feature will be added later)
                    JObject updated = new JObject();
                    updated["Version"] = 2;
                    updated["Object"] = "ProductionGraphViewer";

                    updated["SavedPresetName"] = cache.PresetName; //we will import into the currently selected preset. Any failures are handled as missings.
                    JArray enabledModsArr = original["EnabledMods"] as JArray ?? new JArray();
                    updated["IncludedMods"] = new JArray(enabledModsArr.Select(t => t.ToString() + "|0").ToList());

                    updated["Unit"] = original["Unit"]; //original is per sec then per min, which maps nicely to our new units 
                    updated["ViewOffset"] = string.Format("{0}, {1}", 0, 0);
                    updated["ViewScale"] = 1;

                    updated["ExtraProdForNonMiners"] = false;
                    updated["AssemblerSelectorStyle"] = (int)AssemblerSelector.Style.Best;
                    updated["ModuleSelectorStyle"] = (int)ModuleSelector.Style.Productivity;
                    updated["FuelPriorityList"] = new JArray();

                    updated["EnabledRecipes"] = original["EnabledRecipes"];
                    JArray enabledAssemblersArr = original["EnabledAssemblers"] as JArray ?? new JArray();
                    updated["EnabledAssemblers"] = enabledAssemblersArr;
                    if (original["EnabledMiners"] is JArray enabledMinersArr) {
                        foreach (JToken t in enabledMinersArr) {
                            if (JsonTokens.AsString(t) is string miner)
                                enabledAssemblersArr.Add(miner);
                        }
                    }

                    updated["EnabledModules"] = original["EnabledModules"];
                    updated["EnabledBeacons"] = new JArray();

                    updated["OldImport"] = true; //special flag for the graph informing it that this is an old save

                    JObject updatedGraph = new JObject();
                    updated["ProductionGraph"] = updatedGraph;

                    updatedGraph["Version"] = 2;
                    updatedGraph["Object"] = "ProductionGraph";

                    updatedGraph["IncludedAssemblers"] = new JArray(new string[] { "###NONE-ASSEMBLER###" }); //there is no info in old foreman files about assembler status. This will make all assemblers be 'missing', but this can be solved by auto-setting assembler for all nodes after import

                    updatedGraph["IncludedModules"] = new JArray(); //no info - thus none
                    updatedGraph["IncludedBeacons"] = new JArray(); //no info - thus none

                    //item processing
                    HashSet<string> includedItems = new HashSet<string>();
                    foreach (string item in nodesOld.Where(t => JsonTokens.AsString(t["NodeType"]) is "PassThrough" or "Supply" or "Consumer").Select(t => JsonTokens.AsString(t["ItemName"])).OfType<string>())
                        includedItems.Add(item);
                    foreach (string item in linksOld.Select(t => JsonTokens.AsString(t["Item"])).OfType<string>())
                        includedItems.Add(item);
                    updatedGraph["IncludedItems"] = new JArray(includedItems);

                    //recipe processing
                    Dictionary<string, Tuple<HashSet<string>, HashSet<string>>> recipeFossils = new Dictionary<string, Tuple<HashSet<string>, HashSet<string>>>();
                    Dictionary<int, string> recipeNames = new Dictionary<int, string>();

                    JArray includedRecipes = new JArray();
                    updatedGraph["IncludedRecipes"] = includedRecipes;
                    Dictionary<string, int> recipeIDs = new Dictionary<string, int>();

                    for (int i = 0; i < nodesOld.Count; i++) {
                        JToken node = nodesOld[i];
                        if (JsonTokens.AsString(node["NodeType"]) == "Recipe") {
                            if (JsonTokens.AsString(node["RecipeName"]) is not string recipeName)
                                continue;
                            recipeNames.Add(i, recipeName);
                            if (!recipeFossils.ContainsKey(recipeName))
                                recipeFossils.Add(recipeName, new Tuple<HashSet<string>, HashSet<string>>(new HashSet<string>(), new HashSet<string>()));
                        }
                    }

                    foreach (JToken link in linksOld) {
                        if (JsonTokens.AsInt32(link["Supplier"]) is not int supplierId || JsonTokens.AsInt32(link["Consumer"]) is not int consumerId)
                            continue;
                        if (JsonTokens.AsString(link["Item"]) is not string item)
                            continue;
                        if (recipeNames.ContainsKey(consumerId))
                            recipeFossils[recipeNames[consumerId]].Item1.Add(item);
                        if (recipeNames.ContainsKey(supplierId))
                            recipeFossils[recipeNames[supplierId]].Item2.Add(item);
                    }

                    foreach (var recipeFossil in recipeFossils) {
                        JObject? includedRecipe = null;

                        if (cache.Recipes.ContainsKey(recipeFossil.Key)) {
                            Recipe recipe = cache.Recipes[recipeFossil.Key];
                            bool fits = true;
                            foreach (string ingredient in recipeFossil.Value.Item1)
                                fits &= cache.Items.ContainsKey(ingredient) && recipe.IngredientSet.ContainsKey(cache.Items[ingredient]);
                            foreach (string product in recipeFossil.Value.Item2)
                                fits &= cache.Items.ContainsKey(product) && recipe.ProductSet.ContainsKey(cache.Items[product]);
                            if (fits) {
                                JObject ingredients = new JObject();
                                foreach (Item ingredient in recipe.IngredientList)
                                    ingredients[ingredient.Name] = recipe.IngredientSet[ingredient];

                                JObject products = new JObject();
                                foreach (Item product in recipe.ProductList)
                                    products[product.Name] = recipe.ProductSet[product];

                                includedRecipe = new JObject
                                {
                                    {"Name", recipe.Name },
                                    {"RecipeID", includedRecipes.Count },
                                    {"isMissing", false },
                                    {"Ingredients", ingredients },
                                    {"Products", products }
                                };
                            }
                        }

                        if (includedRecipe == null) {
                            JObject ingredients = new JObject();
                            foreach (string ingredient in recipeFossil.Value.Item1)
                                ingredients[ingredient] = 1;

                            JObject products = new JObject();
                            foreach (string product in recipeFossil.Value.Item2)
                                products[product] = 1;

                            includedRecipe = new JObject()
                            {
                                {"Name", recipeFossil.Key },
                                {"RecipeID", includedRecipes.Count },
                                {"isMissing", true },
                                {"Ingredients", ingredients },
                                {"Products", products }
                            };
                        }

                        if (includedRecipe is null)
                            throw new InvalidOperationException("Included recipe was not built.");

                        if (JsonTokens.AsString(includedRecipe["Name"]) is not string recipeNameKey)
                            continue;
                        if (JsonTokens.AsInt32(includedRecipe["RecipeID"]) is not int recipeId)
                            continue;
                        recipeIDs.Add(recipeNameKey, recipeId);
                        includedRecipes.Add(includedRecipe);
                    }

                    //node processing
                    JArray nodes = new JArray();
                    updatedGraph["Nodes"] = nodes;

                    List<string> nodeLocations = locsOld.Select(t => t.ToString()).ToList();
                    HashSet<int> processedNodeIDs = new HashSet<int>();

                    for (int i = 0; i < nodesOld.Count; i++) {
                        JToken originalNode = nodesOld[i];
                        int rateTypeInt = JsonTokens.AsInt32(originalNode["RateType"]) ?? (int)RateType.Auto;
                        JObject newNode = new JObject
                        {
                            { "RateType", rateTypeInt },
                            {"NodeID", i },
                            {"Location", nodeLocations[i] }
                        };
                        if (rateTypeInt == (int)RateType.Manual && JsonTokens.AsDouble(originalNode["DesiredRate"]) is double desiredRate)
                            newNode["DesiredRate"] = desiredRate;

                        processedNodeIDs.Add(i);
                        switch (JsonTokens.AsString(originalNode["NodeType"])) {
                            case "Consumer":
                                newNode["NodeType"] = (int)NodeType.Consumer;
                                if (JsonTokens.AsString(originalNode["ItemName"]) is string consumerItem)
                                    newNode["Item"] = consumerItem;
                                break;
                            case "PassThrough":
                                newNode["NodeType"] = (int)NodeType.Passthrough;
                                if (JsonTokens.AsString(originalNode["ItemName"]) is string passthroughItem)
                                    newNode["Item"] = passthroughItem;
                                break;
                            case "Supply":
                                newNode["NodeType"] = (int)NodeType.Supplier;
                                if (JsonTokens.AsString(originalNode["ItemName"]) is string supplyItem)
                                    newNode["Item"] = supplyItem;
                                break;
                            case "Recipe":
                                newNode["NodeType"] = (int)NodeType.Recipe;
                                if (JsonTokens.AsString(originalNode["RecipeName"]) is string recipeName && recipeIDs.TryGetValue(recipeName, out int mappedRecipeId))
                                    newNode["RecipeID"] = mappedRecipeId;
                                newNode["Neighbours"] = 0;
                                newNode["ExtraProductivity"] = 0;

                                newNode["RateType"] = (int)RateType.Auto; //we switched to an assembler based approach, which unfortunately cant be carried over

                                newNode["Assembler"] = "###NONE-ASSEMBLER###";
                                newNode["AssemblerModules"] = new JArray();
                                break;
                            default:
                                processedNodeIDs.Remove(i);
                                break;
                        }

                        nodes.Add(newNode);
                    }

                    //node link processing
                    JArray nodeLinks = new JArray();
                    updatedGraph["NodeLinks"] = nodeLinks;

                    foreach (JToken link in linksOld) {
                        if (JsonTokens.AsInt32(link["Supplier"]) is not int supplierId || JsonTokens.AsInt32(link["Consumer"]) is not int consumerId)
                            continue;
                        if (JsonTokens.AsString(link["Item"]) is not string itemLink)
                            continue;

                        if (processedNodeIDs.Contains(supplierId) && processedNodeIDs.Contains(consumerId))
                            nodeLinks.Add(new JObject
                            {
                                {"SupplierID", supplierId },
                                {"ConsumerID", consumerId },
                                {"Item", itemLink }
                            });
                    }
                    original = updated;
                } else {
                    //unknown file format
                    MessageBox.Show("Unknown file format.", "Cant load save", MessageBoxButtons.OK);
                    return null;
                }
            }

            if (JsonTokens.AsInt32(original["Version"]) is int saveVersion) {
                if (saveVersion == 1) {
                    //Version update 1 -> 2:
                    //	Graph now has the extra productivity for non-miners value
                    original["Version"] = 2;

                    original["ExtraProdForNonMiners"] = false;
                }

                if (JsonTokens.AsInt32(original["Version"]) is int versionBeforeFive && versionBeforeFive < 5) {
                    //Version update 2 -> 6:
                    //	No changes in main save (all changes are within the graph)
                    original["Version"] = 6;
                }

                if (JsonTokens.AsInt32(original["Version"]) is 6) {
                    //Version update 7:
                    //  Added EnabledQualities

                    JArray qualities = new JArray();
                    foreach (Quality quality in cache.Qualities.Values.Where(q => q.Enabled))
                        qualities.Add(quality.Name);
                    original["EnabledQualities"] = qualities;

                    original["Version"] = 7;
                }
            }

            return original;
        }

        public static JObject? UpdateGraph(JObject original, DataCache cache) {
            if (original["Version"] == null || JsonTokens.AsString(original["Object"]) != "ProductionGraph") {
                //this is most likely the 'original' foreman graph. At the moment there isnt a conversion in place to bring it up to current standard (Feature will be added later)
                MessageBox.Show("Imported graph could not be updated to current foreman version.\nSorry.", "Cant process import", MessageBoxButtons.OK);
                return null;
            }

            if (original["Nodes"] is not JArray graphNodeArray || original["NodeLinks"] is not JArray graphLinkArray)
                return null;

            if (JsonTokens.AsInt32(original["Version"]) is 1) {
                //Version update 1 -> 2:
                //	recipe node now has "ExtraPoductivity" value added
                original["Version"] = 2;

                foreach (JToken nodeJToken in graphNodeArray.Where(jt => JsonTokens.AsInt32(jt["NodeType"]) is int nodeTypeInt && (NodeType)nodeTypeInt == NodeType.Recipe).ToList())
                    nodeJToken["ExtraProductivity"] = 0;
            }

            if (JsonTokens.AsInt32(original["Version"]) is 2) {
                //Version update 2 -> 3:
                //	Nodes now have Direction parameter
                original["Version"] = 3;

                foreach (JToken nodeJToken in graphNodeArray.ToList())
                    nodeJToken["Direction"] = (int)NodeDirection.Up;
            }

            if (JsonTokens.AsInt32(original["Version"]) is 3) {
                //Version update 3 -> 4:
                //	Passthrough nodes now have SDraw parameter
                original["Version"] = 4;

                foreach (JToken nodeJToken in graphNodeArray.Where(n => JsonTokens.AsInt32(n["NodeType"]) is int passthroughType && (NodeType)passthroughType == NodeType.Passthrough).ToList())
                    nodeJToken["SDraw"] = true;
            }

            if (JsonTokens.AsInt32(original["Version"]) is 4) {
                //Version update 4 -> 5:
                //	ProductionGraph gained new properties:
                //		EnableExtraProductivityForNonMiners
                //		DefaultNodeDirection
                //		Solver_PullOutputNodes
                //		Solver_PullOutputNodesPower
                //		Solver_LowPriorityPower
                original["Version"] = 5;

                original["EnableExtraProductivityForNonMiners"] = false;
                original["DefaultNodeDirection"] = (int)NodeDirection.Up;
                original["Solver_PullOutputNodes"] = false;
                original["Solver_PullOutputNodesPower"] = 1f;
                original["Solver_LowPriorityPower"] = 2f;
            }

            if (JsonTokens.AsInt32(original["Version"]) is 5) {
                //Version update 5 -> 6:
                //  All nodes now feature a unified 'DesiredSetValue' that replaces the "DesiredAssemblers" from recipe nodes and "DesiredRatePerSec" from all other nodes
                //  This value is specific to each node type (ex: recipe = #assemblers, spoil = #stacks, grow = #tiles, most other nodes = #throughput/s)

                //  Also a new group was added to represent plant processes (IncludedPlantProcesses) - old saves will not have anything here, so just a blank node is fine

                foreach (JToken nodeJToken in graphNodeArray) {
                    if (JsonTokens.AsDouble(nodeJToken["DesiredAssemblers"]) is double desiredAssemblers)
                        nodeJToken["DesiredSetValue"] = desiredAssemblers;
                    //if (nodeJToken["DesiredRatePerSec"] != null)
                    //    nodeJToken["DesiredSetValue"] = (double)nodeJToken["DesiredRatePerSec"];
                    if (JsonTokens.AsDouble(nodeJToken["DesiredRate"]) is double desiredRate)
                        nodeJToken["DesiredSetValue"] = desiredRate;
                }

                original["IncludedPlantProcesses"] = new JArray();

                original["Version"] = 6;
            }

            if (JsonTokens.AsInt32(original["Version"]) is 6) {
                //Version update 6 -> 7:
                //  Added 'included qualities'  (list of included qualities set as name = level, include only the 'default' normal quality)
                //  Added 'maxQualityIterations'  (int value representing max number of quality tiers a recipe node will output with quality modules)
                //  Added quality options for recipes, assemblers, beacons, modules, and items

                string defaultQualityName = cache.DefaultQuality?.Name ?? "normal";

                JArray qualities = new JArray();
                JObject qualityJObject = new JObject
                {
                    { "Key", "normal" },
                    { "Value", 0 }
                };
                qualities.Add(qualityJObject);

                original["IncludedQualities"] = qualities;
                original["MaxQualitySteps"] = 5; //5 is the base number of quality modules in factorio, so its a nice value (using the current max length value could cause issues when combined with those '200 quality' mods)
                original["DefaultQulity"] = defaultQualityName;

                foreach (JToken nodeJToken in graphNodeArray) {
                    if (JsonTokens.AsInt32(nodeJToken["NodeType"]) is not int nodeTypeInt)
                        continue;
                    switch ((NodeType)nodeTypeInt) {
                        case NodeType.Passthrough:
                        case NodeType.Supplier:
                        case NodeType.Consumer:
                        case NodeType.Spoil:
                        case NodeType.Plant:
                            nodeJToken["BaseQuality"] = defaultQualityName;
                            break;

                        case NodeType.Recipe:
                            nodeJToken["RecipeQuality"] = defaultQualityName;
                            nodeJToken["AssemblerQuality"] = defaultQualityName;

                            JArray newAssemblerModules = new JArray();
                            if (nodeJToken["AssemblerModules"] is JArray asmModArray) {
                                foreach (JToken module in asmModArray) {
                                    if (JsonTokens.AsString(module) is string moduleName)
                                        newAssemblerModules.Add(new JObject { ["Name"] = moduleName, ["Quality"] = defaultQualityName });
                                }
                            }
                            nodeJToken["AssemblerModules"] = newAssemblerModules;

                            if (nodeJToken["Beacon"] != null) {
                                nodeJToken["BeaconQuality"] = defaultQualityName;
                                JArray newBeaconModules = new JArray();
                                if (nodeJToken["BeaconModules"] is JArray beaconModArray) {
                                    foreach (JToken module in beaconModArray) {
                                        if (JsonTokens.AsString(module) is string beaconModuleName)
                                            newBeaconModules.Add(new JObject { ["Name"] = beaconModuleName, ["Quality"] = defaultQualityName });
                                    }
                                }
                                nodeJToken["BeaconModules"] = newBeaconModules;
                            }

                            break;
                    }
                }

                foreach (JToken linkJToken in graphLinkArray)
                    linkJToken["Quality"] = defaultQualityName;

                original["Version"] = 7;
            }

            return original;
        }
    }
}