using System.Threading.Tasks;
using UnityEngine;

namespace Generation.Jobs
{
    public class UnloadChunkJob : JobBase<bool>
    {
        public ChunkJobType Type => ChunkJobType.Load;
        public Vector2Int Position { get; }
        public float Priority { get; set; }
        public bool IsRunning { get; set; }

        public UnloadChunkJob(Vector2Int position)
        {
            Position = position;
        }

        public override async Task ExecuteAsync()
        {
            if (WorldLoader.TryGetInstancesAtPosition(Position, out var instances))
            {
                foreach (var instance in instances)
                {
                    var type = instance.GetType();

                    instance.Data.Unbind();
                    WorldLoader.GetPool(type).Release(instance);
                }
            }

            CompleteSource.SetResult(true);
        }
    }
}
