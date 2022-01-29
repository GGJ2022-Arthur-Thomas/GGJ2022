using UnityEngine;

namespace ExtensionMethods
{
    public static class ColorExtensions
    {
        public static string ToHexString(this Color c)
        {
            string r = Mathf.CeilToInt(c.r * 255).ToString("X2");
            string g = Mathf.CeilToInt(c.g * 255).ToString("X2");
            string b = Mathf.CeilToInt(c.b * 255).ToString("X2");
            return string.Format("#{0}{1}{2}", r, g, b);
        }

        public static Color WithAlpha(this Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }
    }
}