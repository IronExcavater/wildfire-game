using System.Collections.Generic;
using UnityEngine;
using Utilities.Attributes;

namespace Generation
{
    [CreateAssetMenu(fileName = "GeneratorPasses", menuName = "Generation/Generator Passes")]
    public class GeneratorPasses : ScriptableObject
    {
        [SerializeReference, PolymorphicField] public List<GeneratorPass> passes = new();
    }
}
