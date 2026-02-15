#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities.Editor;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SetRefAttribute))]
public class SetRefAttributeDrawer : PropertyDrawer
{
    private bool _updateReferenceCalled;
    private Dictionary<string, bool> _referenceValidDictionary = new();
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SetRefAttribute setRefAttribute = (SetRefAttribute)attribute;
        
        // Rect size variables
        float buttonSize = 20f;
        float maxWidth = position.width;
        float fieldWidth = maxWidth - buttonSize - setRefAttribute.FieldWidthOffset;
        Rect fieldRect = position;
        Rect refreshButtonRect =
            new Rect(fieldWidth + buttonSize, position.y, buttonSize, buttonSize);
        fieldRect.width = fieldWidth;
        
        if (_updateReferenceCalled)
        {
            DrawSetRefGUI(property, fieldRect, refreshButtonRect);
            return;
        }

        if (!_updateReferenceCalled)
        {
            UpdateReference(property);
            DrawSetRefGUI(property, fieldRect, refreshButtonRect);
            _updateReferenceCalled = true;
        }
    }

    private void DrawSetRefGUI(SerializedProperty property, Rect fieldRect, Rect refreshButtonRect)
    {
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUI.PropertyField(fieldRect, property);
        }
        bool referenceValid = IsReferenceValid(property);
        GUIHelper.PushColor(referenceValid ? Color.green : Color.red);

        if (GUI.Button(refreshButtonRect, "R"))
            UpdateReference(property);
        
        GUIHelper.PopColor();
    }

    private bool IsReferenceValid(SerializedProperty property)
    {
        return _referenceValidDictionary.TryGetValue(property.name, out var valid) && valid;
    }

    private void SetReferenceValid(string key, bool value)
    {
        _referenceValidDictionary[key] = value;
    }

    private void UpdateReference(SerializedProperty property)
    {
        _updateReferenceCalled = true;

        MonoBehaviour monoBehaviour = property.serializedObject.targetObject as MonoBehaviour;
        Transform transform = monoBehaviour.transform;

        // Collect all fields from the inheritance hierarchy
        List<FieldInfo> fields = new List<FieldInfo>();
        Type currentType = monoBehaviour.GetType();
        while (currentType != null)
        {
            fields.AddRange(currentType.GetFields(
                BindingFlags.Instance | 
                BindingFlags.NonPublic | 
                BindingFlags.Public | 
                BindingFlags.DeclaredOnly));
            currentType = currentType.BaseType;
        }

        foreach (var field in fields)
        {
            SetRefAttribute attribute = Attribute.GetCustomAttribute(field, typeof(SetRefAttribute)) as SetRefAttribute;
            if (attribute != null)
            {
                if (property.propertyPath != field.Name)
                    continue;

                Transform targetTransform = SetRefHelper.GetTargetTransform(transform, attribute);
                object component = null;

                if (targetTransform != null)
                {
                    if (attribute.ComponentType == null)
                    {
                        System.Type fieldType = field.FieldType;
                        var componentType = fieldType;
                        var methods = typeof(TransformExtensions).GetMethods(
                            BindingFlags.Static | BindingFlags.Public);
                        var method = methods.FirstOrDefault(x => x.IsGenericMethod).MakeGenericMethod(componentType);
                        component = method.Invoke(null, new object[] { targetTransform, attribute.ObjectName });
                    }
                    else if (attribute.ComponentType != null && string.IsNullOrEmpty(attribute.ObjectName))
                    {
                        component = transform.GetComponent(attribute.ComponentType);
                        if (component == null)
                            component = transform.GetComponentInChildren(attribute.ComponentType, true);
                    }
                    else
                    {
                        var componentType = attribute.ComponentType;
                        var methods = typeof(TransformExtensions).GetMethods(
                            BindingFlags.Static | BindingFlags.Public);
                        var method = methods.FirstOrDefault(x => x.IsGenericMethod).MakeGenericMethod(componentType);
                        component = method.Invoke(null, new object[] { targetTransform, attribute.ObjectName });
                    }
                }

                var oldFieldValue = field.GetValue(monoBehaviour);
                object newFieldValue = null;
                if (component != null)
                {
                    try
                    {
                        field.SetValue(monoBehaviour, component);
                        SetReferenceValid(field.Name, true);
                        newFieldValue = component;
                    }
                    catch (Exception e)
                    {
                        field.SetValue(monoBehaviour, null);
                        SetReferenceValid(field.Name, false);
                        SetRefHelper.LogErrorMessage(attribute, transform, targetTransform);
                        newFieldValue = null;
                    }
                    finally
                    {
                        SetDirtyIfNeeded(oldFieldValue, newFieldValue, monoBehaviour);
                    }
                }
                else
                {
                    field.SetValue(monoBehaviour, component);
                    SetReferenceValid(field.Name, false);
                    SetDirtyIfNeeded(oldFieldValue, newFieldValue, monoBehaviour);
                    SetRefHelper.LogErrorMessage(attribute, transform, targetTransform);
                }
            }
        }
    }

    private static void SetDirtyIfNeeded(object oldFieldValue, object newFieldValue, MonoBehaviour monoBehaviour)
    {
        if (oldFieldValue != newFieldValue)
        {
            EditorUtility.SetDirty(monoBehaviour);
            // Debug.LogError($"{monoBehaviour.gameObject.name} is dirty");
        }
    }
}
#endif