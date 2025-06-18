using System;
using Generation.Data;

namespace Generation.Passes
{
    [Serializable]
    public class FoliagePass : GeneratorPass
    {
        public override void Apply(World world)
        {
            /*var chunkSize = WorldGenerator.ChunkSize;

            foreach (var chunk in world.Chunks.Values)
            {
                if (!chunk.TryGetEntityOfType(typeof(Terrain), out Property<Entity> terrain))
                {
                    terrain = new Property<Entity>(new Entity(typeof(Terrain)));
                    chunk.AddEntity(terrain);
                }

                if (!terrain.Value.TryGetProperty("Heightmap", out Property<float[,]> heightmap))
                {
                    heightmap = new Property<float[,]>(new float[chunkSize, chunkSize]);
                    terrain.Value.AddProperty("Heightmap", heightmap);
                }

                for (var y = 0; y < chunkSize; y++)
                for (var x = 0; x < chunkSize; x++)
                {
                    var nx = chunk.Position.x * chunkSize + x;
                    var ny = chunk.Position.y * chunkSize + y;
                    heightmap.Value[x, y] += Mathf.PerlinNoise(nx, ny);
                }
            }*/
        }
    }
}
