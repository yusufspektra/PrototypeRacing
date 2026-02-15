using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class JsonExtensions
    {
        public static T DeepCopyObjectWithUnityJson<T>(this T obj)
        {
            string json = JsonUtility.ToJson(obj);
            return JsonUtility.FromJson<T>(json);
        }

        public static T[] DeepCopyObjectWithUnityJson<T>(this T[] obj)
        {
            var wrapper = new JsonArrayWrapper<T>(obj);
            string serialized = JsonUtility.ToJson(wrapper);
            var cloned = JsonUtility.FromJson<JsonArrayWrapper<T>>(serialized).array;
            return cloned;
        }

        public static List<T> DeepCopyObjectWithUnityJson<T>(this List<T> obj)
        {
            var wrapper = new JsonListWrapper<T>(obj);
            string serialized = JsonUtility.ToJson(wrapper);
            var cloned = JsonUtility.FromJson<JsonListWrapper<T>>(serialized).list;
            return cloned;
        }

        public static T DeepCopyObjectWithJson<T>(this T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string SerializeObject(this object obj, bool indentedFormatting = false)
        {
            if (indentedFormatting)
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            else
                return JsonConvert.SerializeObject(obj);
        }

        public static string SerializeObjectWithUnityJson(this object obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public static string SerializeObjectWithUnityJson<T>(this T[] obj)
        {
            var wrapper = new JsonArrayWrapper<T>(obj);
            string serialized = JsonUtility.ToJson(wrapper);

            int firstBracket = serialized.IndexOf("[", StringComparison.Ordinal);
            int lastBracket = serialized.LastIndexOf("]", StringComparison.Ordinal);

            if (firstBracket >= 0 && lastBracket > firstBracket)
            {
                return serialized.Substring(firstBracket, lastBracket - firstBracket + 1);
            }

            Debug.LogError("Could not extract JSON array/list");
            return serialized;
        }

        public static string SerializeObjectWithUnityJson<T>(this List<T> obj)
        {
            var wrapper = new JsonListWrapper<T>(obj);
            string serialized = JsonUtility.ToJson(wrapper);

            int firstBracket = serialized.IndexOf("[", StringComparison.Ordinal);
            int lastBracket = serialized.LastIndexOf("]", StringComparison.Ordinal);

            if (firstBracket >= 0 && lastBracket > firstBracket)
            {
                return serialized.Substring(firstBracket, lastBracket - firstBracket + 1);
            }

            Debug.LogError("Could not extract JSON array/list");
            return serialized;
        }

        public static T DeserializeWithUnityJson<T>(this string json)
        {
            Type type = typeof(T);

            if (type.IsArray)
            {
                string trim = json.Trim();
                if (trim.StartsWith("[") && trim.EndsWith("]"))
                {
                    // {"array":["a","b"]}
                    Type wrapperType = typeof(JsonArrayWrapper<>).MakeGenericType(type.GetElementType());
                    string newJson = $"{{\"array\":{trim}}}";
                    object wrapper = JsonUtility.FromJson(newJson, wrapperType);
                    var arrayField = wrapperType.GetField("array");
                    object resultArray = arrayField.GetValue(wrapper);
                    return (T)resultArray;
                }
                else
                {
                    return JsonUtility.FromJson<T>(json);
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                string trim = json.Trim();
                if (trim.StartsWith("[") && trim.EndsWith("]"))
                {
                    // {"list":["a","b"]}
                    Type wrapperType = typeof(JsonListWrapper<>).MakeGenericType(type.GetGenericArguments()[0]);
                    string newJson = $"{{\"list\":{trim}}}";
                    object wrapper = JsonUtility.FromJson(newJson, wrapperType);
                    var listField = wrapperType.GetField("list");
                    object resultArray = listField.GetValue(wrapper);
                    return (T)resultArray;
                }
                else
                {
                    return JsonUtility.FromJson<T>(json);
                }
            }
            else
            {
                return JsonUtility.FromJson<T>(json);
            }

            return default;
        }
    }
}