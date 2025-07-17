using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Load.Json;
using Newtonsoft.Json;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Load
{
    [DoNotDestroySingleton]
    public class SaveManager : Singleton<SaveManager>
    {
        private readonly JsonSerializerSettings _jsonSettings = new()
        {
            Formatting = Formatting.Indented,
            Converters =
            {
                new PropertyConverter()
            }
        };

        private static readonly HashSet<IProperty> _currentlyLoading = new();

        private Property<Settings> _settings = new();
        public static Property<Settings> Settings => Instance._settings;

        protected override void Awake()
        {
            base.Awake();

            Save(_settings);
            _settings.AddListener((_, _) => Save(_settings));
            Load(_settings);
        }

        private static string JsonPath(IProperty property, out string name)
        {
            name = Utils.GetName(Instance, property).ToLower();
            name = Regex.Replace(name, @"^[_\s]+", "");
            name = Regex.Replace(name, "[^a-z0-9]+", "-");
            return $"{Application.persistentDataPath}/{name}.json";
        }

        private static void Save(IProperty property)
        {
            if (_currentlyLoading.Contains(property)) return;

            var path = JsonPath(property, out var name);

            try
            {
                var json = JsonConvert.SerializeObject(property, Instance._jsonSettings);
                File.WriteAllText(path, json);
                Debug.Log($"Saved {name} to {path}");
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"No permission to write save file: {path}\n{e}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {name}: {e}");
            }
        }

        private static void Load(IProperty property)
        {
            _currentlyLoading.Add(property);

            var path = JsonPath(property, out var name);

            try
            {
                var text = File.ReadAllText(path);
                var propertyType = property.GetType();
                var valueProp = propertyType.GetProperty("Value");
                var setValue = propertyType.GetMethod("SetValue");
                var originalValue = valueProp?.GetValue(property);

                // Deserialize and apply
                var temp = JsonConvert.DeserializeObject(text, propertyType, Instance._jsonSettings);
                var newValue = valueProp?.GetValue(temp);
                setValue?.Invoke(property, new[] { newValue, true });

                // Compare references directly
                var updatedValue = valueProp?.GetValue(property);
                Utils.HasReferenceIntegrity(originalValue, updatedValue, name);
                Debug.Log($"Loaded {name} from {path}");
            }
            catch (FileNotFoundException e)
            {
                Debug.LogWarning(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"No permission to read save file: {path}\n{e}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load {name}: {e}");
            }
            finally
            {
                _currentlyLoading.Remove(property);
            }
        }
    }
}
