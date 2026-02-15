#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#endif

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class SetRefHelper
    {
#if UNITY_EDITOR
        public static void SetInitialReferences(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
                return;

            Transform transform = monoBehaviour.transform;

            // Traverse all fields in inheritance hierarchy
            List<FieldInfo> fields = new();
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
                var attribute = Attribute.GetCustomAttribute(field, typeof(SetRefAttribute)) as SetRefAttribute;
                if (attribute == null)
                    continue;

                Transform targetTransform = GetTargetTransform(transform, attribute);
                object component = null;

                if (targetTransform != null)
                {
                    Type fieldType = field.FieldType;
                    Type componentType = attribute.ComponentType ?? fieldType;
                    MethodInfo method = typeof(TransformExtensions)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.IsGenericMethod)?
                        .MakeGenericMethod(componentType);

                    component = method?.Invoke(null, new object[] { targetTransform, attribute.ObjectName });
                }

                object oldValue = field.GetValue(monoBehaviour);
                object newValue = component;

                try
                {
                    field.SetValue(monoBehaviour, component);
                }
                catch
                {
                    field.SetValue(monoBehaviour, null);
                    newValue = null;
                }

                if (!Equals(oldValue, newValue))
                {
                    EditorUtility.SetDirty(monoBehaviour);
                }

                // Error logging
                if (component == null)
                {
                    LogErrorMessage(attribute, transform, targetTransform);
                }
            }
        }

        public static Transform GetTargetTransform(Transform transform, SetRefAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.ParentName) && attribute.RootOffset == 0)
                return transform;

            Transform current = transform;
            for (int i = 0; i < 100 && current != null; ++i)
            {
                if (attribute.RootOffset > 0)
                {
                    if (i >= attribute.RootOffset)
                        break;
                    current = current.parent;
                }
                else
                {
                    var found = current.Find(attribute.ParentName);
                    if (found != null)
                        return found;
                    current = current.parent;
                }
            }

            return current;
        }

        public static void LogErrorMessage(SetRefAttribute attribute, Transform transform, Transform targetTransform)
        {
            if (targetTransform == null && !string.IsNullOrEmpty(attribute.ParentName))
            {
                Debug.LogError($"Could not find parent named '{attribute.ParentName}' above object {transform.name}",
                    transform.gameObject);
            }
            else if (targetTransform == null && attribute.RootOffset > 0)
            {
                Debug.LogError($"{transform.name} does not have '{attribute.RootOffset}' parents",
                    transform.gameObject);
            }
            else if (attribute.ComponentType == null)
            {
                Debug.LogError($"Could not find '{attribute.ObjectName}' on gameObject '{transform.gameObject.name}'",
                    transform.gameObject);
            }
            else if (string.IsNullOrEmpty(attribute.ObjectName))
            {
                Debug.LogError(
                    $"Could not find object of type '{attribute.ComponentType}' on gameObject '{transform.gameObject.name}'",
                    transform.gameObject);
            }
            else
            {
                Debug.LogError(
                    $"Could not find '{attribute.ObjectName}' of type '{attribute.ComponentType}' on gameObject '{transform.gameObject.name}'",
                    transform.gameObject);
            }
        }
#endif
    }
}