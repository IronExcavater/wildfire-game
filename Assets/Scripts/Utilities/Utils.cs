using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Utilities
{
    public static class Utils
    {
        public static void SetLayerRecursive(this GameObject obj, int layer)
        {
            foreach (var c in obj.GetComponentsInChildren<Transform>(true))
                c.gameObject.layer = layer;
        }


        public static Vector3 ClosestPointOnLine(this Vector3 point, Vector3 linePoint, Vector3 lineDirection,
            float length = float.MaxValue)
        {
            if (length.Equals(float.MaxValue)) length = lineDirection.magnitude;
            lineDirection.Normalize();
            return linePoint + lineDirection * point.ClosestDistanceOnLine(linePoint, lineDirection, length);
        }

        public static float ClosestDistanceOnLine(this Vector3 point, Vector3 linePoint, Vector3 lineDirection,
            float length = float.MaxValue)
        {
            if (length.Equals(float.MaxValue)) length = lineDirection.magnitude;
            lineDirection.Normalize();
            return Mathf.Clamp(Vector3.Dot(point - linePoint, lineDirection), 0, length);
        }

        public static void AddValueToAverage(ref double average, ref int count, double value)
        {
            average += (value - average) / ++count;
        }

        public static void RemoveValueFromAverage(ref double average, ref int count, double value)
        {
            if (count <= 1)
            {
                average = count = 0;
                return;
            }
            average += (average - value) / --count;
        }

        public static int EuclideanMod(int a, int b) => (a % b + b) % b;

        public static Type GetBaseType(this Type type)
        {
            if (type.IsArray) return type.GetElementType();

            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length == 1) return genericArgs[0];

                var enumerableInterface = type
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (enumerableInterface != null) return enumerableInterface.GetGenericArguments()[0];
            }

            return type;
        }

        public static List<Type> GetSubtypes(this Type type, Func<Type, bool> predicate = null)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException e)
                    {
                        Debug.LogWarning($"Failed to load types from assembly {a.FullName}:\n{e.Message}");
                        return e.Types.Where(t => t != null);
                    }
                })
                .Where(t => t != type && type.IsAssignableFrom(t) && (predicate?.Invoke(t) ?? true))
                .ToList();
        }

        public static bool IsSubclassOfRawGeneric(this Type type, Type baseType)
        {
            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (current == baseType) return true;
                type = type.BaseType;
            }
            return false;
        }

        public static string GetName<TParent>(TParent parent, object value)
        {
            if (parent == null || value == null) return null;

            foreach (var field in parent.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (ReferenceEquals(field.GetValue(parent), value))
                    return field.Name;
            return null;
        }

        public static List<T> GetRange<T>(this IReadOnlyList<T> source, int index, int count)
        {
            return source.Skip(index).Take(count).ToList();
        }

        public static bool HasReferenceIntegrity(object oldValue, object newValue, string contextName = "")
        {
            if (oldValue == null || newValue == null) return false;

            var fields = oldValue.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => !f.FieldType.IsPrimitive)
                .ToList();

            var stable = true;
            foreach (var field in fields)
            {
                var oldRef = field.GetValue(oldValue);
                var newRef = field.GetValue(newValue);
                if (ReferenceEquals(oldRef, newRef)) continue;

                Debug.LogWarning($"⚠ Reference changed for {contextName}: Field '{field.Name}'");
                stable = false;
            }

            if (stable)
                Debug.Log($"✓ Internal references preserved for {contextName}");
            else
                Debug.LogWarning($"⚠ Internal references changed for {contextName}");

            return stable;
        }

        static readonly string[] SkipNamespaces = {
            "System.Reflection", "System.Type", "System.Globalization", "System.Configuration",
            "System.Security", "System.Threading", "System.Diagnostics", "System.Runtime",
            "Microsoft", "UnityEditor", "UnityEngine.SceneManagement", "Newtonsoft.Json"
        };

        public static string ToString(object obj, HashSet<object> visited = null, int indent = 0)
        {
            if (obj == null) return "null";

            visited ??= new HashSet<object>();
            if (!obj.GetType().IsValueType && !visited.Add(obj))
                return "[CyclicRef]";

            var type = obj.GetType();
            var indentStr = new string(' ', indent * 2);
            var childIndentStr = new string(' ', (indent + 1) * 2);

            if (type.IsPrimitive || obj is string) return obj.ToString();

            if (obj is IEnumerable enumerable)
            {
                var items = new List<string>();
                foreach (var e in enumerable)
                {
                    try { items.Add($"{childIndentStr}{ToString(e, visited, indent + 1)}"); }
                    catch (Exception ex) { items.Add($"{childIndentStr}[Error: {ex.GetType().Name}]"); }
                }
                return "[\n" + string.Join(",\n", items) + $"\n{indentStr}]";
            }

            if (type.Namespace != null && SkipNamespaces.Any(ns => type.Namespace.StartsWith(ns)))
                return $"<{type.Name}>";

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            var parts = new List<string>();
            foreach (var field in fields)
                parts.Add($"{childIndentStr}{field.Name}: {ToString(field.GetValue(obj), visited, indent + 1)}");

            foreach (var prop in props)
                try
                {
                    parts.Add($"{childIndentStr}{prop.Name}: {ToString(prop.GetValue(obj), visited, indent + 1)}");
                }
                catch (Exception e)
                {
                    parts.Add($"{childIndentStr}{prop.Name}: [Error: {e.GetType().Name}]");
                }

            return $"{type.Name} {{\n{string.Join(",\n", parts)}\n{indentStr}}}";
        }
    }
}
