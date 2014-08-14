using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public class ProductionGraph
	{
		public List<ProductionNode> Nodes = new List<ProductionNode>();
		public bool Complete {
			get
			{
				foreach (Item item in Demand.Keys)
				{
					if (GetDemand(item) > GetSupply(item))
					{
						return false;
					}
				}

				return true;
			}
		}
		public int[,] AdjacencyMatrix
		{
			get
			{
				int[,] matrix = new int[Nodes.Count(), Nodes.Count()];
				for (int i = 0; i < Nodes.Count(); i++)
				{
					for (int j = 0; j < Nodes.Count(); j++)
					{
						if (Nodes[j].CanTakeFrom(Nodes[i]))
						{
							matrix[i, j] = 1;
						}
					}
				}
				return matrix;
			}
		}

		public int[,] PathMatrix //O(n^4) time complexity. May need to be updated.
		{
			get
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

				return pathMatrix;
			}
		}

		private Dictionary<Item, float> Demand
		{
			get
			{
				var dict = new Dictionary<Item, float>();
				foreach (ProductionNode node in Nodes)
				{
					foreach (Item item in node.Inputs.Keys)
					{
						if (dict.ContainsKey(item))
						{
							dict[item] += node.InputRate(item);
						}
						else
						{
							dict.Add(item, node.InputRate(item));
						}
					}
				}
				return dict;
			}
		}
		private Dictionary<Item, float> Supply
		{
			get
			{
				var dict = new Dictionary<Item, float>();
				foreach (ProductionNode node in Nodes)
				{
					foreach (Item item in node.Outputs.Keys)
					{
						if (dict.ContainsKey(item))
						{
							dict[item] += node.OutputRate(item);
						}else {
							dict.Add(item, node.OutputRate(item));
						}
					}
				}
				return dict;
			}
		}

		public float GetDemand(Item item)
		{
			if (Demand.ContainsKey(item))
			{
				return Demand[item];
			}
			else
			{
				return 0.0f;
			}
		}

		public float GetSupply(Item item)
		{
			if (Supply.ContainsKey(item))
			{
				return Supply[item];
			}
			else
			{
				return 0.0f;
			}
		}

		public void IterateNodeDemands()
		{
			foreach (Item item in Demand.Keys.ToList().Where(i => GetDemand(i) > GetSupply(i)))
			{
				if (Nodes.Any(n => n.OutputRate(item) > 0))
				{
					ProductionNode node;
					node = Nodes.Find(n => n.OutputRate(item) > 0);
					node.AddOutput(item, GetDemand(item) - GetSupply(item));
				}
				else
				{
					if (!item.Recipes.Any())
					{
						SupplyNode node = new SupplyNode(item, GetDemand(item) - GetSupply(item), this);
						Nodes.Add(node);
					}
					else
					{
						RecipeNode node = new RecipeNode(item.Recipes.First(), this);
						Nodes.Add(node);
						node.AddOutput(item, GetDemand(item) - GetSupply(item));
					}
				}
			}
		}

		public List<ProductionNode> GetInputlessNodes()
		{
			List<ProductionNode> list = new List<ProductionNode>();
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
					list.Add(Nodes[i]);
				}
			}

			return list;
		}

		public List<ProductionNode> GetTopologicalSort()
		{
			int[,] matrix = AdjacencyMatrix;
			List<ProductionNode> L = new List<ProductionNode>();	//Final sorted list
			List<ProductionNode> S = GetInputlessNodes();

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
					// Edges mean there's a cycle somewhere
					System.Diagnostics.Debug.Assert(matrix[i, j] == 0);
				}
			}

			return L;
		}
	}
}