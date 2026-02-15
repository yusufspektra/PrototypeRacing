using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    [InfoBox("Please use Info Logger Toggle editor window from 'Tools/Info Logger/Toggle Window'", InfoMessageType.Error)]
    public class InfoLoggerSettings : SingletonScriptableObject<InfoLoggerSettings>
    {
        [System.Serializable]
        public class LoggerDefinition
        {
            public string path;
            public string loggerName;
            public bool isEnabled = true;
            public Color defaultColor = Color.white;
            public Color overridedColor = Color.white;
            
            public bool debugLogsEnabled = true;
            public bool warningsLogsEnabled = true;
            public bool errorLogsEnabled = true;
            public bool exceptionLogsEnabled = true;
        }

        [ReadOnly]
        public List<LoggerDefinition> definitions = new List<LoggerDefinition>();

        public LoggerDefinition FindLoggerDefinition(string loggerName)
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].loggerName == loggerName)
                {
                    return definitions[i];
                }
            }

            return null;
        }
    }
}