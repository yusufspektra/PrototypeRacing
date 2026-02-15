using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class Json
        {
            public static bool IsValidUnityJson<T>(string strInput, out T result) where T : class
            {
                try
                {
                    result = JsonUtility.FromJson(strInput, typeof(T)) as T;
                    return true;
                }
                catch (Exception)
                {
                    result = null;
                    return false;
                }
            }

            public static bool IsValidJson<T>(string strInput)
            {
                if (IsValidJson(strInput))
                {
                    try
                    {
                        var convertedObj = JsonConvert.DeserializeObject<T>(strInput);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                return false;
            }

            public static bool IsValidJson<T>(string strInput, out T result)
            {
                if (IsValidJson(strInput))
                {
                    try
                    {
                        var convertedObj = JsonConvert.DeserializeObject<T>(strInput);
                        result = convertedObj;
                        return true;
                    }
                    catch (Exception ex)
                    {
#if UNITY_ENGINE
                if (Application.isEditor)
                    Debug.LogError(ex);
#endif
                        result = default(T);
                        return false;
                    }
                }

                result = default(T);
                return false;
            }

            public static bool IsValidJson(string strInput, bool forceTryParse = true)
            {
                if (string.IsNullOrEmpty(strInput) || strInput.Length < 2)
                    return false;

                strInput = strInput.Trim();
                if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                    (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
                {
                    if (!forceTryParse)
                    {
                        return true;
                    }

                    try
                    {
                        var obj = JToken.Parse(strInput);
                        return true;
                    }
                    catch (JsonReaderException jex)
                    {
                        //Exception in parsing json
                        //Debug.Log(jex.Message);
                        return false;
                    }
                    catch (Exception ex) //some other exception
                    {
                        //Debug.Log(ex.ToString());
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }
}