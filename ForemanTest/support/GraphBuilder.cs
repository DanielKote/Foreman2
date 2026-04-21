using Foreman;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForemanTest {
    // A fluid interface for building up production graphs for testing. See references for usage.
    public class GraphBuilder {
        public static SubgroupPrototype TestSubgroup = new(null, "", "");


        private static int counter;

        protected static int GetSequence() {
            counter += 1;
            return counter;
        }

        private List<Tuple<ProductionNodeBuilder, ProductionNodeBuilder>> links;
        private ISet<ProductionNodeBuilder> nodes;

        protected GraphBuilder() {
            links = [];
            nodes = new HashSet<ProductionNodeBuilder>();
        }

        public static GraphBuilder Create() {
            return new GraphBuilder();
        }

        internal SingletonNodeBuilder Supply(string item) {
            var node = new SingletonNodeBuilder(SupplierNode.Create).Item(item);
            nodes.Add(node);
            return node;
        }


        public SingletonNodeBuilder Consumer(string item) {
            var node = new SingletonNodeBuilder(ConsumerNode.Create).Item(item);
            nodes.Add(node);
            return node;
        }

        internal RecipeBuilder Recipe(string name = null) {
            var node = new RecipeBuilder(name);
            nodes.Add(node);
            return node;
        }

        internal SingletonNodeBuilder Passthrough(string item) {
            var node = new SingletonNodeBuilder(PassthroughNode.Create).Item(item);
            nodes.Add(node);
            return node;
        }

        // Link the provided nodes by automatically matching up inputs to outputs.
        // The same builder can be passed to multiple different invocations, to enable building of complex graphs.
        internal void Link(params ProductionNodeBuilder[] nodeBuilders) {
            IEnumerable<ProductionNodeBuilder> bs = nodeBuilders;
            var pairs = bs.Zip(bs.Skip(1), Tuple.Create);

            links.AddRange(pairs);
        }

        internal BuiltData Build() {
            var dCache = new DataCache(true);
            var graph = new ProductionGraph(dCache);

            foreach (var node in nodes) {
                node.Build(graph);
            }

            foreach (var link in links) {
                var lhs = link.Item1;
                var rhs = link.Item2;

                foreach (var item in lhs.Built.Outputs.Intersect(rhs.Built.Inputs)) {
                    NodeLink.Create(lhs.Built, rhs.Built, item);
                }
            }

            return new BuiltData(graph);
        }

        public abstract class ProductionNodeBuilder {
            public BaseNode Built { get; protected set; } // TODO: Build if not already
            internal abstract void Build(ProductionGraph graph);
        }

        public class SingletonNodeBuilder(Func<Item, ProductionGraph, BaseNode> f) : ProductionNodeBuilder {
            private Func<ItemPrototype, ProductionGraph, BaseNode> createFunction = f;

            public string itemName { get; private set; }
            public float target { get; private set; }

            internal SingletonNodeBuilder Item(string item) {
                itemName = item;
                return this;
            }

            internal SingletonNodeBuilder Target(float target) {
                this.target = target;
                return this;
            }

            internal override void Build(ProductionGraph graph) {
                Built = createFunction(new ItemPrototype(graph.DCache, itemName, "", false, TestSubgroup, ""), graph);

                if (target > 0) {
                    Built.desiredRate = target;
                    Built.rateType = RateType.Manual;
                } else {
                    Built.rateType = RateType.Auto;
                }
            }
        }

        internal class RecipeBuilder : ProductionNodeBuilder {
            private Dictionary<string, float> inputs;
            private Dictionary<string, float> outputs;
            private string name;
            private double efficiency;

            public float target { get; private set; }

            internal RecipeBuilder(string name) {
                inputs = new Dictionary<string, float>();
                outputs = new Dictionary<string, float>();
                this.name = name;
            }

            internal override void Build(ProductionGraph graph) {
                var duration = 1;
                if (name == null)
                    name = "recipe-" + GetSequence();

                var recipe = new RecipePrototype(graph.DCache, name, "", TestSubgroup, "") {
                    Time = duration
                };
                foreach (var kvp in inputs)
                    recipe.InternalOneWayAddIngredient(graph.DCache.Items[kvp.Key] as ItemPrototype, kvp.Value);
                foreach (var kvp in outputs)
                    recipe.InternalOneWayAddProduct(graph.DCache.Items[kvp.Key] as ItemPrototype, kvp.Value);

                Built = RecipeNode.Create(recipe, graph);
                Built.ProductivityBonus = efficiency;

                if (target > 0) {
                    Built.desiredRate = target;
                    Built.rateType = RateType.Manual;
                } else {
                    Built.rateType = RateType.Auto;
                }
            }

            internal RecipeBuilder Input(string itemName, float amount) {
                inputs.Add(itemName, amount);
                return this;
            }

            internal RecipeBuilder Output(string itemName, float amount) {
                outputs.Add(itemName, amount);
                return this;
            }

            internal RecipeBuilder Target(float target) {
                this.target = target;
                return this;
            }

            internal RecipeBuilder Efficiency(double bonus) {
                efficiency = bonus;
                return this;
            }

            //private Dictionary<Item, float> itemizeKeys(Dictionary<string, float> d)
            //{
            //    return d.ToDictionary(kp => new Item(kp.Key), kp => kp.Value);
            //}
        }

        public class BuiltData(ProductionGraph graph) {
            public ProductionGraph Graph { get; internal set; } = graph;

            public float SupplyRate(string itemName) {
                return Suppliers(itemName).Where(x => x is SupplierNode).Select(x => x.actualRate).Sum();
            }

            private IEnumerable<BaseNode> Suppliers(string itemName) {
                return Graph.GetSuppliers(new ItemPrototype(Graph.DCache, itemName, "", false, TestSubgroup, ""));
            }

            public float ConsumedRate(string itemName) {
                return Consumers(itemName).Where(x => x is ConsumerNode).Select(x => x.actualRate).Sum();
            }

            private IEnumerable<BaseNode> Consumers(string itemName) {
                return Graph.GetConsumers(new ItemPrototype(Graph.DCache, itemName, "", false, TestSubgroup, ""));
            }

            public float RecipeRate(string name) {
                return Graph.Nodes
                    .Where(x => x is RecipeNode && ((RecipeNode) x).BaseRecipe.Name == name)
                    .Select(x => x.actualRate)
                    .Sum();
            }

            internal double RecipeInputRate(string name, string itemName) {
                return Graph.Nodes
                    .Where(x => x is RecipeNode && ((RecipeNode) x).BaseRecipe.Name == name)
                    .Select(x => (RecipeNode) x)
                    .First()
                    .GetSuppliedRate(new ItemPrototype(Graph.DCache, itemName, "", false, TestSubgroup, ""));
            }
        }
    }
}