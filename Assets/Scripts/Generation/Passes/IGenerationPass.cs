using System;
using UnityEngine;
using Utilities;

namespace Generation.Passes
{
    public interface IGenerationPass
    {
        int SeedOffset(string seedString, Type systemType)
        {
            var raw = WorldGenerator.HashSeed(seedString + systemType.FullName);
            return raw % 100_000_000;
        }

        Vector2 GetNoiseJitter(float x, float y, float amount)
        {
            var jitterAmount = Utils.StaticNoise(x, y) * amount;
            var jitterAngle = Utils.StaticNoise(x, y) * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(jitterAngle), Mathf.Sin(jitterAngle)) * jitterAmount;
        }
    }
}
