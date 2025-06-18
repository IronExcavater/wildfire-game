using System;
using System.Collections.Generic;
using System.Linq;
using Generation.Objects;
using UnityEditor;
using UnityEngine;
using Utilities.Serializables;

namespace Editor.Utilities
{
    public class SerializedDictionaryValidator
    {
        private static readonly Dictionary<string, Type> _typeCache = new();

        public void ValidateEntry(SerializedProperty dictionary, SerializedProperty key, SerializedProperty value, out string message, out MessageType type)
        {
            message = null;
            type = MessageType.Error;

            switch (dictionary.type)
            {
                case nameof(SerializedObjectPoolDictionary):
                    var keyType = ResolveType(key.stringValue);
                    Debug.Log($"String: {key.stringValue}, Expected: {typeof(TerrainObject)}");
                    if (keyType == null || !typeof(MonoBehaviour).IsAssignableFrom(keyType))
                    {
                        message = $"{key.stringValue} is not a valid MonoBehaviour type [{keyType}]";
                        return;
                    }

                    for (var i = 0; i < value.arraySize; i++)
                    {
                        var obj = value.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;

                        if (obj == null || obj.GetComponent(keyType) == null)
                        {
                            message = $"{obj} is missing component {keyType}";
                            return;
                        }
                    }
                    break;
            }
        }

        public static Type ResolveType(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return null;

            if (_typeCache.TryGetValue(fullName, out var cached))
                return cached;

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.FullName == fullName);

            _typeCache[fullName] = type;
            return type;
        }
    }
}
