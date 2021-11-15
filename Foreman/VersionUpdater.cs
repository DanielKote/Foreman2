using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public static class VersionUpdater
	{
		//at some point we need to come back here and actuall fill in the version updater from the base foreman to the current version.


		public static JObject UpdateSave(JObject original, DataCache cache)
		{
			if (original["Version"] == null || original["Object"] == null || (string)original["Object"] != "ProductionGraphViewer")
			{
				if (original["Nodes"] != null && original["NodeLinks"] != null && original["ElementLocations"] != null)
				{
					//this is most likely the 'original' foreman graph. At the moment there isnt a conversion in place to bring it up to current standard (Feature will be added later)
					JObject updated = new JObject();
					updated["Version"] = 2;
					updated["Object"] = "ProductionGraphViewer";

					updated["SavedPresetName"] = cache.PresetName; //we will import into the currently selected preset. Any failures are handled as missings.
					updated["IncludedMods"] = new JArray(original["EnabledMods"].Select(t => (string)t + "|0").ToList());

					updated["Unit"] = original["Unit"]; //original is per sec then per min, which maps nicely to our new units 
					updated["ViewOffset"] = string.Format("{0}, {1}", 0, 0);
					updated["ViewScale"] = 1;

					updated["ExtraProdForNonMiners"] = false;
					updated["AssemblerSelectorStyle"] = (int)AssemblerSelector.Style.Best;
					updated["ModuleSelectorStyle"] = (int)ModuleSelector.Style.Productivity;
					updated["FuelPriorityList"] = new JArray();

					updated["EnabledRecipes"] = original["EnabledRecipes"];
					updated["EnabledAssemblers"] = original["EnabledAssemblers"];
					foreach (string miner in original["EnabledMiners"].Select(t => (string)t))
						((JArray)updated["EnabledAssemblers"]).Add(miner);

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
					foreach (string item in original["Nodes"].Where(t => (string)t["NodeType"] == "PassThrough" || (string)t["NodeType"] == "Supply" || (string)t["NodeType"] == "Consumer").Select(t => (string)t["ItemName"]))
						includedItems.Add(item);
					foreach (string item in original["NodeLinks"].Select(t => (string)t["Item"]))
						includedItems.Add(item);
					updatedGraph["IncludedItems"] = new JArray(includedItems);

					//recipe processing
					Dictionary<string, Tuple<HashSet<string>, HashSet<string>>> recipeFossils = new Dictionary<string, Tuple<HashSet<string>, HashSet<string>>>();
					Dictionary<int, string> recipeNames = new Dictionary<int, string>();

					JArray includedRecipes = new JArray();
					updatedGraph["IncludedRecipes"] = includedRecipes;
					Dictionary<string, int> recipeIDs = new Dictionary<string, int>();

					for(int i = 0; i < ((JArray)original["Nodes"]).Count; i++)
					{
						JToken node = original["Nodes"][i];
						if((string)node["NodeType"] == "Recipe")
						{
							recipeNames.Add(i, (string)node["RecipeName"]);
							if(!recipeFossils.ContainsKey((string)node["RecipeName"]))
								recipeFossils.Add((string)node["RecipeName"], new Tuple<HashSet<string>, HashSet<string>>(new HashSet<string>(), new HashSet<string>()));
						}
					}

					foreach(JToken link in original["NodeLinks"])
					{
						int supplierId = (int)link["Supplier"];
						int consumerId = (int)link["Consumer"];
						string item = (string)link["Item"];
						if (recipeNames.ContainsKey(consumerId))
							recipeFossils[recipeNames[consumerId]].Item1.Add(item);
						if (recipeNames.ContainsKey(supplierId))
							recipeFossils[recipeNames[supplierId]].Item2.Add(item);
					}

					foreach(var recipeFossil in recipeFossils)
					{
						JObject includedRecipe = null;

						if(cache.Recipes.ContainsKey(recipeFossil.Key))
						{
							Recipe recipe = cache.Recipes[recipeFossil.Key];
							bool fits = true;
							foreach (string ingredient in recipeFossil.Value.Item1)
								fits &= cache.Items.ContainsKey(ingredient) && recipe.IngredientSet.ContainsKey(cache.Items[ingredient]);
							foreach (string product in recipeFossil.Value.Item2)
								fits &= cache.Items.ContainsKey(product) && recipe.ProductSet.ContainsKey(cache.Items[product]);
							if(fits)
							{
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

						if(includedRecipe == null)
						{
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

						recipeIDs.Add((string)includedRecipe["Name"], (int)includedRecipe["RecipeID"]);
						includedRecipes.Add(includedRecipe);
					}

					//node processing
					JArray nodes = new JArray();
					updatedGraph["Nodes"] = nodes;

					List<string> nodeLocations = original["ElementLocations"].Select(t => (string)t).ToList();
					HashSet<int> processedNodeIDs = new HashSet<int>();

					for (int i = 0; i < original["Nodes"].Count(); i++)
					{
						JToken originalNode = original["Nodes"][i];
						JObject newNode = new JObject
						{
							{ "RateType", (int)originalNode["RateType"] },
							{"NodeID", i },
							{"Location", nodeLocations[i] }
						};
						if ((int)newNode["RateType"] == (int)RateType.Manual)
							newNode["DesiredRate"] = (double)originalNode["DesiredRate"];

						processedNodeIDs.Add(i);
						switch((string)originalNode["NodeType"])
						{
							case "Consumer":
								newNode["NodeType"] = (int)NodeType.Consumer;
								newNode["Item"] = (string)originalNode["ItemName"];
								break;
							case "PassThrough":
								newNode["NodeType"] = (int)NodeType.Passthrough;
								newNode["Item"] = (string)originalNode["ItemName"];
								break;
							case "Supply":
								newNode["NodeType"] = (int)NodeType.Supplier;
								newNode["Item"] = (string)originalNode["ItemName"];
								break;
							case "Recipe":
								newNode["NodeType"] = (int)NodeType.Recipe;
								newNode["RecipeID"] = recipeIDs[(string)originalNode["RecipeName"]];
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

					foreach(JToken link in original["NodeLinks"])
					{
						int supplierId = (int)link["Supplier"];
						int consumerId = (int)link["Consumer"];
						string item = (string)link["Item"];

						if (processedNodeIDs.Contains(supplierId) && processedNodeIDs.Contains(consumerId))
							nodeLinks.Add(new JObject
							{
								{"SupplierID", supplierId },
								{"ConsumerID", consumerId },
								{"Item", item }
							});
					}
					original = updated;
				}
				else
				{
					//unknown file format
					MessageBox.Show("Unknown file format.", "Cant load save", MessageBoxButtons.OK);
					return null;
				}
			}

			if ((int)original["Version"] == 1)
			{
				//Version update 1 -> 2:
				//	Graph now has the extra productivity for non-miners value 
				original["Version"] = 2;

				original["ExtraProdForNonMiners"] = false;
			}

			if ((int)original["Version"] < 5)
			{
				//Version update 2 -> 5:
				//	No changes in main save (all changes are within the graph)
				original["Version"] = 5;
			}

			return original;
		}

		public static JToken UpdateGraph(JToken original)
		{
			if (original["Version"] == null || original["Object"] == null || (string)original["Object"] != "ProductionGraph")
			{
				//this is most likely the 'original' foreman graph. At the moment there isnt a conversion in place to bring it up to current standard (Feature will be added later)
				MessageBox.Show("Imported graph could not be updated to current foreman version.\nSorry.", "Cant process import", MessageBoxButtons.OK);
				return null;
			}

			if((int)original["Version"] == 1)
			{
				//Version update 1 -> 2:
				//	recipe node now has "ExtraPoductivity" value added
				original["Version"] = 2;

				foreach (JToken nodeJToken in original["Nodes"].Where(jt => (NodeType)(int)jt["NodeType"] == NodeType.Recipe).ToList())
					nodeJToken["ExtraProductivity"] = 0;
			}

			if ((int)original["Version"] == 2)
			{
				//Version update 2 -> 3:
				//	Nodes now have Direction parameter
				original["Version"] = 3;

				foreach (JToken nodeJToken in original["Nodes"].ToList())
					nodeJToken["Direction"] = (int)NodeDirection.Up;
			}

			if ((int)original["Version"] == 3)
			{
				//Version update 3 -> 4:
				//	Passthrough nodes now have SDraw parameter
				original["Version"] = 4;

				foreach (JToken nodeJToken in original["Nodes"].Where(n => (NodeType)(int)n["NodeType"] == NodeType.Passthrough).ToList())
					nodeJToken["SDraw"] = true;
			}

			if ((int)original["Version"] == 4)
			{
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

			return original;
		}
	}
}
