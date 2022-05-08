using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	public partial class ProductionGraph
	{
		/// <summary>
		/// A directed graph where the nodes have been assigned to layers.
		/// <para>This class uses 1-based indexing like the Brandes/Köpf algorithm</para>>
		/// </summary>
		private class LayeredGraph : ILayeredGraph<LayeredGraph.Node>
		{
			public class Node
			{
				public ReadOnlyBaseNode BaseNode { get; }
				public int Layer { get; }
				public int Pos { get; set; }

				public Node(ReadOnlyBaseNode node, int layer) { BaseNode = node; Layer = layer; }
			}

			private readonly IDictionary<ReadOnlyBaseNode, Node> nodes;
			private readonly List<List<Node>> layers;

			public LayeredGraph(Dictionary<ReadOnlyBaseNode, int> layering)
			{
				nodes = layering
					.ToDictionary(n => n.Key, n => new Node(n.Key, n.Value));

				layers = nodes.Values
					.GroupBy(Layer)
					.OrderBy(layer => layer.Key)
					.Select(layer => layer.OrderBy(n => n.BaseNode.Location.X).Select(UpdatePos).ToList())
					.ToList();
			}

			public int Height => layers.Count();
			public int Width(int i) => layers[i - 1].Count();

			public IEnumerable<Node> Nodes => nodes.Values;
			public Node this[int i, int j] => layers[i - 1][j - 1];

			public IEnumerable<Node> UpperNeighbors(Node node) => Neigbors(node, NodeDirection.Up);
			public IEnumerable<Node> LowerNeighbors(Node node) => Neigbors(node, NodeDirection.Down);

			public bool IsBend(Node node) => ProductionGraph.IsBend(node.BaseNode);

			public int Pos(Node node) => node.Pos;
			public int Layer(Node node) => node.Layer;

			public Node this[ReadOnlyBaseNode node] => nodes[node];

			private IEnumerable<Node> Neigbors(Node node, NodeDirection direction)
			{
				return node.BaseNode.NodeDirection == direction
					? node.BaseNode.OutputLinks.Select(l => nodes[l.Consumer])
					: node.BaseNode.InputLinks.Select(l => nodes[l.Supplier]);
			}

			private static Node UpdatePos(Node n, int i)
			{
				n.Pos = i + 1;
				return n;
			}

			public void ReduceCrossings()
			{
				if (Height > 0)
				{
					foreach (var i in Enumerable.Range(1, Height - 1))
						layers[i] = layers[i].OrderBy(e => UpperNeighbors(e).Select(Pos).DefaultIfEmpty(0).Average()).Select(UpdatePos).ToList();

					foreach (var i in Enumerable.Range(0, Height - 1).Reverse())
						layers[i] = layers[i].OrderBy(e => LowerNeighbors(e).Select(Pos).DefaultIfEmpty(0).Average()).Select(UpdatePos).ToList();
				}
			}
		}

		private Dictionary<ReadOnlyBaseNode, int> Normalize()
		{
			var layering = new Dictionary<ReadOnlyBaseNode, int>();
			var callStack = new HashSet<ReadOnlyBaseNode>();

			int ComputeLayer(ReadOnlyBaseNode node)
			{
				while (true)
				{
					var consumers = node.OutputLinks.Select(l => l.Consumer).ToList();

					if (consumers.Count() == 0)
						return 1;

					var maxConsumerLayer = consumers.Select(GetLayer).Max();
					var maxConsumers = consumers.Where(c => GetLayer(c) == maxConsumerLayer).ToList();

					if (IsBend(node) || !maxConsumers.All(IsBend))
						return maxConsumerLayer + 1;

					// Remove extraneous passthrough nodes. If we get here, then all predecessors of the
					// current node are bends. We delete them and start over.
					// We don't just delete all bends, even if they would be recreated below to keep the
					// layout more stable.
					foreach (var consumer in maxConsumers)
					{
						(RequestNodeController(consumer) as PassthroughNodeController).JoinLinks();
						layering.Remove(consumer);
					}
				}
			}

			int GetLayer(ReadOnlyBaseNode node)
			{
				// Break cycles: the current node is already on the call stack we have detected a cycle.
				// We just return 0 (to not unnecessarily shift down other nodes) and rely on the other
				// call further up the call stack to return a more useful layer. This is pretty arbitrary,
				// but at least it prevents infinite loops.
				if (callStack.Contains(node)) return 0;
				callStack.Add(node);

				if (!layering.ContainsKey(node))
					layering[node] = ComputeLayer(node);

				callStack.Remove(node);
				return layering[node];
			}

			// Visit nodes at the bottom before nodes that are further up. For acyclic graphs this makes
			// no difference, but for cyclic graphs it keeps the layout more stable and gives the user
			// limited control about where to break the cycle by rearranging the nodes. This isn't very
			// flexible, but it's better than nothing.
			foreach (var node in Nodes.OrderByDescending(n => n.Location.Y))
				GetLayer(node);

			// Add additional passthrough nodes to ensure all node links span only one layer.
			foreach (var link in NodeLinks.ToList())
			{
				var i1 = GetLayer(link.Consumer);
				var i2 = GetLayer(link.Supplier);

				if (i2 - i1 > 1)
				{
					var consumer = link.Consumer;
					foreach (var i in Enumerable.Range(i1 + 1, i2 - i1 - 1))
					{
						var passthrough = CreatePassthroughNode(link.Item, new Point(0, 0)); // TODO: Chose a better startup coordinate
						(RequestNodeController(passthrough) as PassthroughNodeController).SetSimpleDraw(true);
						layering[passthrough] = i;
						CreateLink(passthrough, consumer, link.Item);
						consumer = passthrough;
					}
					CreateLink(link.Supplier, consumer, link.Item);
					DeleteLink(link);
				}
			}

			return layering;
		}

		public void LayoutGraph(bool reduceCrossings, Func<ReadOnlyBaseNode, int> nodeWidth)
		{
			var graph = new LayeredGraph(Normalize());

			if (reduceCrossings)
				graph.ReduceCrossings();

			var locations = new CoordinateAssignment().AssignCoordinates(graph, n => nodeWidth(n.BaseNode));

			// TODO: Should we really just ignore if there is no controller?
			// This is probably because we're calling the layouter at the wrong time...
			foreach (var node in graph.Nodes)
				RequestNodeController(node.BaseNode)?.SetLocation(locations[node]);
		}

		private static bool IsBend(ReadOnlyBaseNode node) =>
			node is ReadOnlyPassthroughNode passthrough && passthrough.SimpleDraw &&
			node.InputLinks.Count() == 1 &&
			node.OutputLinks.Count() == 1;
	}
}

