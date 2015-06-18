using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace Foreman
{
	public enum AmountType { FixedAmount, Rate }
	public enum RateUnit { PerMinute, PerSecond }

	[Serializable()]
	public class ProductionGraph: ISerializable
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

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AmountType", SelectedAmountType);
			info.AddValue("Unit", SelectedUnit);
			info.AddValue("OneAssemblerPerRecipe", OneAssemblerPerRecipe);
			info.AddValue("Nodes", Nodes);
			info.AddValue("NodeLinks", GetAllNodeLinks());

			List<ProductionEntity> enabledAssemblers = new List<ProductionEntity>();
		}

		public ProductionGraph(SerializationInfo info, StreamingContext context)
		{
			var enumerator = info.GetEnumerator();

			while (enumerator.MoveNext())
			{
				switch (enumerator.Name)
				{
					case "Nodes":
						{
							JArray nodes = (enumerator.Value as JArray);
							foreach (var node in nodes)
							{
								switch (node.Value<String>("NodeType"))
								{
									case "Consumer":
										{
											Item item = DataCache.Items[node.Value<String>("ItemName")];
											ConsumerNode.Create(item, this);
											break;
										}
									case "Supply":
										{
											Item item = DataCache.Items[node.Value<String>("ItemName")];
											SupplyNode.Create(item, this);
											break;
										}
									case "Recipe":
										{
											Recipe recipe = DataCache.Recipes[node.Value<String>("RecipeName")];
											RecipeNode.Create(recipe, this);
											break;
										}
								}
							}
							break;
						}
					case "NodeLinks":
						{
							JArray nodelinks = (enumerator.Value as JArray);
							foreach (var nodelink in nodelinks)
							{
								ProductionNode supplier = Nodes[nodelink.Value<int>("Supplier")];
								ProductionNode consumer = Nodes[nodelink.Value<int>("Consumer")];
								Item item = DataCache.Items[nodelink.Value<string>("Item")];
								NodeLink.Create(supplier, consumer, item);
							}
							break;
						}
				}
			}
		}
	}
}