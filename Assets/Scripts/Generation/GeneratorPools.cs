using UnityEngine;
using Utilities.Attributes;
using Utilities.Serializables;

namespace Generation
{
    [CreateAssetMenu(fileName = "GeneratorPools", menuName = "Generation/Generator Pools")]
    public class GeneratorPools : ScriptableObject
    {
        [SerializedDictionaryField] public SerializedObjectPoolDictionary pools = new();
    }
}
