using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using lpsolve55;

namespace Foreman
{
	public enum AmountType { FixedAmount, Rate }
	public enum RateUnit { PerMinute, PerSecond }

	public class ProductionGraph
	{
		public List<ProductionNode> Nodes = new List<ProductionNode>();
		private int[,] pathMatrixCache = null;
		private int[,] adjacencyMatrixCache = null;
		public HashSet<Recipe> CyclicRecipes = new HashSet<Recipe>();
		private AmountType selectedAmountType = AmountType.FixedAmount;
		public RateUnit SelectedUnit = RateUnit.PerSecond;
		public bool OneAssemblerPerRecipe = false;

		public AmountType SelectedAmountType
		{
			get
			{
				return selectedAmountType;
			}
			set
			{
				selectedAmountType = value;
			}
		}

		public IEnumerable<NodeLink> GetAllNodeLinks()
		{
			foreach (ProductionNode node in Nodes)
			{
				foreach (NodeLink link in node.InputLinks)
				{
					yield return link;
				}
			}
		}

		public void InvalidateCaches()
		{
			pathMatrixCache = null;
			adjacencyMatrixCache = null;
		}

		public int[,] AdjacencyMatrix
		{
			get
			{
				if (adjacencyMatrixCache == null)
				{
					int[,] matrix = new int[Nodes.Count(), Nodes.Count()];
					for (int i = 0; i < Nodes.Count(); i++)
					{
						for (int j = 0; j < Nodes.Count(); j++)
						{
							if (Nodes[j].InputLinks.Any(l => l.Supplier == Nodes[i]))
							{
								matrix[i, j] = 1;
							}
						}
					}
					adjacencyMatrixCache = matrix;
				}
				return (int[,])adjacencyMatrixCache.Clone();
			}
		}
		
		public IEnumerable<ProductionNode> GetSuppliers(Item item)
		{
			foreach (ProductionNode node in Nodes)
			{
				if (node.Outputs.Contains(item))
				{
					yield return node;
				}
			}
		}

		public IEnumerable<ProductionNode> GetConsumers(Item item)
		{
			foreach (ProductionNode node in Nodes)
			{
				if (node.Inputs.Contains(item))
				{
					yield return node;
				}
			}
		}

        private Dictionary<Item, double> GetNeededItems()
        {
            Dictionary<Item, double> neededItems = new Dictionary<Item, double>();

            foreach (ProductionNode node in Nodes.ToList())
            {
                foreach (Item item in node.Inputs)
                {
                    double amountNeeded = Math.Round(node.GetUnsatisfiedDemand(item), 4);
                    if (amountNeeded > 0)
                    {
                        double priorDemand = 0;
                        if (neededItems.TryGetValue(item, out priorDemand))
                        {
                            neededItems[item] = priorDemand + amountNeeded;
                        }
                        else
                        {
                            neededItems[item] = amountNeeded;
                        }
                    }
                }
            }

            return neededItems;
        }

        private void HarvestReleventRecipes(Dictionary<Item, double> neededItems, HashSet<Item> rawItems, HashSet<Recipe> relevantRecipes, HashSet<Item> itemsVisited)
        {
            Queue<Item> itemsToVisit = new Queue<Item>(neededItems.Keys);

            while (itemsToVisit.Count > 0)
            {
                Item curItem = itemsToVisit.Dequeue();
                if (itemsVisited.Contains(curItem))
                {
                    continue;
                }
                relevantRecipes.UnionWith(curItem.Recipes);
                relevantRecipes.RemoveWhere(r => CyclicRecipes.Contains(r)); // Keep the pool clean.
                bool hasRecipe = false;
                foreach (Recipe recipe in curItem.Recipes)
                {
                    if (CyclicRecipes.Contains(recipe))
                    {
                        continue;
                    }
                    hasRecipe = true;
                    foreach (KeyValuePair<Item, float> inputItem in recipe.Ingredients)
                    {
                        if (!itemsVisited.Contains(inputItem.Key))
                        {
                            itemsToVisit.Enqueue(inputItem.Key);
                        }
                    }
                }
                if (!hasRecipe)
                {
                    rawItems.Add(curItem);
                }

                itemsVisited.Add(curItem);
            }
        }

		public void SatisfyAllItemDemands()
		{
            Dictionary<Item, double> neededItems = GetNeededItems();

            HashSet<Recipe> relevantRecipes = new HashSet<Recipe>();
            HashSet<Item> rawItems = new HashSet<Item>();
            HashSet<Item> itemsVisited = new HashSet<Item>();
            HarvestReleventRecipes(neededItems, rawItems, relevantRecipes, itemsVisited);

            // Make a strict ordering of production nodes, these form the columns of our solver
            List<String> prodNodes = new List<String>();
            lpsolve.Init(".");
            int lp = lpsolve.make_lp(0, rawItems.Count() + relevantRecipes.Count());
            int colIdx = 1;  // Yup, all arrays in lpsolve are 1-based.
            foreach (Item item in rawItems)
            {
                prodNodes.Add(item.Name);
                lpsolve.set_col_name(lp, colIdx, item.Name);
                colIdx++;
            }
            foreach (Recipe recipe in relevantRecipes)
            {
                prodNodes.Add(recipe.Name);
                lpsolve.set_col_name(lp, colIdx, recipe.Name);
                colIdx++;
            }

            // Each row in the solution is an item involved, positive entry for sources, negative
            // for costs.
            int rowIdx = 1;
            foreach (Item curItem in itemsVisited)
            {
                double[] row = new double[prodNodes.Count() + 1];
                colIdx = 1;
                foreach (Item rawItem in rawItems)
                {
                    if (rawItem.Name == curItem.Name)
                    {
                        row[colIdx] = 1;
                    } else {
                        row[colIdx] = 0;
                    }
                    colIdx++;
                }
                foreach (Recipe curRecipe in relevantRecipes)
                {
                    float amount;
                    if (curRecipe.Ingredients.TryGetValue(curItem, out amount))
                    {
                        row[colIdx] -= amount;
                    }
                    if (curRecipe.Results.TryGetValue(curItem, out amount))
                    {
                        row[colIdx] += amount;
                    }
                    colIdx++;
                }
                double neededAmount = 0;
                neededItems.TryGetValue(curItem, out neededAmount);
                lpsolve.add_constraint(lp, row, lpsolve.lpsolve_constr_types.GE, neededAmount);
                lpsolve.set_row_name(lp, rowIdx, curItem.Name);
                rowIdx++;
            }

            double[] objectiveRow = new double[prodNodes.Count() + 1];
            colIdx = 1;
            foreach (string name in prodNodes)
            {
                // Going to probably need some kind of UI for defining cost for raw items, but 
                // in the absense of that I'm just setting everything to 1 except for water.
                if (name == "water")
                {
                    objectiveRow[colIdx] = .0000001;
                }
                else if (colIdx <= rawItems.Count())
                {
                    objectiveRow[colIdx] = 1;
                }
                else
                {
                    objectiveRow[colIdx] = 0;
                }
                colIdx++;
            }

            lpsolve.set_obj_fn(lp, objectiveRow);

            lpsolve.set_outputfile(lp, "lpresult.txt");
            lpsolve.solve(lp);
            lpsolve.print_lp(lp);
            lpsolve.print_solution(lp, 1);

            double[] solutionRow = new double[prodNodes.Count()];
            lpsolve.get_variables(lp, solutionRow);

            // At this point I have no idea how to translate #of recipes as throughput into nodes in the graph,
            // I'll just print them out to the debug output.
            for (int j = 0; j < prodNodes.Count(); j++)
                System.Diagnostics.Debug.WriteLine(lpsolve.get_col_name(lp, j + 1) + ": " + solutionRow[j]);

            lpsolve.delete_lp(lp);


            // Old code here since the above doesn't do anything useful yet:
			bool nodeChosen;

			do
			{
				nodeChosen = false;

				foreach (ProductionNode node in Nodes.ToList())
				{
					foreach (Item item in node.Inputs)
					{
						if (Math.Round(node.GetUnsatisfiedDemand(item), 4) > 0)
						{
							nodeChosen = true;
							AutoSatisfyNodeDemand(node, item);
							break;
						}
					}
				}
			} while (nodeChosen);
		}

		public void AutoSatisfyNodeDemand(ProductionNode node, Item item)
		{
			if (node.InputLinks.Any(l => l.Item == item))	//Increase throughput of existing node link
			{
				NodeLink link = node.InputLinks.First(l => l.Item == item);
				//link.Amount += node.GetExcessDemand(item);
			}
			else if (Nodes.Any(n => n.Outputs.Contains(item)))	//Add link from existing node
			{
				ProductionNode existingNode = Nodes.Find(n => n.Outputs.Contains(item));
				NodeLink.Create(existingNode, node, item);
			}
			else if (item.Recipes.Any(r => !CyclicRecipes.Contains(r)))	//Create new recipe node and link from it
			{
				RecipeNode newNode = RecipeNode.Create(item.Recipes.First(r => !CyclicRecipes.Contains(r)), this);
				NodeLink.Create(newNode, node, item);
			}
			else	//Create new supply node and link from it
			{
				SupplyNode newNode = SupplyNode.Create(item, this);
				NodeLink.Create(newNode, node, item, node.GetUnsatisfiedDemand(item));
			}

			ReplaceCycles();
		}

		public void CreateRecipeNodeToSatisfyItemDemand(ProductionNode node, Item item, Recipe recipe)
		{
			RecipeNode newNode = RecipeNode.Create(recipe, this);
			NodeLink.Create(newNode, node, item, node.GetUnsatisfiedDemand(item));
		}

		public void CreateSupplyNodeToSatisfyItemDemand(ProductionNode node, Item item)
		{
			SupplyNode newNode = SupplyNode.Create(item, node.Graph);
			NodeLink.Create(newNode, node, item, node.GetUnsatisfiedDemand(item));
		}

		//Replace recipe cycles with a simple supplier node so that they don't cause infinite loops. This is a workaround.
		public void ReplaceCycles()
		{
			foreach (var StrongComponent in GetStronglyConnectedComponents().Where(scc => scc.Count() > 1))
			{
				foreach (ProductionNode node in StrongComponent)
				{
					foreach (NodeLink link in node.InputLinks.ToList().Union(node.OutputLinks.ToList()))
					{
						link.Destroy();
					}
					CyclicRecipes.Add((node as RecipeNode).BaseRecipe);
					Nodes.Remove(node);
				}
			}
			InvalidateCaches();
		}

		public IEnumerable<ProductionNode> GetInputlessNodes()
		{
			foreach (ProductionNode node in Nodes)
			{
				if (!node.InputLinks.Any())
				{
					yield return node;
				}
			}
		}

		private class TarjanNode
		{
			public readonly ProductionNode SourceNode;
			public int Index = -1;
			public int LowLink = -1;
			public HashSet<TarjanNode> Links = new HashSet<TarjanNode>(); //Links to other nodes

			public TarjanNode(ProductionNode sourceNode)
			{
				this.SourceNode = sourceNode;
			}
		}

		public IEnumerable<IEnumerable<ProductionNode>> GetStronglyConnectedComponents()
		{
			List<List<ProductionNode>> strongList = new List<List<ProductionNode>>();
			Stack<TarjanNode> S = new Stack<TarjanNode>();
			Dictionary<ProductionNode, TarjanNode> tNodes = new Dictionary<ProductionNode, TarjanNode>();
			int indexCounter = 0;

			foreach (ProductionNode n in Nodes)
			{
				tNodes.Add(n, new TarjanNode(n));
			}

			foreach (ProductionNode n in Nodes)
			{
				foreach (ProductionNode m in Nodes)
				{
					if (m.InputLinks.Any(l => l.Supplier == n))
					{
						tNodes[n].Links.Add(tNodes[m]);
					}
				}
			}

			foreach (TarjanNode v in tNodes.Values)
			{
				if (v.Index == -1)
				{
					StrongConnect(strongList, S, indexCounter, v);
				}
			}

			return strongList;
		}

		private void StrongConnect(List<List<ProductionNode>> strongList, Stack<TarjanNode> S, int indexCounter, TarjanNode v)
		{
			v.Index = indexCounter;
			v.LowLink = indexCounter++;
			S.Push(v);

			foreach (TarjanNode w in v.Links)
			{
				if (w.Index == -1)
				{
					StrongConnect(strongList, S, indexCounter, w);
					v.LowLink = Math.Min(v.LowLink, w.LowLink);
				}
				else if (S.Contains(w))
				{
					v.LowLink = Math.Min(v.LowLink, w.LowLink);
				}
			}

			{
				TarjanNode w = null;
				if (v.LowLink == v.Index)
				{
					strongList.Add(new List<ProductionNode>());
					do
					{
						w = S.Pop();
						strongList.Last().Add(w.SourceNode);
					} while (w != v);
				}
			}
		}

		public List<ProductionNode> GetTopologicalSort()
		{
			int[,] matrix = AdjacencyMatrix;
			List<ProductionNode> L = new List<ProductionNode>();	//Final sorted list
			List<ProductionNode> S = GetInputlessNodes().ToList();

			while (S.Any())
			{
				ProductionNode node = S.First();
				S.Remove(node);
				L.Add(node);

				int n = Nodes.IndexOf(node);

				for (int m = 0; m < Nodes.Count(); m++)
				{
					if (matrix[n, m] == 1)
					{
						matrix[n, m] = 0;
						int edgeCount = 0;
						for(int i = 0; i < matrix.GetLength(1); i++)
						{
							edgeCount += matrix[i, m];
						}
						if (edgeCount == 0)
						{
							S.Insert(0, Nodes[m]);
						}
					}
				}
			}

			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					// Edges mean there's a cycle somewhere and the sort can't be completed
				}
			}

			return L;
		}
	}
}