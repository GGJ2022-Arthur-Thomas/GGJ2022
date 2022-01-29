using UnityEngine;
using System.Collections.Generic;

namespace ExtensionMethods
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns a random element within the dictionary, based on its size.
        /// </summary>
        public static KeyValuePair<T1, T2> GetRandomElement<T1, T2>(this Dictionary<T1, T2> dictionary)
        {
            int randomIndex = dictionary.GetRandomIndex();
            int i = 0;
            foreach (KeyValuePair<T1, T2> kvp in dictionary)
            {
                if (i == randomIndex)
                {
                    return kvp;
                }
                i++;
            }
            return new KeyValuePair<T1, T2>(default(T1), default(T2));
        }

        /// <summary>
        /// Returns a random int between 0 and the size of the dictionary.
        /// </summary>
        public static int GetRandomIndex<T1, T2>(this Dictionary<T1, T2> dictionary)
        {
            return Random.Range(0, dictionary.Count);
        }
    }
}