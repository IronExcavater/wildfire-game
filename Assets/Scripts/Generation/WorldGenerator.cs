using System.Collections.Generic;
using Generation.Data;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldGenerator : Singleton<WorldGenerator>
    {
        [SerializeField] private Vector2Int _worldSize = new(10, 10);
        public static Vector2Int WorldSize => Instance._worldSize;

        [SerializeField] private int _chunkSize = 32;
        public static int ChunkSize => Instance._chunkSize;

        private World _world;
        public static World World => Instance._world;

        private List<GeneratorPass> _passes = new();
        public IReadOnlyList<GeneratorPass> Passes => _passes.AsReadOnly();

        [SerializeField] private GeneratorPasses _generatorPasses;

        public void AddPass(GeneratorPass pass) => _passes.Add(pass);

        protected override void Awake()
        {
            base.Awake();
            if (_generatorPasses) _passes.AddRange(_generatorPasses.passes);
            _world = new World(WorldSize, ChunkSize, _passes.ToArray());
        }
    }
}
