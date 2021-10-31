using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	public static class CyclicNodeTester
	{
		//https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
		private class TarjanNode
		{
			public readonly BaseNode SourceNode;
			public int Index = -1;
			public int LowLink = -1;
			public HashSet<TarjanNode> Links = new HashSet<TarjanNode>(); //Links to other nodes

			public TarjanNode(BaseNode sourceNode)
			{
				this.SourceNode = sourceNode;
			}
		}


		//A strongly connected component is a set of nodes in a directed graph that each has a route to every other node in the set.
		//In this case it means there is a potential manufacturing loop e.g. emptying/refilling oil barrels
		//Each individual node counts as a SCC by itself, but we're only interested in groups so there is a parameter to ignore them
		public static IEnumerable<IEnumerable<BaseNode>> GetStronglyConnectedComponents(DataCache dataCache)
		{
			//setup test Graph and add in all recipes
			ProductionGraph testGraph = new ProductionGraph();
			foreach (Recipe recipe in dataCache.Recipes.Values)
				testGraph.CreateRecipeNode(recipe, new Point(0, 0));

			//link every possible ingredinet-product
			foreach (BaseNode node in testGraph.Nodes)
				foreach (Item item in node.Inputs)
					foreach (BaseNode existingNode in testGraph.Nodes.Where(n => n.Outputs.Contains(item)))
						if (existingNode != node)
							testGraph.CreateLink(existingNode, node, item);

			//process the created graph to calculate the strongly linked components
			List<List<BaseNode>> strongList = new List<List<BaseNode>>();
			Stack<TarjanNode> S = new Stack<TarjanNode>();
			Dictionary<BaseNode, TarjanNode> tNodes = new Dictionary<BaseNode, TarjanNode>();
			int indexCounter = 0;

			foreach (BaseNode n in testGraph.Nodes)
				tNodes.Add(n, new TarjanNode(n));

			foreach (BaseNode n in testGraph.Nodes)
				foreach (BaseNode m in testGraph.Nodes)
					if (m.InputLinks.Any(l => l.Supplier == n))
						tNodes[n].Links.Add(tNodes[m]);

			foreach (TarjanNode v in tNodes.Values)
				if (v.Index == -1)
					StrongConnect(strongList, S, indexCounter, v);

			return strongList.Where(scc => scc.Count > 1);
		}

		private static void StrongConnect(List<List<BaseNode>> strongList, Stack<TarjanNode> S, int indexCounter, TarjanNode v)
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
					strongList.Add(new List<BaseNode>());
					do
					{
						w = S.Pop();
						strongList.Last().Add(w.SourceNode);
					} while (w != v);
				}
			}
		}
	}
}
