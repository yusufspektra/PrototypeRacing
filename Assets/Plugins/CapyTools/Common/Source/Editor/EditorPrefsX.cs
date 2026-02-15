using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
namespace CapyTools.Common.Editor {

    public class EditorPrefsX {

        [Serializable]
        private class PreferenceData
        {
            public List<KeyValue> Data = new();
            public PreferenceData(Dictionary<string, string> dictionary)
            {
                Data = new List<KeyValue>();
                foreach (var kvp in dictionary)
                    Data.Add(new KeyValue(kvp.Key, kvp.Value));
            }
            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> dict = new();
                foreach (var kvp in Data)
                    dict[kvp.Key] = kvp.Value;
                return dict;
            }
        }

        [Serializable]
        private class KeyValue
        {
            public string Key;
            public string Value;

            public KeyValue(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }

        public string GlobalKey { get; }
        private Dictionary<string, string> _cache;
        private bool _isDirty;
        private bool _loaded = false;

        public EditorPrefsX(string globalKey)
        {
            GlobalKey = globalKey;
            LoadPreferences();
            EditorApplication.quitting += SavePreferences;
            AssemblyReloadEvents.beforeAssemblyReload += SavePreferences;
        }

        private void LoadPreferences()
        {
            if (EditorPrefs.HasKey(GlobalKey))
            {
                string json = EditorPrefs.GetString(GlobalKey);
                PreferenceData data = JsonUtility.FromJson<PreferenceData>(json);
                _cache = data != null ? data.ToDictionary() : new Dictionary<string, string>();
            }
            else
            {
                _cache = new Dictionary<string, string>();
            }

            _loaded = true;
        }

        public void SavePreferences()
        {
            if (!_isDirty) return;
            if (!_loaded) return;


            string json = JsonUtility.ToJson(new PreferenceData(_cache));
            EditorPrefs.SetString(GlobalKey, json);
            _isDirty = false;

            if (json.Length * sizeof(char) > 1024 * 1024) // max size of EditorPrefs is 1MB
            {  
                _cache.Clear(); 
                EditorPrefs.DeleteKey(GlobalKey);
                Debug.LogError("CapyTools: Preferences data is too large. Resetting data.");
            }

        }

        public void SetString(string key, string value)
        {
            _cache[key] = value;
            _isDirty = true;
        }

        public string GetString(string key, string defaultValue = "")
        {
            return _cache.TryGetValue(key, out var value) ? value : defaultValue;
        }
        public void SetInt(string key, int value)
        {
            SetString(key, value.ToString());
        }
        public int GetInt(string key, int defaultValue = 0)
        {
            return int.TryParse(GetString(key), out var result) ? result : defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            SetString(key, value.ToString());
        }
        public float GetFloat(string key, float defaultValue = 0f)
        {
            return float.TryParse(GetString(key), out var result) ? result : defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            SetString(key, value ? "1" : "0");
        }
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (HasKey(key))
                return GetString(key) == "1";
            return defaultValue;
        }

        public bool HasKey(string key) => _cache.ContainsKey(key);
        public void DeleteKey(string key)
        {
            if (_cache.Remove(key))
                _isDirty = true;
        }
        public void DeleteAll()
        {
            _cache.Clear();
            _isDirty = true;
        }


        public void SetColor(string key, Color color) {
            SetString(key, ColorToString(color));
        }

        public Color GetColor(string key, Color defaultValue) {
            string colorString = GetString(key, ColorToString(defaultValue));
            return StringToColor(colorString);
        }

        public string ColorToString(Color color) {
            return string.Format("{0};{1};{2};{3}", color.r, color.g, color.b, color.a);
        }

        public Color StringToColor(string colorString) {
            string[] strings = colorString.Split(';');
            if (strings.Length != 4) {
                return Color.white;
            }
            return new Color(float.Parse(strings[0]), float.Parse(strings[1]), float.Parse(strings[2]), float.Parse(strings[3]));
        }
    }
}