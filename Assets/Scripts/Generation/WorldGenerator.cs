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

        public static async Task<Chunk> GetChunk(Vector2Int position)
        {
            if (!World.Chunks.TryGetValue(position, out var chunk))
            {
                chunk = await JobManager.Enqueue(new GenerateChunkJob(position));
                World.Chunks[position] = chunk;
            }

            return chunk;
        }

        /*public static Task<Chunk> GetChunk(Vector2Int position)
        {
            lock (World.Chunks) if (World.Chunks.TryGetValue(position, out var chunk)) return Task.FromResult(chunk);

            lock (Instance._generateTasks)
            lock (Instance._generateQueue)
            {
                if (Instance._generateTasks.TryGetValue(position, out var tcs)) return tcs.Task;

                tcs = new TaskCompletionSource<Chunk>();
                Instance._generateQueue.Enqueue(position);
                Instance._generateTasks.Add(position, tcs);
                return tcs.Task;
            }
        }

        private async void GenerateChunkAsync(Vector2Int position)
        {
            lock (World.Chunks) if (World.Chunks.TryGetValue(position, out _) || _generateTasks.ContainsKey(position)) return;

            await Task.Run(() =>
            {
                var chunk = new Chunk(position);

                foreach (var pass in Passes)
                    pass.Apply(World, chunk);

                lock (World.Chunks) World.Chunks[position] = chunk;

                if (_generateTasks.Remove(position, out var tcs)) tcs.SetResult(chunk);
                Debug.Log($"Generated chunk at {position}");
            });
        }*/
    }
}
