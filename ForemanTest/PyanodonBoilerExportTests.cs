using Foreman;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ForemanTest {
    [TestClass]
    public class PyanodonBoilerExportTests : ForemanTestBase {
        private const string WaterSteam250 = "§§r:b:water:steam:250";

        [TestMethod]
        public async Task Pyanodon_OilBoiler_ExportsWaterToSteam_InPresetJson() {
            JsonObject root = PyanodonPresetTestSupport.LoadPreparedPresetJson();
            JsonNode? oilBoiler = PyanodonPresetTestSupport.FindEntity(root, "oil-boiler-mk01");
            Assert.IsNotNull(oilBoiler, "Pyanodon export should include oil-boiler-mk01.");

            Assert.AreEqual("water", PresetJson.GetString(oilBoiler, "fluid_ingredient"),
                "Oil burner boiling input should be water (re-export preset after foremanexport Lua fix).");
            Assert.AreEqual("steam", PresetJson.GetString(oilBoiler, "fluid_product"),
                "Oil burner boiling output should be steam.");
            Assert.AreEqual(250, PresetJson.GetDouble(oilBoiler, "target_temperature"));
        }

        [TestMethod]
        public async Task Pyanodon_OilBoiler_AndBoiler_LinkedToWaterSteam250() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync();

            Assert.IsTrue(snapshot.Cache.Recipes.TryGetValue(WaterSteam250, out Recipe? recipe),
                $"DataCache should contain {WaterSteam250} after preset load.");
            Assert.IsTrue(snapshot.Cache.Assemblers.ContainsKey("oil-boiler-mk01"),
                "oil-boiler-mk01 should load as an assembler when fluid boxes are exported correctly.");

            var assemblerNames = recipe!.Assemblers.Select(a => a.Name).ToHashSet();
            Assert.IsTrue(assemblerNames.Contains("oil-boiler-mk01"),
                $"{WaterSteam250} should list the oil burner as an assembler option.");
            Assert.IsTrue(assemblerNames.Contains("boiler"),
                $"{WaterSteam250} should still list the regular boiler.");
        }
    }
}