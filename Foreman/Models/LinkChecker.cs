using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	static class LinkChecker
	{
		public static bool IsPossibleConnection(Item item, ReadOnlyBaseNode supplier, ReadOnlyBaseNode consumer)
		{
			if (!supplier.Outputs.Contains(item) || !consumer.Inputs.Contains(item))
				return false;
			if (!(item is Fluid fluid) || !fluid.IsTemperatureDependent)
				return true;

			fRange supplierTempRange = GetTemperatureRange(fluid, supplier, LinkType.Output, true);
			fRange consumerTempRange = GetTemperatureRange(fluid, consumer, LinkType.Input, true);

			if (supplierTempRange.Ignore || consumerTempRange.Ignore)
				return true;

			return consumerTempRange.Contains(supplierTempRange);
		}

		public static fRange GetTemperatureRange(Fluid fluid, ReadOnlyBaseNode node, LinkType direction, bool includeSelf)
		{
			//LinkType.Input : means we have a bunch of nodes ABOVE consuming the items, and we are connecting them to a single source
			//					SO: we need to check all directly-up connected recipes for min&max temp consumption. minTemp is set to be the maximum minTemp of each consumer, and maxTemp is set to be the minimum maxTemp of each consumer
			//					THIS CAN ALLOW FOR WRONG SIDE RANGES (ex: 20 -> 0 range), which means NO VALID TEMP WOULD WORK. Any valid producer must fit inside this consumer range.

			//LinkType.Output: means we have a bunch of nodes BELOW supplying the items, and we are connecting them to a single consumer
			//					SO: we need to check all directly-down connected recipes for min&max temp production. minTemp is set to be the minimum produced temperature, and maxTemp is set to be the maximum produced temperature
			//					ALL RANGES ARE RIGHT SIDE RANGES (ex: 0 -> 20), and basically require the consumer to accept any temperature within this range (producer range must be inside consumer range)

			//Include Self: if true, will add the temperature limits of the provided node (used when checking links for validity), if false will not (used for checking temperature dependent fuel burners for incoming temperatures)
			//					Additionally all checks pass through the passthrough-nodes, but in the case of includeSelf if the provided node isnt a passthrough node then we stop right away with only checking the provided node.
			//					if however we dont include self then we use our starter node as the 'starting point', ignoring its temperatures and treating it as a passthrough node

			double minTemp = (direction == LinkType.Input) ? double.NegativeInfinity : double.PositiveInfinity;
			double maxTemp = (direction == LinkType.Input) ? double.PositiveInfinity : double.NegativeInfinity;

			bool gotOne = false;

			HashSet<ReadOnlyBaseNode> visitedNodes = new HashSet<ReadOnlyBaseNode>();
			void Internal_GetMinMaxTempForNode(ReadOnlyBaseNode cNode)
			{
				if (visitedNodes.Contains(cNode))
					return;
				visitedNodes.Add(cNode);

				if (cNode is ReadOnlyPassthroughNode || (cNode == node && !includeSelf))
				{
					if (direction == LinkType.Input)
						foreach (ReadOnlyNodeLink link in cNode.OutputLinks.Where(n => n.Consumer is ReadOnlyRecipeNode || n.Consumer is ReadOnlyPassthroughNode))
							Internal_GetMinMaxTempForNode(link.Consumer);
					else //if(direction == LinkType.Output)
						foreach (ReadOnlyNodeLink link in cNode.InputLinks.Where(n => n.Supplier is ReadOnlyRecipeNode || n.Supplier is ReadOnlyPassthroughNode))
							Internal_GetMinMaxTempForNode(link.Supplier);
				}
				if (cNode is ReadOnlyRecipeNode && (cNode != node || includeSelf)) //RecipeNode
				{
					Recipe recipe = ((ReadOnlyRecipeNode)cNode).BaseRecipe;
					if (direction == LinkType.Input && recipe.IngredientSet.ContainsKey(fluid)) //have to check for ingredient inclusion due to fuel/fuel-remains
					{
						minTemp = Math.Max(minTemp, recipe.IngredientTemperatureMap[fluid].Min);
						maxTemp = Math.Min(maxTemp, recipe.IngredientTemperatureMap[fluid].Max);
						gotOne = true;
					}
					else if(direction == LinkType.Input && ((ReadOnlyRecipeNode)cNode).SelectedAssembler.IsTemperatureFluidBurner) //special case for fluid burner
					{
						minTemp = Math.Max(minTemp, ((ReadOnlyRecipeNode)cNode).SelectedAssembler.FluidFuelTemperatureRange.Min);
						maxTemp = Math.Min(maxTemp, ((ReadOnlyRecipeNode)cNode).SelectedAssembler.FluidFuelTemperatureRange.Max);
						gotOne = true;
					}
					else if (direction == LinkType.Output && recipe.ProductSet.ContainsKey(fluid))
					{
						minTemp = Math.Min(minTemp, recipe.ProductTemperatureMap[fluid]);
						maxTemp = Math.Max(maxTemp, recipe.ProductTemperatureMap[fluid]);
						gotOne = true;
					}
				}
			}

			Internal_GetMinMaxTempForNode(node);
			if (gotOne)
				return new fRange(minTemp, maxTemp, false);
			else
				return new fRange(0, 0, true);
		}
	}
}
