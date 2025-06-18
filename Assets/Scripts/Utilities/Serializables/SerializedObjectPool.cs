using System.Collections.Generic;
using System;
using UnityEngine;

namespace Utilities.Serializables
{
    [Serializable]
    public class SerializedObjectPoolDictionary : SerializedDictionary<SerializedObjectPool, string, List<GameObject>> { }

    [Serializable]
    public class SerializedObjectPool : KeyValuePair<string, List<GameObject>> { }
}
