using System.Collections.Generic;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Objects;
using UnityEngine;

namespace Generation.Jobs
{
    public class LoadChunkJob : JobBase<List<DataObject<Entity>>>
    {
        public LoadChunkJob(Vector2Int position) : base(ChunkJobType.LoadChunk, position)
        {
        }

        public override async Task ExecuteAsync()
        {
            var chunk = await WorldGenerator.GetChunk(Position);
            var instances = new List<DataObject<Entity>>();

            foreach (var entity in chunk.Entities)
            {
                CancelSource.Token.ThrowIfCancellationRequested();

                var type = entity.Value.Type.Value;
                var pool = WorldLoader.GetPool(type);
                var instance = (DataObject<Entity>)pool.Get();
                instance.gameObject.name = entity.Value.ToString();

                instances.Add(instance);
                instance.Data.Bind(entity);
            }

            CompleteSource.SetResult(instances);
        }
    }
}
