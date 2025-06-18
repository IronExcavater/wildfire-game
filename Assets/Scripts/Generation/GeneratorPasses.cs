using System.Collections.Generic;
using Generation.Passes;
using UnityEngine;
using Utilities.Attributes;

namespace Generation
{
    [CreateAssetMenu(fileName = "Passes", menuName = "Generation/Generator Passes")]
    public class GeneratorPasses : ScriptableObject
    {
        [SerializeReference, PolymorphicField] public List<GeneratorPass> passes = new();
    }
}
