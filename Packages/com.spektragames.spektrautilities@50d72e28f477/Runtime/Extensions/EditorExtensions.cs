using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
#endif

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class EditorExtensions
    {
#if UNITY_EDITOR
        private static readonly MethodInfo _getFieldInfoFromProperty;

        static EditorExtensions()
        {
            var scriptAttributeUtility =
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
            Assert.IsNotNull(scriptAttributeUtility, "ScriptAttributeUtility != null");

            _getFieldInfoFromProperty = scriptAttributeUtility.GetMethod(nameof(GetFieldInfoFromProperty),
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(_getFieldInfoFromProperty, "_getFieldInfoFromProperty != null");
        }

        public static FieldInfo GetFieldInfoFromProperty(this SerializedProperty property)
        {
            Type type = null;
            var fieldInfo = (FieldInfo)_getFieldInfoFromProperty.Invoke(null,
                new object[]
                {
                    property, type
                });
            return fieldInfo;
        }

        public static bool TryGetSerializedPropertyObject(
            this InspectorProperty inspectorProperty,
            out UnityEngine.Object serializedObject)
        {
            if (inspectorProperty != null &&
                inspectorProperty.SerializationRoot != null &&
                inspectorProperty.SerializationRoot.ValueEntry != null &&
                inspectorProperty.SerializationRoot.ValueEntry.WeakSmartValue != null &&
                inspectorProperty.SerializationRoot.ValueEntry.WeakSmartValue is UnityEngine.Object castedObj)
            {
                serializedObject = castedObj;
                return true;
            }

            serializedObject = null;
            return false;
        }

        public static bool TryGetSerializedPropertyObject<T>(
            this IPropertyValueEntry<T> valueEntry,
            out UnityEngine.Object serializedObject)
        {
            if (valueEntry != null &&
                valueEntry.Property != null &&
                valueEntry.Property.SerializationRoot != null &&
                valueEntry.Property.SerializationRoot.ValueEntry != null &&
                valueEntry.Property.SerializationRoot.ValueEntry.WeakSmartValue != null &&
                valueEntry.Property.SerializationRoot.ValueEntry.WeakSmartValue is UnityEngine.Object castedObj)
            {
                serializedObject = castedObj;
                return true;
            }

            serializedObject = null;
            return false;
        }

        public static bool TryGetSerializedPropertyObject(
            this IPropertyValueEntry valueEntry,
            out UnityEngine.Object serializedObject)
        {
            if (valueEntry != null &&
                valueEntry.Property != null &&
                valueEntry.Property.SerializationRoot != null &&
                valueEntry.Property.SerializationRoot.ValueEntry != null &&
                valueEntry.Property.SerializationRoot.ValueEntry.WeakSmartValue != null &&
                valueEntry.Property.SerializationRoot.ValueEntry.WeakSmartValue is UnityEngine.Object castedObj)
            {
                serializedObject = castedObj;
                return true;
            }

            serializedObject = null;
            return false;
        }

        public static GameObject GetChildWithName(this GameObject go, string childName)
        {
            GameObject childObj = null;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                if (go.transform.GetChild(i).name.Equals(childName))
                {
                    childObj = go.transform.GetChild(i).gameObject;
                }
            }

            return childObj;
        }

        public static GameObject GetChildWithName(this GameObject[] rootObjects, string childName)
        {
            GameObject childObj = null;
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].name.Equals(childName))
                {
                    childObj = rootObjects[i].gameObject;
                }
            }

            return childObj;
        }

        public static T GetSerializedPropertyValue<T>(this SerializedProperty property)
        {
            object @object = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');

            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[^2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();

            // Get the last object of the property path.
            foreach (string path in propertyNames)
            {
                if (@object != null)
                    @object = @object.GetType()
                        .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                        ?.GetValue(@object);
            }

            if (@object != null && @object.GetType().GetInterfaces().Contains(typeof(IList<T>)))
            {
                int propertyIndex = int.Parse(property.propertyPath[^2].ToString());

                return ((IList<T>)@object)[propertyIndex];
            }
            else return (T)@object;
        }

        public static void SetSerializedPropertyValue<T>(this SerializedProperty property, T value)
        {
            object targetObject = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');

            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[^2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();

            // Traverse to the last object in the property path.
            for (int i = 0; i < propertyNames.Length - 1; i++)
            {
                if (targetObject != null)
                {
                    FieldInfo field = targetObject.GetType().GetField(propertyNames[i],
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    targetObject = field?.GetValue(targetObject);
                }
            }

            if (targetObject != null)
            {
                FieldInfo field = targetObject.GetType().GetField(propertyNames[^1],
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (field != null)
                {
                    if (typeof(IList<T>).IsAssignableFrom(field.FieldType))
                    {
                        // Handle arrays and lists
                        int propertyIndex = int.Parse(property.propertyPath[^2].ToString());
                        IList<T> list = (IList<T>)field.GetValue(targetObject);
                        list[propertyIndex] = value;
                    }
                    else
                    {
                        // Set the value for regular fields
                        field.SetValue(targetObject, value);
                    }
                }
            }

            // Apply changes to the serialized object.
            property.serializedObject.ApplyModifiedProperties();
        }

        public static T GetTargetObjectOfProperty<T>(this SerializedProperty property) where T : class
        {
            if (property == null) return null;

            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
                        .Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_ImpWithoutIndex(obj, element);
                }
            }

            return obj as T;

            object GetValue_ImpWithoutIndex(object source, string name)
            {
                if (source == null)
                    return null;
                var type = source.GetType();

                while (type != null)
                {
                    var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (f != null)
                        return f.GetValue(source);

                    var p = type.GetProperty(name,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p != null)
                        return p.GetValue(source, null);

                    type = type.BaseType;
                }

                return null;
            }

            object GetValue_Imp(object source, string name, int index)
            {
                var enumerable = GetValue_ImpWithoutIndex(source, name) as System.Collections.IEnumerable;
                if (enumerable == null) return null;
                var enm = enumerable.GetEnumerator();

                for (int i = 0; i <= index; i++)
                {
                    if (!enm.MoveNext()) return null;
                }

                return enm.Current;
            }
        }
#endif
    }
}