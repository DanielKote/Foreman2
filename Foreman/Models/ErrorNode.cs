using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
	public class ErrorNode : ProductionNode
	{
		public override string DisplayName { get { return "ERROR NODE"; } }
		public override IEnumerable<Item> Inputs { get { return new List<Item>(); } }
		public override IEnumerable<Item> Outputs { get { return new List<Item>(); } }
		public override float GetConsumeRate(Item item) { Trace.Fail(String.Format("Error node not consume {0}, nothing should be asking for the consumption rate!", item.FriendlyName)); return 0; }
		public override float GetSupplyRate(Item item) { Trace.Fail(String.Format("Error node not suppy {0}, nothing should be asking for the supply rate!", item.FriendlyName)); return 0; }
		internal override double inputRateFor(Item item) { Trace.Fail(String.Format("Error node not consume {0}, nothing should be asking for the input rate!", item.FriendlyName)); return 0; }
		internal override double outputRateFor(Item item) { Trace.Fail(String.Format("Error node not suppy {0}, nothing should be asking for the output rate!", item.FriendlyName)); return 0; }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "ERROR");
		}

		protected ErrorNode(ProductionGraph graph) : base(graph) { }

		public static ErrorNode Create(ProductionGraph graph)
		{
			ErrorNode node = new ErrorNode(graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
		}
	}
}
