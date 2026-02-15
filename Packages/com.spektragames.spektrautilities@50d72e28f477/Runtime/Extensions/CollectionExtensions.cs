using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class CollectionExtensions
    {
        public static KeyValuePair<T, TW>? FirstElementOfDictionary<T, TW>(this Dictionary<T, TW> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
                return null;

            using (var enumerator = dictionary.GetEnumerator())
            {
                while (enumerator.MoveNext()) return enumerator.Current;
            }

            return null;
        }

        public static bool IsNullOrEmpty<T, W>(this Dictionary<T, W> dictionary)
        {
            return dictionary == null || dictionary.Count <= 0;
        }

        public static bool IsNullOrEmpty<T>(this ICollection list)
        {
            return list == null || list.Count <= 0;
        }

        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count <= 0;
        }

        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length <= 0;
        }

        public static List<T> ShiftElement<T>(this List<T> list, int oldIndex, int newIndex)
        {
            var listArray = list.ToArray();
            if (oldIndex == newIndex)
                return list;

            var tmp = list[oldIndex];

            if (newIndex < oldIndex)
                Array.Copy(list.ToArray(), newIndex, listArray, newIndex + 1, oldIndex - newIndex);
            else
                Array.Copy(list.ToArray(), oldIndex + 1, listArray, oldIndex, newIndex - oldIndex);

            listArray[newIndex] = tmp;

            return listArray.ToList();
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var rndm = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rndm.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool HaveIndex(this ICollection list, int index)
        {
            if (list == null || list.Count == 0)
                return false;
            if (index < 0)
                return false;

            return index < list.Count;
        }

        public static bool HaveIndex(this Array array, int index)
        {
            if (array == null || array.Length == 0)
                return false;
            if (index < 0)
                return false;

            return index < array.Length;
        }

        public static T[] SubArrayDeepClone<T>(this T[] data, int index, int? length = null)
        {
            var subLength = -1;
            if (length != null)
                subLength = length.Value;
            else
                subLength = data.Length - index;

            var arrCopy = new T[subLength];
            Array.Copy(data, index, arrCopy, 0, subLength);
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, arrCopy);
                ms.Position = 0;
                return (T[])bf.Deserialize(ms);
            }
        }

        public static bool CheckListsAreSame<T>(this List<T> list, List<T> compareList) where T : class
        {
            if (list == null && compareList == null)
                //UnityEngine.Debug.Log("1");
                return true;

            if (list == null && compareList != null)
                //UnityEngine.Debug.Log("2");
                return false;

            if (list != null && compareList == null)
                //UnityEngine.Debug.Log("3");
                return false;

            if (list != null && compareList != null)
            {
                //UnityEngine.Debug.Log("4");
                if (list.Count != compareList.Count)
                    //UnityEngine.Debug.Log("5");
                    return false;

                for (var i = 0; i < list.Count; i++)
                    if (list[i] != compareList[i])
                    {
#if UNITY_EDITOR
                        if (list[i] is string)
                        {
                            if (!string.Equals(list[i] as string, compareList[i] as string))
                                //Debug.LogError("\"" + list[i] as string + "\" - \"" + compareList[i] as string + "\"");
                                //UnityEngine.Debug.Log("6");
                                return false;
                            continue;
                        }
#endif
                        //UnityEngine.Debug.Log("7");
                        return false;
                    }

                //UnityEngine.Debug.Log("8");
                return true;
            }

            //UnityEngine.Debug.Log("9");
            return false;
        }
    }
}