using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
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

        public static string SeedString { get; set; }
        public static int SeedInt => HashSeed(SeedString);

        private World _world = new();
        public static World World => Instance._world;

        private List<GeneratorPass> _passes = new();
        public static IReadOnlyList<GeneratorPass> Passes => Instance._passes.AsReadOnly();

        [SerializeField] private GeneratorPasses _generatorPasses;

        public void AddPass(GeneratorPass pass) => _passes.Add(pass);

        protected override void Awake()
        {
            base.Awake();
            if (_generatorPasses != null) _passes.AddRange(_generatorPasses.passes);
            SeedString = _generatorPasses?.seed ?? "default";
        }

        public static async Task<Chunk> GetChunk(Vector2Int position, IJob parent = null)
        {
            if (!World.Chunks.TryGetValue(position, out var chunk))
            {
                var generateJob = new GenerateChunkJob(position) { Parent = parent };
                chunk = await JobManager.Enqueue(generateJob);
                World.Chunks.TryAdd(position, chunk);
            }

            return chunk;
        }

        public static int HashSeed(string seed)
        {
            unchecked
            {
                var hash = 23;
                foreach (var c in seed) hash = hash * 31 + c;
                return Math.Clamp(hash, 100_000, 2_000_000_000);
            }
        }

        public static void Regenerate()
        {
            Debug.LogWarning("Regenerating world");
            World.Dispose();
        }
    }
}
