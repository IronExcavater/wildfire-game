using UnityEngine;
using Utilities;
using Utilities.Attributes;

namespace Generation
{
    [CreateAssetMenu(fileName = "GeneratorPools", menuName = "Generation/Generator Pools")]
    public class GeneratorPool : ScriptableObject
    {
        [SerializedDictionaryField] public SerializedObjectPoolDictionary pools = new();
    }
}
