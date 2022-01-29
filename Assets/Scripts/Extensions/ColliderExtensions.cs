using UnityEngine;

namespace ExtensionMethods
{
    public static class ColliderExtensions
    {
        public static Vector3 GetRandomPointIn(this Collider collider)
        {
            // Bounds coordinates are World Space and not local position!
            return new Vector3
            (
                Random.Range(collider.bounds.min.x, collider.bounds.max.x),
                collider.bounds.min.y, // Floor of collider
                Random.Range(collider.bounds.min.z, collider.bounds.max.z)
            );
        }
    }
}