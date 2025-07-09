#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using Generation.Passes;
using UnityEngine;
using Utilities.Attributes;

namespace Generation
{
    [CreateAssetMenu(fileName = "GeneratorPasses", menuName = "Generation/Generator Passes")]
    public class GeneratorPasses : ScriptableObject
    {
        [SerializeReference, PolymorphicField] public List<GeneratorPass> passes = new();

        #if UNITY_EDITOR
        private const float DebounceTime = 0.5f;
        private static float _lastChangedTime;
        private static bool _waiting;

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            _lastChangedTime = (float)EditorApplication.timeSinceStartup;
            if (_waiting) return;
            _waiting = true;
            EditorApplication.update += DebouncedClear;
        }

        private void DebouncedClear()
        {
            var now = (float)EditorApplication.timeSinceStartup;
            if (now - _lastChangedTime < DebounceTime) return;

            _waiting = false;
            EditorApplication.update -= DebouncedClear;
            WorldGenerator.Regenerate();
        }
        #endif
    }
}
