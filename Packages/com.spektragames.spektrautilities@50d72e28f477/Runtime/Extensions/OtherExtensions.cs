using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class OtherExtensions
    {
        public static void LogException(this Exception exception)
        {
            Debug.LogError(ParseException(exception));
        }

        public static string ParseException(this Exception exception)
        {
            if (Application.isEditor)
            {
                try
                {
                    StringBuilder result = new StringBuilder();
                    result.Append(Environment.NewLine);

                    string exceptionDetail = exception.ToString();
                    string prefix = exceptionDetail.Substring(0, exceptionDetail.IndexOf(exception.Message));

                    result.Append(prefix + exception.Message);
                    result.Append(Environment.NewLine);

                    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(exception, true);
                    result.Append(StacktraceToLog(stackTrace));

                    return result.ToString();
                }
                catch (Exception)
                {
                    return exception.ToString();
                }
            }
            else
            {
                return exception.ToString();
            }
        }

        public static string StacktraceToLog(this StackTrace stackTrace)
        {
            StringBuilder result = new StringBuilder();

            result.Append("### PARSE EXCEPTION BEGIN ###");
            result.Append(Environment.NewLine);

            var frames = stackTrace.GetFrames();
            for (int i = 0; i < frames.Length; i++)
            {
                try
                {
                    string fileName = null;
                    bool cantGetFileName = false;
                    try
                    {
                        fileName = Helpers.File.NormalPathToRelativePath(frames[i].GetFileName());
                    }
                    catch (Exception e)
                    {
                        cantGetFileName = true;
                        string methodName = "no_method";
                        if (frames[i].GetMethod() != null)
                        {
                            var method = frames[i].GetMethod();
                            methodName = method.DeclaringType.Name + "." + method.Name;
                        }

                        fileName = "<filename unknown>:" + frames[i].GetFileLineNumber().ToString() + ":" +
                                   frames[i].GetFileColumnNumber().ToString()
                                   + " - " + methodName;
                    }

                    int lineNumber = frames[i].GetFileLineNumber();

                    string trace = null;
                    if (cantGetFileName)
                    {
                        trace = string.Format("<a href=\"{0}\">{0}</a>", fileName);
                    }
                    else
                    {
                        trace = string.Format("<a href=\"{0}\" line=\"{1}\">{0}:{1}</a>", fileName, lineNumber);
                    }

                    result.Append("\tat ");
                    result.Append(trace);

                    if (i != frames.Length - 1)
                        result.Append(Environment.NewLine);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    result.Append("Can't parse stacktrace");
                }
            }

            result.Append(Environment.NewLine);
            result.Append("### PARSE EXCEPTION END ###");
            result.Append(Environment.NewLine);

            return result.ToString();
        }
        
        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        public static Color ToColorFromHex(this string hex)
        {
            string hexToUse = hex;

            if (!hexToUse.StartsWith("#"))
                hexToUse = "#" + hex;

            if (ColorUtility.TryParseHtmlString(hexToUse, out Color color))
            {
                return color;
            }
            else
            {
                Debug.LogError(hex);
                return Color.white;
            }
        }

        public static string ToHexFromColor(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
        

        public static void CopyToClipboard(this string s)
        {
            TextEditor te = new TextEditor();
            te.text = s;
            te.SelectAll();
            te.Copy();
        }

        public static void ForceRemoveAllListeners(this Action action)
        {
            if (action != null)
            {
                Delegate[] listeners = action.GetInvocationList();
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (listeners[i] != null)
                        action -= listeners[i] as Action;
                }
            }
        }
        
        public static void ForceRemoveAllListeners<T>(this Action<T> action)
        {
            if (action != null)
            {
                Delegate[] listeners = action.GetInvocationList();
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (listeners[i] != null)
                        action -= listeners[i] as Action<T>;
                }
            }
        }

        #region Invoke Safe
        
        public static void InvokeSafe(this UnityEvent action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured in unity event {action}.\n{e}");
            }
        }
        
        public static void InvokeSafe(this Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured in action {action}.\n{e}");
            }
        }
        
        public static void InvokeSafe<T>(this Action<T> action, T arg)
        {
            try
            {
                action?.Invoke(arg);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured in action {action}.\n{e}");
            }
        }
        
        public static void InvokeSafe<T>(this UnityEvent<T> action, T arg)
        {
            try
            {
                action?.Invoke(arg);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured in unity event {action}.\n{e}");
            }
        }
        
        #endregion
    }
}