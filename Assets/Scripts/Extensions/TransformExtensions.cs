using UnityEngine;

namespace ExtensionMethods
{
    public static class TransformExtensions
    {
        public static void ClearChildren(this Transform t)
        {
            foreach (Transform child in t)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static void SetXPosition(this Transform t, float newX, bool local = true)
        {
            if (local) t.localPosition = new Vector3(newX, t.localPosition.y, t.localPosition.z);
            else t.position = new Vector3(newX, t.position.y, t.position.z);
        }

        public static void SetYPosition(this Transform t, float newY, bool local = true)
        {
            if (local) t.localPosition = new Vector3(t.localPosition.x, newY, t.localPosition.z);
            else t.position = new Vector3(t.position.x, newY, t.position.z);
        }

        public static void SetZPosition(this Transform t, float newZ, bool local = true)
        {
            if (local) t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, newZ);
            else t.position = new Vector3(t.position.x, t.position.y, newZ);
        }

        public static void SetXEulerAngles(this Transform t, float newX, bool local = true)
        {
            if (local) t.localEulerAngles = new Vector3(newX, t.localEulerAngles.y, t.localEulerAngles.z);
            else t.eulerAngles = new Vector3(newX, t.eulerAngles.y, t.eulerAngles.z);
        }

        public static void SetYEulerAngles(this Transform t, float newY, bool local = true)
        {
            if (local) t.localEulerAngles = new Vector3(t.localEulerAngles.x, newY, t.localEulerAngles.z);
            else t.eulerAngles = new Vector3(t.eulerAngles.x, newY, t.eulerAngles.z);
        }

        public static void SetZEulerAngles(this Transform t, float newZ, bool local = true)
        {
            if (local) t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, newZ);
            else t.eulerAngles = new Vector3(t.eulerAngles.x, t.eulerAngles.y, newZ);
        }

        public static void SetXScale(this Transform t, float newX)
        {
            t.localScale = new Vector3(newX, t.localScale.y, t.localScale.z);
        }

        public static void SetYScale(this Transform t, float newY)
        {
            t.localScale = new Vector3(t.localScale.x, newY, t.localScale.z);
        }

        public static void SetZScale(this Transform t, float newZ)
        {
            t.localScale = new Vector3(t.localScale.x, t.localScale.y, newZ);
        }
    }
}