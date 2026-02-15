using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class Math
        {
            public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
            {
                if (a == b)
                    return 0f;

                var av = value - a;
                var ab = b - a;
                return Mathf.Clamp01(Vector3.Dot(av, ab) / Vector3.Dot(ab, ab));
            }

            public static float Distance(Vector3 a, Vector3 b)
            {
                var vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
                return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
            }
        }
    }
}