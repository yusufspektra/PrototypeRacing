using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class MathExtensions
    {
        public static Quaternion ToQuaternion(this Vector3 v)
        {
            return Quaternion.Euler(v);
        }
        
        public static bool Approximately(this Vector3 me, Vector3 other, float allowedDifference)
        {
            var equal = true;

            if (Mathf.Abs(me.x - other.x) > allowedDifference) equal = false;
            if (Mathf.Abs(me.y - other.y) > allowedDifference) equal = false;
            if (Mathf.Abs(me.z - other.z) > allowedDifference) equal = false;

            return equal;
        }

        public static bool Approximately(this Quaternion quatA, Quaternion quatB, float acceptableAngle)
        {
            var angle = Quaternion.Angle(quatA, quatB);
            return angle <= acceptableAngle;
        }
    }
}