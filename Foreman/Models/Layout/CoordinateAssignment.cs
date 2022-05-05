using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	/// <summary>
	/// A graph whose nodes have been assigned to a list of ordered layers.
	/// </summary>
	/// <typeparam name="N">the type of the graph nodes</typeparam>
	/// <see cref="CoordinateAssignment"/>
	public interface ILayeredGraph<N>
	{
		int Height { get; }
		int Width(int i);

		IEnumerable<N> Nodes { get; }
		N this[int i, int j] { get; }

		IEnumerable<N> UpperNeighbors(N node);
		IEnumerable<N> LowerNeighbors(N node);

		bool IsBend(N node);

		int Pos(N node);
		int Layer(N node);
	}

	/// <summary>
	/// Implementation of an algorithm by Ulrik Brandes and Boris Köpf:
	/// "Fast and Simple Horizontal Coordinate Assignment".
	/// </summary>
	/// 
	/// <see href="http://nbn-resolving.de/urn:nbn:de:bsz:352-opus-73381"/>
	/// <see href="https://dx.doi.org/10.1007/3-540-45848-4_3">
	public class CoordinateAssignment
	{
		/// <summary>
		/// A graph adapter that allows to view a graph with left/right or top/down inverted.
		/// </summary>
		/// <typeparam name="N">the type of the graph nodes</typeparam>
		private class FlippedGraph<N> : ILayeredGraph<N>
		{
			private readonly ILayeredGraph<N> graph;
			private readonly bool vFlip, hFlip;

			public FlippedGraph(ILayeredGraph<N> underlying, bool vFlip, bool hFlip)
			{
				this.graph = underlying;
				this.vFlip = vFlip;
				this.hFlip = hFlip;
			}

			public int Height => graph.Height;
			public int Width(int i) => graph.Width(V(i));

			public IEnumerable<N> Nodes => graph.Nodes;
			public N this[int i, int j] => graph[V(i), H(i, j)];

			public IEnumerable<N> UpperNeighbors(N node) => vFlip ? graph.LowerNeighbors(node) : graph.UpperNeighbors(node);
			public IEnumerable<N> LowerNeighbors(N node) => vFlip ? graph.UpperNeighbors(node) : graph.LowerNeighbors(node);

			public bool IsBend(N node) => graph.IsBend(node);

			public int Pos(N node) => H(Layer(node), graph.Pos(node));
			public int Layer(N node) => V(graph.Layer(node));

			int V(int i) => vFlip ? Height + 1 - i : i;
			int H(int i, int j) => hFlip ? Width(i) + 1 - j : j;
		}

		/// <summary>
		/// Computes coordinates for a layered graph.
		/// </summary>
		/// <typeparam name="N">the type of the graph nodes</typeparam>
		/// <param name="graph">the graph</param>
		/// <param name="nodeWidth">a function that returns the width of a node</param>
		/// <returns>a dictionary that contains coordinates for all nodes</returns>
		public IDictionary<N, Point> AssignCoordinates<N>(ILayeredGraph<N> graph, Func<N, int> nodeWidth) where N : class
		{
			var layouts = new Dictionary<(bool, bool), Dictionary<N, int>>();

			foreach (var vFlip in new[] { false, true })
			{
				foreach (var hFlip in new[] { false, true })
				{
					var flipped = new FlippedGraph<N>(graph, vFlip, hFlip);
					var markedSegments = MarkType1Conflicts(flipped);
					var (root, align) = VerticalAlignment(flipped, markedSegments);
					layouts[(vFlip, hFlip)] = HorizontalCompaction(flipped, root, align, nodeWidth);
				}
			}

			var minWidth = layouts.Values.Select(l => l.Values.DefaultIfEmpty(0).Max()).Min();

			int x(N node)
			{
				return new[] {
					layouts[(false, false)][node],
					minWidth - layouts[(false, true)][node],
					layouts[(true, false)][node],
					minWidth - layouts[(true, true)][node]
				}.OrderBy(val => val).Skip(1).Take(2).Sum() / 2;
			}

			return graph.Nodes.ToDictionary(
				node => node,
				node => new Point(x(node), 192 * graph.Layer(node))); // TODO: Make vertical distance configurable
		}

		private IEnumerable<int> Range(int from, int to) => (to < from) ? Enumerable.Empty<int>() : Enumerable.Range(from, to - from + 1);

		private HashSet<(N, N)> MarkType1Conflicts<N>(ILayeredGraph<N> graph)
		{
			var markedSegments = new HashSet<(N, N)>();

			foreach (var i in Range(2, graph.Height - 2))
			{
				int k0 = 0, l = 1;

				foreach (var l1 in Range(1, graph.Width(i + 1)))
				{
					var vl1 = graph[i + 1, l1];
					var incidentToInnerSegment =
						graph.IsBend(vl1) &&
						graph.IsBend(graph.UpperNeighbors(vl1).Single());

					if (l1 == graph.Width(i + 1) || incidentToInnerSegment)
					{
						var k1 = graph.Width(i);

						if (incidentToInnerSegment)
							k1 = graph.Pos(graph.UpperNeighbors(vl1).Single());

						while (l < l1)
						{
							foreach (var vik in graph.UpperNeighbors(graph[i + 1, l]))
								if (graph.Pos(vik) < k0 || graph.Pos(vik) > k1) markedSegments.Add((vik, graph[i + 1, l]));

							++l;
						}

						k0 = k1;
					}
				}
			}

			return markedSegments;
		}

		private IEnumerable<N> Middle<N>(ILayeredGraph<N> graph, IEnumerable<N> nodes)
		{
			var neighbors = nodes.OrderBy(v => graph.Pos(v)).ToList();
			var l = neighbors.Count;

			if (l > 0)
			{
				if (l % 2 == 0) yield return neighbors[(l - 1) / 2];
				yield return neighbors[l / 2];
			}
		}

		private (Dictionary<N, N>, Dictionary<N, N>) VerticalAlignment<N>(ILayeredGraph<N> graph, HashSet<(N, N)> markedSegments) where N : class
		{
			var root = graph.Nodes.ToDictionary(v => v);
			var align = graph.Nodes.ToDictionary(v => v);

			foreach (var i in Range(1, graph.Height))
			{
				var r = 0;
				foreach (var k in Range(1, graph.Width(i)))
				{
					var vik = graph[i, k];
					foreach (var um in Middle(graph, graph.UpperNeighbors(vik)))
					{
						if (align[vik] == vik)
						{
							if (!markedSegments.Contains((um, vik)) && r < graph.Pos(um))
							{
								align[um] = vik;
								root[vik] = root[um];
								align[vik] = root[vik];
								r = graph.Pos(um);
							}
						}
					}
				}
			}

			return (root, align);
		}

		private Dictionary<N, int> HorizontalCompaction<N>(ILayeredGraph<N> graph, Dictionary<N, N> root, Dictionary<N, N> align, Func<N, int> nodeWidth) where N : class
		{
			var sink = graph.Nodes.ToDictionary(v => v);
			var shift = graph.Nodes.ToDictionary(v => v, v => int.MaxValue);
			var x = new Dictionary<N, int>();

			void placeBlock(N v)
			{
				if (!x.ContainsKey(v))
				{
					x[v] = 0;
					var w = v;

					do
					{
						if (graph.Pos(w) > 1)
						{
							var predW = graph[graph.Layer(w), graph.Pos(w) - 1];
							var delta = (nodeWidth(w) + nodeWidth(predW)) / 2;

							var u = root[predW];
							placeBlock(u);
							if (sink[v] == v) sink[v] = sink[u];
							if (sink[v] != sink[u])
								shift[sink[u]] = Math.Min(shift[sink[u]], x[v] - x[u] - delta);
							else
								x[v] = Math.Max(x[v], x[u] + delta);
						}
						w = align[w];
					} while (w != v);
				}
			}

			foreach (var v in graph.Nodes)
				if (root[v] == v) placeBlock(v);

			foreach (var v in graph.Nodes)
			{
				x[v] = x[root[v]];
				if (shift[sink[root[v]]] < int.MaxValue)
					x[v] = x[v] + shift[sink[root[v]]];
			}

			return x;
		}
	}
}