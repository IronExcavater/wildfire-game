using System.Threading.Tasks;
using UnityEngine;

namespace Generation.Jobs
{
    public class UnloadChunkJob : JobBase<bool>
    {
        public UnloadChunkJob(Vector2Int position) : base(ChunkJobType.UnloadChunk, position)
        {
        }

        public override async Task Start()
        {
            if (WorldLoader.TryGetInstancesAtPosition(Position, out var instances))
            {
                JobManager.CancelAllJobsOfTypeAtPosition<BuildTerrainJob>(Position);
                JobManager.CancelAllJobsOfTypeAtPosition<LoadChunkJob>(Position);

                if (instances != null)
                {
                    foreach (var instance in instances)
                    {
                        var type = instance.GetType();

                        instance.Data.Unbind();
                        WorldLoader.GetPool(type).Release(instance);
                    }
                }
            }

            CompleteSource.SetResult(true);
        }
    }
}
