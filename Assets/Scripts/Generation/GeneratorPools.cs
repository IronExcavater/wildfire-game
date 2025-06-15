using System.Collections.Generic;
using UnityEngine;
using Utilities.Attributes;

namespace Generation
{
    [CreateAssetMenu(fileName = "GeneratorPools", menuName = "Generation/Generator Pools")]
    public class GeneratorPool : ScriptableObject
    {
        [SerializeReference, PolymorphicField] public List<GeneratorPass> pools = new();
    }
}
