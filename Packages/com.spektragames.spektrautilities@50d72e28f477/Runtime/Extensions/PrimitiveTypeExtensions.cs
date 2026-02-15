using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class PrimitiveTypeExtensions
    {
        public static byte ConvertBoolArrayToByte(this bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            var index = 8 - source.Length;

            // Loop through the array
            foreach (var b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }

        private static readonly bool[] BoolArrayBuffer0 = new bool[8];
        private static readonly bool[] BoolArrayBuffer1 = new bool[8];

        public static bool[] ConvertByteToBoolArray(this byte b)
        {
            // prepare the return result
            for (var i = 0; i < BoolArrayBuffer0.Length; i++)
                BoolArrayBuffer0[i] = false;

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (var i = 0; i < 8; i++)
                BoolArrayBuffer0[i] = (b & (1 << i)) != 0;

            for (var i = 0; i < BoolArrayBuffer0.Length; i++) BoolArrayBuffer1[i] = BoolArrayBuffer0[8 - i - 1];

            return BoolArrayBuffer1;
        }

        private const char DotChar = '.';
        private const char CommaChar = ',';
        private static CultureInfo commaCulture = null;
        private static CultureInfo pointCulture = null;

        public static float ToFloat(this string str)
        {
            str = str.Trim();

            if (str == "0") return 0;

            var containsComma = str.Contains(CommaChar.ToString());
            var containsPoint = str.Contains(DotChar.ToString());

            if (containsComma && str.Split(',').Length == 2)
                return (float)Convert.ToDouble(str, commaCulture);
            if (containsPoint && str.Split(DotChar).Length == 2)
                return (float)Convert.ToDouble(str, pointCulture);
            if (!containsComma && !containsPoint && int.TryParse(str, out var parsedInt)) return parsedInt;

            throw new Exception("Invalid input : " + str);
        }

        public static double ToDouble(this string str)
        {
            str = str.Trim();

            if (str == "0") return 0;

            var containsComma = str.Contains(CommaChar.ToString());
            var containsPoint = str.Contains(DotChar.ToString());

            if (containsComma && str.Split(',').Length == 2)
                return Convert.ToDouble(str, commaCulture);
            if (containsPoint && str.Split(DotChar).Length == 2)
                return Convert.ToDouble(str, pointCulture);
            if (!containsComma && !containsPoint && int.TryParse(str, out var parsedInt)) return parsedInt;

            throw new Exception("Invalid input : " + str);
        }

        public static T GetNextForNonSequenceEnum<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            var Arr = (T[])Enum.GetValues(src.GetType());
            var j = Array.IndexOf(Arr, src) + 1;
            return Arr.Length == j ? Arr[0] : Arr[j];
        }

        public static T GetPreviousForNonSequenceEnum<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            var Arr = (T[])Enum.GetValues(src.GetType());
            var j = Array.IndexOf(Arr, src) - 1;
            return j == -1 ? Arr[Arr.Length - 1] : Arr[j];
        }

        public static T GetNextEnum<T>(this T v) where T : struct
        {
            return
                Enum.GetValues(v.GetType())
                    .Cast<T>()
                    .Concat(new[] { default(T) })
                    .SkipWhile(e => !v.Equals(e))
                    .Skip(1)
                    .First();
        }

        public static T GetPreviousEnum<T>(this T v) where T : struct
        {
            return Enum.GetValues(v.GetType())
                .Cast<T>()
                .Concat(new[] { default(T) })
                .Reverse()
                .SkipWhile(e => !v.Equals(e))
                .Skip(1)
                .First();
        }

        public static List<T> GetEnabledFlags<T>(this T enumVariable) where T : Enum
        {
            var enabledFlags = new List<T>();

            // Iterate through all possible values
            foreach (T flag in Enum.GetValues(typeof(T)))
                // Check if the flag is set in the provided enum variable
                if (enumVariable.HasFlag(flag))
                    enabledFlags.Add(flag);

            return enabledFlags;
        }

        public static string FloatToDecimalString(this float value)
        {
            var str = value.ToString();
            str = str.Replace(",", ".");
            if (!str.Contains("."))
                str += ".";
            var indexOfDot = str.IndexOf(".");
            for (var i = indexOfDot + 1; i < indexOfDot + 3; i++)
                try
                {
                    var tmp = str[i];
                }
                catch (Exception)
                {
                    str += "0";
                }

            str = str.Substring(0, indexOfDot + 3);

            return str;
        }

        public static string GetRandomLetter(this System.Random rndm)
        {
            var chars = "_abcdefghijklmnopqrstuvwxyz1234567890";
            var num = rndm.Next(0, chars.Length - 1);
            return chars[num].ToString();
        }

        public static bool ParseBool(this string str)
        {
            if (str == null)
                return false;

            if (str.ToLower().Contains("true"))
                return true;
            if (str.ToLower().Contains("false"))
                return false;
            throw new Exception("Can't parsing bool: " + str);
        }

        public static int ToInt(this string str, bool throwOnFail = false)
        {
            if (int.TryParse(str, out var i)) 
                return i;
            
            if (throwOnFail)
            {
                throw new Exception("Can't parsing int: " + str);
            }

            return i;
        }

        public static long ToLong(this string str)
        {
            long i = 0;
            if (!long.TryParse(str, out i)) Debug.LogError("Can't parsing long: " + str);

            return i;
        }

        public static bool IsInt(this string str)
        {
            var i = 0;
            return int.TryParse(str, out i);
        }

        public static bool ToBool(this string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            if (str.ToLower().StartsWith("tru"))
                return true;
            return false;
        }

        public static float NextFloat(this System.Random random, float min, float max)
        {
            return ((float)(random.NextDouble())) * (max - min) + min;
        }
    }
}