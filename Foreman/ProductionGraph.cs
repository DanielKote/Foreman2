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
		private int[,] adjacencyMatrixCache = null;
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

		public ProductionGraph() { }

		public void InvalidateCaches()
		{
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
			HashSet<ProductionNode> nodesToVisit = new HashSet<ProductionNode>(Nodes);

			while (nodesToVisit.Any())
			{
				ProductionNode currentNode = nodesToVisit.First();
				nodesToVisit.Remove(nodesToVisit.First());
				
				nodesToVisit.UnionWith(CreateOrLinkAllPossibleRecipeNodes(currentNode));
				nodesToVisit.UnionWith(CreateOrLinkAllPossibleSupplyNodes(currentNode));
				CreateAllPossibleInputLinks();
			}
		}

		public void LinkUpAllOutputs()
		{
			foreach (ProductionNode node in Nodes.ToList())
			{
				foreach (Item item in node.Outputs)
				{
					if (node.GetExcessOutput(item) > 0 || !node.OutputLinks.Any(l => l.Item == item))
					{
						if (Nodes.Any(n => n.Inputs.Contains(item) && (n.rateType == RateType.Auto) && !(n.InputLinks.Any(l => l.Supplier == node))))
						{
							NodeLink.Create(node, Nodes.First(n => n.Inputs.Contains(item)), item);
						}
						else
						{
							var newNode = ConsumerNode.Create(item, this);
							newNode.rateType = RateType.Auto;
							NodeLink.Create(node, newNode, item);
						}
					}
				}
			}
		}

		public void UpdateLinkThroughputs()
		{
			foreach (NodeLink link in GetAllNodeLinks())
			{
				link.Throughput = 0;
			}

			foreach (ProductionNode node in Nodes)
			{
				foreach (Item item in node.Outputs)
				{
					List<NodeLink> outLinksForThisItem = new List<NodeLink>();
					foreach (NodeLink link in node.OutputLinks)
					{
						link.Throughput += Math.Min(link.Consumer.GetUnsatisfiedDemand(link.Item), node.GetUnusedOutput(item));
					}
				}
			}

			foreach (ProductionNode node in Nodes)
			{
				foreach (Item item in node.Inputs)
				{
					List<NodeLink> inLinksForThisItem = new List<NodeLink>();
					foreach (NodeLink link in node.InputLinks)
					{
						link.Throughput += Math.Min(link.Consumer.GetUnsatisfiedDemand(link.Item), link.Supplier.GetUnusedOutput(item));
					}
				}
			}
		}

		public void UpdateNodeValues()
		{
			foreach (ProductionNode node in Nodes.Where(n => n.rateType == RateType.Manual))
			{
				node.desiredRate = node.actualRate;
			}

			try
			{
				this.FindOptimalGraphToSatisfyFixedNodes();
			}
			catch (OverflowException)
			{
				//If the numbers here are so big they're causing an overflow, there's not much I can do about it. It's already pretty clear in the UI that the values are unusable.
				//At least this way it doesn't crash...
			}
			UpdateLinkThroughputs();
		}

		public void CreateAllPossibleInputLinks()
		{
			foreach (ProductionNode node in Nodes)
			{
				CreateAllLinksForNode(node);
			}
		}

		//Returns true if a new link was created
		public void CreateAllLinksForNode(ProductionNode node)
		{
			foreach (Item item in node.Inputs)
			{
				foreach (ProductionNode existingNode in Nodes.Where(n => n.Outputs.Contains(item)))
				{
					if (existingNode != node)
					{
						NodeLink.Create(existingNode, node, item);
					}
				}
			}
		}

		//Returns any nodes that are created
		public IEnumerable<ProductionNode> CreateOrLinkAllPossibleRecipeNodes(ProductionNode node)
		{
			List<ProductionNode> createdNodes = new List<ProductionNode>();

			foreach (Item item in node.Inputs)
			{
				var recipePool = item.Recipes.Where(r => !r.IsCyclic);   //Ignore recipes that can ultimately supply themselves, like filling/emptying barrels or certain modded recipes

				foreach (Recipe recipe in recipePool.Where(r => r.Enabled))
				{
					var existingNodes = Nodes.OfType<RecipeNode>().Where(n => n.BaseRecipe == recipe);

					if (!existingNodes.Any())
					{
						RecipeNode newNode = RecipeNode.Create(recipe, this);
						NodeLink.Create(newNode, node, item);
						createdNodes.Add(newNode);
					}
					else
					{
						foreach (RecipeNode existingNode in existingNodes)
						{
							NodeLink.Create(existingNode, node, item);
						}
					}
				}
			}

			return createdNodes;
		}

		//Returns any nodes that are created
		public IEnumerable<ProductionNode> CreateOrLinkAllPossibleSupplyNodes(ProductionNode node)
		{
			List<ProductionNode> createdNodes = new List<ProductionNode>();

			var unlinkedItems = node.Inputs.Where(i => !node.InputLinks.Any(nl => nl.Item == i));

			foreach (Item item in unlinkedItems)
			{
				var existingNodes = Nodes.OfType<SupplyNode>().Where(n => n.SuppliedItem == item);

				if (!existingNodes.Any())
				{
					SupplyNode newNode = SupplyNode.Create(item, this);
					NodeLink.Create(newNode, node, item);
					createdNodes.Add(newNode);
				}
				else
				{
					foreach (SupplyNode existingNode in existingNodes)
					{
						NodeLink.Create(existingNode, node, item);
					}
				}
			}
			return createdNodes;
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

		//https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
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

		//A strongly connected component is a set of nodes in a directed graph that each has a route to every other node in the set.
		//In this case it means there is a potential manufacturing loop e.g. emptying/refilling oil barrels
		//Each individual node counts as a SCC by itself, but we're only interested in groups so there is a parameter to ignore them
		public IEnumerable<IEnumerable<ProductionNode>> GetStronglyConnectedComponents(bool ignoreSingles)
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

			if (ignoreSingles)
			{
				return strongList.Where(scc => scc.Count > 1);
			}
			else
			{
				return strongList;
			}
		}

		public IEnumerable<IEnumerable<ProductionNode>> GetConnectedComponents()
		{
			HashSet<ProductionNode> unvisitedNodes = new HashSet<ProductionNode>(Nodes);

			List<HashSet<ProductionNode>> connectedComponents = new List<HashSet<ProductionNode>>();

			while (unvisitedNodes.Any())
			{
				connectedComponents.Add(new HashSet<ProductionNode>());
				HashSet<ProductionNode> toVisitNext = new HashSet<ProductionNode>();
				toVisitNext.Add(unvisitedNodes.First());

				while (toVisitNext.Any())
				{
					ProductionNode currentNode = toVisitNext.First();

					foreach (NodeLink link in currentNode.InputLinks)
					{
						if (unvisitedNodes.Contains(link.Supplier))
						{
							toVisitNext.Add(link.Supplier);
						}
					}
					foreach (NodeLink link in currentNode.OutputLinks)
					{
						if (unvisitedNodes.Contains(link.Consumer))
						{
							toVisitNext.Add(link.Consumer);
						}
					}

					connectedComponents.Last().Add(currentNode);
					toVisitNext.Remove(currentNode);
					unvisitedNodes.Remove(currentNode);
				}
			}

			return connectedComponents;
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