using System;
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
    }
}
