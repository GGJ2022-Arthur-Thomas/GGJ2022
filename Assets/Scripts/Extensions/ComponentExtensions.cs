using UnityEngine;

namespace ExtensionMethods
{
    public static class ComponentExtensions
    {
        public static bool HasComponent<T>(this Component component) where T : Component
        {
            return component.GetComponent<T>() != null;
        }

        public static T GetParent<T>(this Component component) where T : Component
        {
            Transform parent = component.transform.parent;

            while (parent != null && !parent.HasComponent<T>())
            {
                parent = parent.parent;
            }

            if (parent == null)
                return null;
            else
                return parent.GetComponent<T>();
        }
    }
}