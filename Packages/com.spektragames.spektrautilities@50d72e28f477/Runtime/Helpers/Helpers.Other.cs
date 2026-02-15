using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class Other
        {
            public static void Quit()
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
            }

            public static string GetProjectName()
            {
                string[] s = Application.dataPath.Replace("\\", "/").Split('/');
                string projectName = s[s.Length - 2];
                return projectName;
            }

            public static string DeviceID()
            {
#if UNITY_EDITOR
                return SystemInfo.deviceUniqueIdentifier + GetProjectName();
#elif UNITY_IOS
        return SystemInfo.deviceUniqueIdentifier;
#elif UNITY_ANDROID
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
        string androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
        up.Dispose();
        up = null;
        currentActivity.Dispose();
        currentActivity = null;
        contentResolver.Dispose();
        contentResolver = null;
        secure.Dispose();
        secure = null;
        return androidId;
#else
        return SystemInfo.deviceUniqueIdentifier;
#endif
            }

            public static float InverseLerp(Color a, Color b, Color value)
            {
                if (a == b)
                    return 0f;

                Vector4 vA = new Vector4(a.r, a.g, a.b, a.a);
                Vector4 vB = new Vector4(b.r, b.g, b.b, b.a);
                Vector4 vValue = new Vector4(value.r, value.g, value.b, value.a);

                Vector4 AV = vValue - vA;
                Vector4 AB = vB - vA;
                return Mathf.Clamp01(Vector4.Dot(AV, AB) / Vector4.Dot(AB, AB));
            }

            public static UnityEngine.Matrix4x4 MatrixLerp(UnityEngine.Matrix4x4 from, UnityEngine.Matrix4x4 to,
                float time)
            {
                var ret = new UnityEngine.Matrix4x4();
                for (var i = 0; i < 16; i++)
                    ret[i] = Mathf.Lerp(from[i], to[i], time);
                return ret;
            }
            
            public static int VersionToNumber(string version)
            {
                const int baseMultiplier = 1000;
                var parts = version.Split('.');
                var number = 0;

                foreach (var part in parts)
                {
                    if (!int.TryParse(part, out var partInt))
                    {
                        Debug.LogError($"Error converting part to integer: {part}");
                        return -1;
                    }

                    number = number * baseMultiplier + partInt;
                }

                return number;
            }
        }
    }
}