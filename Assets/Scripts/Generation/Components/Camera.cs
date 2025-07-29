using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Generation.Components
{
    public struct Camera : IComponentData
    {
        public float3 Position;
    }
}
