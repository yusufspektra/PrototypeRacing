#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnhancedEnum))]
public class EnhancedEnumDrawer : PropertyDrawer
{
    private static List<Type> _cachedEnumTypes = null;
    private static Dictionary<Type, List<object>> _cachedFieldTypes = null;

    private static List<Type> CachedEnumTypes
    {
        get
        {
            if (_cachedEnumTypes == null)
            {
                _cachedEnumTypes = new();
                _cachedFieldTypes = new Dictionary<Type, List<object>>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; i++)
                {
                    var types = assemblies[i].GetTypes()
                        .Where(x => !x.IsAbstract && !x.IsInterface &&
                                    x.BaseType != null && x.IsSubclassOf(typeof(EnhancedEnum)))
                        .Select(x => x);
                    var rootTypes = types.Where(type => !types.Any(
                        otherType => otherType != type && otherType.IsSubclassOf(type))).ToList();

                    foreach (var type in rootTypes)
                    {
                        _cachedEnumTypes.Add(type);

                        var fieldTypes = type.GetFields(BindingFlags.Static | BindingFlags.Public)
                            .Where(field => field.FieldType.IsSubclassOf(typeof(EnhancedEnum)))
                            .ToList();

                        _cachedFieldTypes.TryGetValue(type, out var fieldValueList);
                        fieldValueList ??= new List<object>();
                        for (var j = 0; j < fieldTypes.Count; j++)
                        {
                            var fieldValue = fieldTypes[j].GetValue(null);
                            var fakeEnum = fieldValue as EnhancedEnum;
                            if (fieldTypes[j].Name != fakeEnum.EnumName)
                            {
                                Debug.LogError($"Static field {fieldTypes[j].Name} is corrupt");
                            }

                            fieldValueList.Add(fieldValue);
                        }

                        _cachedFieldTypes[type] = fieldValueList;
                    }
                }
            }

            return _cachedEnumTypes;
        }
    }

    private static Dictionary<Type, List<object>> CachedFieldTypes
    {
        get
        {
            if (_cachedFieldTypes == null)
            {
                var dummy = CachedEnumTypes;
            }

            return _cachedFieldTypes;
        }
    }

    private static FieldInfo _cachedValueField = null;
    private static FieldInfo _cachedEnumNameField = null;

    private static FieldInfo CachedValueField
    {
        get
        {
            if (_cachedValueField == null)
            {
                _cachedValueField =
                    typeof(EnhancedEnum).GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return _cachedValueField;
        }
    }

    private static FieldInfo CachedEnumNameField
    {
        get
        {
            if (_cachedEnumNameField == null)
            {
                _cachedEnumNameField =
                    typeof(EnhancedEnum).GetField("_enumName", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return _cachedEnumNameField;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EnhancedEnum enhancedEnum = property.GetTargetObjectOfProperty<EnhancedEnum>();

        var valueProp = property.FindPropertyRelative("_value"); // Int
        var enumNameProp = property.FindPropertyRelative("_enumName"); // String
        var categoryProp = property.FindPropertyRelative("_category"); // String

        var fieldInfo = property.GetFieldInfoFromProperty();

        if (valueProp != null &&
            enumNameProp != null &&
            categoryProp != null)
        {
            var component = property.serializedObject.targetObject;
            if (component != null) // Reference correction
            {
                if (fieldInfo != null)
                {
                    try
                    {
                        object fieldValue = fieldInfo.GetValue(component);
                        if (fieldValue != null)
                        {
                            for (var i = 0; i < CachedEnumTypes.Count; i++)
                            {
                                Type categoryType = CachedEnumTypes[i];
                                if (CachedFieldTypes.TryGetValue(categoryType, out var fieldValueList))
                                {
                                    for (var j = 0; j < fieldValueList.Count; j++)
                                    {
                                        EnhancedEnum enumValue = (EnhancedEnum)fieldValueList[j];
                                        if (enumValue != null && ReferenceEquals(enumValue, fieldValue))
                                        {
                                            ConstructorInfo constructor =
                                                categoryType.GetConstructor(new Type[] { categoryType });
                                            fieldInfo.SetValue(component,
                                                constructor.Invoke(new object[] { fieldValue }));
                                            EditorUtility.SetDirty(property.serializedObject.targetObject);
                                            // Debug.LogError("Remove reference to static object");
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            var enumCategories = GetEnumCategories(fieldInfo?.FieldType);
            if (string.IsNullOrEmpty(categoryProp.stringValue))
            {
                List<string> values = new List<string>();
                values.Add("--Choose Category--");
                values.AddRange(enumCategories);
                int selectedValue = EditorGUI.Popup(position, label.text, 0, values.ToArray());
                if (selectedValue != 0)
                {
                    categoryProp.stringValue = values[selectedValue];
                    var enumVals = GetEnums(CachedEnumTypes.FirstOrDefault(x => x.Name == categoryProp.stringValue));
                    enumNameProp.stringValue = enumVals[0].name;
                    valueProp.intValue = enumVals[0].index;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }

                EditorGUI.EndProperty();
                return;
            }

            Type enumType = CachedEnumTypes.FirstOrDefault(x => x.Name == categoryProp.stringValue);
            var enumValues = GetEnums(enumType);
            var enumNames = enumValues.Select(x => x.name).ToArray();

            float labelHeight = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 2 - 5, labelHeight), "Category");
            EditorGUI.LabelField(
                new Rect(position.x + position.width / 2 + 5, position.y, position.width / 2 - 5, labelHeight), "Enum");
            EditorGUILayout.PrefixLabel(""); // This fixes the layout

            Rect categoryRect = new Rect(position.x, position.y + labelHeight, position.width / 2 - 5, position.height);
            Rect valueRect = new Rect(position.x + position.width / 2 + 5, position.y + labelHeight,
                position.width / 2 - 5, position.height);

            int currentEnumIndex = Array.IndexOf(enumCategories, categoryProp.stringValue);
            int selectedEnumIndex = EditorGUI.Popup(categoryRect, currentEnumIndex, enumCategories);
            if (currentEnumIndex != selectedEnumIndex)
            {
                // Category changed
                categoryProp.stringValue = enumCategories[selectedEnumIndex];
                enumValues = GetEnums(CachedEnumTypes.FirstOrDefault(x => x.Name == categoryProp.stringValue));
                enumNames = enumValues.Select(x => x.name).ToArray();
                enumNameProp.stringValue = enumValues[0].name;
                valueProp.intValue = enumValues[0].index;
                enhancedEnum?.SetDirty();
                property?.serializedObject?.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            int currentEnumValueIndex = Array.IndexOf(enumNames, enumNameProp.stringValue);
            int selectedEnumValueIndex = EditorGUI.Popup(valueRect, currentEnumValueIndex, enumNames);
            if (currentEnumValueIndex != selectedEnumValueIndex)
            {
                enumNameProp.stringValue = enumNames[selectedEnumValueIndex];
                valueProp.intValue = enumValues.First(x => x.name == enumNameProp.stringValue).index;
                enhancedEnum?.SetDirty();
                property?.serializedObject?.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        EditorGUI.EndProperty();
    }

    private string[] GetEnumCategories(Type fieldType)
    {
        // Handle arrays
        if (fieldType.IsArray)
            fieldType = fieldType.GetElementType();

        // Handle List<T> and other generic collections
        else if (fieldType.IsGenericType &&
                 fieldType.GetGenericTypeDefinition() == typeof(List<>))
            fieldType = fieldType.GetGenericArguments()[0];
    
        if (fieldType != null && CachedEnumTypes.FirstOrDefault(o => o == fieldType) != null)
            return new string[] { fieldType.Name };
    
        return CachedEnumTypes.Where(fieldType.IsAssignableFrom)
            .Select(x => x.Name).ToArray();
    }

    private List<(int index, string name)> GetEnums(Type categoryType)
    {
        List<(int index, string name)> response = new();

        if (!CachedFieldTypes.TryGetValue(categoryType, out var fieldValueList))
        {
            Debug.LogError($"Field values not found for type {categoryType}");
            return response;
        }

        for (var i = 0; i < fieldValueList.Count; i++)
        {
            int enumValueInt = (int)CachedValueField.GetValue(fieldValueList[i]);
            string enumNameString = (string)CachedEnumNameField.GetValue(fieldValueList[i]);
            response.Add((enumValueInt, enumNameString));
        }

        return response;
    }
}

#endif