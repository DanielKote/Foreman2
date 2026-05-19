using Foreman.DataCaching.DataTypes;
using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman {
    public partial class RecipeNode {
        //------------------------------------------------------------------------ warning / errors functions

        public override List<string> GetErrors() {
            var output = new List<string>();

            if ((ErrorSet & RecipeNode.Errors.RecipeIsMissing) != 0) {
                output.Add(string.Format(DisplayCulture.Format, "> Recipe \"{0}\" doesnt exist in preset!", recipeDefinition.FriendlyName));
                return output; //missing recipe is an automatic end -> we dont care about any other errors, since the only solution is to delete the node.
            }
            if ((ErrorSet & RecipeNode.Errors.RQualityIsMissing) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Recipe's Quality \"{0}\" doesnt exist in preset!", recipeQuality.FriendlyName));

            if ((ErrorSet & RecipeNode.Errors.AssemblerIsMissing) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Assembler \"{0}\" doesnt exist in preset!", SelectedAssembler.Assembler.FriendlyName));
            if ((ErrorSet & RecipeNode.Errors.AQualityIsMissing) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Assembler's Quality \"{0}\" doesnt exist in preset!", SelectedAssembler.Quality.FriendlyName));

            if ((ErrorSet & RecipeNode.Errors.BurnerNoFuelSet) != 0)
                output.Add("> Burner Assembler has no fuel set!");
            if ((ErrorSet & RecipeNode.Errors.FuelIsMissing) != 0)
                output.Add("> Burner Assembler's fuel doesnt exist in preset!");
            if ((ErrorSet & RecipeNode.Errors.InvalidFuel) != 0)
                output.Add("> Burner Assembler has an invalid fuel set!");
            if ((ErrorSet & RecipeNode.Errors.InvalidFuelRemains) != 0)
                output.Add("> Burning result doesnt match fuel's burn result!");
            if ((ErrorSet & RecipeNode.Errors.AModuleIsMissing) != 0)
                output.Add("> Some of the assembler modules dont exist in preset!");
            if ((ErrorSet & RecipeNode.Errors.AModuleLimitExceeded) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Assembler has too many modules ({0}/{1})!", AssemblerModules.Count, SelectedAssembler.Assembler.ModuleSlots));
            if ((ErrorSet & RecipeNode.Errors.AModuleQualityIsMissing) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Assembler's Module's Quality \"{0}\" doesnt exist in preset!", AssemblerModules.First(m => m.Quality.IsMissing).Quality.FriendlyName));

            if ((ErrorSet & RecipeNode.Errors.BeaconIsMissing) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Beacon \"{0}\" doesnt exist in preset!", SelectedBeacon.Beacon?.FriendlyName ?? "(none)"));
            if ((ErrorSet & RecipeNode.Errors.BQualityIsMissing) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Beacon's Quality \"{0}\" doesnt exist in preset!", SelectedBeacon.Quality?.FriendlyName ?? "(none)"));

            if ((ErrorSet & RecipeNode.Errors.BModuleIsMissing) != 0)
                output.Add("> Some of the beacon modules dont exist in preset!");
            if ((ErrorSet & RecipeNode.Errors.BModuleLimitExceeded) != 0)
                output.Add("> Beacon has too many modules!");
            if ((ErrorSet & RecipeNode.Errors.BModuleQualityIsMissing) != 0)
                output.Add(string.Format(DisplayCulture.Format, "> Beacon's Module's Quality \"{0}\" doesnt exist in preset!", BeaconModules.First(m => m.Quality.IsMissing).Quality.FriendlyName));

            if ((ErrorSet & RecipeNode.Errors.InvalidLinks) != 0)
                output.Add("> Some links are invalid!");

            return output;
        }

        public override List<string> GetWarnings() {
            var output = new List<string>();

            //recipe
            if ((WarningSet & RecipeNode.Warnings.RecipeIsDisabled) != 0)
                output.Add("X> Selected recipe is disabled.");
            if ((WarningSet & RecipeNode.Warnings.RecipeIsUnavailable) != 0)
                output.Add("X> Selected recipe is unavailable in regular play.");

            if ((WarningSet & RecipeNode.Warnings.NoAvailableAssemblers) != 0)
                output.Add("X> No enabled assemblers for this recipe.");
            else {
                if ((WarningSet & RecipeNode.Warnings.AssemblerIsDisabled) != 0)
                    output.Add("> Selected assembler is disabled.");
                if ((WarningSet & RecipeNode.Warnings.AssemblerIsUnavailable) != 0)
                    output.Add("> Selected assembler is unavailable in regular play.");
            }

            //fuel
            if ((WarningSet & RecipeNode.Warnings.NoAvailableFuels) != 0)
                output.Add("X> No fuel can be produced.");
            else {
                if ((WarningSet & RecipeNode.Warnings.FuelIsUnavailable) != 0)
                    output.Add("> Selected fuel is unavailable in regular play.");
                if ((WarningSet & RecipeNode.Warnings.FuelIsUncraftable) != 0)
                    output.Add("> Selected fuel cant be produced.");
            }
            if ((WarningSet & RecipeNode.Warnings.TemeratureFluidBurnerInvalidLinks) != 0)
                output.Add("> Temperature based fuel uses multiple incoming temperatures (fuel use # might be wrong).");

            //modules & beacon modules
            if ((WarningSet & RecipeNode.Warnings.AModuleIsDisabled) != 0)
                output.Add("> Some selected assembler modules are disabled.");
            if ((WarningSet & RecipeNode.Warnings.AModuleIsUnavailable) != 0)
                output.Add("> Some selected assembler modules are unavailable in regular play.");
            if ((WarningSet & RecipeNode.Warnings.BeaconIsDisabled) != 0)
                output.Add("> Selected beacon is disabled.");
            if ((WarningSet & RecipeNode.Warnings.BeaconIsUnavailable) != 0)
                output.Add("> Selected beacon is unavailable in regular play.");
            if ((WarningSet & RecipeNode.Warnings.BModuleIsDisabled) != 0)
                output.Add("> Some selected beacon modules are disabled.");
            if ((WarningSet & RecipeNode.Warnings.BModuleIsUnavailable) != 0)
                output.Add("> Some selected beacon modules are unavailable in regular play.");

            return output;
        }

        //----------------------------------------------------------------------- Get functions (single assembler/beacon info)

        public double GetGeneratorMinimumTemperature() {
            if (SelectedAssembler.Assembler.EntityType == EntityType.Generator) {
                //minimum temperature accepted by generator is the largest of either the default temperature (at which point the power generation is 0 and it actually doesnt consume anything), or the set min temp
                var fluidBase = (IFluid)recipeDefinition.IngredientList[0]; //generators have 1 input & 0 output. only input is the fluid being consumed.
                return Math.Max(fluidBase.DefaultTemperature + 0.1, recipeDefinition.IngredientTemperatureMap[fluidBase].Min);
            }
            Trace.Fail("Cant ask for minimum generator temperature for a non-generator!");
            return 0;
        }

        public double GetGeneratorMaximumTemperature() {
            if (SelectedAssembler.Assembler.EntityType == EntityType.Generator)
                return recipeDefinition.IngredientTemperatureMap[recipeDefinition.IngredientList[0]].Max;
            Trace.Fail("Cant ask for maximum generator temperature for a non-generator!");
            return 0;
        }

        public double GetGeneratorAverageTemperature() {
            if (SelectedAssembler.Assembler.EntityType != EntityType.Generator)
                Trace.Fail("Cant ask for average generator temperature for a non-generator!");

            return GetAverageIncomingTemperature(this, recipeDefinition.IngredientList[0]);

            double GetAverageIncomingTemperature(BaseNode? node, IItem item) {
                if (node is PassthroughNode || node == this) {
                    double totalFlow = 0;
                    double totalTemperatureFlow = 0;
                    double totalTemperature = 0;
                    foreach (NodeLink link in node.InputLinks) //Throughput node: all same item. Generator node: only input is the fluid item.
                    {
                        totalFlow += link.Throughput;
                        double temperature = GetAverageIncomingTemperature(link.SupplierNode, item);
                        totalTemperatureFlow += temperature * link.Throughput;
                        totalTemperature += temperature;
                    }
                    return totalFlow == 0
                        ? node.InputLinks.Count == 0 ? SelectedAssembler.Assembler.OperationTemperature : totalTemperature / node.InputLinks.Count
                        : totalTemperatureFlow / totalFlow;
                }
                if (node is SupplierNode)
                    return SelectedAssembler.Assembler.OperationTemperature; //assume supplier is optimal temperature (cant exactly set to infinity or something as that would just cause the final result to be infinity)
                if (node is RecipeNode recipeNode)
                    return recipeNode.recipeDefinition.ProductTemperatureMap[item];
                Trace.Fail("Unexpected node type in generator calculation!");
                return 0;
            }
        }

        public double GetGeneratorEffectivity() {
            var fluid = (IFluid)recipeDefinition.IngredientList[0];
            return Math.Min((GetGeneratorAverageTemperature() - fluid.DefaultTemperature) / (SelectedAssembler.Assembler.OperationTemperature - fluid.DefaultTemperature), 1);
        }

        public double GetGeneratorElectricalProduction() //Watts
        {
            if (SelectedAssembler.Assembler.EntityType == EntityType.Generator)
                return SelectedAssembler.Assembler.GetEnergyProduction(SelectedAssembler.Quality) * GetGeneratorEffectivity();
            return SelectedAssembler.Assembler.GetEnergyProduction(SelectedAssembler.Quality); //no consumption multiplier => generators cant have modules / beacon effects
        }


        public double GetAssemblerSpeed() {
            return SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier();
        }

        public double GetAssemblerEnergyConsumption() //Watts
        {
            return SelectedAssembler.Assembler.GetEnergyDrain() + (SelectedAssembler.Assembler.GetEnergyConsumption(SelectedAssembler.Quality) * GetConsumptionMultiplier());
        }

        public double GetAssemblerPollutionProduction() //pollution/sec
        {
            //there are now multiple types of pollution, so not sure how to handle this (at least in terms of displaying it)
            if (!SelectedAssembler)
                return 0;
            return 0;// SelectedAssembler.Pollution * GetPollutionMultiplier() * GetAssemblerEnergyConsumption(); //pollution is counted in per energy //POLLUTION UPDATER REQUIRED
        }

        public double GetBeaconEnergyConsumption() //Watts
        {
            return !SelectedBeacon || SelectedBeacon.Beacon is not IBeacon beacon || SelectedBeacon.Quality is not IQuality quality
                ? 0
                : beacon.EnergySource != EnergySource.Electric ? 0 : beacon.GetEnergyProduction(quality) + beacon.GetEnergyDrain();
        }

        public double GetBeaconPollutionProduction() //pollution/sec
        {
            if (!SelectedBeacon)
                return 0;
            //once again - multiple types of pollution, so not sure how to handle this at this time
            return 0; // SelectedBeacon.Pollution * GetBeaconEnergyConsumption(); //POLLUTION UPDATE REQUIRED
        }

        //----------------------------------------------------------------------- Get functions (totals)

        public double GetTotalCrafts() {
            return GetAssemblerSpeed() * MyGraph.GetRateMultipler() / recipeDefinition.Time;
        }

        public double GetTotalAssemblerFuelConsumption() //fuel items / time unit
        {
            return Fuel == null ? 0 : MyGraph.GetRateMultipler() * InputRateForFuel();
        }

        public double GetTotalAssemblerElectricalConsumption() // J/sec (W)
        {
            if (SelectedAssembler.Assembler.EnergySource != EnergySource.Electric)
                return 0;

            double partialAssembler = ActualSetValue % 1;
            double entireAssemblers = ActualSetValue - partialAssembler;

            return (((entireAssemblers + (partialAssembler < 0.05 ? 0 : 1)) * SelectedAssembler.Assembler.GetEnergyDrain()) + (ActualSetValue * SelectedAssembler.Assembler.GetEnergyConsumption(SelectedAssembler.Quality) * GetConsumptionMultiplier())); //if there is more than 5% of an extra assembler, assume there is +1 assembler working x% of the time (full drain, x% uptime)
        }

        public double GetTotalGeneratorElectricalProduction() // J/sec (W) ; this is also when the temperature range of incoming fuel is taken into account
        {
            return GetGeneratorElectricalProduction() * ActualSetValue;
        }

        public int GetTotalBeacons() {
            if (!SelectedBeacon)
                return 0;
            return (int)Math.Ceiling(((int)(ActualSetValue + 0.8) * BeaconsPerAssembler) + BeaconsConst); //assume 0.2 assemblers (or more) is enough to warrant an extra 'beacons per assembler' row
        }

        public double GetTotalBeaconElectricalConsumption() // J/sec (W)
        {
            return !SelectedBeacon ? 0 : GetTotalBeacons() * GetBeaconEnergyConsumption();
        }


    }
}
