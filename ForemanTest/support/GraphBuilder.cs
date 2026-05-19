using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ForemanTest.support {
    // Fluent builder for small production graphs used by solver tests.
    public class GraphBuilder {
        private readonly List<Tuple<ProductionNodeBuilder, ProductionNodeBuilder>> links = [];
        private readonly HashSet<ProductionNodeBuilder> nodes = [];

        private GraphBuilder() { }

        public static GraphBuilder Create() => new();

        internal SingletonNodeBuilder Supply(string item) {
            var node = new SingletonNodeBuilder(isSupplier: true).Item(item);
            nodes.Add(node);
            return node;
        }

        public SingletonNodeBuilder Consumer(string item) {
            var node = new SingletonNodeBuilder(isSupplier: false).Item(item);
            nodes.Add(node);
            return node;
        }

        internal RecipeBuilder Recipe(string? name = null) {
            var node = new RecipeBuilder(name);
            nodes.Add(node);
            return node;
        }

        internal SingletonNodeBuilder Passthrough(string item) {
            var node = new SingletonNodeBuilder(isSupplier: null).Item(item);
            nodes.Add(node);
            return node;
        }

        internal void Link(params ProductionNodeBuilder[] nodeBuilders) {
            var sequence = (IEnumerable<ProductionNodeBuilder>)nodeBuilders;
            links.AddRange(sequence.Zip(sequence.Skip(1), Tuple.Create));
        }

        internal BuiltData Build() {
            var cache = new DataCache(filterRecipes: true);
            var subgroup = new SubgroupPrototype(cache, "§§test:subgroup", "z");
            var quality = new QualityPrototype(cache, "normal", "Normal", "a");

            var graph = new ProductionGraph { DefaultAssemblerQuality = quality };

            var context = new BuildContext(cache, subgroup, quality);

            foreach (var node in nodes)
                node.Build(context, graph);

            foreach (var (lhs, rhs) in links) {
                foreach (var item in lhs.BuiltOutputs.Intersect(rhs.BuiltInputs, ItemQualityPairComparer.Instance))
                    graph.CreateLink(lhs.BuiltNode, rhs.BuiltNode, item);
            }

            return new BuiltData(graph, context);
        }

        public sealed class BuildContext(DataCache cache, SubgroupPrototype subgroup, IQuality quality) {
            public DataCache Cache { get; } = cache;
            public SubgroupPrototype Subgroup { get; } = subgroup;
            public IQuality Quality { get; } = quality;
            public IAssembler TestAssembler { get; } = TestPrototypeFactory.CreateTestAssembler(cache);

            public ItemQualityPair ItemPair(string name) {
                var item = TestDataCacheHelper.GetOrCreateItem(Cache, Subgroup, name);
                return new ItemQualityPair(item, Quality);
            }
        }

        public abstract class ProductionNodeBuilder {
            private BaseNode? builtNode;

            public BaseNode BuiltNode {
                get => builtNode ?? throw new InvalidOperationException("Call Build before reading BuiltNode.");
                protected set => builtNode = value;
            }
            public IEnumerable<ItemQualityPair> BuiltInputs { get; protected set; } = [];
            public IEnumerable<ItemQualityPair> BuiltOutputs { get; protected set; } = [];

            internal abstract void Build(BuildContext context, ProductionGraph graph);
        }

        public class SingletonNodeBuilder : ProductionNodeBuilder {
            private readonly bool? isSupplier;
            private string itemName = "";
            private float target;

            internal SingletonNodeBuilder(bool? isSupplier) => this.isSupplier = isSupplier;

            internal SingletonNodeBuilder Item(string item) {
                itemName = item;
                return this;
            }

            internal SingletonNodeBuilder Target(float targetValue) {
                target = targetValue;
                return this;
            }

            internal override void Build(BuildContext context, ProductionGraph graph) {
                var pair = context.ItemPair(itemName);
                BaseNode node = isSupplier switch {
                    true => graph.CreateSupplierNode(pair, Point.Empty),
                    false => graph.CreateConsumerNode(pair, Point.Empty),
                    _ => graph.CreatePassthroughNode(pair, Point.Empty)
                };
                BuiltNode = node;
                BuiltInputs = node.Inputs;
                BuiltOutputs = node.Outputs;

                if (graph.RequestNodeController(node) is BaseNodeController controller) {
                    if (target > 0) {
                        controller.SetRateType(RateType.Manual);
                        controller.SetDesiredSetValue(target);
                    } else
                        controller.SetRateType(RateType.Auto);
                }
            }
        }

        sealed internal class RecipeBuilder : ProductionNodeBuilder {
            private readonly Dictionary<string, float> inputs = [];
            private readonly Dictionary<string, float> outputs = [];
            private string? name;
            private double efficiency;
            public float Target { get; private set; }

            internal RecipeBuilder(string? name) => this.name = name;

            internal override void Build(BuildContext context, ProductionGraph graph) {
                name ??= "recipe-" + Guid.NewGuid().ToString("N")[..8];

                var recipe = new RecipePrototype(context.Cache, name, name, context.Subgroup, "z");
                TestPrototypeFactory.SetRecipeTime(recipe, 1);
                TestPrototypeFactory.LinkRecipeAndAssembler(recipe, (AssemblerPrototype)context.TestAssembler);
                TestDataCacheHelper.RegisterRecipe(context.Cache, recipe);

                foreach (var kvp in inputs) {
                    var item = TestDataCacheHelper.GetOrCreateItem(context.Cache, context.Subgroup, kvp.Key);
                    recipe.InternalOneWayAddIngredient(item, kvp.Value);
                }

                foreach (var kvp in outputs) {
                    var item = TestDataCacheHelper.GetOrCreateItem(context.Cache, context.Subgroup, kvp.Key);
                    // ProductPSet must be non-zero for ExtraProductivityBonus to affect solver ratios.
                    double pQuantity = efficiency > 0 ? kvp.Value : 0;
                    recipe.InternalOneWayAddProduct(item, kvp.Value, pQuantity);
                }

                var recipeNode = graph.CreateRecipeNode(new RecipeQualityPair(recipe, context.Quality), Point.Empty);
                BuiltNode = recipeNode;
                BuiltInputs = recipeNode.Inputs;
                BuiltOutputs = recipeNode.Outputs;

                if (graph.RequestNodeController(recipeNode) is RecipeNodeController recipeController) {
                    recipeController.SetExtraProductivityBonus(efficiency);
                    if (Target > 0) {
                        recipeController.SetRateType(RateType.Manual);
                        recipeController.SetDesiredSetValue(Target);
                    } else
                        recipeController.SetRateType(RateType.Auto);
                }
            }

            internal RecipeBuilder Input(string itemName, float amount) {
                inputs[itemName] = amount;
                return this;
            }

            internal RecipeBuilder Output(string itemName, float amount) {
                outputs[itemName] = amount;
                return this;
            }

            internal RecipeBuilder SetTarget(float targetValue) {
                Target = targetValue;
                return this;
            }

            internal RecipeBuilder SetEfficiency(double bonus) {
                efficiency = bonus;
                return this;
            }
        }

        public class BuiltData(ProductionGraph graph, GraphBuilder.BuildContext context) {
            public ProductionGraph Graph { get; } = graph;
            public DataCache Cache { get; } = context.Cache;
            private readonly BuildContext context = context;

            public void Solve() => Graph.OptimizeGraphNodeValues();

            public float SupplyRate(string itemName) =>
                (float)Suppliers(itemName).OfType<SupplierNode>().Sum(n => n.ActualRate);

            public float ConsumedRate(string itemName) =>
                (float)Consumers(itemName).OfType<ConsumerNode>().Sum(n => n.ActualRate);

            public float RecipeRate(string name) =>
                (float)Graph.Nodes
                    .OfType<RecipeNode>()
                    .Where(n => n.BaseRecipe.Recipe?.Name == name)
                    .Sum(n => n.ActualRate);

            /// <summary>Sum of incoming link throughput for an item (legacy GetSuppliedRate).</summary>
            public double RecipeInputRate(string recipeName, string itemName) =>
                RecipeInputRate(recipeName, itemName, supplier: null);

            /// <summary>Incoming throughput from a specific supplier node, if given.</summary>
            public double RecipeInputRate(string recipeName, string itemName, BaseNode? supplier) {
                var recipeNode = Graph.Nodes
                    .OfType<RecipeNode>()
                    .First(n => n.BaseRecipe.Recipe?.Name == recipeName);
                return recipeNode.InputLinks
                    .Where(l => l.Item.Item?.Name == itemName && (supplier is null || l.SupplierNode == supplier))
                    .Sum(l => l.Throughput);
            }

            private IEnumerable<BaseNode> Suppliers(string itemName) =>
                Graph.GetSuppliers(context.ItemPair(itemName));

            private IEnumerable<BaseNode> Consumers(string itemName) =>
                Graph.GetConsumers(context.ItemPair(itemName));
        }

        private sealed class ItemQualityPairComparer : IEqualityComparer<ItemQualityPair> {
            public static ItemQualityPairComparer Instance { get; } = new();
            public bool Equals(ItemQualityPair x, ItemQualityPair y) =>
                x.Item?.Name == y.Item?.Name && x.Quality?.Name == y.Quality?.Name;
            public int GetHashCode(ItemQualityPair obj) =>
                HashCode.Combine(obj.Item?.Name, obj.Quality?.Name);
        }
    }
}
