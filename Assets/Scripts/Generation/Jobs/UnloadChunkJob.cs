using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Objects;
using UnityEngine;

namespace Generation.Jobs
{
    public class UnloadChunkJob : JobBase<bool>
    {
        public UnloadChunkJob(Vector2Int position) : base(ChunkJobType.UnloadChunk, position)
        {
        }

        public override float ComputePriority(Vector2Int cameraChunk) => Priority = -(Position - cameraChunk).magnitude;

        public override void Activate(Vector2Int cameraChunk)
        {
            base.Activate(cameraChunk);
            JobManager.CancelAllJobsOfTypeAtPosition<BuildTerrainJob>(Position);
            JobManager.CancelAllJobsOfTypeAtPosition<LoadChunkJob>(Position);
        }

        public override async Task Start()
        {
            List<(Type, DataObject<Entity>)> toRelease = new();

            await Task.Run(() =>
            {
                if (WorldLoader.TryGetInstancesAtPosition(Position, out var instances) && instances != null)
                {
                    foreach (var instance in instances)
                    {
                        toRelease.Add((instance.GetType(), instance));
                        instance.Data.Unbind();
                    }
                }
            });

            var grouped = toRelease.GroupBy(t => t.Item1);
            foreach (var group in grouped)
            {
                var pool = WorldLoader.GetPool(group.Key);
                foreach (var (_, instance) in group)
                {
                    instance.Data.Unbind();
                    pool.Release(instance);
                }
            }

            CompleteSource.SetResult(true);
        }
    }
}
