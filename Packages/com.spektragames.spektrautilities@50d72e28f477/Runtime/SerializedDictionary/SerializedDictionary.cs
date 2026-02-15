using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    protected List<TKey> keyData = new List<TKey>();
	
    [SerializeField, HideInInspector]
    protected List<TValue> valueData = new List<TValue>();

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        OnAfterDeserialize();
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        OnBeforeSerialize();
    }

    protected virtual void OnBeforeSerialize()
    {
        this.keyData.Clear();
        this.valueData.Clear();

        foreach (var item in this)
        {
            this.keyData.Add(item.Key);
            this.valueData.Add(item.Value);
        }
    }
    
    protected virtual void OnAfterDeserialize()
    {
        this.Clear();
        
        if (keyData.Count != valueData.Count)
            throw new Exception($"There are {keyData.Count} keys and {valueData.Count} values after deserialization. Make sure that both key and value types are serializable.");
        
        for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
        {
            this[this.keyData[i]] = this.valueData[i];
        }
    }
}