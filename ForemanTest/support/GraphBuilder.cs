using Foreman;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForemanTest
{
    // A fluid interface for building up production graphs for testing. See references for usage.
    public class GraphBuilder
    {
        private static int counter = 0;
        protected static int GetSequence()
        {
            counter += 1;
            return counter;
        }

        private List<Tuple<ProductionNodeBuilder, ProductionNodeBuilder>> links;
        private ISet<ProductionNodeBuilder> nodes;

        protected GraphBuilder()
        {
            this.links = new List<Tuple<ProductionNodeBuilder, ProductionNodeBuilder>>();
            this.nodes = new HashSet<ProductionNodeBuilder>();
        }

        public static GraphBuilder Create()
        {
            return new GraphBuilder();
        }

        internal SingletonNodeBuilder Supply(string item)
        {
            var node = new SingletonNodeBuilder(SupplyNode.Create).Item(item);
            this.nodes.Add(node);
            return node;
        }


        public SingletonNodeBuilder Consumer(string item)
        {
            var node = new SingletonNodeBuilder(ConsumerNode.Create).Item(item);
            this.nodes.Add(node);
            return node;
        }

        internal RecipeBuilder Recipe()
        {
            var node = new RecipeBuilder();
            this.nodes.Add(node);
            return node;
        }

        // Link the provided nodes by automatically matching up inputs to outputs.
        // The same builder can be passed to multiple different invocations, to enable building of complex graphs.
        internal void Link(params ProductionNodeBuilder[] nodeBuilders)
        {
            var bs = ((IEnumerable<ProductionNodeBuilder>)nodeBuilders);
            var pairs = bs.Zip(bs.Skip(1), Tuple.Create);

            this.links.AddRange(pairs);
        }

        internal BuiltData Build()
        {
            var graph = new ProductionGraph();

            foreach (var node in this.nodes)
            {
                node.Build(graph);
            }

            foreach (var link in this.links)
            {
                var lhs = link.Item1;
                var rhs = link.Item2;

                foreach (var item in lhs.Built.Outputs.Intersect(rhs.Built.Inputs))
                {
                    NodeLink.Create(lhs.Built, rhs.Built, item);
                }
            }
            return new BuiltData(graph);
        }

        abstract public class ProductionNodeBuilder
        {

            public ProductionNode Built { get; protected set; } // TODO: Build if not already
            abstract internal void Build(ProductionGraph graph);
        }

        public class SingletonNodeBuilder : ProductionNodeBuilder
        {
            private Func<Item, ProductionGraph, ProductionNode> createFunction;

            public SingletonNodeBuilder(Func<Item, ProductionGraph, ProductionNode> f)
            {
                this.createFunction = f;
            }

            public string itemName { get; private set; }
            public float target { get; private set; }

            internal SingletonNodeBuilder Item(string item)
            {
                this.itemName = item;
                return this;
            }

            internal SingletonNodeBuilder Target(float target)
            {
                this.target = target;
                return this;
            }

            internal override void Build(ProductionGraph graph)
            {
                Built = this.createFunction(new Item(itemName), graph);

                if (target > 0)
                {
                    this.Built.desiredRate = target;
                    this.Built.rateType = RateType.Manual;
                }
            }
        }

        internal class RecipeBuilder : ProductionNodeBuilder
        {
            private Dictionary<string, float> inputs;
            private Dictionary<string, float> outputs;

            internal RecipeBuilder()
            {
                this.inputs = new Dictionary<string, float>();
                this.outputs = new Dictionary<string, float>();
            }

            internal override void Build(ProductionGraph graph)
            {
                var duration = 1;
                var recipeName = "recipe-" + GetSequence();

                Recipe recipe = new Recipe(recipeName, duration, itemizeKeys(inputs), itemizeKeys(outputs));
                Built = RecipeNode.Create(recipe, graph);
            }

            internal RecipeBuilder Input(string itemName, float amount)
            {
                inputs.Add(itemName, amount);
                return this;
            }

            internal RecipeBuilder Output(string itemName, float amount)
            {
                outputs.Add(itemName, amount);
                return this;
            }

            private Dictionary<Item, float> itemizeKeys(Dictionary<string, float> d)
            {
                return d.ToDictionary(kp => new Item(kp.Key), kp => kp.Value);
            }
        }

        public class BuiltData
        {
            public ProductionGraph Graph { get; internal set; }

            public BuiltData(ProductionGraph graph)
            {
                this.Graph = graph;
            }

            public ProductionNode Supply(string itemName)
            {
                return Graph.GetSuppliers(new Item(itemName)).First();
            }
        }
    }

}
