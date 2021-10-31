using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class DataCache
	{
		public string PresetName { get; private set; }

		public IEnumerable<Group> AvailableGroups { get { return groups.Values.Where(g => g.Available); } }
		public IEnumerable<Subgroup> AvailableSubgroups{ get { return subgroups.Values.Where(g => g.Available); } }
		public IEnumerable<Item> AvailableItems { get { return items.Values.Where(g => g.Available); } }
		public IEnumerable<Recipe> AvailableRecipes { get { return recipes.Values.Where(g => g.Available); } }

		//mods: <name, version>
		//others: <name, object>

		public IReadOnlyDictionary<string, string> IncludedMods { get { return includedMods; } }
		//public IReadOnlyDictionary<string, Technology> Technologies { get { return technologies; } } //at the moment technology isnt actually used. All the links are set up (tech to recipe, tech to tech, recipe to tech, etc), but enabling/disabling technologies does not impact anything. This is due to (A) not having a technology screen and (B) to allow save file loading of enabled status to just load enabled recipes and not care about tech->recipe inconsistencies (if any)
		public IReadOnlyDictionary<string, Group> Groups { get { return groups; } }
		public IReadOnlyDictionary<string, Subgroup> Subgroups { get { return subgroups; } }
		public IReadOnlyDictionary<string, Item> Items { get { return items; } }
		public IReadOnlyDictionary<string, Recipe> Recipes { get { return recipes; } }
		public IReadOnlyDictionary<string, Assembler> Assemblers { get { return assemblers; } }
		public IReadOnlyDictionary<string, Module> Modules { get { return modules; } }
		public IReadOnlyDictionary<string, Beacon> Beacons { get { return beacons; } }

		//missing objects are not linked properly and just have the minimal values necessary to function. They are just placeholders, and cant actually be added to graph except while importing. They are also not solved for.
		public Subgroup MissingSubgroup { get { return missingSubgroup; } }
		public IReadOnlyDictionary<string, Item> MissingItems { get { return missingItems; } }
		public IReadOnlyDictionary<string, Assembler> MissingAssemblers { get { return missingAssemblers; } }
		public IReadOnlyDictionary<string, Module> MissingModules { get { return missingModules; } }
		public IReadOnlyDictionary<string, Beacon> MissingBeacons { get { return missingBeacons; } }
		public IReadOnlyDictionary<RecipeShort, Recipe> MissingRecipes { get { return missingRecipes; } }

		public static Bitmap UnknownIcon { get { return IconCache.GetUnknownIcon(); } }
		private static Bitmap noBeaconIcon;
		public static Bitmap NoBeaconIcon { get { if (noBeaconIcon == null) noBeaconIcon = IconCache.GetIcon(Path.Combine("Graphics", "NoBeacon.png"), 64); return noBeaconIcon; } }
		//public Technology StartingTech { get { return startingTech; } }

		private Dictionary<string, string> includedMods; //name : version
		private Dictionary<string, Technology> technologies;
		private Dictionary<string, Group> groups;
		private Dictionary<string, Subgroup> subgroups;
		private Dictionary<string, Item> items;
		private Dictionary<string, Recipe> recipes;
		private Dictionary<string, Assembler> assemblers;
		private Dictionary<string, Module> modules;
		private Dictionary<string, Beacon> beacons;

		private Dictionary<string, Item> missingItems;
		private Dictionary<string, Assembler> missingAssemblers;
		private Dictionary<string, Module> missingModules;
		private Dictionary<string, Beacon> missingBeacons;
		private Dictionary<RecipeShort, Recipe> missingRecipes;

		private GroupPrototype extractionGroup;
		private SubgroupPrototype extractionSubgroupItems;
		private SubgroupPrototype extractionSubgroupFluids;
		private SubgroupPrototype extractionSubgroupFluidsOP; //offshore pumps
		private SubgroupPrototype missingSubgroup;
		private TechnologyPrototype startingTech;

		private const float defaultRecipeTime = 0.5f;

		public DataCache()
		{
			includedMods = new Dictionary<string, string>();
			technologies = new Dictionary<string, Technology>();
			groups = new Dictionary<string, Group>();
			subgroups = new Dictionary<string, Subgroup>();
			items = new Dictionary<string, Item>();
			recipes = new Dictionary<string, Recipe>();
			assemblers = new Dictionary<string, Assembler>();
			modules = new Dictionary<string, Module>();
			beacons = new Dictionary<string, Beacon>();

			extractionGroup = new GroupPrototype(this, "$g:extraction_group", "Resource Extraction", "zzzzzzz");
			extractionGroup.SetIconAndColor(new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "MiningIcon.png"), 64), Color.Gray));
			extractionSubgroupItems = new SubgroupPrototype(this, "$sg:extraction_items", "1");
			extractionSubgroupItems.myGroup = extractionGroup;
			extractionGroup.subgroups.Add(extractionSubgroupItems);
			extractionSubgroupFluids = new SubgroupPrototype(this, "$sg:extraction_fluids", "2");
			extractionSubgroupFluids.myGroup = extractionGroup;
			extractionGroup.subgroups.Add(extractionSubgroupFluids);
			extractionSubgroupFluidsOP = new SubgroupPrototype(this, "$sg:extraction_fluids_2", "3");
			extractionSubgroupFluidsOP.myGroup = extractionGroup;
			extractionGroup.subgroups.Add(extractionSubgroupFluidsOP);

			missingItems = new Dictionary<string, Item>();
			missingAssemblers = new Dictionary<string, Assembler>();
			missingModules = new Dictionary<string, Module>();
			missingBeacons = new Dictionary<string, Beacon>();
			missingRecipes = new Dictionary<RecipeShort, Recipe>(new RecipeShortNaInPrComparer());

			missingSubgroup = new SubgroupPrototype(this, "$MISSING-SG", "");
			missingSubgroup.myGroup = new GroupPrototype(this, "$MISSING-G", "MISSING", "");

			startingTech = new TechnologyPrototype(this, "", "");
		}

		public async Task LoadAllData(Preset preset, IProgress<KeyValuePair<int, string>> progress)
		{
			Clear();
			Dictionary<string, List<RecipePrototype>> craftingCategories = new Dictionary<string, List<RecipePrototype>>();
			Dictionary<string, List<RecipePrototype>> resourceCategories = new Dictionary<string, List<RecipePrototype>>();
			Dictionary<string, List<ItemPrototype>> fuelCategories = new Dictionary<string, List<ItemPrototype>>();
			Dictionary<Item, string> burnResults = new Dictionary<Item, string>();

			JObject jsonData = JObject.Parse(File.ReadAllText(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" })));
			Dictionary<string, IconColorPair> iconCache = await IconCache.LoadIconCache(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".dat" }), progress, 0, 90);
			PresetName = preset.Name;

			await Task.Run(() =>
			{
				progress.Report(new KeyValuePair<int, string>(90, "Processing Data...")); //this is SUPER quick, so we dont need to worry about timing stuff here

				groups.Add(extractionGroup.Name, extractionGroup);
				subgroups.Add(extractionSubgroupItems.Name, extractionSubgroupItems);
				subgroups.Add(extractionSubgroupFluids.Name, extractionSubgroupFluids);
				subgroups.Add(extractionSubgroupFluidsOP.Name, extractionSubgroupFluidsOP);

				//process each section
				foreach (var objJToken in jsonData["mods"].ToList())
					ProcessMod(objJToken);
				foreach (var objJToken in jsonData["subgroups"].ToList())
					ProcessSubgroup(objJToken);
				foreach (var objJToken in jsonData["groups"].ToList())
					ProcessGroup(objJToken, iconCache);
				foreach (var objJToken in jsonData["items"].ToList())
					ProcessItem(objJToken, iconCache, fuelCategories, burnResults);
				foreach (var objJToken in jsonData["fluids"].ToList())
					ProcessFluid(objJToken, iconCache);
				foreach (ItemPrototype item in items.Values)
					ProcessBurnItem(item, fuelCategories, burnResults); //link up any items with burn remains
				foreach (var objJToken in jsonData["recipes"].ToList())
					ProcessRecipe(objJToken, iconCache, craftingCategories);
				foreach (var objJToken in jsonData["resources"].ToList())
					ProcessResource(objJToken, resourceCategories);
				foreach (var objJToken in jsonData["modules"].ToList())
					ProcessModule(objJToken, iconCache);
				foreach (var objJToken in jsonData["technologies"].ToList())
					ProcessTechnology(objJToken, iconCache);
				foreach (var objJToken in jsonData["technologies"].ToList())
					ProcessTechnologyP2(objJToken); //required to properly link technology prerequisites
				foreach (var objJToken in jsonData["assemblers"].ToList())
					ProcessAssembler(objJToken, iconCache, craftingCategories);
				foreach (var objJToken in jsonData["miners"].ToList())
					ProcessMiner(objJToken, iconCache, resourceCategories);
				foreach (var objJToken in jsonData["offshorepumps"].ToList())
					ProcessOffshorePump(objJToken, iconCache);
				foreach (var objJtoken in jsonData["beacons"].ToList())
					ProcessBeacon(objJtoken, iconCache);

				//process assemblers, miners, and offshore pumps one more time to read the item-burner / fluid-burner information
				foreach (var objJtoken in jsonData["assemblers"].ToList())
					ProcessBurnerInfo(objJtoken, fuelCategories);
				foreach (var objJtoken in jsonData["miners"].ToList())
					ProcessBurnerInfo(objJtoken, fuelCategories);
				foreach (var objJtoken in jsonData["offshorepumps"].ToList())
					ProcessBurnerInfo(objJtoken, fuelCategories);


				//remove these temporary dictionaries (no longer necessary)
				craftingCategories.Clear();
				resourceCategories.Clear();
				fuelCategories.Clear();
				burnResults.Clear();

				//sort
				foreach (GroupPrototype g in groups.Values)
					g.SortSubgroups();
				foreach (SubgroupPrototype sg in subgroups.Values)
					sg.SortIRs();

				UpdateUnavailableStatus();
#if DEBUG
				//PrintAllAvailabilities();
#endif

				progress.Report(new KeyValuePair<int, string>(98, "Finalizing..."));
				progress.Report(new KeyValuePair<int, string>(100, "Done!"));
			});
		}

		public void Clear()
		{
			includedMods.Clear();
			technologies.Clear();
			groups.Clear();
			subgroups.Clear();
			items.Clear();
			recipes.Clear();
			assemblers.Clear();
			modules.Clear();
			beacons.Clear();

			missingItems.Clear();
			missingAssemblers.Clear();
			missingModules.Clear();
			missingBeacons.Clear();
			missingRecipes.Clear();
		}

		//------------------------------------------------------Import processing

		public void ProcessImportedItemsSet(IEnumerable<string> itemNames) //will ensure that all items are now part of the data cache -> existing ones (regular and missing) are skipped, new ones are added to MissingItems
		{
			foreach (string iItem in itemNames)
			{
				if (!items.ContainsKey(iItem) && !missingItems.ContainsKey(iItem)) //want to check for missing items too - in this case dont want duplicates
				{
					ItemPrototype missingItem = new ItemPrototype(this, iItem, iItem, false, missingSubgroup, "", true); //just assume it isnt a fluid. we dont honestly care (no temperatures)
					missingItems.Add(missingItem.Name, missingItem);
				}
			}
		}

		public void ProcessImportedAssemblersSet(IEnumerable<string> assemblerNames)
		{
			foreach(string iAssembler in assemblerNames)
			{
				if(!assemblers.ContainsKey(iAssembler) && !missingAssemblers.ContainsKey(iAssembler))
				{
					AssemblerPrototype missingAssembler = new AssemblerPrototype(this, iAssembler, iAssembler, false, true); //dont know, dont care about miner status :/
					missingAssemblers.Add(missingAssembler.Name, missingAssembler);
				}
			}
		}

		public void ProcessImportedModulesSet(IEnumerable<string> moduleNames)
		{
			foreach (string iModule in moduleNames)
			{
				if (!modules.ContainsKey(iModule) && !missingModules.ContainsKey(iModule))
				{
					ModulePrototype missingModule = new ModulePrototype(this, iModule, iModule, true);
					missingModules.Add(missingModule.Name, missingModule);
				}
			}
		}

		public void ProcessImportedBeaconsSet(IEnumerable<string> beaconNames)
		{
			foreach (string iBeacon in beaconNames)
			{
				if (!beacons.ContainsKey(iBeacon) && !missingBeacons.ContainsKey(iBeacon))
				{
					BeaconPrototype missingBeacon = new BeaconPrototype(this, iBeacon, iBeacon, true);
					missingBeacons.Add(missingBeacon.Name, missingBeacon);
				}
			}
		}

		public Dictionary<long, Recipe> ProcessImportedRecipesSet(IEnumerable<RecipeShort> recipeShorts) //will ensure all recipes are now part of the data cache -> each one is checked against existing recipes (regular & missing), and if it doesnt exist are added to MissingRecipes. Returns a set of links of original recipeID (NOT! the noew recipeIDs) to the recipe
		{
			Dictionary<long, Recipe> recipeLinks = new Dictionary<long, Recipe>();
			foreach (RecipeShort recipeShort in recipeShorts)
			{
				Recipe recipe = null;

				//recipe check #1 : does its name exist in database (note: we dont quite care about extra missing recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)
				bool recipeExists = recipes.ContainsKey(recipeShort.Name);
				if (recipeExists)
				{
					//recipe check #2 : do the number of ingredients & products match?
					recipe = recipes[recipeShort.Name];
					recipeExists &= recipeShort.Ingredients.Count == recipe.IngredientList.Count;
					recipeExists &= recipeShort.Products.Count == recipe.ProductList.Count;
				}
				if(recipeExists)
				{ 
					//recipe check #3 : do the ingredients & products from the loaded data match the actual recipe? (names, not quantities -> this is to allow some recipes to pass; ex: normal->expensive might change the values, but importing such a recipe should just use the 'correct' quantities and soft-pass the different recipe)
					foreach (string ingredient in recipeShort.Ingredients.Keys)
						recipeExists &= items.ContainsKey(ingredient) && recipe.IngredientSet.ContainsKey(items[ingredient]);
					foreach (string product in recipeShort.Products.Keys)
						recipeExists &= items.ContainsKey(product) && recipe.ProductSet.ContainsKey(items[product]);
				}
				if (!recipeExists)
				{
					bool missingRecipeExists = missingRecipes.ContainsKey(recipeShort);

					if (missingRecipeExists)
					{
						recipe = missingRecipes[recipeShort];
					}
					else
					{
						RecipePrototype missingRecipe = new RecipePrototype(this, recipeShort.Name, recipeShort.Name, missingSubgroup, "", true);
						foreach (var ingredient in recipeShort.Ingredients)
						{
							if (items.ContainsKey(ingredient.Key))
								missingRecipe.InternalOneWayAddIngredient((ItemPrototype)items[ingredient.Key], ingredient.Value);
							else
								missingRecipe.InternalOneWayAddIngredient((ItemPrototype)missingItems[ingredient.Key], ingredient.Value);
						}
						foreach (var product in recipeShort.Products)
						{
							if (items.ContainsKey(product.Key))
								missingRecipe.InternalOneWayAddProduct((ItemPrototype)items[product.Key], product.Value);
							else
								missingRecipe.InternalOneWayAddProduct((ItemPrototype)missingItems[product.Key], product.Value);
						}
						missingRecipes.Add(recipeShort, missingRecipe);
						recipe = missingRecipe;
					}
				}
				recipeLinks.Add(recipeShort.RecipeID, recipe);
			}
			return recipeLinks;
		}

		//------------------------------------------------------Static preset functions

		public static PresetInfo ReadPresetInfo(Preset preset)
		{
			Dictionary<string, string> mods = new Dictionary<string, string>();
			string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });
			if (!File.Exists(presetPath))
				return new PresetInfo(null, false, false);

			JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
			foreach (var objJToken in jsonData["mods"].ToList())
				mods.Add((string)objJToken["name"], (string)objJToken["version"]);

			return new PresetInfo(mods, (int)jsonData["difficulty"][0] == 1, (int)jsonData["difficulty"][1] == 1);
		}

		public static PresetErrorPackage TestPreset(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<RecipeShort> recipeShorts)
		{

			string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });
			if (!File.Exists(presetPath))
				return null;

			//parse preset (note: this is preset data, so we are guaranteed to only have one name per item/recipe/mod/etc.)
			JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
			HashSet<string> presetItems = new HashSet<string>();
			Dictionary<string, RecipeShort> presetRecipes = new Dictionary<string, RecipeShort>();
			Dictionary<string, string> presetMods = new Dictionary<string, string>();

			foreach (var objJToken in jsonData["mods"].ToList())
				presetMods.Add((string)objJToken["name"], (string)objJToken["version"]);
			foreach (var objJToken in jsonData["items"].ToList())
				presetItems.Add((string)objJToken["name"]);
			foreach (var objJToken in jsonData["fluids"].ToList())
				presetItems.Add((string)objJToken["name"]);

			foreach (var objJToken in jsonData["recipes"].ToList())
			{
				RecipeShort recipe = new RecipeShort((string)objJToken["name"]);
				foreach (var ingredientJToken in objJToken["ingredients"].ToList())
				{
					string ingredientName = (string)ingredientJToken["name"];
					if (recipe.Ingredients.ContainsKey(ingredientName))
						recipe.Ingredients[ingredientName] += (float)ingredientJToken["amount"];
					else
						recipe.Ingredients.Add(ingredientName, (float)ingredientJToken["amount"]);
				}
				foreach (var productJToken in objJToken["products"].ToList())
				{
					string productName = (string)productJToken["name"];
					if (recipe.Products.ContainsKey(productName))
						recipe.Products[productName] += (float)productJToken["amount"];
					else
						recipe.Products.Add(productName, (float)productJToken["amount"]);
				}
				presetRecipes.Add(recipe.Name, recipe);
			}

			//have to process mining & offshore pumps (since we convert them to recipes as well)
			foreach(var objJToken in jsonData["resources"])
			{
				if (objJToken["products"].Count() == 0)
					continue;

				RecipeShort recipe = new RecipeShort("$r:"+(string)objJToken["name"]);

				foreach (var productJToken in objJToken["products"])
				{
					string productName = (string)productJToken["name"];
					if (recipe.Products.ContainsKey(productName))
						recipe.Products[productName] += (float)productJToken["amount"];
					else
						recipe.Products.Add(productName, (float)productJToken["amount"]);
				}
				if (recipe.Products.Count == 0)
					continue;

				if (objJToken["required_fluid"] != null && (float)objJToken["fluid_amount"] != 0)
					recipe.Ingredients.Add((string)objJToken["required_fluid"], (float)objJToken["fluid_amount"]);

				presetRecipes.Add(recipe.Name, recipe);
			}

			foreach(var objJToken in jsonData["offshorepumps"])
			{
				string fluidName = (string)objJToken["fluid"];
				RecipeShort recipe = new RecipeShort("$r:" + fluidName);
				recipe.Products.Add(fluidName, 60);

				if (!presetRecipes.ContainsKey(recipe.Name))
					presetRecipes.Add(recipe.Name, recipe);
			}

			//compare to provided mod/item/recipe sets (recipes have a chance of existing in multitudes - aka: missing recipes)
			PresetErrorPackage errors = new PresetErrorPackage(preset);
			foreach (var mod in modList)
			{
				errors.RequiredMods.Add(mod.Key + "|" + mod.Value);

				if (!presetMods.ContainsKey(mod.Key))
					errors.MissingMods.Add(mod.Key + "|" + mod.Value);
				else if (presetMods[mod.Key] != mod.Value)
					errors.WrongVersionMods.Add(mod.Key + "|" + mod.Value + "|" + presetMods[mod.Key]);
			}
			foreach (var mod in presetMods)
				if (!modList.ContainsKey(mod.Key))
					errors.AddedMods.Add(mod.Key + "|" + mod.Value);

			foreach (string itemName in itemList)
			{
				errors.RequiredItems.Add(itemName);

				if (!presetItems.Contains(itemName))
					errors.MissingItems.Add(itemName);
			}

			foreach (RecipeShort recipeS in recipeShorts)
			{
				errors.RequiredRecipes.Add(recipeS.Name);
				if (recipeS.isMissing)
				{
					if (presetRecipes.ContainsKey(recipeS.Name) && recipeS.Equals(presetRecipes[recipeS.Name]))
						errors.ValidMissingRecipes.Add(recipeS.Name);
					else
						errors.IncorrectRecipes.Add(recipeS.Name);
				}
				else
				{
					if (!presetRecipes.ContainsKey(recipeS.Name))
						errors.MissingRecipes.Add(recipeS.Name);
					else if (!recipeS.Equals(presetRecipes[recipeS.Name]))
						errors.IncorrectRecipes.Add(recipeS.Name);
				}
			}

			return errors;
		}

		//------------------------------------------------------Data cache load helper functions (all the process functions from LoadAllData)

		private void ProcessMod(JToken objJToken)
		{
			includedMods.Add((string)objJToken["name"], (string)objJToken["version"]);
		}

		private void ProcessSubgroup(JToken objJToken)
		{
			SubgroupPrototype subgroup = new SubgroupPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["order"]);

			subgroups.Add(subgroup.Name, subgroup);
		}

		private void ProcessGroup(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
		{
			GroupPrototype group = new GroupPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				(string)objJToken["order"]);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				group.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			foreach (var subgroupJToken in objJToken["subgroups"])
			{
				((SubgroupPrototype)subgroups[(string)subgroupJToken]).myGroup = group;
				group.subgroups.Add((SubgroupPrototype)subgroups[(string)subgroupJToken]);
			}
			groups.Add(group.Name, group);
		}

		private void ProcessItem(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories, Dictionary<Item, string> burnResults)
		{
			ItemPrototype item = new ItemPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				false, //item (not a fluid)
				(SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
				(string)objJToken["order"]);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

			if (objJToken["fuel_category"] != null)
			{
				item.FuelValue = (float)objJToken["fuel_value"];
				if (!fuelCategories.ContainsKey((string)objJToken["fuel_category"]))
					fuelCategories.Add((string)objJToken["fuel_category"], new List<ItemPrototype>());
				fuelCategories[(string)objJToken["fuel_category"]].Add(item);
			}
			if (objJToken["burnt_result"] != null)
				burnResults.Add(item, (string)objJToken["burnt_result"]);

			items.Add(item.Name, item);
		}

		private void ProcessBurnItem(ItemPrototype item, Dictionary<string, List<ItemPrototype>> fuelCategories, Dictionary<Item, string> burnResults)
		{
			if (burnResults.ContainsKey(item))
				item.BurnResult = items[burnResults[item]];
		}

		private void ProcessFluid(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
		{
			ItemPrototype item = new ItemPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				true, //fluid
				(SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
				(string)objJToken["order"]);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

			item.DefaultTemperature = (double)objJToken["default_temperature"];
			if (objJToken["fuel_value"] != null)
				item.FuelValue = (float)objJToken["fuel_value"];

			items.Add(item.Name, item);
		}

		private void ProcessRecipe(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories)
		{
			RecipePrototype recipe = new RecipePrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				(SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
				(string)objJToken["order"]);

			recipe.Time = (float)objJToken["energy"] > 0 ? (float)objJToken["energy"] : defaultRecipeTime;
			if ((bool)objJToken["enabled"]) //due to the way the import of presets happens, enabled at this stage means the recipe is available without any research necessary (aka: available at start)
			{
				recipe.myUnlockTechnologies.Add(startingTech);
				startingTech.unlockedRecipes.Add(recipe);
			}

			string category = (string)objJToken["category"];
			if (!craftingCategories.ContainsKey(category))
				craftingCategories.Add(category, new List<RecipePrototype>());
			craftingCategories[category].Add(recipe);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				recipe.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
				recipe.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);

			foreach (var productJToken in objJToken["products"].ToList())
			{
				string name = (string)productJToken["name"];
				float amount = (float)productJToken["amount"];
				float temperature = 0;
				if ((string)productJToken["type"] == "fluid" && productJToken["temperature"] != null)
				{
					temperature = (float)productJToken["temperature"];
					if (((ItemPrototype)items[name]).DefaultTemperature != temperature)
						((ItemPrototype)items[name]).IsTemperatureDependent = true;
				}

				if (amount != 0)
				{
					recipe.InternalOneWayAddProduct((ItemPrototype)items[name], amount, temperature);
					((ItemPrototype)items[name]).productionRecipes.Add(recipe);
				}
			}

			foreach (var ingredientJToken in objJToken["ingredients"].ToList())
			{
				string name = (string)ingredientJToken["name"];
				float amount = (float)ingredientJToken["amount"];

				float minTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["minimum_temperature"] != null) ? (float)ingredientJToken["minimum_temperature"] : float.NegativeInfinity;
				float maxTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["maximum_temperature"] != null) ? (float)ingredientJToken["maximum_temperature"] : float.PositiveInfinity;

				if (amount != 0)
				{
					recipe.InternalOneWayAddIngredient((ItemPrototype)items[name], amount, minTemp, maxTemp);
					((ItemPrototype)items[name]).consumptionRecipes.Add(recipe);
				}
			}

			recipes.Add(recipe.Name, recipe);
		}

		private void ProcessResource(JToken objJToken, Dictionary<string, List<RecipePrototype>> resourceCategories)
		{
			if (objJToken["products"].Count() == 0)
				return;

			RecipePrototype recipe = new RecipePrototype(
				this,
				"$r:" + (string)objJToken["name"],
				(string)objJToken["localised_name"] + " Extraction",
				(string)objJToken["products"][0]["type"] == "fluid" ? extractionSubgroupFluids : extractionSubgroupItems,
				(string)objJToken["name"]);

			recipe.Time = (float)objJToken["mining_time"];

			foreach (var productJToken in objJToken["products"])
			{
				if (!items.ContainsKey((string)productJToken["name"]))
					continue;
				ItemPrototype product = (ItemPrototype)items[(string)productJToken["name"]];
				recipe.InternalOneWayAddProduct(product, (float)productJToken["amount"]);
				product.productionRecipes.Add(recipe);
			}
			if (recipe.productList.Count == 0)
				return;

			if (objJToken["required_fluid"] != null && (float)objJToken["fluid_amount"] != 0)
			{
				ItemPrototype reqLiquid = (ItemPrototype)items[(string)objJToken["required_fluid"]];
				recipe.InternalOneWayAddIngredient(reqLiquid, (float)objJToken["fluid_amount"]);
				reqLiquid.consumptionRecipes.Add(recipe);
			}

			recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

			string category = (string)objJToken["resource_category"];
			if (!resourceCategories.ContainsKey(category))
				resourceCategories.Add(category, new List<RecipePrototype>());
			resourceCategories[category].Add(recipe);

			recipe.myUnlockTechnologies.Add(startingTech);
			startingTech.unlockedRecipes.Add(recipe);

			recipes.Add(recipe.Name, recipe);
		}

		private void ProcessModule(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
		{
			ModulePrototype module = new ModulePrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"]);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				module.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
				module.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);

			module.SpeedBonus = (float)objJToken["module_effects_speed"];
			module.ProductivityBonus = (float)objJToken["module_effects_productivity"];
			module.ConsumptionBonus = (float)objJToken["module_effects_consumption"];
			module.PollutionBonus = (float)objJToken["module_effects_pollution"];
			module.Tier = (int)objJToken["tier"];

			foreach (var recipe in objJToken["limitations"])
			{
				if (recipes.ContainsKey((string)recipe)) //only add if the recipe is in the list of recipes (if it isnt, means it was deleted either in data phase of LUA or during foreman export cleanup)
				{
					((RecipePrototype)recipes[(string)recipe]).modules.Add(module);
					module.recipes.Add((RecipePrototype)recipes[(string)recipe]);
				}
			}

			if (objJToken["limitations"].Count() == 0) //means all recipes work
			{
				foreach (RecipePrototype recipe in recipes.Values)
				{
					recipe.modules.Add(module);
					module.recipes.Add(recipe);
				}
			}

			//string category = (string)objJToken["category"]; //apparently, this is USELESS! the allowance of modules into entites is based off of their bonus% values rather than categories (now? maybe it was different before?)

			modules.Add(module.Name, module);
		}

		private void ProcessTechnology(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
		{
			TechnologyPrototype technology = new TechnologyPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"]);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				technology.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

			technology.Available = !(bool)objJToken["hidden"] && (bool)objJToken["enabled"]; //not sure - factorio documentation states 'enabled' means 'available at start', but in this case 'enabled' being false seems to represent the technology not appearing on screen (same as hidden)??? I will just work with what tests show -> tech is available if it is enabled & not hidden.

			foreach (var recipe in objJToken["recipes"])
			{
				if (recipes.ContainsKey((string)recipe))
				{
					((RecipePrototype)recipes[(string)recipe]).myUnlockTechnologies.Add(technology);
					technology.unlockedRecipes.Add((RecipePrototype)recipes[(string)recipe]);
				}
			}

			technologies.Add(technology.Name, technology);
		}

		private void ProcessTechnologyP2(JToken objJToken)
		{
			TechnologyPrototype technology = (TechnologyPrototype)technologies[(string)objJToken["name"]];
			foreach (var prerequisite in objJToken["prerequisites"])
			{
				if (technologies.ContainsKey((string)prerequisite))
				{
					technology.prerequisites.Add((TechnologyPrototype)technologies[(string)prerequisite]);
					((TechnologyPrototype)technologies[(string)prerequisite]).postTechs.Add(technology);
				}
			}
		}

		private void ProcessAssembler(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories)
		{
			AssemblerPrototype assembler = new AssemblerPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				false);

			assembler.Speed = (float)objJToken["crafting_speed"];
			assembler.ModuleSlots = (int)objJToken["module_inventory_size"];
			assembler.BaseProductivityBonus = (float)objJToken["base_productivity"];

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				assembler.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
				assembler.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);
			if (objJToken["associated_items"] != null)
				foreach (string item in objJToken["associated_items"].Select(i => (string)i))
					if (items.ContainsKey(item))
						assembler.associatedItems.Add((ItemPrototype)items[item]);

			foreach (var categoryJToken in objJToken["crafting_categories"])
			{
				if (craftingCategories.ContainsKey((string)categoryJToken))
				{
					foreach (RecipePrototype recipe in craftingCategories[(string)categoryJToken])
					{
						recipe.assemblers.Add(assembler);
						assembler.recipes.Add(recipe);
					}
				}
			}

			List<string> allowedEffectsList = objJToken["allowed_effects"].Select(token => (string)token).ToList();
			bool[] allowedEffects = new bool[] {
				allowedEffectsList.Contains("consumption"),
				allowedEffectsList.Contains("speed"),
				allowedEffectsList.Contains("productivity"),
				allowedEffectsList.Contains("pollution") };

			foreach (ModulePrototype module in modules.Values.Where(module =>
				 (allowedEffects[0] || module.ConsumptionBonus == 0) &&
				 (allowedEffects[1] || module.SpeedBonus == 0) &&
				 (allowedEffects[2] || module.ProductivityBonus == 0) &&
				 (allowedEffects[3] || module.PollutionBonus == 0)))
			{
				module.assemblers.Add(assembler);
				assembler.modules.Add(module);
			}

			assemblers.Add(assembler.Name, assembler);
		}

		private void ProcessMiner(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> resourceCategories)
		{
			if (!items.ContainsKey((string)objJToken["name"])) //ex: character
				return;

			AssemblerPrototype assembler = new AssemblerPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				true);

			assembler.Speed = (float)objJToken["mining_speed"];
			assembler.ModuleSlots = (int)objJToken["module_inventory_size"];
			assembler.BaseProductivityBonus = objJToken["base_productivity"] == null? 0f : (float)objJToken["base_productivity"];

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				assembler.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
				assembler.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);
			if (objJToken["associated_items"] != null)
				foreach (string item in objJToken["associated_items"].Select(i => (string)i))
					if (items.ContainsKey(item))
						assembler.associatedItems.Add((ItemPrototype)items[item]);

			foreach (var categoryJToken in objJToken["resource_categories"])
			{
				if (resourceCategories.ContainsKey((string)categoryJToken))
				{
					foreach (RecipePrototype recipe in resourceCategories[(string)categoryJToken])
					{
						recipe.assemblers.Add(assembler);
						assembler.recipes.Add(recipe);
					}
				}
			}

			List<string> allowedEffectsList = objJToken["allowed_effects"].Select(token => (string)token).ToList();
			bool[] allowedEffects = new bool[] {
				allowedEffectsList.Contains("consumption"),
				allowedEffectsList.Contains("speed"),
				allowedEffectsList.Contains("productivity"),
				allowedEffectsList.Contains("pollution") };

			foreach (ModulePrototype module in modules.Values.Where(module =>
				 (allowedEffects[0] || module.ConsumptionBonus == 0) &&
				 (allowedEffects[1] || module.SpeedBonus == 0) &&
				 (allowedEffects[2] || module.ProductivityBonus == 0) &&
				 (allowedEffects[3] || module.PollutionBonus == 0)))
			{
				module.assemblers.Add(assembler);
				assembler.modules.Add(module);
			}
			assemblers.Add(assembler.Name, assembler);
		}

		private void ProcessOffshorePump(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
		{
			AssemblerPrototype assembler = new AssemblerPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				true);

			assembler.Speed = (float)objJToken["pumping_speed"];
			assembler.ModuleSlots = 0;
			assembler.BaseProductivityBonus = 0;

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				assembler.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
				assembler.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);
			if (objJToken["associated_items"] != null)
				foreach (string item in objJToken["associated_items"].Select(i => (string)i))
					if (items.ContainsKey(item))
						assembler.associatedItems.Add((ItemPrototype)items[item]);

			string fluidName = (string)objJToken["fluid"];
			if (!items.ContainsKey(fluidName))
				return;
			ItemPrototype fluid = (ItemPrototype)items[fluidName];

			//now to add an extra recipe that will be used to 'mine' this fluid
			RecipePrototype recipe;
			if (!recipes.ContainsKey("$r:" + fluid.Name))
			{
				recipe = new RecipePrototype(
					this,
					"$r:" + fluid.Name,
					fluid.FriendlyName + " Extraction",
					extractionSubgroupFluidsOP,
					fluid.Name);

				recipe.SetIconAndColor(new IconColorPair(fluid.Icon, fluid.AverageColor));
				recipe.Time = 1;
				recipe.InternalOneWayAddProduct(fluid, 60);

				recipe.myUnlockTechnologies.Add(startingTech);
				startingTech.unlockedRecipes.Add(recipe);

				recipes.Add(recipe.Name, recipe);
			}
			else
				recipe = (RecipePrototype)recipes["$r:" + fluid.Name];

			recipe.assemblers.Add(assembler);
			assembler.recipes.Add(recipe);

			assemblers.Add(assembler.Name, assembler);
		}

		private void ProcessBeacon(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
		{
			BeaconPrototype beacon = new BeaconPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"]);

			beacon.Effectivity = (float)objJToken["distribution_effectivity"];
			beacon.ModuleSlots = (int)objJToken["module_inventory_size"];

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				beacon.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
				beacon.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);
			if (objJToken["associated_items"] != null)
				foreach (string item in objJToken["associated_items"].Select(i => (string)i))
					if (items.ContainsKey(item))
						beacon.associatedItems.Add((ItemPrototype)items[item]);

			List<string> allowedEffectsList = objJToken["allowed_effects"].Select(token => (string)token).ToList();
			bool[] allowedEffects = new bool[] {
				allowedEffectsList.Contains("consumption"),
				allowedEffectsList.Contains("speed"),
				allowedEffectsList.Contains("productivity"),
				allowedEffectsList.Contains("pollution") };

			foreach (ModulePrototype module in modules.Values.Where(module =>
				 (allowedEffects[0] || module.ConsumptionBonus == 0) &&
				 (allowedEffects[1] || module.SpeedBonus == 0) &&
				 (allowedEffects[2] || module.ProductivityBonus == 0) &&
				 (allowedEffects[3] || module.PollutionBonus == 0)))
			{
				module.beacons.Add(beacon);
				beacon.validModules.Add(module);
			}

			beacons.Add(beacon.Name, beacon);
		}

		private void ProcessBurnerInfo(JToken objJToken, Dictionary<string, List<ItemPrototype>> fuelCategories)
		{
			if (!items.ContainsKey((string)objJToken["name"])) //ex: character
				return;

			AssemblerPrototype assembler = (AssemblerPrototype)assemblers[(string)objJToken["name"]];
			assembler.EnergyConsumption = (float)objJToken["max_energy_usage"];
			if (objJToken["fuel_type"] != null)
			{
				assembler.IsBurner = true;
				assembler.EnergyEffectivity = (float)objJToken["fuel_effectivity"];

				if ((string)objJToken["fuel_type"] == "item")
				{
					foreach (var categoryJToken in objJToken["fuel_categories"])
					{
						if (fuelCategories.ContainsKey((string)categoryJToken))
						{
							foreach (ItemPrototype item in fuelCategories[(string)categoryJToken])
							{
								assembler.fuels.Add(item);
								item.fuelsAssemblers.Add(assembler);
							}
						}
					}

				}
				else if ((string)objJToken["fuel_type"] == "fluid")
				{
					if ((bool)objJToken["burns_fluid"] == false) //this entity burns the fluid and calculates power based on fluid temperature. So.... I will just say it isnt a burner. FK THAT! (is this a leftover from old factorio with steam turbines burning any fluid?)
					{
						assembler.IsBurner = false;
						assembler.EnergyEffectivity = 0;
						return;
					}

					foreach (ItemPrototype fluid in items.Values.Where(i => i.IsFluid && i.FuelValue > 0))
					{
						assembler.fuels.Add(fluid);
						fluid.fuelsAssemblers.Add(assembler);
					}
				}
			}
		}

		//------------------------------------------------------Finalization steps of LoadAllData (cleanup and cyclic checks)

		private void UpdateUnavailableStatus()
		{
			//The data read by the dataCache (json preset) includes everything. We need to now process it such that any items/recipes that cant be used dont appear.
			//thus any object that has Unavailable set to true should be ignored. We will leave the option to use them to the user, but in most cases its better without them

			//quick function to depth-first search the tech tree to calculate the availability of the technology. Hashset used to keep track of visited tech and not have to re-check them.
			HashSet<TechnologyPrototype> temp_unlockableTechSet = new HashSet<TechnologyPrototype>();
			bool IsUnlockable(TechnologyPrototype tech)
			{
				if (!tech.Available)
					return false;
				else if (temp_unlockableTechSet.Contains(tech))
					return true;
				else if (tech.prerequisites.Count == 0)
					return true;
				else
				{
					bool available = true;
					foreach (TechnologyPrototype preTech in tech.prerequisites)
						available = available && IsUnlockable(preTech);
					tech.Available = available;

					if (available)
						temp_unlockableTechSet.Add(tech);
					return available;
				}
			}

			//step 0: delete any recipe that has no assembler. This is the only type of deletion that we will do, as we MUST enforce the 'at least 1 assembler' per recipe. The only recipes with no assemblers linked are those added to 'missing' category, and those are handled separately.
			//this means that any pure-hand-crafting recipe is removed. sorry.
			foreach(RecipePrototype recipe in recipes.Values.Where(r => r.Assemblers.Count == 0).ToList())
			{
				foreach (ItemPrototype ingredient in recipe.ingredientList)
					ingredient.consumptionRecipes.Remove(recipe);
				foreach (ItemPrototype product in recipe.productList)
					product.productionRecipes.Remove(recipe);
				foreach (TechnologyPrototype tech in recipe.myUnlockTechnologies)
					tech.unlockedRecipes.Remove(recipe);
				foreach (ModulePrototype module in recipe.modules)
					module.recipes.Remove(recipe);
				recipe.mySubgroup.recipes.Remove(recipe);

				recipes.Remove(recipe.Name);
				ErrorLogging.LogLine(string.Format("Removal of {0} due to having no assemblers associated with it.", recipe));
			}

			//step 1: update tech unlock status
			foreach (TechnologyPrototype tech in technologies.Values)
				IsUnlockable(tech);

			//step 2: update recipe unlock status
			foreach (TechnologyPrototype tech in technologies.Values)
				foreach (RecipePrototype recipe in tech.unlockedRecipes)
					recipe.Available |= tech.Available;
			foreach (RecipePrototype recipe in startingTech.unlockedRecipes)
				recipe.Available = true;

			//step 3: mark any recipe for barelling / crating as unavailable
			foreach (RecipePrototype recipe in recipes.Values)
				if (recipe.Name != "empty-barrel" && (recipe.Name.EndsWith("-barrel") || recipe.Name.StartsWith("deadlock-")))
					recipe.Available = false;

			//step 4: mark any recipe with no unlocks, no vaild assemblers, and 0->0 recipes (industrial revolution... what are those aetheric glow recipes?) as unavailable.
			foreach (RecipePrototype recipe in recipes.Values.ToList())
				if (recipe.myUnlockTechnologies.Count == 0 || recipe.assemblers.Count == 0 || (recipe.productList.Count == 0 && recipe.ingredientList.Count == 0))
					recipe.Available = false;

			//step 5: mark any useless items as unavailable (nothing/unavailable recipes produce it, it isnt consumed by anything / only consumed by incineration / only consumed by unavailable recipes)
			//note: while this gets rid of those annoying 'burn/incinerate' auto-generated recipes, if the modder decided to have a 'recycle' auto-generated recipe (item->raw ore or something), we will be forced to accept those items as 'available'
			foreach (ItemPrototype item in items.Values.ToList())
			{
				if (item.productionRecipes.FirstOrDefault(r => r.Available) == null)
				{
					bool useful = false;
					foreach (RecipePrototype r in item.consumptionRecipes.Where(r => r.Available))
						useful |= (r.ingredientList.Count > 1 || r.productList.Count != 0); //recipe with multiple items coming in or some ingredients coming out -> not an incineration type
					if(!useful)
					{
						item.Available = false;
						foreach (RecipePrototype r in item.consumptionRecipes) //from above these recipes are all item->nothing
							r.Available = false;
					}
				}
			}

			//step 6: clean up groups and subgroups (delete any subgroups that have no items/recipes, then delete any groups that have no subgroups)
			foreach(SubgroupPrototype subgroup in subgroups.Values.ToList())
			{
				if (subgroup.items.Count == 0 && subgroup.recipes.Count == 0)
				{
					((GroupPrototype)subgroup.MyGroup).subgroups.Remove(subgroup);
					subgroups.Remove(subgroup.Name);
				}
			}
			foreach (GroupPrototype group in groups.Values.ToList())
				if (group.subgroups.Count == 0)
					groups.Remove(group.Name);

			//step 7: update subgroups and groups to set them to unavailable if they only contain unavailable items/recipes
			foreach (SubgroupPrototype subgroup in subgroups.Values)
				if (subgroup.items.FirstOrDefault(i => i.Available) == null && subgroup.recipes.FirstOrDefault(r => r.Available) == null)
					subgroup.Available = false;
			foreach (GroupPrototype group in groups.Values)
				if (group.subgroups.FirstOrDefault(sg => sg.Available) == null)
					group.Available = false;

			//step 8: update all the 'available' sets where necessary (we actually make a separate set for these so we dont have to rely on 'where' calls every time)
			foreach (TechnologyPrototype tech in technologies.Values)
				tech.UpdateAvailabilities();
			foreach (GroupPrototype group in groups.Values)
				group.UpdateAvailabilities();
			foreach (SubgroupPrototype sgroup in subgroups.Values)
				sgroup.UpdateAvailabilities();
			foreach (ItemPrototype item in items.Values)
				item.UpdateAvailabilities();
			foreach (AssemblerPrototype assembler in assemblers.Values)
				assembler.UpdateAvailabilities();
			foreach (ModulePrototype module in modules.Values)
				module.UpdateAvailabilities();

			//step 9: finally, we set the 'default' enabled statuses of recipes,assemblers,modules & beacons to their available status.
			foreach (RecipePrototype recipe in recipes.Values)
				recipe.Enabled = recipe.Available;
			foreach (AssemblerPrototype assembler in assemblers.Values)
				assembler.Enabled = assembler.Available;
			foreach (ModulePrototype module in modules.Values)
				module.Enabled = module.Available;
			foreach (BeaconPrototype beacon in beacons.Values)
				beacon.Enabled = beacon.Available;
		}

		//--------------------------------------------------------------------DEBUG PRINTING FUNCTIONS

		private void PrintAllAvailabilities()
		{
			Console.WriteLine("AVAILABLE: ----------------------------------------------------------------");
			Console.WriteLine("Technologies:");
			foreach (TechnologyPrototype tech in technologies.Values)
				if (tech.Available) Console.WriteLine("    " + tech);
			Console.WriteLine("Groups:");
			foreach (GroupPrototype group in groups.Values)
				if (group.Available) Console.WriteLine("    " + group);
			Console.WriteLine("Subgroups:");
			foreach (SubgroupPrototype sgroup in subgroups.Values)
				if (sgroup.Available) Console.WriteLine("    " + sgroup);
			Console.WriteLine("Items:");
			foreach (ItemPrototype item in items.Values)
				if (item.Available) Console.WriteLine("    " + item);
			Console.WriteLine("Assemblers:");
			foreach (AssemblerPrototype assembler in assemblers.Values)
				if (assembler.Available) Console.WriteLine("    " + assembler);
			Console.WriteLine("Modules:");
			foreach (ModulePrototype module in modules.Values)
				if (module.Available) Console.WriteLine("    " + module);
			Console.WriteLine("Recipes:");
			foreach (RecipePrototype recipe in recipes.Values)
				if(recipe.Available) Console.WriteLine("    " + recipe);
			Console.WriteLine("Beacons:");
			foreach (BeaconPrototype beacon in beacons.Values)
				if(beacon.Available) Console.WriteLine("    " + beacon);
			Console.WriteLine("UNAVAILABLE: ----------------------------------------------------------------");
			Console.WriteLine("Technologies:");
			foreach (TechnologyPrototype tech in technologies.Values)
				if (!tech.Available) Console.WriteLine("    " + tech);
			Console.WriteLine("Groups:");
			foreach (GroupPrototype group in groups.Values)
				if (!group.Available) Console.WriteLine("    " + group);
			Console.WriteLine("Subgroups:");
			foreach (SubgroupPrototype sgroup in subgroups.Values)
				if (!sgroup.Available) Console.WriteLine("    " + sgroup);
			Console.WriteLine("Items:");
			foreach (ItemPrototype item in items.Values)
				if (!item.Available) Console.WriteLine("    " + item);
			Console.WriteLine("Assemblers:");
			foreach (AssemblerPrototype assembler in assemblers.Values)
				if (!assembler.Available) Console.WriteLine("    " + assembler);
			Console.WriteLine("Modules:");
			foreach (ModulePrototype module in modules.Values)
				if (!module.Available) Console.WriteLine("    " + module);
			Console.WriteLine("Recipes:");
			foreach (RecipePrototype recipe in recipes.Values)
				if (!recipe.Available) Console.WriteLine("    " + recipe);
			Console.WriteLine("Beacons:");
			foreach (BeaconPrototype beacon in beacons.Values)
				if (!beacon.Available) Console.WriteLine("    " + beacon);
		}
	}
}
