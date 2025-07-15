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
        [SerializeField] public string seed;
        [SerializeReference, PolymorphicField] public List<GeneratorPass> passes = new();

        #if UNITY_EDITOR
        private const float DebounceTime = 0.5f;
        private static float _lastChangedTime;
        private static bool _waiting;

        private static readonly (int Min, int Max) SeedLengthClamp = (10, 15);
        private string _lastSeed;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(seed))
            {
                RandomizeSeed();
                Debug.LogWarning("Seed was empty. Generated random seed.");
            }
            else if (!IsSeedValid(seed))
            {
                seed = _lastSeed;
                if (!IsSeedValid(seed)) _lastSeed = RandomizeSeed();
                Debug.LogWarning($"Seed must be between {SeedLengthClamp.Min} and {SeedLengthClamp.Max} characters.");
            }
            else
                _lastSeed = seed;

            if (Application.isPlaying)
            {
                _lastChangedTime = (float)EditorApplication.timeSinceStartup;
                if (_waiting) return;
                _waiting = true;
                EditorApplication.update += DebouncedClear;
            }
        }

        private bool IsSeedValid(string seed)
        {
            return seed.Length > SeedLengthClamp.Min && seed.Length < SeedLengthClamp.Max;
        }

        public string RandomizeSeed()
        {
            seed = System.Guid.NewGuid().ToString("N")
                .Substring(0, Random.Range(SeedLengthClamp.Min, SeedLengthClamp.Max));
            if (Application.isPlaying) WorldGenerator.SeedString = seed;
            return seed;
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
