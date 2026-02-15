using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class SetRefAttribute : PropertyAttribute
{
    public string ObjectName { get; private set; }
    public Type ComponentType { get; private set; }
    public float FieldWidthOffset { get; private set; }
    public string ParentName { get; private set; }
    public int RootOffset { get; private set; }

    public SetRefAttribute(string objectName, float fieldWidthOffset = 0)
    {
        ParentName = "";
        ObjectName = objectName;
        ComponentType = null;
        RootOffset = 0;
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }
    
    public SetRefAttribute(string objectName, Type componentType, float fieldWidthOffset = 0)
    {
        ParentName = "";
        ObjectName = objectName;
        ComponentType = componentType;
        RootOffset = 0;
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }

    public SetRefAttribute(Type componentType, float fieldWidthOffset = 0)
    {
        ParentName = "";
        ObjectName = "";
        ComponentType = componentType;
        RootOffset = 0;
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }

    #region Search Level

    public SetRefAttribute(int rootOffset, string objectName, float fieldWidthOffset = 0)
    {
        ParentName = "";
        ObjectName = objectName;
        ComponentType = null;
        RootOffset = Mathf.Clamp(rootOffset, 0, 100);
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }
    
    public SetRefAttribute(int rootOffset, string objectName, Type componentType, float fieldWidthOffset = 0)
    {
        ParentName = "";
        ObjectName = objectName;
        ComponentType = componentType;
        RootOffset = Mathf.Clamp(rootOffset, 0, 100);
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }

    public SetRefAttribute(int rootOffset, Type componentType, float fieldWidthOffset = 0)
    {
        ParentName = "";
        ObjectName = "";
        ComponentType = componentType;
        RootOffset = Mathf.Clamp(rootOffset, 0, 100);
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }

    #endregion

    #region Parent Name

    public SetRefAttribute(string parentName, string objectName, float fieldWidthOffset = 0)
    {
        ParentName = parentName;
        ObjectName = objectName;
        ComponentType = null;
        RootOffset = 0;
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }

    public SetRefAttribute(string parentName, string objectName, Type componentType, float fieldWidthOffset = 0)
    {
        ParentName = parentName;
        ObjectName = objectName;
        ComponentType = componentType;
        RootOffset = 0;
        FieldWidthOffset = fieldWidthOffset < 0 ? 0 : fieldWidthOffset;
    }

    #endregion
}