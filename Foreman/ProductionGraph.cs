using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public AmountType SelectedAmountType
		{
			get
			{
				return selectedAmountType;
			}
			set
			{
				selectedAmountType = value;
				UpdateNodeAmounts();
			}
		}

		public void InvalidateCaches()
		{
			pathMatrixCache = null;
			adjacencyMatrixCache = null;
		}

		public void UpdateNodeAmounts()
		{
			var sortedNodes = GetTopologicalSort();

			foreach (ProductionNode node in sortedNodes)
			{
				node.MinimiseInputs();
			}
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

		public int[,] PathMatrix
		{
			get
			{
				if (pathMatrixCache == null)
				{
					int[,] adjacencyMatrix = AdjacencyMatrix;
					List<int[,]> iterations = new List<int[,]>();
					iterations.Add(adjacencyMatrix);

					for (int i = 0; i < Nodes.Count() - 1; i++)
					{
						iterations.Add(iterations[i].Multiply(adjacencyMatrix));
					}

					int[,] pathMatrix = new int[Nodes.Count(), Nodes.Count()];
					foreach (int[,] matrix in iterations)
					{
						pathMatrix = pathMatrix.Add(matrix);
					}
					pathMatrixCache = pathMatrix;
				}

				return (int[,])pathMatrixCache.Clone();
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

		public void SatisfyAllItemDemands()
		{
			bool nodeChosen;

			do
			{
				nodeChosen = false;

				foreach (ProductionNode node in Nodes.ToList())
				{
					foreach (Item item in node.Inputs)
					{
						if (Math.Round(node.GetExcessDemand(item), 4) > 0)
						{
							nodeChosen = true;
							SatisfyNodeDemand(node, item);
							break;
						}
					}
				}
			} while (nodeChosen);
		}

		public void SatisfyNodeDemand(ProductionNode node, Item item)
		{
			if (node.InputLinks.Any(l => l.Item == item))	//Increase throughput of existing node link
			{
				NodeLink link = node.InputLinks.First(l => l.Item == item);
				link.Amount += node.GetExcessDemand(item);
			}
			else if (Nodes.Any(n => n.Outputs.Contains(item)))	//Add link from existing node
			{
				ProductionNode existingNode = Nodes.Find(n => n.Outputs.Contains(item));
				NodeLink.Create(existingNode, node, item, node.GetExcessDemand(item));
			}
			else if (item.Recipes.Any(r => !CyclicRecipes.Contains(r)))	//Create new recipe node and link from it
			{
				RecipeNode newNode = RecipeNode.Create(item.Recipes.First(r => !CyclicRecipes.Contains(r)), this);
				NodeLink.Create(newNode, node, item, node.GetExcessDemand(item));
			}
			else	//Create new supply node and link from it
			{
				SupplyNode newNode = SupplyNode.Create(item, this);
				NodeLink.Create(newNode, node, item, node.GetExcessDemand(item));
			}

			ReplaceCycles();
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
			var matrix = AdjacencyMatrix;

			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				int incomingEdgeSum = 0;
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					incomingEdgeSum += matrix[j, i];
				}
				if (incomingEdgeSum == 0)
				{
					yield return Nodes[i];
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
			Dictionary<int, TarjanNode> tNodes = new Dictionary<int, TarjanNode>();
			int indexCounter = 0;

			for (int i = 0; i < Nodes.Count(); i++)
			{
				tNodes.Add(i, new TarjanNode(Nodes[i]));
			}

			for (int i = 0; i < Nodes.Count(); i++)
			{
				for (int j = 0; j < Nodes.Count(); j++)
				{
					if (AdjacencyMatrix[i, j] > 0)
					{
						tNodes[i].Links.Add(tNodes[j]);
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