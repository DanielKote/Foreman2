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

		public ProductionGraph() {}

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

		public void LinkUpAllInputs()
		{
			bool graphChanged;

			do
			{
				graphChanged = false;

				foreach (ProductionNode node in Nodes.ToList())
				{
					foreach (Item item in node.Inputs)
					{
						if (!node.InputLinks.Any(l => l.Item == item))
						{
							CreateAppropriateLink(node, item);
							//graphChanged = true;
						}
					}
				}
			} while (graphChanged);
		}

		public void UpdateNodeValues()
		{
			foreach (ProductionNode node in Nodes.Where(n => n.rateType == RateType.Manual))
			{
				node.desiredRate = node.actualRate;
			}

			foreach (ProductionNode node in Nodes.Where(n => n.rateType == RateType.Auto))
			{
				node.actualRate = node.desiredRate = 0f;
			}

			foreach (NodeLink link in GetAllNodeLinks())
			{
				link.Throughput = 0f;
			}

			// Go down the list and increase each auto node's production rate to satisfy every manual node
			foreach (ProductionNode startingNode in Nodes.Where(n => n.rateType == RateType.Manual))
			{
				Stack<NodeLink> routeHome = new Stack<NodeLink>();	//The links we need to take to get back to the starting node
				int[] linkIndices = new int[Nodes.Count];	//Record which link we took at each tier of the depth-first traversal. Increment it when we go back up from that tier and reset it to 0 when we go down a tier.

				ProductionNode currentNode = startingNode;

				do
				{
					if (linkIndices[routeHome.Count()] < currentNode.InputLinks.Count())
					{
						NodeLink nextLink = currentNode.InputLinks[linkIndices[routeHome.Count()]];
						linkIndices[routeHome.Count()]++;
						if (nextLink.Supplier.rateType == RateType.Auto)
						{
							routeHome.Push(nextLink);
							nextLink.Throughput += currentNode.GetUnsatisfiedDemand(nextLink.Item);

							currentNode = nextLink.Supplier;
							currentNode.actualRate = currentNode.GetRateDemandedByOutputs();
							currentNode.desiredRate = currentNode.actualRate;
						}
						else
						{
							nextLink.Throughput += Math.Min(nextLink.Supplier.GetUnusedOutput(nextLink.Item), currentNode.GetUnsatisfiedDemand(nextLink.Item));
						}
					}
					else
					{
						if (routeHome.Any())
						{
							linkIndices[routeHome.Count()] = 0;
							currentNode = routeHome.Pop().Consumer;
						}
					}

				} while (!(currentNode == startingNode && linkIndices[0] >= startingNode.InputLinks.Count()));
			}

			//Go up the list and make each node go as fast as it can, given the amounts being input to it
			var sortedNodes = GetTopologicalSort();
			foreach (var node in sortedNodes)
			{
				if (node.rateType == RateType.Auto)
				{
					node.actualRate = node.GetRateLimitedByInputs();
				}
				foreach (Item item in node.Outputs)
				{
					float remainingOutput = node.GetTotalOutput(item);
					foreach (NodeLink link in node.OutputLinks.Where(l => l.Item == item))
					{
						link.Throughput = Math.Min(link.Throughput, remainingOutput);
						remainingOutput -= link.Throughput;
					}
				}
			}

			//Find any remaining auto nodes with rate = 0 and make them use as many items as they can
			//These nodes are probably nodes at the top of the flowchart with nothing above them demanding items
			foreach (var node in sortedNodes.Where(n => n.rateType == RateType.Auto && n.desiredRate == 0))
			{
				foreach (NodeLink link in node.InputLinks)
				{
					link.Throughput = link.Supplier.GetTotalOutput(link.Item) - link.Supplier.GetUsedOutput(link.Item);
				}
				node.actualRate = node.GetRateLimitedByInputs();
				if (node.actualRate > 0)
				{
					node.desiredRate = node.actualRate;
				}
				else
				{
					//This node can't run with the available items, so free them up for another node to potentially use (and so the node doesn't display the wrong throughput for each item)
					node.InputLinks.ForEach(l => l.Throughput = 0);
				}
			}
		}

		public void CreateAppropriateLink(ProductionNode node, Item item)
		{
			if (node is RecipeNode && CyclicRecipes.Contains((node as RecipeNode).BaseRecipe))
			{
				return;
			}
			
			if (Nodes.Any(n => n.Outputs.Contains(item)))	//Add link from existing node
			{
				ProductionNode existingNode = Nodes.Find(n => n.Outputs.Contains(item));
				NodeLink.Create(existingNode, node, item);
			}
			else if (item.Recipes.Any(r => !CyclicRecipes.Contains(r)))	//Create new recipe node and link from it
			{
				RecipeNode newNode = RecipeNode.Create(item.Recipes.First(r => !CyclicRecipes.Contains(r)), this);
				NodeLink.Create(newNode, node, item);
			}
			else //Create new supply node and link from it
			{
				SupplyNode newNode = SupplyNode.Create(item, this);
				NodeLink.Create(newNode, node, item, node.GetUnsatisfiedDemand(item));
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