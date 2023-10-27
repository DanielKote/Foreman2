using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class DataCache
	{
		public string PresetName { get; private set; }

		public IEnumerable<Group> AvailableGroups { get { return groups.Values.Where(g => g.Available); } }
		public IEnumerable<Subgroup> AvailableSubgroups { get { return subgroups.Values.Where(g => g.Available); } }
		public IEnumerable<Item> AvailableItems { get { return items.Values.Where(g => g.Available); } }
		public IEnumerable<Recipe> AvailableRecipes { get { return recipes.Values.Where(g => g.Available); } }

		//mods: <name, version>
		//others: <name, object>

		public IReadOnlyDictionary<string, string> IncludedMods { get { return includedMods; } }
		public IReadOnlyDictionary<string, Technology> Technologies { get { return technologies; } }
		public IReadOnlyDictionary<string, Group> Groups { get { return groups; } }
		public IReadOnlyDictionary<string, Subgroup> Subgroups { get { return subgroups; } }
		public IReadOnlyDictionary<string, Item> Items { get { return items; } }
		public IReadOnlyDictionary<string, Recipe> Recipes { get { return recipes; } }
		public IReadOnlyDictionary<string, Assembler> Assemblers { get { return assemblers; } }
		public IReadOnlyDictionary<string, Module> Modules { get { return modules; } }
		public IReadOnlyDictionary<string, Beacon> Beacons { get { return beacons; } }
		public IReadOnlyList<Item> SciencePacks { get { return sciencePacks; } }
		public IReadOnlyDictionary<Item, ICollection<Item>> SciencePackPrerequisites { get { return sciencePackPrerequisites; } }

		public Assembler PlayerAssembler { get { return playerAssember; } }
		public Assembler RocketAssembler { get { return rocketAssembler; } }
		public Technology StartingTech { get { return startingTech; } }

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


		private Dictionary<string, string> includedMods; //name : version
		private Dictionary<string, Technology> technologies;
		private Dictionary<string, Group> groups;
		private Dictionary<string, Subgroup> subgroups;
		private Dictionary<string, Item> items;
		private Dictionary<string, Recipe> recipes;
		private Dictionary<string, Assembler> assemblers;
		private Dictionary<string, Module> modules;
		private Dictionary<string, Beacon> beacons;
		private List<Item> sciencePacks;
		private Dictionary<Item, ICollection<Item>> sciencePackPrerequisites;

		private Dictionary<string, Item> missingItems;
		private Dictionary<string, Assembler> missingAssemblers;
		private Dictionary<string, Module> missingModules;
		private Dictionary<string, Beacon> missingBeacons;
		private Dictionary<RecipeShort, Recipe> missingRecipes;

		private GroupPrototype extraFormanGroup;
		private SubgroupPrototype extractionSubgroupItems;
		private SubgroupPrototype extractionSubgroupFluids;
		private SubgroupPrototype extractionSubgroupFluidsOP; //offshore pumps
		private SubgroupPrototype energySubgroupBoiling; //water to steam (boilers)
		private SubgroupPrototype energySubgroupEnergy; //heat production (heat consumption is processed as 'fuel'), steam consumption, burning to energy
		private SubgroupPrototype rocketLaunchSubgroup; //any rocket launch recipes will go here

		private ItemPrototype HeatItem;
		private RecipePrototype HeatRecipe;
		private RecipePrototype BurnerRecipe; //for burner-generators

		private Bitmap ElectricityIcon;

		private AssemblerPrototype playerAssember; //for hand crafting. Because Fk automation, thats why.
		private AssemblerPrototype rocketAssembler; //for those rocket recipes

		private SubgroupPrototype missingSubgroup;
		private TechnologyPrototype startingTech;
		private AssemblerPrototype missingAssembler; //missing recipes will have this set as their one and only assembler.

		private readonly bool UseRecipeBWLists;
		private static readonly Regex[] recipeWhiteList = { new Regex("^empty-barrel$") }; //whitelist takes priority over blacklist
		private static readonly Regex[] recipeBlackList = { new Regex("-barrel$"), new Regex("^deadlock-packrecipe-"), new Regex("^deadlock-unpackrecipe-"), new Regex("^deadlock-plastic-packaging$") };

		private Dictionary<string, IconColorPair> iconCache;

		private static readonly double MaxTemp = 10000000; //some mods set the temperature ranges as 'way too high' and expect factorio to handle it (it does). Since we prefer to show temperature ranges we will define any temp beyond these as no limit
		private static readonly double MinTemp = -MaxTemp;

		public DataCache(bool filterRecipes) //if true then the read recipes will be filtered by the white and black lists above. In most cases this is desirable (why bother with barreling, etc???), but if the user want to use them, then so be it.
		{
			UseRecipeBWLists = filterRecipes;

			includedMods = new Dictionary<string, string>();
			technologies = new Dictionary<string, Technology>();
			groups = new Dictionary<string, Group>();
			subgroups = new Dictionary<string, Subgroup>();
			items = new Dictionary<string, Item>();
			recipes = new Dictionary<string, Recipe>();
			assemblers = new Dictionary<string, Assembler>();
			modules = new Dictionary<string, Module>();
			beacons = new Dictionary<string, Beacon>();
			sciencePacks = new List<Item>();
			sciencePackPrerequisites = new Dictionary<Item, ICollection<Item>>();

			missingItems = new Dictionary<string, Item>();
			missingAssemblers = new Dictionary<string, Assembler>();
			missingModules = new Dictionary<string, Module>();
			missingBeacons = new Dictionary<string, Beacon>();
			missingRecipes = new Dictionary<RecipeShort, Recipe>(new RecipeShortNaInPrComparer());

			GenerateHelperObjects();
			Clear();
		}

		private void GenerateHelperObjects()
		{
			startingTech = new TechnologyPrototype(this, "§§t:starting_tech", "Starting Technology");
			startingTech.Tier = 0;

			extraFormanGroup = new GroupPrototype(this, "§§g:extra_group", "Resource Extraction\nPower Generation\nRocket Launches", "~~~z1");
			extraFormanGroup.SetIconAndColor(new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "ExtraGroupIcon.png"), 64), Color.Gray));

			extractionSubgroupItems = new SubgroupPrototype(this, "§§sg:extraction_items", "1");
			extractionSubgroupItems.myGroup = extraFormanGroup;
			extraFormanGroup.subgroups.Add(extractionSubgroupItems);

			extractionSubgroupFluids = new SubgroupPrototype(this, "§§sg:extraction_fluids", "2");
			extractionSubgroupFluids.myGroup = extraFormanGroup;
			extraFormanGroup.subgroups.Add(extractionSubgroupFluids);

			extractionSubgroupFluidsOP = new SubgroupPrototype(this, "§§sg:extraction_fluids_2", "3");
			extractionSubgroupFluidsOP.myGroup = extraFormanGroup;
			extraFormanGroup.subgroups.Add(extractionSubgroupFluidsOP);

			energySubgroupBoiling = new SubgroupPrototype(this, "§§sg:energy_boiling", "4");
			energySubgroupBoiling.myGroup = extraFormanGroup;
			extraFormanGroup.subgroups.Add(energySubgroupBoiling);

			energySubgroupEnergy = new SubgroupPrototype(this, "§§sg:energy_heat", "5");
			energySubgroupEnergy.myGroup = extraFormanGroup;
			extraFormanGroup.subgroups.Add(energySubgroupEnergy);

			rocketLaunchSubgroup = new SubgroupPrototype(this, "§§sg:rocket_launches", "6");
			rocketLaunchSubgroup.myGroup = extraFormanGroup;
			extraFormanGroup.subgroups.Add(rocketLaunchSubgroup);

			IconColorPair heatIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "HeatIcon.png"), 64), Color.DarkRed);
			IconColorPair burnerGeneratorIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "BurnerGeneratorIcon.png"), 64), Color.DarkRed);
			IconColorPair playerAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "PlayerAssembler.png"), 64), Color.Gray);
			IconColorPair rocketAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "RocketAssembler.png"), 64), Color.Gray);
			HeatItem = new ItemPrototype(this, "§§i:heat", "Heat (1MJ)", new SubgroupPrototype(this, "-", "-"), "-"); //we dont want heat to appear as an item in the lists, so just give it a blank subgroup.
			HeatItem.SetIconAndColor(heatIcon);
			HeatItem.FuelValue = 1000000; //1MJ - nice amount

			HeatRecipe = new RecipePrototype(this, "§§r:h:heat-generation", "Heat Generation", energySubgroupEnergy, "1");
			HeatRecipe.SetIconAndColor(heatIcon);
			HeatRecipe.InternalOneWayAddProduct(HeatItem, 1, 0);
			HeatItem.productionRecipes.Add(HeatRecipe);
			HeatRecipe.Time = 1;

			BurnerRecipe = new RecipePrototype(this, "§§r:h:burner-electicity", "Burner Generator", energySubgroupEnergy, "2");
			BurnerRecipe.SetIconAndColor(burnerGeneratorIcon);
			BurnerRecipe.Time = 1;

			playerAssember = new AssemblerPrototype(this, "§§a:player-assembler", "Player", EntityType.Assembler, EnergySource.Void);
			playerAssember.EnergyConsumption = 0;
			playerAssember.EnergyDrain = 0;
			playerAssember.EnergyProduction = 0;
			playerAssember.SetIconAndColor(playerAssemblerIcon);
			
			rocketAssembler = new AssemblerPrototype(this, "§§a:rocket-assembler", "Rocket", EntityType.Rocket, EnergySource.Void);
			rocketAssembler.EnergyConsumption = 0;
			rocketAssembler.EnergyDrain = 0;
			rocketAssembler.EnergyProduction = 0;
			rocketAssembler.SetIconAndColor(rocketAssemblerIcon);

			ElectricityIcon = IconCache.GetIcon(Path.Combine("Graphics", "ElectricityIcon.png"), 64);

			missingSubgroup = new SubgroupPrototype(this, "§§MISSING-SG", "");
			missingSubgroup.myGroup = new GroupPrototype(this, "§§MISSING-G", "MISSING", "");

			missingAssembler = new AssemblerPrototype(this, "§§a:MISSING-A", "missing assembler", EntityType.Assembler, EnergySource.Void, true);
		}

		public async Task LoadAllData(Preset preset, IProgress<KeyValuePair<int, string>> progress, bool loadIcons = true)
		{
			Clear();

			Dictionary<string, List<RecipePrototype>> craftingCategories = new Dictionary<string, List<RecipePrototype>>();
			Dictionary<string, List<RecipePrototype>> resourceCategories = new Dictionary<string, List<RecipePrototype>>();
			Dictionary<string, List<ItemPrototype>> fuelCategories = new Dictionary<string, List<ItemPrototype>>();
			fuelCategories.Add("§§fc:liquids", new List<ItemPrototype>()); //the liquid fuels category
			Dictionary<Item, string> burnResults = new Dictionary<Item, string>();

			PresetName = preset.Name;
			JObject jsonData = PresetProcessor.PrepPreset(preset);
			if (jsonData == null)
				return;

			iconCache = loadIcons ? await IconCache.LoadIconCache(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".dat" }), progress, 0, 90) : new Dictionary<string, IconColorPair>();

			await Task.Run(() =>
			{
				progress.Report(new KeyValuePair<int, string>(90, "Processing Data...")); //this is SUPER quick, so we dont need to worry about timing stuff here

				//process each section (order is rather important here)
				foreach (var objJToken in jsonData["mods"].ToList())
					ProcessMod(objJToken);
				foreach (var objJToken in jsonData["subgroups"].ToList())
					ProcessSubgroup(objJToken);
				foreach (var objJToken in jsonData["groups"].ToList())
					ProcessGroup(objJToken, iconCache);
				foreach (var objJToken in jsonData["fluids"].ToList())
					ProcessFluid(objJToken, iconCache, fuelCategories);
				foreach (var objJToken in jsonData["items"].ToList())
					ProcessItem(objJToken, iconCache, fuelCategories, burnResults); //items after fluids to take care of duplicates (if name exists in fluid and in item set, then only the fluid is counted)
				foreach (ItemPrototype item in items.Values)
					ProcessBurnItem(item, fuelCategories, burnResults); //link up any items with burn remains
				foreach (var objJToken in jsonData["recipes"].ToList())
					ProcessRecipe(objJToken, iconCache, craftingCategories);

				foreach (var objJToken in jsonData["modules"].ToList())
					ProcessModule(objJToken, iconCache);
				foreach (var objJToken in jsonData["resources"].ToList())
					ProcessResource(objJToken, resourceCategories);
				foreach (var objJToken in jsonData["technologies"].ToList())
					ProcessTechnology(objJToken, iconCache);
				foreach (var objJToken in jsonData["technologies"].ToList())
					ProcessTechnologyP2(objJToken); //required to properly link technology prerequisites
				foreach (var objJToken in jsonData["entities"].ToList())
					ProcessEntity(objJToken, iconCache, craftingCategories, resourceCategories, fuelCategories);

				//process launch products
				foreach (var objJToken in jsonData["items"].Where(t => t["launch_products"] != null).ToList())
					ProcessRocketLaunch(objJToken);
				foreach (var objJToken in jsonData["fluids"].Where(t => t["launch_products"] != null).ToList())
					ProcessRocketLaunch(objJToken);

				//process character
				ProcessCharacter(jsonData["entities"].First(a => (string)a["name"] == "character"), craftingCategories);

				//add rocket assembler
				assemblers.Add(rocketAssembler.Name, rocketAssembler);

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

				//The data read by the dataCache (json preset) includes everything. We need to now process it such that any items/recipes that cant be used dont appear.
				//thus any object that has Unavailable set to true should be ignored. We will leave the option to use them to the user, but in most cases its better without them


				//delete any recipe that has no assembler. This is the only type of deletion that we will do, as we MUST enforce the 'at least 1 assembler' per recipe. The only recipes with no assemblers linked are those added to 'missing' category, and those are handled separately.
				//note that even hand crafting has been handled: there is a player assembler that has been added. So the only recipes removed here are those that literally can not be crafted.
				foreach (RecipePrototype recipe in recipes.Values.Where(r => r.Assemblers.Count == 0).ToList())
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
					Console.WriteLine(string.Format("Removal of {0} due to having no assemblers associated with it.", recipe));
				}

				//calculate the availability of various recipes and entities (based on their unlock technologies + entity place objects' unlock technologies)
				ProcessAvailableStatuses();

				//calculate the science packs for each technology (based on both their listed science packs, the science packs of their prerequisites, and the science packs required to research the science packs)
				ProcessSciencePacks();

				//delete any groups/subgroups without any items/recipes within them, and sort by order
				CleanupGroups();

				//check each fluid to see if all production recipe temperatures can fit within all consumption recipe ranges. if not, then the item / fluid is set to be 'temperature dependent' and requires further processing when checking link validity.
				UpdateFluidTemperatureDependencies();

#if DEBUG
				//PrintDataCache();
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

			if (iconCache != null)
			{
				foreach (var iconset in iconCache.Values)
					iconset.Icon.Dispose();
				iconCache.Clear();
			}

			groups.Add(extraFormanGroup.Name, extraFormanGroup);
			subgroups.Add(extractionSubgroupItems.Name, extractionSubgroupItems);
			subgroups.Add(extractionSubgroupFluids.Name, extractionSubgroupFluids);
			subgroups.Add(extractionSubgroupFluidsOP.Name, extractionSubgroupFluidsOP);
			items.Add(HeatItem.Name, HeatItem);
			recipes.Add(HeatRecipe.Name, HeatRecipe);
			recipes.Add(BurnerRecipe.Name, BurnerRecipe);
			technologies.Add(StartingTech.Name, startingTech);


		}

		//------------------------------------------------------Import processing

		public void ProcessImportedItemsSet(IEnumerable<string> itemNames) //will ensure that all items are now part of the data cache -> existing ones (regular and missing) are skipped, new ones are added to MissingItems
		{
			foreach (string iItem in itemNames)
			{
				if (!items.ContainsKey(iItem) && !missingItems.ContainsKey(iItem)) //want to check for missing items too - in this case dont want duplicates
				{
					ItemPrototype missingItem = new ItemPrototype(this, iItem, iItem, missingSubgroup, "", true); //just assume it isnt a fluid. we dont honestly care (no temperatures)
					missingItems.Add(missingItem.Name, missingItem);
				}
			}
		}

		public void ProcessImportedAssemblersSet(IEnumerable<string> assemblerNames)
		{
			foreach (string iAssembler in assemblerNames)
			{
				if (!assemblers.ContainsKey(iAssembler) && !missingAssemblers.ContainsKey(iAssembler))
				{
					AssemblerPrototype missingAssembler = new AssemblerPrototype(this, iAssembler, iAssembler, EntityType.Assembler, EnergySource.Void, true); //dont know, dont care about entity type we will just treat it as a void-assembler (and let fuel io + recipe figure it out)
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
					BeaconPrototype missingBeacon = new BeaconPrototype(this, iBeacon, iBeacon, EnergySource.Void, true);
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
				if (recipeExists)
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
								missingRecipe.InternalOneWayAddProduct((ItemPrototype)items[product.Key], product.Value, 0);
							else
								missingRecipe.InternalOneWayAddProduct((ItemPrototype)missingItems[product.Key], product.Value, 0);
						}
						missingRecipe.assemblers.Add(missingAssembler);
						missingAssembler.recipes.Add(missingRecipe);

						missingRecipes.Add(recipeShort, missingRecipe);
						recipe = missingRecipe;
					}
				}
				if (recipeLinks.ContainsKey(recipeShort.RecipeID))
					;
				recipeLinks.Add(recipeShort.RecipeID, recipe);
			}
			return recipeLinks;
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

		private void ProcessFluid(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories)
		{
			FluidPrototype item = new FluidPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				(SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
				(string)objJToken["order"]);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

			item.DefaultTemperature = (double)objJToken["default_temperature"];
			item.SpecificHeatCapacity = (double)objJToken["heat_capacity"];

			if (objJToken["fuel_value"] != null && (double)objJToken["fuel_value"] > 0)
			{
				item.FuelValue = (double)objJToken["fuel_value"];
				item.PollutionMultiplier = (double)objJToken["pollution_multiplier"];
				fuelCategories["§§fc:liquids"].Add(item);
			}

			items.Add(item.Name, item);
		}

		private void ProcessItem(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories, Dictionary<Item, string> burnResults)
		{
			if (items.ContainsKey((string)objJToken["name"])) //special handling for fluids which appear in both items & fluid lists (ex: fluid-unknown)
				return;

			ItemPrototype item = new ItemPrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				(SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
				(string)objJToken["order"]);

			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

			item.StackSize = (int)objJToken["stack"];

			if (objJToken["fuel_category"] != null && (double)objJToken["fuel_value"] > 0) //factorio eliminates any 0fuel value fuel from the list (checked)
			{
				item.FuelValue = (double)objJToken["fuel_value"];
				item.PollutionMultiplier = (double)objJToken["pollution_multiplier"];

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
			{
				item.BurnResult = items[burnResults[item]];
				((ItemPrototype)items[burnResults[item]]).FuelOrigin = item;
			}
		}

		private void ProcessRecipe(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories)
		{
			RecipePrototype recipe = new RecipePrototype(
				this,
				(string)objJToken["name"],
				(string)objJToken["localised_name"],
				(SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
				(string)objJToken["order"]);

			recipe.Time = (double)objJToken["energy"];
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
				ItemPrototype product = (ItemPrototype)items[(string)productJToken["name"]];
				double amount = (double)productJToken["amount"];
				if (amount != 0)
				{
					if ((string)productJToken["type"] == "fluid")
						recipe.InternalOneWayAddProduct(product, amount, (double)productJToken["p_amount"], productJToken["temperature"] == null ? ((FluidPrototype)product).DefaultTemperature : (double)productJToken["temperature"]);
					else
						recipe.InternalOneWayAddProduct(product, amount, (double)productJToken["p_amount"]);

					product.productionRecipes.Add(recipe);
				}
			}

			foreach (var ingredientJToken in objJToken["ingredients"].ToList())
			{
				ItemPrototype ingredient = (ItemPrototype)items[(string)ingredientJToken["name"]];
				double amount = (double)ingredientJToken["amount"];
				if (amount != 0)
				{
					double minTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["minimum_temperature"] != null) ? (double)ingredientJToken["minimum_temperature"] : double.NegativeInfinity;
					double maxTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["maximum_temperature"] != null) ? (double)ingredientJToken["maximum_temperature"] : double.PositiveInfinity;
					if (minTemp < MinTemp) minTemp = double.NegativeInfinity;
					if (maxTemp > MaxTemp) maxTemp = double.PositiveInfinity;

					recipe.InternalOneWayAddIngredient(ingredient, amount, minTemp, maxTemp);
					ingredient.consumptionRecipes.Add(recipe);
				}
			}

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

			module.SpeedBonus = (double)objJToken["module_effects_speed"];
			module.ProductivityBonus = (double)objJToken["module_effects_productivity"];
			module.ConsumptionBonus = (double)objJToken["module_effects_consumption"];
			module.PollutionBonus = (double)objJToken["module_effects_pollution"];
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

			modules.Add(module.Name, module);
		}

		private void ProcessResource(JToken objJToken, Dictionary<string, List<RecipePrototype>> resourceCategories)
		{
			if (objJToken["products"].Count() == 0)
				return;

			string extractionRecipeName = "§§r:e:" + (string)objJToken["name"];
			RecipePrototype recipe = new RecipePrototype(
				this,
				extractionRecipeName,
				(string)objJToken["localised_name"] + " Extraction",
				(string)objJToken["products"][0]["type"] == "fluid" ? extractionSubgroupFluids : extractionSubgroupItems,
				(string)objJToken["name"]);

			recipe.Time = (double)objJToken["mining_time"];

			foreach (var productJToken in objJToken["products"])
			{
				if (!items.ContainsKey((string)productJToken["name"]) || (double)productJToken["amount"] <= 0)
					continue;
				ItemPrototype product = (ItemPrototype)items[(string)productJToken["name"]];
				recipe.InternalOneWayAddProduct(product, (double)productJToken["amount"], (double)productJToken["amount"]);
				product.productionRecipes.Add(recipe);
			}

			if (recipe.productList.Count == 0)
			{
				recipe.mySubgroup.recipes.Remove(recipe);
				return;
			}

			if (objJToken["required_fluid"] != null && (double)objJToken["fluid_amount"] != 0)
			{
				ItemPrototype reqLiquid = (ItemPrototype)items[(string)objJToken["required_fluid"]];
				recipe.InternalOneWayAddIngredient(reqLiquid, (double)objJToken["fluid_amount"]);
				reqLiquid.consumptionRecipes.Add(recipe);
			}

			foreach (ModulePrototype module in modules.Values) //we will let the assembler sort out which module can be used with this recipe
			{
				module.recipes.Add(recipe);
				recipe.modules.Add(module);
			}

			recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

			string category = (string)objJToken["resource_category"];
			if (!resourceCategories.ContainsKey(category))
				resourceCategories.Add(category, new List<RecipePrototype>());
			resourceCategories[category].Add(recipe);

			//resource recipe will be processed when adding to miners (each miner that can use this recipe will have its recipe's techs added to unlock tech of the resource recipe)
			//recipe.myUnlockTechnologies.Add(startingTech);
			//startingTech.unlockedRecipes.Add(recipe);

			recipes.Add(recipe.Name, recipe);
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

			foreach (var ingredientJToken in objJToken["research_unit_ingredients"].ToList())
			{
				string name = (string)ingredientJToken["name"];
				double amount = (double)ingredientJToken["amount"];

				if (amount != 0)
				{
					technology.InternalOneWayAddSciPack((ItemPrototype)items[name], amount);
					((ItemPrototype)items[name]).consumptionTechnologies.Add(technology);
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
			if(technology.prerequisites.Count == 0) //entire tech tree will stem from teh 'startingTech' node.
			{
				technology.prerequisites.Add(startingTech);
				startingTech.postTechs.Add(technology);
			}
		}

		private void ProcessCharacter(JToken objJtoken, Dictionary<string, List<RecipePrototype>> craftingCategories)
		{
			AssemblerAdditionalProcessing(objJtoken, playerAssember, craftingCategories);
			assemblers.Add(playerAssember.Name, playerAssember);
		}

		private void ProcessEntity(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories, Dictionary<string, List<RecipePrototype>> resourceCategories, Dictionary<string, List<ItemPrototype>> fuelCategories)
		{
			string type = (string)objJToken["type"];
			if (type == "character") //character is processed later
				return;

			EntityObjectBasePrototype entity;
			EnergySource esource =
				((string)objJToken["fuel_type"] == "item") ? EnergySource.Burner :
				((string)objJToken["fuel_type"] == "fluid") ? EnergySource.FluidBurner :
				((string)objJToken["fuel_type"] == "electricity") ? EnergySource.Electric :
				((string)objJToken["fuel_type"] == "heat") ? EnergySource.Heat : EnergySource.Void;
			EntityType etype =
				type == "beacon" ? EntityType.Beacon :
				type == "mining-drill" ? EntityType.Miner :
				type == "offshore-pump" ? EntityType.OffshorePump :           
				type == "furnace" || type == "assembling-machine" || type == "rocket-silo" ? EntityType.Assembler :
				type == "boiler" ? EntityType.Boiler :
				type == "generator" ? EntityType.Generator :
				type == "burner-generator" ? EntityType.BurnerGenerator :
				type == "reactor" ? EntityType.Reactor : EntityType.ERROR;
			if (etype == EntityType.ERROR)
				Trace.Fail(string.Format("Unexpected type of entity ({0} in json data!", type));


			if (etype == EntityType.Beacon)
				entity = new BeaconPrototype(this,
					(string)objJToken["name"],
					(string)objJToken["localised_name"],
					esource,
					false);
			else
				entity = new AssemblerPrototype(this,
					(string)objJToken["name"],
					(string)objJToken["localised_name"],
					etype,
					esource,
					false); ;

			//icons
			if (iconCache.ContainsKey((string)objJToken["icon_name"]))
				entity.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
			else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
				entity.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);

			//associated items
			if (objJToken["associated_items"] != null)
				foreach (string item in objJToken["associated_items"].Select(i => (string)i))
					if (items.ContainsKey(item))
						entity.associatedItems.Add((ItemPrototype)items[item]);

			//base parameters
			entity.Speed = objJToken["speed"] == null ? 0.5f : (double)objJToken["speed"];
			entity.ModuleSlots = objJToken["module_inventory_size"] == null ? 0 : (int)objJToken["module_inventory_size"];

			//modules
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
				entity.modules.Add(module);
				if (entity is AssemblerPrototype aEntity)
					module.assemblers.Add(aEntity);
				else if (entity is BeaconPrototype bEntity)
					module.beacons.Add(bEntity);
			}

			//energy types
			EntityEnergyFurtherProcessing(objJToken, entity, fuelCategories);

			//assembler / beacon specific parameters
			if (etype == EntityType.Beacon)
			{
				BeaconPrototype bEntity = (BeaconPrototype)entity;
				bEntity.BeaconEffectivity = objJToken["distribution_effectivity"] == null ? 0.5f : (double)objJToken["distribution_effectivity"];
				beacons.Add(bEntity.Name, bEntity);
			}
			else
			{
				AssemblerPrototype aEntity = (AssemblerPrototype)entity;
				aEntity.BaseProductivityBonus = objJToken["base_productivity"] == null ? 0f : (double)objJToken["base_productivity"];

				bool success = false;
				switch (etype)
				{
					case EntityType.Assembler:
						success = AssemblerAdditionalProcessing(objJToken, aEntity, craftingCategories);
						break;
					case EntityType.Boiler:
						success = BoilerAdditionalProcessing(objJToken, aEntity);
						break;
					case EntityType.BurnerGenerator:
						success = BurnerGeneratorAdditionalProcessing(objJToken, aEntity);
						break;
					case EntityType.Generator:
						success = GeneratorAdditionalProcessing(objJToken, aEntity);
						break;
					case EntityType.Miner:
						success = MinerAdditionalProcessing(objJToken, aEntity, resourceCategories);
						break;
					case EntityType.OffshorePump:
						success = OffshorePumpAdditionalProcessing(objJToken, aEntity);
						break;
					case EntityType.Reactor:
						success = ReactorAdditionalProcessing(objJToken, aEntity);
						break;
				}
				if (success)
					assemblers.Add(aEntity.Name, aEntity);
			}
		}

		private void EntityEnergyFurtherProcessing(JToken objJToken, EntityObjectBasePrototype entity, Dictionary<string, List<ItemPrototype>> fuelCategories)
		{
			entity.ConsumptionEffectivity = (double)objJToken["fuel_effectivity"];
			entity.Pollution = (double)objJToken["pollution"]; //calculated as per energy, so no tick to second conversion necessary
			entity.EnergyProduction = (double)objJToken["energy_production"] * 60f; //in seconds

			switch (entity.EnergySource)
			{
				case EnergySource.Burner:
					entity.EnergyDrain = 0;
					entity.EnergyConsumption = (double)objJToken["max_energy_usage"] * 60f; //in seconds
					foreach (var categoryJToken in objJToken["fuel_categories"])
					{
						if (fuelCategories.ContainsKey((string)categoryJToken))
						{
							foreach (ItemPrototype item in fuelCategories[(string)categoryJToken])
							{
								entity.fuels.Add(item);
								item.fuelsEntities.Add(entity);
							}
						}
					}
					break;

				case EnergySource.FluidBurner:
					entity.EnergyDrain = 0;
					entity.EnergyConsumption = (double)objJToken["max_energy_usage"] * 60f; //in seconds
					
					entity.IsTemperatureFluidBurner = !(bool)objJToken["burns_fluid"];
					entity.FluidFuelTemperatureRange = new fRange(objJToken["minimum_fuel_temperature"] == null ? double.NegativeInfinity : (double)objJToken["minimum_fuel_temperature"], objJToken["maximum_fuel_temperature"] == null ? double.PositiveInfinity : (double)objJToken["maximum_fuel_temperature"]);
					string fuelFilter = objJToken["fuel_filter"] == null ? null : (string)objJToken["fuel_filter"];

					if (objJToken["fuel_filter"] != null)
					{
						ItemPrototype fuel = (ItemPrototype)items[(string)objJToken["fuel_filter"]];
						if (entity.IsTemperatureFluidBurner || fuelCategories["§§fc:liquids"].Contains(fuel))
						{
							entity.fuels.Add(fuel);
							fuel.fuelsEntities.Add(entity);
						}
						//else
						//	; //there is no valid fuel for this entity. Realistically this means it cant be used. It will thus have an error when placed (no fuel selected -> due to no fuel existing)
					}
					else if(!entity.IsTemperatureFluidBurner)
					{
						//add in all liquid fuels
						foreach (ItemPrototype fluid in fuelCategories["§§fc:liquids"])
						{
							entity.fuels.Add(fluid);
							fluid.fuelsEntities.Add(entity);
						}
					}
					else //ok, this is a bit of a FK U, but this basically means this entity can burn any fluid, and burns it as a temperature range. This is how the old steam generators worked (where you could feed in hot sulfuric acid and it would just burn through it no problem). If you want to use it, fine. Here you go.
					{
						foreach(FluidPrototype fluid in items.Values.Where(i => i is Fluid))
						{
							entity.fuels.Add(fluid);
							fluid.fuelsEntities.Add(entity);
						}
					}
					break;

				case EnergySource.Heat:
					entity.EnergyDrain = 0;
					entity.EnergyConsumption = (double)objJToken["max_energy_usage"] * 60f; //in seconds
					entity.fuels.Add(HeatItem);
					HeatItem.fuelsEntities.Add(entity);
					break;

				case EnergySource.Electric:
					entity.EnergyDrain = (double)objJToken["drain"] * 60f; //seconds
					entity.EnergyConsumption = (double)objJToken["max_energy_usage"] * 60f; //seconds
					break;

				case EnergySource.Void:
				default:
					entity.EnergyConsumption = 0;
					entity.EnergyDrain = 0;
					break;
			}
		}

		private bool AssemblerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, Dictionary<string, List<RecipePrototype>> craftingCategories) //recipe user
		{
			foreach (var categoryJToken in objJToken["crafting_categories"])
			{
				if (craftingCategories.ContainsKey((string)categoryJToken))
				{
					foreach (RecipePrototype recipe in craftingCategories[(string)categoryJToken])
					{
						if (TestRecipeEntityPipeFit(recipe, objJToken))
						{
							recipe.assemblers.Add(aEntity);
							aEntity.recipes.Add(recipe);
						}
					}
				}
			}
			return true;
		}

		private bool MinerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, Dictionary<string, List<RecipePrototype>> resourceCategories) //resource provider
		{
			foreach (var categoryJToken in objJToken["resource_categories"])
			{
				if (resourceCategories.ContainsKey((string)categoryJToken))
				{
					foreach (RecipePrototype recipe in resourceCategories[(string)categoryJToken])
					{
						if (TestRecipeEntityPipeFit(recipe, objJToken))
						{
							ProcessEntityRecipeTechlink(aEntity, recipe);
							recipe.assemblers.Add(aEntity);
							aEntity.recipes.Add(recipe);
						}
					}
				}
			}
			return true;
		}

		private bool OffshorePumpAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) //fluid provider (vanilla -> water, mods can add extra)
		{
			if (objJToken["fluid_product"] == null)
				return false;

			string fluidName = (string)objJToken["fluid_product"];
			ItemPrototype fluid = (ItemPrototype)items[fluidName];

			//now to add an extra recipe that will be used to 'mine' this fluid
			RecipePrototype recipe;
			string extractionRecipeName = "§§r:e:" + fluid.Name;
			if (!recipes.ContainsKey(extractionRecipeName))
			{
				recipe = new RecipePrototype(
					this,
					extractionRecipeName,
					fluid.FriendlyName + " Extraction",
					extractionSubgroupFluidsOP,
					fluid.Name);

				recipe.SetIconAndColor(new IconColorPair(fluid.Icon, fluid.AverageColor));
				recipe.Time = 1;

				recipe.InternalOneWayAddProduct(fluid, 60, 60);
				fluid.productionRecipes.Add(recipe);


				foreach (ModulePrototype module in modules.Values) //we will let the assembler sort out which module can be used with this recipe
				{
					module.recipes.Add(recipe);
					recipe.modules.Add(module);
				}

				recipes.Add(recipe.Name, recipe);
			}
			else
				recipe = (RecipePrototype)recipes[extractionRecipeName];

			ProcessEntityRecipeTechlink(aEntity, recipe);
			recipe.assemblers.Add(aEntity);
			aEntity.recipes.Add(recipe);

			return true;
		}

		private bool BoilerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) //Uses whatever the default energy source of it is to convert water into steam of a given temperature
		{
			if (objJToken["fluid_ingredient"] == null || objJToken["fluid_product"] == null)
				return false;
			FluidPrototype ingredient = (FluidPrototype)items[(string)objJToken["fluid_ingredient"]];
			ItemPrototype product = (ItemPrototype)items[(string)objJToken["fluid_product"]];

			//boiler is a ingredient to product conversion with product coming out at the  target_temperature *C at a rate based on energy efficiency & energy use to bring the INGREDIENT to the given temperature (basically ingredient goes from default temp to target temp, then shifts to product). we will add an extra recipe for this
			double temp = (double)objJToken["target_temperature"];

			//I will be honest here. Testing has shown that the actual 'speed' is dependent on the incoming temperature (not the default temperature), as could have likely been expected.
			//this means that if you put in 65* water instead of 15* water to boil it to 165* steam it will result in 1.5x the 'maximum' output as listed in the factorio info menu and calculated below.
			//so if some mod does some wonky things like water pre-heating, or uses boiler to heat other fluids at non-default temperatures (I havent found any such mods, but testing shows it is possible to make such a mod)
			//then the values calculated here will be wrong.
			//Still, for now I will leave it as is.
			if (ingredient.SpecificHeatCapacity == 0)
				aEntity.Speed = 0;
			else
				aEntity.Speed = (double)(aEntity.EnergyConsumption / ((temp - ingredient.DefaultTemperature) * ingredient.SpecificHeatCapacity * 60)); //by placing this here we can keep the recipe as a 1 sec -> 60 production, simplifying recipe comparing for presets.

			RecipePrototype recipe;
			string boilRecipeName = string.Format("§§r:b:{0}:{1}:{2}", ingredient.Name, product.Name, temp.ToString());
			if (!recipes.ContainsKey(boilRecipeName))
			{
				recipe = new RecipePrototype(
					this,
					boilRecipeName,
					ingredient == product ? string.Format("{0} boiling to {1}°c", ingredient.FriendlyName, temp.ToString()) : string.Format("{0} boiling to {1}°c {2}", ingredient.FriendlyName, temp.ToString(), product.FriendlyName),
					energySubgroupBoiling,
					boilRecipeName);

				recipe.SetIconAndColor(new IconColorPair(IconCache.ConbineIcons(ingredient.Icon, product.Icon, ingredient.Icon.Height), product.AverageColor));

				recipe.Time = 1;

				recipe.InternalOneWayAddIngredient(ingredient, 60);
				ingredient.consumptionRecipes.Add(recipe);
				recipe.InternalOneWayAddProduct(product, 60, 60, temp);
				product.productionRecipes.Add(recipe);


				foreach (ModulePrototype module in modules.Values) //we will let the assembler sort out which module can be used with this recipe
				{
					module.recipes.Add(recipe);
					recipe.modules.Add(module);
				}

				recipes.Add(recipe.Name, recipe);
			}
			else
				recipe = (RecipePrototype)recipes[boilRecipeName];

			ProcessEntityRecipeTechlink(aEntity, recipe);
			recipe.assemblers.Add(aEntity);
			aEntity.recipes.Add(recipe);

			return true;
		}

		private bool GeneratorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) //consumes steam (at the provided temperature up to the given maximum) to generate electricity
		{
			if (objJToken["fluid_ingredient"] == null)
				return false;
			FluidPrototype ingredient = (FluidPrototype)items[(string)objJToken["fluid_ingredient"]];

			aEntity.Speed = (double)objJToken["fluid_usage_per_tick"];
			aEntity.OperationTemperature = (double)objJToken["full_power_temperature"];
			double minTemp = (double)(objJToken["minimum_temperature"] ?? double.NaN);
			double maxTemp = (double)(objJToken["maximum_temperature"] ?? double.NaN);
			if (!double.IsNaN(minTemp) && minTemp < ingredient.DefaultTemperature) minTemp = ingredient.DefaultTemperature;
			if (!double.IsNaN(maxTemp) && maxTemp > MaxTemp) maxTemp = double.NaN;

			//actual energy production is a bit more complicated here (as it involves actual temperatures), but we will have to handle it in the graph (after all values have been calculated and we know the amounts and temperatures getting passed here, we can calc the energy produced)

			RecipePrototype recipe;
			string generationRecipeName = string.Format("§§r:g:{0}:{1}>{2}", ingredient.Name, minTemp, maxTemp);
			if (!recipes.ContainsKey(generationRecipeName))
			{
				recipe = new RecipePrototype(
					this,
					generationRecipeName,
					string.Format("{0} to Electricity", ingredient.FriendlyName),
					energySubgroupEnergy,
					generationRecipeName);

				recipe.SetIconAndColor(new IconColorPair(IconCache.ConbineIcons(ingredient.Icon, ElectricityIcon, ingredient.Icon.Height, false), ingredient.AverageColor));

				recipe.Time = 1;

				recipe.InternalOneWayAddIngredient(ingredient, 60, double.IsNaN(minTemp) ? double.NegativeInfinity : minTemp, double.IsNaN(maxTemp) ? double.PositiveInfinity : maxTemp);

				ingredient.consumptionRecipes.Add(recipe);

				foreach (ModulePrototype module in modules.Values) //we will let the assembler sort out which module can be used with this recipe
				{
					module.recipes.Add(recipe);
					recipe.modules.Add(module);
				}

				recipes.Add(recipe.Name, recipe);
			}
			else
				recipe = (RecipePrototype)recipes[generationRecipeName];

			ProcessEntityRecipeTechlink(aEntity, recipe);
			recipe.assemblers.Add(aEntity);
			aEntity.recipes.Add(recipe);

			return true;
		}

		private bool BurnerGeneratorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) //consumes fuel to generate electricity
		{
			aEntity.recipes.Add(BurnerRecipe);
			BurnerRecipe.assemblers.Add(aEntity);
			ProcessEntityRecipeTechlink(aEntity, BurnerRecipe);

			aEntity.Speed = 1f; //doesnt matter -> the recipe is empty.

			return true;
		}

		private bool ReactorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity)
		{
			aEntity.NeighbourBonus = objJToken["neighbour_bonus"] == null ? 0 : (double)objJToken["neighbour_bonus"];
			aEntity.recipes.Add(HeatRecipe);
			HeatRecipe.assemblers.Add(aEntity);
			ProcessEntityRecipeTechlink(aEntity, HeatRecipe);

			aEntity.Speed = (aEntity.EnergyConsumption) / HeatItem.FuelValue; //the speed of producing 1MJ of energy as heat for this reactor

			return true;
		}

		private void ProcessEntityRecipeTechlink(EntityObjectBasePrototype entity, RecipePrototype recipe)
		{
			if (entity.associatedItems.Count == 0)
			{
				recipe.myUnlockTechnologies.Add(startingTech);
				startingTech.unlockedRecipes.Add(recipe);
			}
			else
			{
				foreach (Item placeItem in entity.associatedItems)
				{
					foreach (Recipe placeItemRecipe in placeItem.ProductionRecipes)
					{
						foreach (TechnologyPrototype tech in placeItemRecipe.MyUnlockTechnologies)
						{
							recipe.myUnlockTechnologies.Add(tech);
							tech.unlockedRecipes.Add(recipe);
						}
					}
				}
			}
		}

		private bool TestRecipeEntityPipeFit(RecipePrototype recipe, JToken objJToken) //returns true if the fluid boxes of the entity (assembler or miner) can accept the provided recipe (with its in/out fluids)
		{
			int inPipes = (int)objJToken["in_pipes"];
			List<string> inPipeFilters = objJToken["in_pipe_filters"].Select(o => (string)o).ToList();
			int outPipes = (int)objJToken["out_pipes"];
			List<string> outPipeFilters = objJToken["out_pipe_filters"].Select(o => (string)o).ToList();
			int ioPipes = (int)objJToken["io_pipes"];
			List<string> ioPipeFilters = objJToken["io_pipe_filters"].Select(o => (string)o).ToList();

			int inCount = 0; //unfiltered
			int outCount = 0; //unfiltered
			foreach(Fluid inFluid in recipe.ingredientList.Where(i => i is Fluid))
			{
				if (inPipeFilters.Contains(inFluid.Name))
				{
					inPipes--;
					inPipeFilters.Remove(inFluid.Name);
				}
				else if (ioPipeFilters.Contains(inFluid.Name))
				{
					ioPipes--;
					ioPipeFilters.Remove(inFluid.Name);
				}
				else
					inCount++;
			}
			foreach (Fluid outFluid in recipe.productList.Where(i => i is Fluid))
			{
				if (outPipeFilters.Contains(outFluid.Name))
				{
					outPipes--;
					outPipeFilters.Remove(outFluid.Name);
				}
				else if (ioPipeFilters.Contains(outFluid.Name))
				{
					ioPipes--;
					ioPipeFilters.Remove(outFluid.Name);
				}
				else
					outCount++;
			}
			//remove any unused filtered pipes from the equation - they cant be used due to the filters.
			inPipes -= inPipeFilters.Count;
			ioPipes -= ioPipeFilters.Count;
			outPipes -= outPipeFilters.Count;

			//return true if the remaining unfiltered ingredients & products (fluids) can fit into the remaining unfiltered pipes
			return (inCount - inPipes <= ioPipes && outCount - outPipes <= ioPipes && inCount + outCount <= inPipes + outPipes + ioPipes); 
		}

		private void ProcessRocketLaunch(JToken objJToken)
		{
			if (!items.ContainsKey("rocket-part") || !recipes.ContainsKey("rocket-part") || !assemblers.ContainsKey("rocket-silo"))
			{
				ErrorLogging.LogLine(string.Format("No Rocket silo / rocket part found! launch product for {0} will be ignored.", (string)objJToken["name"]));
				return;
			}

			ItemPrototype rocketPart = (ItemPrototype)items["rocket-part"];
			RecipePrototype rocketPartRecipe = (RecipePrototype)recipes["rocket-part"];
			ItemPrototype launchItem = (ItemPrototype)items[(string)objJToken["name"]];

			RecipePrototype recipe = new RecipePrototype(
				this,
				string.Format("§§r:rl:launch-{0}", launchItem.Name),
				string.Format("Rocket Launch: {0}", launchItem.FriendlyName),
				rocketLaunchSubgroup,
				launchItem.Name);

			recipe.Time = 1; //placeholder really...

			//process products - have to calculate what the maximum input size of the launch item is so as not to waste any products (ex: you can launch 2000 science packs, but you will only get 100 fish. so input size must be set to 100 -> 100 science packs to 100 fish)
			int inputSize = launchItem.StackSize;
			Dictionary<ItemPrototype, double> products = new Dictionary<ItemPrototype, double>();
			Dictionary<ItemPrototype, double> productTemp = new Dictionary<ItemPrototype, double>();
			foreach (var productJToken in objJToken["launch_products"].ToList())
			{
				ItemPrototype product = (ItemPrototype)items[(string)productJToken["name"]];
				double amount = (double)productJToken["amount"];
				if (amount != 0)
				{
					if (inputSize * amount > product.StackSize)
						inputSize = (int)Math.Ceiling(product.StackSize / amount);

					amount = inputSize * amount;

					if ((string)productJToken["type"] == "fluid")
						productTemp.Add(product, productJToken["temperature"] == null ? ((FluidPrototype)product).DefaultTemperature : (double)productJToken["temperature"]);
					products.Add(product, amount);

					product.productionRecipes.Add(recipe);
					recipe.SetIconAndColor(new IconColorPair(product.Icon, Color.DarkGray));
				}
			}
			foreach (ItemPrototype product in products.Keys)
				recipe.InternalOneWayAddProduct(product, inputSize * products[product], 0, productTemp.ContainsKey(product) ? productTemp[product] : double.NaN);

			recipe.InternalOneWayAddIngredient(launchItem, inputSize);
			launchItem.consumptionRecipes.Add(recipe);

			recipe.InternalOneWayAddIngredient(rocketPart, 100);
			rocketPart.consumptionRecipes.Add(recipe);

			foreach (TechnologyPrototype tech in rocketPartRecipe.myUnlockTechnologies)
			{
				recipe.myUnlockTechnologies.Add(tech);
				tech.unlockedRecipes.Add(recipe);
			}

			recipe.assemblers.Add(rocketAssembler);
			rocketAssembler.recipes.Add(recipe);

			recipes.Add(recipe.Name, recipe);
		}

		//------------------------------------------------------Finalization steps of LoadAllData (cleanup and cyclic checks)

		private void ProcessSciencePacks()
		{
			//DFS for processing the required sci packs of each technology. Basically some research only requires 1 sci pack, but to unlock it requires researching tech with many sci packs. Need to account for that
			Dictionary<TechnologyPrototype, HashSet<Item>> techRequirements = new Dictionary<TechnologyPrototype, HashSet<Item>>();
			HashSet<Item> sciPacks = new HashSet<Item>();
			HashSet<Item> TechRequiredSciPacks(TechnologyPrototype tech)
			{
				if (techRequirements.ContainsKey(tech))
					return techRequirements[tech];

				HashSet<Item> requiredItems = new HashSet<Item>(tech.sciPackList);
				foreach (TechnologyPrototype prereq in tech.prerequisites)
					foreach (Item sciPack in TechRequiredSciPacks(prereq))
						requiredItems.Add(sciPack);

				sciPacks.UnionWith(requiredItems);
				techRequirements.Add(tech, requiredItems);

				return requiredItems;
			}

			//tech ordering - set each technology's 'tier' to be its furthest distance from the 'starting tech' node
			HashSet<TechnologyPrototype> visitedTech = new HashSet<TechnologyPrototype>();
			visitedTech.Add(startingTech); //tier 0, everything starts from here.
			int GetTechnologyTier(TechnologyPrototype tech)
			{
				if (!visitedTech.Contains(tech))
				{
					int maxPrerequisiteTier = 0;
					foreach (TechnologyPrototype prereq in tech.prerequisites)
						maxPrerequisiteTier = Math.Max(maxPrerequisiteTier, GetTechnologyTier(prereq));
					tech.Tier = maxPrerequisiteTier + 1;
					visitedTech.Add(tech);
				}
				return tech.Tier;
			}

			//science pack processing - DF again where we want to calculate which science packs are required to get to the given science pack
			HashSet<Item> visitedPacks = new HashSet<Item>();
			void UpdateSciencePackPrerequisites(Item sciPack)
			{
				if (visitedPacks.Contains(sciPack))
					return;

				//for simplicities sake we will only account for prerequisites of the first available production recipe (or first non-available if no available production recipes exist). This means that if (for who knows what reason) there are multiple valid production recipes only the first one will count!
				HashSet<Item> prerequisites = new HashSet<Item>(sciPack.ProductionRecipes.OrderByDescending(r => r.Available).FirstOrDefault()?.MyUnlockTechnologies.OrderByDescending(t => t.Available).FirstOrDefault()?.SciPackList ?? new Item[0]);
				foreach (Recipe r in sciPack.ProductionRecipes)
					foreach (Technology t in r.MyUnlockTechnologies)
						prerequisites.IntersectWith(t.SciPackList);

				//prerequisites now contains all the immediate required sci packs. we will now Update their prerequisites via this function, then add their prerequisites to our own set before finalizing it.
				foreach (Item prereq in prerequisites.ToList())
				{
					UpdateSciencePackPrerequisites(prereq);
					prerequisites.UnionWith(sciencePackPrerequisites[prereq]);
				}
				sciencePackPrerequisites.Add(sciPack, prerequisites);
				visitedPacks.Add(sciPack);
			}

			//step 1: update tech unlock status & science packs (add a 0 cost pack to the tech if it has no such requirement but its prerequisites do), set tech tier
			foreach (TechnologyPrototype tech in technologies.Values)
			{
				TechRequiredSciPacks(tech);
				GetTechnologyTier(tech);
				foreach (ItemPrototype sciPack in techRequirements[tech])
					tech.InternalOneWayAddSciPack(sciPack, 0);
			}

			//step 2: further sci pack processing -> for every available science pack we want to build a list of science packs necessary to aquire it. In a situation with multiple (non-equal) research paths (ex: 3 can be aquired through either pack 1&2 or pack 1 alone), take the intersect (1 in this case). These will be added to the sci pack requirement lists
			foreach (Item sciPack in sciPacks)
				UpdateSciencePackPrerequisites(sciPack);


			//step 2.5: update the technology science packs to account for the science pack prerequisites
			foreach (TechnologyPrototype tech in technologies.Values)
				foreach (Item sciPack in tech.SciPackList.ToList())
					foreach (ItemPrototype reqSciPack in sciencePackPrerequisites[sciPack])
						tech.InternalOneWayAddSciPack(reqSciPack, 0);

			//step 3: calculate science pack tier (minimum tier of technology that unlocks the recipe for the given science pack). also make the sciencePacks list.
			Dictionary<Item, int> sciencePackTiers = new Dictionary<Item, int>();
			foreach (ItemPrototype sciPack in sciPacks)
			{
				int minTier = int.MaxValue;
				foreach (Recipe recipe in sciPack.productionRecipes)
					foreach (Technology tech in recipe.MyUnlockTechnologies)
						minTier = Math.Min(minTier, tech.Tier);
				if (minTier == int.MaxValue) //there are no recipes for this sci pack. EX: space science pack. We will grant it the same tier as the first tech to require this sci pack. This should sort them relatively correctly (ex - placing space sci pack last, and placing seablock starting tech first)
					minTier = techRequirements.Where(kvp => kvp.Value.Contains(sciPack)).Select(kvp => kvp.Key).Min(t => t.Tier);
				sciencePackTiers.Add(sciPack, minTier);
				sciencePacks.Add(sciPack);
			}

			//step 4: update all science pack lists (main sciencePacks list, plus SciPackList of every technology). Sorting is done by A: if science pack B has science pack A as a prerequisite (in sciPackRequiredPacks), then B goes after A. If neither has the other as a prerequisite, then compare by sciencePack tiers
			sciencePacks.Sort((s1, s2) => sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) + (sciencePackPrerequisites[s1].Contains(s2) ? 1000 : sciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));
			foreach (TechnologyPrototype tech in technologies.Values)
				tech.sciPackList.Sort((s1, s2) => sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) + (sciencePackPrerequisites[s1].Contains(s2) ? 1000 : sciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));

			//step 5: create science pack lists for each recipe (list of distinct min-pack sets -> ex: if recipe can be aquired through 4 techs with [ A+B, A+B, A+C, A+B+C ] science pack requirements, we will only include A+B and A+C
			foreach (RecipePrototype recipe in recipes.Values)
			{
				List<List<Item>> sciPackLists = new List<List<Item>>();
				foreach (TechnologyPrototype tech in recipe.myUnlockTechnologies)
				{
					bool exists = false;
					foreach (List<Item> sciPackList in sciPackLists.ToList())
					{
						if (!sciPackList.Except(tech.sciPackList).Any()) // sci pack lists already includes a list that is a subset of the technologies sci pack list (ex: already have A+B while tech's is A+B+C)
							exists = true;
						else if (!tech.sciPackList.Except(sciPackList).Any()) //technology sci pack list is a subset of an already included sci pack list. we will add thi to the list and delete the existing one (ex: have A+B while tech's is A -> need to remove A+B and include A)
							sciPackLists.Remove(sciPackList);
					}
					if (!exists)
						sciPackLists.Add(tech.sciPackList);
				}
				recipe.MyUnlockSciencePacks = sciPackLists;
			}
		}


		private void ProcessAvailableStatuses()
		{
			//quick function to depth-first search the tech tree to calculate the availability of the technology. Hashset used to keep track of visited tech and not have to re-check them.
			//NOTE: factorio ensures no cyclic, so we are guaranteed to have a directed acyclic graph (may be disconnected)
			HashSet<TechnologyPrototype> unlockableTechSet = new HashSet<TechnologyPrototype>();
			bool IsUnlockable(TechnologyPrototype tech)
			{
				if (!tech.Available)
					return false;
				else if (unlockableTechSet.Contains(tech))
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
						unlockableTechSet.Add(tech);
					return available;
				}
			}

			//step 0: check availability of technologies
			foreach (TechnologyPrototype tech in technologies.Values)
				IsUnlockable(tech);

			//step 1: update recipe unlock status
			foreach (RecipePrototype recipe in recipes.Values)
			recipe.Available = recipe.myUnlockTechnologies.Any(t => t.Available);

			//step 2: mark any recipe for barelling / crating as unavailable
			if(UseRecipeBWLists)
				foreach (RecipePrototype recipe in recipes.Values)
					if (!recipeWhiteList.Any(white => white.IsMatch(recipe.Name)) && recipeBlackList.Any(black => black.IsMatch(recipe.Name))) //if we dont match a whitelist and match a blacklist...
						recipe.Available = false;

			//step 3: mark any recipe with no unlocks, or 0->0 recipes (industrial revolution... what are those aetheric glow recipes?) as unavailable.
			foreach (RecipePrototype recipe in recipes.Values)
				if (recipe.myUnlockTechnologies.Count == 0 || (recipe.productList.Count == 0 && recipe.ingredientList.Count == 0 && !recipe.Name.StartsWith("§§"))) //§§ denotes foreman added recipes. ignored during this pass (but not during the assembler check pass)
					recipe.Available = false;

			//step 4 (loop) switch any recipe with no available assemblers to unavailable, switch any useless item to unavailable (no available recipe produces it, it isnt used by any available recipe / only by incineration recipes
			bool clean = false;
			while (!clean)
			{
				clean = true;

				//4.1: mark any recipe with no available assemblers to unavailable.
				foreach (RecipePrototype recipe in recipes.Values.Where(r => r.Available && !r.Assemblers.Any(a => a.Available || (a as AssemblerPrototype) == playerAssember || (a as AssemblerPrototype) == rocketAssembler)))
				{
					recipe.Available = false;
					clean = false;
				}

				//4.2: mark any useless items as unavailable (nothing/unavailable recipes produce it, it isnt consumed by anything / only consumed by incineration / only consumed by unavailable recipes)
				//this will also update assembler availability status for those whose items become unavailable automatically.
				//note: while this gets rid of those annoying 'burn/incinerate' auto-generated recipes, if the modder decided to have a 'recycle' auto-generated recipe (item->raw ore or something), we will be forced to accept those items as 'available'
				foreach (ItemPrototype item in items.Values.Where(i => i.Available && !i.ProductionRecipes.Any(r => r.Available)))
				{
					bool useful = false;
					foreach (RecipePrototype r in item.consumptionRecipes.Where(r => r.Available))
						useful |= (r.ingredientList.Count > 1 || r.productList.Count != 0); //recipe with multiple items coming in or some ingredients coming out -> not an incineration type
					if (!useful && !item.Name.StartsWith("§§"))
					{
						item.Available = false;
						clean = false;
						foreach (RecipePrototype r in item.consumptionRecipes) //from above these recipes are all item->nothing
							r.Available = false;
					}
				}
			}

			//step 5: set the 'default' enabled statuses of recipes,assemblers,modules & beacons to their available status.
			foreach (RecipePrototype recipe in recipes.Values)
				recipe.Enabled = recipe.Available;
			foreach (AssemblerPrototype assembler in assemblers.Values)
				assembler.Enabled = assembler.Available;
			foreach (ModulePrototype module in modules.Values)
				module.Enabled = module.Available;
			foreach (BeaconPrototype beacon in beacons.Values)
				beacon.Enabled = beacon.Available;
			playerAssember.Enabled = true; //its enabled, so it can theoretically be used, but it is set as 'unavailable' so a warning will be issued if you use it.

			rocketAssembler.Enabled = assemblers["rocket-silo"]?.Enabled?? false; //rocket assembler is set to enabled if rocket silo is enabled
			rocketAssembler.Available = assemblers["rocket-silo"] != null; //override
		}

		private void CleanupGroups()
		{
			//step 6: clean up groups and subgroups (delete any subgroups that have no items/recipes, then delete any groups that have no subgroups)
			foreach (SubgroupPrototype subgroup in subgroups.Values.ToList())
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
				if (!subgroup.items.Any(i => i.Available) && !subgroup.recipes.Any(r => r.Available))
					subgroup.Available = false;
			foreach (GroupPrototype group in groups.Values)
				if (!group.subgroups.Any(sg => sg.Available))
					group.Available = false;

			//step 8: sort groups/subgroups
			foreach (GroupPrototype group in groups.Values)
				group.SortSubgroups();
			foreach (SubgroupPrototype sgroup in subgroups.Values)
				sgroup.SortIRs();

		}

		private void UpdateFluidTemperatureDependencies()
		{
			//step 9: update the temperature dependent status of items (fluids)
			foreach (FluidPrototype fluid in items.Values.Where(i => i is Fluid))
			{
				fRange productionRange = new fRange(double.MaxValue, double.MinValue);
				fRange consumptionRange = new fRange(double.MinValue, double.MaxValue); //a bit different -> the min value is the LARGEST minimum of each consumption recipe, and the max value is the SMALLEST max of each consumption recipe

				foreach (Recipe recipe in fluid.productionRecipes)
				{
					productionRange.Min = Math.Min(productionRange.Min, recipe.ProductTemperatureMap[fluid]);
					productionRange.Max = Math.Max(productionRange.Max, recipe.ProductTemperatureMap[fluid]);
				}
				foreach (Recipe recipe in fluid.consumptionRecipes)
				{
					consumptionRange.Min = Math.Max(consumptionRange.Min, recipe.IngredientTemperatureMap[fluid].Min);
					consumptionRange.Max = Math.Min(consumptionRange.Max, recipe.IngredientTemperatureMap[fluid].Max);
				}
				fluid.IsTemperatureDependent = !(consumptionRange.Contains(productionRange));
			}
		}

		//--------------------------------------------------------------------DEBUG PRINTING FUNCTIONS

		private void PrintDataCache()
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
				if (recipe.Available) Console.WriteLine("    " + recipe);
			Console.WriteLine("Beacons:");
			foreach (BeaconPrototype beacon in beacons.Values)
				if (beacon.Available) Console.WriteLine("    " + beacon);

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

			Console.WriteLine("TECHNOLOGIES: ----------------------------------------------------------------");
			Console.WriteLine("Technology tiers:");
			foreach (TechnologyPrototype tech in technologies.Values.OrderBy(t => t.Tier))
			{
				Console.WriteLine("   T:" + tech.Tier.ToString("000") + " : " + tech.Name);
				foreach (TechnologyPrototype prereq in tech.prerequisites)
					Console.WriteLine("      > T:" + prereq.Tier.ToString("000" + " : " + prereq.Name));
			}
			Console.WriteLine("Science Pack order:");
			foreach (Item sciPack in sciencePacks)
				Console.WriteLine("   >" + sciPack.FriendlyName);
			Console.WriteLine("Science Pack prerequisites:");
			foreach (Item sciPack in sciencePacks)
			{
				Console.WriteLine("   >" + sciPack);
				foreach (Item i in sciencePackPrerequisites[sciPack])
					Console.WriteLine("      >" + i);
			}

			Console.WriteLine("RECIPES: ----------------------------------------------------------------");
			foreach(RecipePrototype recipe in recipes.Values)
			{
				Console.WriteLine("R: " + recipe.Name);
				foreach (TechnologyPrototype tech in recipe.myUnlockTechnologies)
					Console.WriteLine("  >" + tech.Tier.ToString("000") + ":" + tech.Name);
				foreach(IReadOnlyList<Item> sciPackList in recipe.MyUnlockSciencePacks)
				{
					Console.Write("    >Science Packs Option: ");
					foreach (Item sciPack in sciPackList)
						Console.Write(sciPack.Name + ", ");
					Console.WriteLine();
				}
			}

			Console.WriteLine("TEMPERATURE DEPENDENT FLUIDS: ----------------------------------------------------------------");
			foreach (ItemPrototype fluid in items.Values.Where(i => i is Fluid f && f.IsTemperatureDependent))
			{
				Console.WriteLine(fluid.Name);
				HashSet<double> productionTemps = new HashSet<double>();
				foreach (Recipe recipe in fluid.productionRecipes)
					productionTemps.Add(recipe.ProductTemperatureMap[fluid]);
				Console.Write("   Production ranges:          >");
				foreach (double temp in productionTemps.ToList().OrderBy(t => t))
					Console.Write(temp + ", ");
				Console.WriteLine();
				Console.Write("   Failed Consumption ranges:  >");
				foreach (Recipe recipe in fluid.consumptionRecipes.Where(r => productionTemps.Any(t => !r.IngredientTemperatureMap[fluid].Contains(t))))
					Console.Write("(" + recipe.IngredientTemperatureMap[fluid].Min + ">" + recipe.IngredientTemperatureMap[fluid].Max + ": " + recipe.Name + "), ");
				Console.WriteLine();
			}
		}
	}
}
