using System.Collections.Generic;
using Generation.Data;
using Generation.Passes;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldGenerator : Singleton<WorldGenerator>
    {
        [SerializeField, Range(1, 128)] private int _chunkSize = 32;
        public static int ChunkSize => Instance._chunkSize;

        [SerializeField, Range(1, 10)] private int _resolution = 2;
        public static int Resolution => Instance._resolution;

        [SerializeField, Range(1, 8)] private int _maxLodLevel = 4;
        public static int MaxLodLevel => Instance._maxLodLevel;

        private World _world = new();
        public static World World => Instance._world;

        private List<GeneratorPass> _passes = new();
        public static IReadOnlyList<GeneratorPass> Passes => Instance._passes.AsReadOnly();

        [SerializeField] private GeneratorPasses _generatorPasses;

        public void AddPass(GeneratorPass pass) => _passes.Add(pass);

        protected override void Awake()
        {
            base.Awake();
            if (_generatorPasses) _passes.AddRange(_generatorPasses.passes);
        }
    }
}
