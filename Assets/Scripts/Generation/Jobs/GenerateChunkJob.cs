using System.Threading.Tasks;
using Generation.Data;
using UnityEngine;

namespace Generation.Jobs
{
    public class GenerateChunkJob : JobBase<Chunk>
    {
        public ChunkJobType Type => ChunkJobType.Generate;
        public Vector2Int Position { get; }
        public float Priority { get; set; }
        public bool IsRunning { get; set; }

        public GenerateChunkJob(Vector2Int position)
        {
            Position = position;
        }

        public override async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                var chunk = new Chunk(Position);

                foreach (var pass in WorldGenerator.Passes)
                    pass.Apply(WorldGenerator.World, chunk);

                CompleteSource.TrySetResult(chunk);
            }, CancelSource.Token);
        }
    }
}
