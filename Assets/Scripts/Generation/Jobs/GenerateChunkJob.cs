using System.Threading.Tasks;
using Generation.Data;
using UnityEngine;

namespace Generation.Jobs
{
    public class GenerateChunkJob : JobBase<Chunk>
    {
        public GenerateChunkJob(Vector2Int position) : base(ChunkJobType.GenerateChunk, position)
        {
        }

        public override async Task Start()
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
