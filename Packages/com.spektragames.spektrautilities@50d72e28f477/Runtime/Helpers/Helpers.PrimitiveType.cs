using System;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class PrimitiveType
        {
            public static bool CheckStringForAscii(string str, out string changedStr)
            {
                if (str == null)
                    str = "";

                var strTemp = str
                    .Replace(" ", "")
                    .Replace("  ", "")
                    .Replace("  ", "")
                    .Replace("__", "_")
                    .Replace(Environment.NewLine, "");

                strTemp = strTemp
                    .Replace("İ", "I")
                    .Replace("ı", "i");

                for (var i = strTemp.Length - 1; i >= 0; i--)
                {
                    var character = strTemp[i].ToString().ToLower();
                    if (!(
                            character == "0" ||
                            character == "1" ||
                            character == "2" ||
                            character == "3" ||
                            character == "4" ||
                            character == "5" ||
                            character == "6" ||
                            character == "7" ||
                            character == "8" ||
                            character == "9" ||
                            character == "_" ||
                            character == "x" ||
                            character == "w" ||
                            character == "q" ||
                            character == "a" ||
                            character == "b" ||
                            character == "c" ||
                            character == "d" ||
                            character == "e" ||
                            character == "f" ||
                            character == "g" ||
                            character == "h" ||
                            character == "i" ||
                            character == "j" ||
                            character == "k" ||
                            character == "l" ||
                            character == "m" ||
                            character == "n" ||
                            character == "o" ||
                            character == "p" ||
                            character == "r" ||
                            character == "s" ||
                            character == "t" ||
                            character == "u" ||
                            character == "v" ||
                            character == "y" ||
                            character == "z"
                        ))
                        strTemp = strTemp.Remove(i, 1);
                }

                changedStr = strTemp;
                return str == strTemp;
            }

            public static bool TryParseEnum<T>(string value, out T result) where T : struct
            {
                if (Enum.TryParse(value, out result)) return true;

                Debug.LogError("Cannot parse enum: " + value);

                return false;
            }

            public static T ParseEnum<T>(string value)
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }

            public static string GetRandomLetter()
            {
                var chars = "_abcdefghijklmnopqrstuvwxyz1234567890";
                var num = UnityEngine.Random.Range(0, chars.Length - 1);
                return chars[num].ToString();
            }

            public static string Base64Encode(string plainText)
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }

            public static string Base64Decode(string base64EncodedData)
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
        }
    }
}