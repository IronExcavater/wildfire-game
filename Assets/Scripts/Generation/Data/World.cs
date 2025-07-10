using System.Collections.Concurrent;
using System.Threading.Tasks;
using Generation.Jobs;
using Generation.Objects;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Generation.Data
{
    public sealed class World : IDisposable
    {
        public readonly ConcurrentDictionary<Vector2Int, Chunk> Chunks = new();

        // Not performant
        public async Task<float> GetAverageHeight(Vector2 worldPosition, int radius, IJob parent = null)
        {
            var resolution = WorldGenerator.Resolution;
            var average = 0d;
            var count = 0;

            for (var dy = -radius; dy <= radius; dy++)
            for (var dx = -radius; dx <= radius; dx++)
            {
                var worldX = worldPosition.x + dx / (float)resolution;
                var worldY = worldPosition.y + dy / (float)resolution;
                Utils.AddValueToAverage(ref average, ref count, await GetHeight(new Vector2(worldX, worldY), parent));
            }

            return (float)average;
        }

        public async Task<Vector3> GetNormal(Vector2 worldPosition, IJob parent = null)
        {
            var offset = 1f / WorldGenerator.Resolution;

            var left = await GetHeight(worldPosition + new Vector2(-offset, 0), parent);
            var right = await GetHeight(worldPosition + new Vector2(+offset, 0), parent);
            var down = await GetHeight(worldPosition + new Vector2(0, -offset), parent);
            var up = await GetHeight(worldPosition + new Vector2(0, +offset), parent);

            var dx = new Vector3(2f * offset, right - left, 0);
            var dz = new Vector3(0, up - down, 2f * offset);

            return Vector3.Normalize(Vector3.Cross(dz, dx));
        }

        public async Task<float> GetHeight(Vector2 worldPosition, IJob parent = null)
        {
            var resolution = WorldGenerator.Resolution;

            var sampleX = worldPosition.x * resolution;
            var sampleY = worldPosition.y * resolution;

            var baseX = Mathf.FloorToInt(sampleX);
            var baseY = Mathf.FloorToInt(sampleY);
            var fracX = sampleX - baseX;
            var fracY = sampleY - baseY;

            var bottomLeft = await GetHeight(new Vector2Int(baseX, baseY), parent);
            var bottomRight = await GetHeight(new Vector2Int(baseX + 1, baseY), parent);
            var topLeft = await GetHeight(new Vector2Int(baseX, baseY + 1), parent);
            var topRight = await GetHeight(new Vector2Int(baseX + 1, baseY + 1), parent);

            var bottomInterp = Mathf.Lerp(bottomLeft, bottomRight, fracX);
            var topInterp = Mathf.Lerp(topLeft, topRight, fracX);
            return Mathf.Lerp(bottomInterp, topInterp, fracY);
        }

        public async Task<float> GetHeight(Vector2Int worldPosition, IJob parent = null)
        {
            var mapSize = WorldGenerator.ChunkSize * WorldGenerator.Resolution;
            var chunkX = Mathf.FloorToInt(worldPosition.x / (float)mapSize);
            var chunkY = Mathf.FloorToInt(worldPosition.y / (float)mapSize);

            var localX = Utils.EuclideanMod(worldPosition.x, mapSize);
            var localY = Utils.EuclideanMod(worldPosition.y, mapSize);
            if (localX < 0)
            {
                chunkX--;
                localX += mapSize;
            }

            if (localY < 0)
            {
                chunkY--;
                localY += mapSize;
            }

            var chunk = await WorldGenerator.GetChunk(new Vector2Int(chunkX, chunkY), parent);
            if (!chunk.TryGetEntityOfType(typeof(TerrainObject), out var terrain)) return 0;
            if (!terrain.Value.TryGetProperty("Heightmap", out Property<float[,]> heightmap)) return 0;
            return heightmap.Value[localX, localY];
        }

        public void Dispose()
        {
            JobManager.CancelAllJobs();
            foreach (var chunk in Chunks.Values)
                chunk.Dispose();
            Chunks.Clear();
        }
    }
}
