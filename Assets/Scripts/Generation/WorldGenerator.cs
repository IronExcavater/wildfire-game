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

        private Dictionary<GenerationStage, List<GeneratorPass>> _passes = new();
        public static IReadOnlyDictionary<GenerationStage, List<GeneratorPass>> Passes => Instance._passes;

        [SerializeField] private GeneratorPasses _generatorPasses;

        public void AddPass(GenerationStage stage, GeneratorPass pass) => _passes[stage].Add(pass);

        protected override void Awake()
        {
            base.Awake();
            if (_generatorPasses != null) _passes = _generatorPasses.passes.Dictionary;
            Debug.Log(_passes[GenerationStage.Terrain]);
            SeedString = _generatorPasses?.seed ?? "default";
        }

        public static Vector2Int WorldToChunk(Vector2 world) =>
            new(Mathf.FloorToInt(world.x / ChunkSize), Mathf.FloorToInt(world.y / ChunkSize));
        public static Vector2 ChunkToWorld(Vector2Int chunk) =>
            new(chunk.x * ChunkSize, chunk.y * ChunkSize);

        public static async Task<Chunk> GetChunk(Vector2Int position, IJob parent = null,
            GenerationStage stage = GenerationStage.Objects)
        {
            TryGetChunkReference(position, out var chunk);

            if (chunk == null || !chunk.IsStageComplete(stage))
            {
                var generateJob = new GenerateChunkJob(position, stage) { Parent = parent };
                chunk = await JobManager.Enqueue(generateJob);
            }

            return chunk;
        }

        public static bool TryGetChunkReference(Vector2Int position, out Chunk chunk)
        {
            return World.Chunks.TryGetValue(position, out chunk);
        }

        [SerializeField] private MinMaxInt seedIntClamp = new(100_000, 100_000_000);

        public static bool IsSeedValid(string seed)
        {
            var hash = HashSeed(seed, false);
            return hash == Instance.seedIntClamp.Clamp(hash);
        }

        public static string RandomizeSeed()
        {
            return System.Guid.NewGuid().ToString("N")
                .Substring(0, Random.Range(SeedLengthClamp.min, SeedLengthClamp.max));
        }

        public static int HashSeed(string seed, bool clamp = true)
        {
            unchecked
            {
                var hash = 23;
                foreach (var c in seed) hash = hash * 31 + c;

                if (!clamp) return hash;
                return Mathf.Max(hash, Instance.seedIntClamp.min) % Instance.seedIntClamp.max;
            }
        }

        public static void Regenerate()
        {
            Debug.LogWarning("Regenerating world");
            World.Dispose();
        }
    }
}
