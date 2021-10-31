using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	static class LinkChecker
	{
		public static bool IsPossibleConnection(Item item, BaseNode supplier, BaseNode consumer)
		{
			return supplier.Outputs.Contains(item) && consumer.Inputs.Contains(item);
		}

		public static bool IsValidTemperatureConnection(Item item, BaseNode supplier, BaseNode consumer)
		{
			if (!IsPossibleConnection(item, supplier, consumer))
				return false;
			if (!item.IsFluid)
				return true;

			fRange supplierTempRange = GetTemperatureRange(item, supplier, LinkType.Output);
			fRange consumerTempRange = GetTemperatureRange(item, consumer, LinkType.Input);

			if (supplierTempRange.Ignore || consumerTempRange.Ignore)
				return true;
			return consumerTempRange.Contains(supplierTempRange);
		}

		public static fRange GetTemperatureRange(Item item, BaseNode node, LinkType direction)
		{
			//LinkType.Input : means we have a bunch of nodes ABOVE consuming the items, and we are connecting them to a single source
			//					SO: we need to check all directly-up connected recipes for min&max temp consumption. minTemp is set to be the maximum minTemp of each consumer, and maxTemp is set to be the minimum maxTemp of each consumer
			//					THIS CAN ALLOW FOR WRONG SIDE RANGES (ex: 20 -> 0 range), which means NO VALID TEMP WOULD WORK. Any valid producer must fit inside this consumer range.

			//LinkType.Output: means we have a bunch of nodes BELOW supplying the items, and we are connecting them to a single consumer
			//					SO: we need to check all directly-down connected recipes for min&max temp production. minTemp is set to be the minimum produced temperature, and maxTemp is set to be the maximum produced temperature
			//					ALL RANGES ARE RIGHT SIDE RANGES (ex: 0 -> 20), and basically require the consumer to accept any temperature within this range (producer range must be inside consumer range)

			double minTemp = (direction == LinkType.Input) ? double.NegativeInfinity : double.PositiveInfinity;
			double maxTemp = (direction == LinkType.Input) ? double.PositiveInfinity : double.NegativeInfinity;

			bool gotOne = false;
			Queue<BaseNode> neQueue = new Queue<BaseNode>(); //RecipeNode or PassthroughNodes
			if (node is RecipeNode || node is PassthroughNode)
				neQueue.Enqueue(node);
			while (neQueue.Count > 0)
			{
				BaseNode cNode = neQueue.Dequeue();
				if (cNode is PassthroughNode)
				{
					if (direction == LinkType.Input)
						foreach (NodeLink link in cNode.OutputLinks.Where(n => n.Consumer is RecipeNode || n.Consumer is PassthroughNode))
							neQueue.Enqueue(link.Consumer);
					else //if(direction == LinkType.Output)
						foreach (NodeLink link in cNode.InputLinks.Where(n => n.Supplier is RecipeNode || n.Supplier is PassthroughNode))
							neQueue.Enqueue(link.Supplier);
				}
				else //RecipeNode
				{
					gotOne = true;
					Recipe recipe = ((RecipeNode)cNode).BaseRecipe;
					if (direction == LinkType.Input && recipe.IngredientSet.ContainsKey(item)) //have to check for ingredient inclusion due to fuel/fuel-remains
					{
						minTemp = Math.Max(minTemp, recipe.IngredientTemperatureMap[item].Min);
						maxTemp = Math.Min(maxTemp, recipe.IngredientTemperatureMap[item].Max);
					}
					else if (direction == LinkType.Output && recipe.ProductSet.ContainsKey(item))
					{
						minTemp = Math.Min(minTemp, recipe.ProductTemperatureMap[item]);
						maxTemp = Math.Max(maxTemp, recipe.ProductTemperatureMap[item]);
					}
				}
			}
			if (gotOne)
				return new fRange(minTemp, maxTemp, false);
			else
				return new fRange(0, 0, true);
		}
	}
}
