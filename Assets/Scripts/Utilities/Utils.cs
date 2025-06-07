using UnityEngine;

namespace Utilities
{
    public static class Utils
    {
        public static void SetLayerRecursive(this GameObject obj, int layer)
        {
            foreach (var c in obj.GetComponentsInChildren<Transform>(true))
                c.gameObject.layer = layer;
        }


        public static Vector3 ClosestPointOnLine(this Vector3 point, Vector3 linePoint, Vector3 lineDirection,
            float length = float.MaxValue)
        {
            if (length.Equals(float.MaxValue)) length = lineDirection.magnitude;
            lineDirection.Normalize();
            return linePoint + lineDirection * point.ClosestDistanceOnLine(linePoint, lineDirection, length);
        }

        public static float ClosestDistanceOnLine(this Vector3 point, Vector3 linePoint, Vector3 lineDirection,
            float length = float.MaxValue)
        {
            if (length.Equals(float.MaxValue)) length = lineDirection.magnitude;
            lineDirection.Normalize();
            return Mathf.Clamp(Vector3.Dot(point - linePoint, lineDirection), 0, length);
        }
    }
}
