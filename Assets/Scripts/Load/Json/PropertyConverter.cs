using System;
using Newtonsoft.Json;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Load.Json
{
    public class PropertyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IProperty).IsAssignableFrom(objectType) && objectType.IsSubclassOfRawGeneric(typeof(PropertyBase<,,>));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueProp = value?.GetType().GetProperty("Value");
            var innerValue = valueProp?.GetValue(value);
            serializer.Serialize(writer, innerValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var innerType = objectType.GetProperty("Value")?.PropertyType;
            var innerValue = serializer.Deserialize(reader, innerType);

            var instance = Activator.CreateInstance(objectType);
            objectType.GetMethod("SetValue")?.Invoke(instance, new[] {innerValue});
            return instance;
        }
    }
}
