using UnityEngine;

namespace ExtensionMethods
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Returns true if x,y,z of given Vector3 are between min and max.
        /// </summary>
        public static bool IsInRange(this Vector3 v, Vector3 min, Vector3 max)
        {
            return v.x.IsInRange(min.x, max.x) &&
                   v.y.IsInRange(min.y, max.y) &&
                   v.z.IsInRange(min.z, max.z);
        }

        public static string ToString(this Vector3 v, char sep)
        {
            return v.x.ToString() + sep + v.y.ToString() + sep + v.z.ToString();
        }
    }
}