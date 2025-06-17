using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class SerializedObjectPoolDictionary : SerializedDictionary<SerializedObjectPool, string, List<GameObject>>
    {
        public override List<GameObject> GetValue(string key)
        {
            _dictionary.TryGetValue(key, out var value);
            return value;
        }
    }

    [Serializable]
    public class SerializedObjectPool : KeyValuePair<string, List<GameObject>> { }
}
