using System;
using Unity.Mathematics;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public struct MinMax
    {
        public float min;
        public float max;

        public MinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Clamp(float v) => Mathf.Clamp(v, min, max);
        public int Clamp(int v) => Mathf.FloorToInt(Mathf.Clamp(v, min, max));

        public float Lerp(float t) => Mathf.Lerp(min, max, t);
        public float range => max - min;

        public static implicit operator float2(MinMax m) => new(m.min, m.max);
        public static implicit operator MinMax(float2 f) => new(f.x, f.y);
    }

    [Serializable]
    public struct MinMaxInt
    {
        public int min;
        public int max;

        public MinMaxInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public float Clamp(float v) => Mathf.Clamp(v, min, max);
        public int Clamp(int v) => Mathf.Clamp(v, min, max);

        public float Lerp(float t) => Mathf.Lerp(min, max, t);
        public int range => max - min;

        public static implicit operator int2(MinMaxInt m) => new(m.min, m.max);
        public static implicit operator MinMaxInt(int2 f) => new(f.x, f.y);
    }
}
