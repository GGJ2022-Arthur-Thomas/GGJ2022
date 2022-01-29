using UnityEngine;

namespace ExtensionMethods
{
    public static class RectTransformExtensions
    {
        public static void SetXPos(this RectTransform rt, float newX)
        {
            rt.anchoredPosition = new Vector2(newX, rt.anchoredPosition.y);
        }

        public static void SetYPos(this RectTransform rt, float newY)
        {
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, newY);
        }

        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void SetXSize(this RectTransform rt, float sizeX)
        {
            rt.sizeDelta = new Vector2(sizeX, rt.sizeDelta.y);
        }

        public static void SetYSize(this RectTransform rt, float sizeY)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, sizeY);
        }

        public static void SetProperties(this RectTransform rt, RectTransform src)
        {
            rt.anchorMin = src.anchorMin;
            rt.anchorMax = src.anchorMax;
            rt.pivot = src.pivot;
            rt.anchoredPosition = src.anchoredPosition;
            rt.sizeDelta = src.sizeDelta;
        }
    }
}