using System;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Passes;
using UnityEngine;

namespace Generation.Jobs
{
    public class GenerateChunkJob : JobBase<Chunk>
    {
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

                chunk ??= new Chunk(WorldGenerator.World, Position);
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

        public override int CompareTo(IJob other)
        {
            var cmp = base.CompareTo(other);
            return other is GenerateChunkJob btj && cmp == 0
                ? Stage.CompareTo(btj.Stage)
                : cmp;
        }

        public override bool Equals(object obj) =>
            obj is GenerateChunkJob other &&
            base.Equals(other) &&
            Stage.Equals(other.Stage);
        public override int GetHashCode() => HashCode.Combine(Type, Position, Stage);

        public override string ToString() => $"{Type} {{ Stage: {Stage} }} job at {Position}";
    }
}
