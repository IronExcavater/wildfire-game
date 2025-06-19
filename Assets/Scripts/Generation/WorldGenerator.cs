using System.Collections.Generic;
using Generation.Data;
using Generation.Passes;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldGenerator : Singleton<WorldGenerator>
    {
        [SerializeField] private int _chunkSize = 32;
        public static int ChunkSize => Instance._chunkSize;

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
