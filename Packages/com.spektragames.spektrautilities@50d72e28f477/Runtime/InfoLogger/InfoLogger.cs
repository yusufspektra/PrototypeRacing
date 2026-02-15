using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public class InfoLogger
    {
        public bool EnableLog { get; private set; } = true;
        private readonly string _loggerName;
        private readonly string _logColor;

        private bool _loggerSettingsChecked = false;

        private bool _debugLogsEnabled = true;
        private bool _warningLogsEnabled = true;
        private bool _errorLogsEnabled = true;
        private bool _exceptionLogsEnabled = true;

        public InfoLogger(string loggerName, string logColor = "")
        {
            this._loggerName = loggerName;
            this._logColor = logColor;

            if (string.IsNullOrEmpty(logColor))
                this._logColor = "white";
        }

        private void CheckLoggerSettings()
        {
            if (_loggerSettingsChecked)
                return;

            if (!string.IsNullOrEmpty(_loggerName) && InfoLoggerSettings.Instance)
            {
                InfoLoggerSettings.LoggerDefinition definition = InfoLoggerSettings.Instance.FindLoggerDefinition(_loggerName);
                if (definition != null)
                {
                    EnableLog = definition.isEnabled;
                    _debugLogsEnabled = definition.debugLogsEnabled;
                    _warningLogsEnabled = definition.warningsLogsEnabled;
                    _errorLogsEnabled = definition.errorLogsEnabled;
                    _exceptionLogsEnabled = definition.exceptionLogsEnabled;
                }
            }

            _loggerSettingsChecked = true;
        }

#if DISABLE_SRDEBUGGER
        [Conditional("DEV_GAME_ENVIRONMENT")]
#endif
        // Main method with switch logic
        private void InternalHandleLog(
            string message,
            LogType logType = LogType.Log,
            Exception exception = null,
            bool isErrorFormat = false,
            UnityEngine.Object context = null)
        {
            CheckLoggerSettings();
            
            if (!EnableLog)
                return;

            if (logType == LogType.Log && !_debugLogsEnabled)
                return;
            if (logType == LogType.Warning && !_warningLogsEnabled)
                return;
            if (logType == LogType.Error && !_errorLogsEnabled)
                return;
            if (logType == LogType.Exception && !_exceptionLogsEnabled)
                return;

            var logMessage = $"<color={_logColor}>[{_loggerName}]</color> {message}";

            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(logMessage, context);
                    break;

                case LogType.Warning:
                    Debug.LogWarning($"{logMessage}", context);
                    break;

                case LogType.Error:
                    Debug.LogError($"{logMessage}", context);
                    break;
                case LogType.Exception:
                    if (isErrorFormat)
                        Debug.LogError($"{logMessage} \nException: {exception?.Message}\nStackTrace: {exception?.StackTrace}", context);
                    else
                        Debug.LogException(exception, context);
                    break;
            }
        }

#if DISABLE_SRDEBUGGER
        [Conditional("DEV_GAME_ENVIRONMENT")]
#endif
        public void Log(string message, UnityEngine.Object context = null)
        {
            InternalHandleLog(message, LogType.Log, context: context);
        }

#if DISABLE_SRDEBUGGER
        [Conditional("DEV_GAME_ENVIRONMENT")]
#endif
        public void LogWarning(string message, UnityEngine.Object context = null)
        {
            InternalHandleLog(message, LogType.Warning, context: context);
        }

#if DISABLE_SRDEBUGGER
        [Conditional("DEV_GAME_ENVIRONMENT")]
#endif
        public void LogError(string message, UnityEngine.Object context = null)
        {
            InternalHandleLog(message, LogType.Error, context: context);
        }

#if DISABLE_SRDEBUGGER
        [Conditional("DEV_GAME_ENVIRONMENT")]
#endif
        public void LogException(Exception exception, bool isErrorFormat = false, UnityEngine.Object context = null)
        {
            InternalHandleLog("Exception occurred", LogType.Exception, exception, isErrorFormat, context);
        }
    }
}