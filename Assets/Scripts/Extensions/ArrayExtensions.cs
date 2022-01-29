using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtensionMethods
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Returns a random element within the array, based on its size.
        /// </summary>
        public static T GetRandomElement<T>(this T[] array)
        {
            return array[array.GetRandomIndex()];
        }

        /// <summary>
        /// Returns a random int between 0 and the size of the array.
        /// </summary>
        public static int GetRandomIndex<T>(this T[] array)
        {
            return Random.Range(0, array.Length);
        }

        /// <summary>
        /// Displays every element in this array.
        /// </summary>
        public static void DebugLog<T>(this T[] array)
        {
            foreach (T elem in array)
            {
                Debug.Log(elem);
            }
        }

        /// <summary>
        /// Returns the last element in this array.
        /// </summary>
        public static T GetLastElement<T>(this T[] array)
        {
            return array[array.Length - 1];
        }

        /// <summary>
        /// Returns all elements in this array, concatenated.
        /// </summary>
        public static string ToStringAll<T>(this T[] array, string sep = "")
        {
            string result = string.Empty;

            for (int i = 0; i < array.Length - 1; i++)
            {
                result += array[i].ToString() + sep;
            }

            result += array[array.Length - 1];

            return result;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var rnd = new System.Random();
            return source.OrderBy(item => rnd.Next());
        }
        
        public static IEnumerable<T> PutToFront<T>(this IEnumerable<T> source, T element)
        {
            return source.OrderBy(item => !item.Equals(element));  // OrderBy is stable
        }
    }
}