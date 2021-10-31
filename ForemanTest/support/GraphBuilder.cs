using Foreman;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForemanTest
{
    // A fluid interface for building up production graphs for testing. See references for usage.
    public class GraphBuilder
    {
        public static SubgroupPrototype TestSubgroup = new SubgroupPrototype(null, "", "");


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
            var node = new SingletonNodeBuilder(SupplierNode.Create).Item(item);
            this.nodes.Add(node);
            return node;
        }


        public SingletonNodeBuilder Consumer(string item)
        {
            var node = new SingletonNodeBuilder(ConsumerNode.Create).Item(item);
            this.nodes.Add(node);
            return node;
        }

        internal RecipeBuilder Recipe(string name = null)
        {
            var node = new RecipeBuilder(name);
            this.nodes.Add(node);
            return node;
        }

        internal SingletonNodeBuilder Passthrough(string item)
        {
            var node = new SingletonNodeBuilder(PassthroughNode.Create).Item(item);
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
            DataCache dCache = new DataCache();
            var graph = new ProductionGraph(dCache);

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

            public BaseNode Built { get; protected set; } // TODO: Build if not already
            abstract internal void Build(ProductionGraph graph);
        }

        public class SingletonNodeBuilder : ProductionNodeBuilder
        {
            private Func<ItemPrototype, ProductionGraph, BaseNode> createFunction;

            public SingletonNodeBuilder(Func<Item, ProductionGraph, BaseNode> f)
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
                Built = this.createFunction(new ItemPrototype(graph.DCache, itemName, "", false, TestSubgroup, ""), graph);

                if (target > 0)
                {
                    this.Built.desiredRate = target;
                    this.Built.rateType = RateType.Manual;
                } else
                {
                    this.Built.rateType = RateType.Auto;
                }
            }
        }

        internal class RecipeBuilder : ProductionNodeBuilder
        {
            private Dictionary<string, float> inputs;
            private Dictionary<string, float> outputs;
            private string name;
            private double efficiency;

            public float target { get; private set; }

            internal RecipeBuilder(string name)
            {
                this.inputs = new Dictionary<string, float>();
                this.outputs = new Dictionary<string, float>();
                this.name = name;
            }

            internal override void Build(ProductionGraph graph)
            {
                var duration = 1;
                if (name == null)
                    name = "recipe-" + GetSequence();

                RecipePrototype recipe = new RecipePrototype(graph.DCache, name, "", TestSubgroup, "");
                recipe.Time = duration;
                foreach (KeyValuePair<string, float> kvp in inputs)
                    recipe.InternalOneWayAddIngredient(graph.DCache.Items[kvp.Key] as ItemPrototype, kvp.Value);
                foreach (KeyValuePair<string, float> kvp in outputs)
                    recipe.InternalOneWayAddProduct(graph.DCache.Items[kvp.Key] as ItemPrototype, kvp.Value);

                Built = RecipeNode.Create(recipe, graph);
                this.Built.ProductivityBonus = efficiency;

                if (target > 0)
                {
                    this.Built.desiredRate = target;
                    this.Built.rateType = RateType.Manual;
                } else
                {
                    this.Built.rateType = RateType.Auto;
                }
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

            internal RecipeBuilder Target(float target)
            {
                this.target = target;
                return this;
            }

            internal RecipeBuilder Efficiency(double bonus)
            {
                this.efficiency = bonus;
                return this;
            }

            //private Dictionary<Item, float> itemizeKeys(Dictionary<string, float> d)
            //{
            //    return d.ToDictionary(kp => new Item(kp.Key), kp => kp.Value);
            //}
        }

        public class BuiltData
        {
            public ProductionGraph Graph { get; internal set; }

            public BuiltData(ProductionGraph graph)
            {
                this.Graph = graph;
            }

            public float SupplyRate(string itemName)
            {
                return Suppliers(itemName).Where(x => x is SupplierNode).Select(x => x.actualRate).Sum();
            }

            private IEnumerable<BaseNode> Suppliers(string itemName)
            {
                return Graph.GetSuppliers(new ItemPrototype(Graph.DCache, itemName, "", false, TestSubgroup, ""));
            }

            public float ConsumedRate(string itemName)
            {
                return Consumers(itemName).Where(x => x is ConsumerNode).Select(x => x.actualRate).Sum();
            }

            private IEnumerable<BaseNode> Consumers(string itemName)
            {
                return Graph.GetConsumers(new ItemPrototype(Graph.DCache, itemName, "", false, TestSubgroup, ""));
            }

            public float RecipeRate(string name)
            {
                return Graph.Nodes
                   .Where(x => x is RecipeNode && ((RecipeNode)x).BaseRecipe.Name == name)
                   .Select(x => x.actualRate)
                   .Sum();
            }

            internal double RecipeInputRate(string name, string itemName)
            {
                return Graph.Nodes
                   .Where(x => x is RecipeNode && ((RecipeNode)x).BaseRecipe.Name == name)
                   .Select(x => (RecipeNode)x)
                   .First()
                   .GetSuppliedRate(new ItemPrototype(Graph.DCache, itemName, "", false, TestSubgroup, ""));
            }
        }
    }
}