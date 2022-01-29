using UnityEngine;

namespace ExtensionMethods
{
    public static class StringExtensions
    {
        /// <summary>
        /// Adds str to the front and tail of the given string.
        /// </summary>
        public static string SurroundWith(this string str, string value)
        {
            return value + str + value;
        }

        /// <summary>
        /// Returns new string without its last char. If string is empty, returns empty.
        /// </summary>
        public static string RemoveLastChar(this string str)
        {
            if (str.Length > 0) return str.Substring(0, str.Length - 1);
            return string.Empty;
        }

        public static byte[] ToByteArray(this string str)
        {
            char[] values = str.ToCharArray();
            byte[] bytes = new byte[values.Length];
            for (int i = 0; i < values.Length; i++) bytes[i] = System.Convert.ToByte(values[i]);
            return bytes;
        }

        /// <summary>
        /// Converts the specified string from hex format ("FFFFFF") to <see cref="Color"/>. 
        /// </summary>
        public static Color ToColor(this string str)
        {
            int r = int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(str.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        /// <summary>
        /// Returns the specified string in the specified <see cref="StringColor"/> using tags.
        /// </summary>
        public static string Colored(this string str, StringColor c)
        {
            return Colored(str, c.ToString().ToLower());
        }

        /// <summary>
        /// Returns the specified string in the specified <see cref="Color"/> using tags.
        /// </summary>
        public static string Colored(this string str, Color c)
        {
            return Colored(str, c.ToHexString());
        }

        /// <summary>
        /// Returns the specified string in the specified color (hex format) using tags.
        /// </summary>
        public static string Colored(this string str, string colorHex)
        {
            return string.Format("<color={0}>{1}</color>", colorHex, str);
        }

        /// <summary>
        /// Returns the specified string in the specified size using tags.
        /// </summary>
        public static string Sized(this string str, int size)
        {
            return string.Format("<size={0}>{1}</size>", size, str);
        }

        /// <summary>
        /// Returns the specified string in bold using tags.
        /// </summary>
        public static string Bold(this string str)
        {
            return string.Format("<b>{0}</b>", str);
        }

        /// <summary>
        /// Returns the specified string in italics using tags.
        /// </summary>
        public static string Italics(this string str)
        {
            return string.Format("<i>{0}</i>", str);
        }
    }

    public enum StringColor
    {
        Aqua,
        Black,
        Blue,
        Brown,
        Cyan,
        Darkblue,
        Fuchsia,
        Ggreen,
        Grey,
        Lightblue,
        Lime,
        Magenta,
        Maroon,
        Navy,
        Olive,
        Purple,
        Red,
        Silver,
        Teal,
        White,
        Yellow
    }
}