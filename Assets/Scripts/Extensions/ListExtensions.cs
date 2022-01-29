using UnityEngine;
using System.Collections.Generic;

namespace ExtensionMethods
{
    public static class ListExtensions
    {
        /// <summary>
        /// Returns a random element within the list, based on its size.
        /// </summary>
        public static T GetRandomElement<T>(this List<T> list)
        {
            return list[list.GetRandomIndex()];
        }

        /// <summary>
        /// Returns a random int between 0 and the size of the list.
        /// </summary>
        public static int GetRandomIndex<T>(this List<T> list)
        {
            return Random.Range(0, list.Count);
        }

        /// <summary>
        /// Returns the last element in this list.
        /// </summary>
        public static T GetLastElement<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }
    }
}