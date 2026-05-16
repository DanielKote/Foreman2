using Foreman;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ForemanTest.support {
    /// <summary>Creates Foreman prototypes for isolated solver tests via reflection (non-public types).</summary>
    internal static class TestPrototypeFactory {
        private static readonly Type AssemblerPrototypeType =
            typeof(DataCache).Assembly.GetType("Foreman.AssemblerPrototype")
            ?? throw new InvalidOperationException("Foreman.AssemblerPrototype not found");

        public static Assembler CreateTestAssembler(DataCache cache) =>
            (Assembler)ReflectionTestHelper.RequireInstance(
                Activator.CreateInstance(
                    AssemblerPrototypeType,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null,
                    args: new object[] { cache, "§§test:assembler", "Test Assembler", EntityType.Assembler, EnergySource.Electric, false },
                    culture: null),
                "Failed to create test assembler prototype.");

        public static void LinkRecipeAndAssembler(RecipePrototype recipe, Assembler assembler) {
            // Internal property backing fields are private and visible to reflection across assemblies.
            var recipeAssemblers = GetAutoPropertyBackingField(recipe, "<assemblers>k__BackingField");
            var assemblerRecipes = GetAutoPropertyBackingField(assembler, "<recipes>k__BackingField");

            ReflectionTestHelper.RequireMethod(recipeAssemblers.GetType(), "Add", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(recipeAssemblers, new object[] { assembler });
            ReflectionTestHelper.RequireMethod(assemblerRecipes.GetType(), "Add", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(assemblerRecipes, new object[] { recipe });
        }

        public static void SetRecipeTime(RecipePrototype recipe, double time) {
            PropertyInfo timeProperty = ReflectionTestHelper.RequireProperty(
                typeof(RecipePrototype), "Time", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo setter = ReflectionTestHelper.Require(
                timeProperty.GetSetMethod(nonPublic: true),
                "RecipePrototype.Time setter was not found.");
            setter.Invoke(recipe, new object[] { time });
        }

        private static object GetAutoPropertyBackingField(object instance, string backingFieldName) {
            FieldInfo field = ReflectionTestHelper.RequireField(
                instance.GetType(), backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return ReflectionTestHelper.RequireInstance(
                field.GetValue(instance),
                $"Backing field {instance.GetType().Name}.{backingFieldName} returned null.");
        }
    }
}