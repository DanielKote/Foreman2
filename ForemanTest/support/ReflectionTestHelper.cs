using System;
using System.Reflection;

namespace ForemanTest.support {
    internal static class ReflectionTestHelper {
        internal static T Require<T>(T? value, string description) where T : class {
            return value is null ? throw new InvalidOperationException(description) : value;
        }

        internal static object RequireInstance(object? value, string description) =>
            value ?? throw new InvalidOperationException(description);

        internal static FieldInfo RequireField(Type type, string name, BindingFlags flags) =>
            Require(type.GetField(name, flags), $"Field {type.Name}.{name} was not found.");

        internal static PropertyInfo RequireProperty(Type type, string name, BindingFlags flags) =>
            Require(type.GetProperty(name, flags), $"Property {type.Name}.{name} was not found.");

        internal static MethodInfo RequireMethod(Type type, string name, BindingFlags flags) =>
            Require(type.GetMethod(name, flags), $"Method {type.Name}.{name} was not found.");
    }
}
