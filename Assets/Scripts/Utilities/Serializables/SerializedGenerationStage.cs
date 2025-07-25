using System;
using System.Collections.Generic;
using Generation.Passes;
using UnityEngine;
using Utilities.Attributes;

namespace Utilities.Serializables
{
    [Serializable]
    public class SerializedGenerationStageDictionary
        : SerializedDictionary<SerializedGenerationStage, GenerationStage, List<GeneratorPass>> { }

    [Serializable]
    public class SerializedGenerationStage : KeyValuePair<GenerationStage, List<GeneratorPass>>
    {
        [SerializeReference, PolymorphicField] public new List<GeneratorPass> Value;

        public override void Initialize()
        {
            base.Value = Value;
        }
    }
}
