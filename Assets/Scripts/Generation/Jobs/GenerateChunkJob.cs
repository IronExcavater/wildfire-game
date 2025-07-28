using System.Threading.Tasks;
using Generation.Components;
using Generation.Data;
using Generation.Passes;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Chunk = Generation.Components.Chunk;

namespace Generation.Jobs
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GenerateChunkJob : ISystem
    {
        private EntityQuery _uninitializedChunks;

        public void OnCreate(ref SystemState state)
        {
            _uninitializedChunks = state.GetEntityQuery(
                ComponentType.ReadOnly<Chunk>(),
                ComponentType.Exclude<ChunkGeneratedTag>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_uninitializedChunks.IsEmptyIgnoreFilter) return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
        }

        public GenerationStage Stage { get; }

        public GenerateChunkJob(Vector2Int position, GenerationStage stage) : base(ChunkJobType.GenerateChunk, position)
        {
            Stage = stage;
        }

        public override async Task Start()
        {
            await Task.Run(async () =>
            {
                WorldGenerator.TryGetChunkReference(Position, out var chunk);

                chunk ??= new Data.Chunk(WorldGenerator.World, Position);
                WorldGenerator.World.Chunks.TryAdd(Position, chunk);

                var previousStage = Stage.GetPreviousStage();
                if (previousStage.HasValue && !chunk.IsStageComplete(previousStage.Value))
                {
                    var previousStageJob = new GenerateChunkJob(Position, previousStage.Value) { Parent = this };
                    await JobManager.Enqueue(previousStageJob);
                }

                var stages = WorldGenerator.Passes;
                stages.TryGetValue(Stage, out var passes);

                if (passes != null && !chunk.IsStageComplete(Stage))
                    foreach (var pass in WorldGenerator.Passes[Stage])
                        if (pass != null) await pass.Apply(chunk, this);

                chunk.MarkStageComplete(Stage);
                CompleteSource.TrySetResult(chunk);
            }, CancelSource.Token);
        }
    }
}
