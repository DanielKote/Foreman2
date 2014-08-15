using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public static class GraphExtensions
	{
		public static Dictionary<Item, float> GetDemand(this IEnumerable<ProductionNode> nodes)
		{
			var dict = new Dictionary<Item, float>();
			foreach (ProductionNode node in nodes)
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
		public static Dictionary<Item, float> GetSupply(this IEnumerable<ProductionNode> nodes)
		{
			var dict = new Dictionary<Item, float>();
			foreach (ProductionNode node in nodes)
			{
				foreach (Item item in node.Outputs.Keys)
				{
					if (dict.ContainsKey(item))
					{
						dict[item] += node.OutputRate(item);
					}
					else
					{
						dict.Add(item, node.OutputRate(item));
					}
				}
			}
			return dict;
		}

		public static float GetDemand(this IEnumerable<ProductionNode> nodes, Item item)
		{
			if (nodes.GetDemand().ContainsKey(item))
			{
				return nodes.GetDemand()[item];
			}
			else
			{
				return 0.0f;
			}
		}

		public static float GetSupply(this IEnumerable<ProductionNode> nodes, Item item)
		{
			if (nodes.GetSupply().ContainsKey(item))
			{
				return nodes.GetSupply()[item];
			}
			else
			{
				return 0.0f;
			}
		}
	}

	public class ProductionGraph
	{
		public List<ProductionNode> Nodes = new List<ProductionNode>();

		public bool Complete {
			get
			{
				foreach (Item item in Nodes.GetDemand().Keys)
				{
					if (Nodes.GetDemand(item) > Nodes.GetSupply(item))
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

		public IEnumerable<ProductionNode> GetSuppliers(Item item)
		{
			foreach (ProductionNode node in Nodes)
			{
				if (node.OutputRate(item) > 0f)
				{
					yield return node;
				}
			}
		}

		public IEnumerable<ProductionNode> GetConsumers(Item item)
		{
			foreach (ProductionNode node in Nodes)
			{
				if (node.InputRate(item) > 0f)
				{
					yield return node;
				}
			}
		}

		public void IterateNodeDemands()
		{
			foreach (Item item in Nodes.GetDemand().Keys.ToList().Where(i => Nodes.GetDemand(i) > Nodes.GetSupply(i)))
			{
				if (Nodes.Any(n => n.OutputRate(item) > 0))
				{
					ProductionNode node;
					node = Nodes.Find(n => n.OutputRate(item) > 0);
					node.MatchDemand(item, Nodes.GetDemand(item) - Nodes.GetSupply(item) + node.OutputRate(item));
				}
				else if (!item.Recipes.Any())
				{
					SupplyNode node = new SupplyNode(item, Nodes.GetDemand(item) - Nodes.GetSupply(item), this);
					Nodes.Add(node);
				}
				else
				{
					RecipeNode node = new RecipeNode(item.Recipes.First(), this);
					Nodes.Add(node);
					node.MatchDemand(item, Nodes.GetDemand(item) - Nodes.GetSupply(item));
				}
			}

			ReplaceCycles();
		}

		//Replace recipe cycles with a simple supplier node so that they don't cause infinite loops. This is a workaround.
		public void ReplaceCycles()
		{
			foreach (var StrongComponent in GetStronglyConnectedComponents().Where(scc => scc.Count() > 1)) //Give cycles a source for their items which doesn't come from within the cycle itself (to avoid infinite loops)
			{
				var supply = StrongComponent.GetSupply();

				Nodes.RemoveAll(n => StrongComponent.Contains(n));
				foreach (Item item in supply.Keys)
				{
					if (Nodes.Except(StrongComponent).GetDemand(item) > 0)
					{
						SupplyNode node = new SupplyNode(item, Nodes.GetDemand(item) - Nodes.GetSupply(item), this);
						Nodes.Add(node);
					}
				}
			}
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
					// Edges mean there's a cycle somewhere
					//System.Diagnostics.Debug.Assert(matrix[i, j] == 0);
				}
			}

			return L;
		}
	}
}