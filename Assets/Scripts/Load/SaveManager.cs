using System;
using System.Collections.Generic;
using System.IO;
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

            _settings.AddListener((_, _) => Save(_settings));
            Load(_settings);
        }

        private static DirectoryInfo SaveDirectory()
        {
            var path = $"{Application.persistentDataPath}/Save/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return new DirectoryInfo(path);
        }

        private static FileInfo SaveFile(IProperty property, out string name)
        {
            name = Utils.GetName(Instance, property).ToLower();
            name = Regex.Replace(name, @"^[_\s]+", "");
            name = Regex.Replace(name, "[^a-z0-9]+", "-");
            return new FileInfo(Path.Combine(SaveDirectory().FullName, $"{name}.json"));
        }

        private static void Save(IProperty property)
        {
            if (_currentlyLoading.Contains(property)) return;

            var file = SaveFile(property, out var name);

            try
            {
                var json = JsonConvert.SerializeObject(property, Instance._jsonSettings);
                File.WriteAllText(file.FullName, json);
                Debug.Log($"Saved {name} to {file.FullName}");
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"No permission to write save file: {file.FullName}\n{e}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {name}: {e}");
            }
        }

        private static void Load(IProperty property)
        {
            _currentlyLoading.Add(property);

            var file = SaveFile(property, out var name);

            try
            {
                var text = File.ReadAllText(file.FullName);
                var propertyType = property.GetType();
                var valueProp = propertyType.GetProperty("Value");
                var setValue = propertyType.GetMethod("SetValue");
                var original = Utils.GetFieldReferences(property);

                // Deserialize and apply
                var temp = JsonConvert.DeserializeObject(text, propertyType, Instance._jsonSettings);
                var newValue = valueProp?.GetValue(temp);
                setValue?.Invoke(property, new[] { newValue, true });

                // Compare references directly
                var updated = Utils.GetFieldReferences(property);
                Utils.HasReferenceIntegrity(original, updated, name);
                Debug.Log($"Loaded {name} from {file.FullName}");
            }
            catch (FileNotFoundException e)
            {
                Debug.LogWarning(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"No permission to read save file: {file.FullName}\n{e}");
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
