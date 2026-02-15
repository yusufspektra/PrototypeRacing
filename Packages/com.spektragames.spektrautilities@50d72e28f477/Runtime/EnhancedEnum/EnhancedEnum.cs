using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class EnhancedEnum
{
    [SerializeField, JsonProperty]
    private string _category;
    [SerializeField, JsonProperty]
    private int _value;
    [SerializeField, JsonProperty]
    private string _enumName;

    [JsonIgnore]
    public string EnumName => _enumName;
    [JsonIgnore]
    public string Category => _category;

    [JsonIgnore]
    public string UniqueIdentifier
    {
        get
        {
            if (_isUniqueIdentifierDirty || string.IsNullOrEmpty(_cachedUniqueIdentifier))
            {
                _cachedUniqueIdentifier = $"{_category}_{_enumName}_{_value}";
                _isUniqueIdentifierDirty = false;
            }
            return _cachedUniqueIdentifier;
        }
    }
    
    [JsonIgnore]
    public int UniqueIdentifierInt
    {
        get
        {
            if (_isUniqueIdentifierIntDirty || _cachedUniqueIdentifierInt == -1)
            {
                string identifier = UniqueIdentifier;
                _cachedUniqueIdentifierInt = GetStableHashCode(identifier);
                _isUniqueIdentifierIntDirty = false;
                
                int GetStableHashCode(string str)
                {
                    unchecked
                    {
                        int hash1 = 5381;
                        int hash2 = hash1;

                        for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                        {
                            hash1 = ((hash1 << 5) + hash1) ^ str[i];
                            if (i == str.Length - 1 || str[i + 1] == '\0')
                                break;
                            hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                        }

                        return hash1 + (hash2 * 1566083941);
                    }
                }
            }
            return _cachedUniqueIdentifierInt;
        }
    }
    
    private string _cachedUniqueIdentifier;
    private bool _isUniqueIdentifierDirty = true;
    
    private int _cachedUniqueIdentifierInt = -1;
    private bool _isUniqueIdentifierIntDirty = true;

    public EnhancedEnum(EnhancedEnum enhancedEnum)
    {
        this._value = enhancedEnum._value;
        this._enumName = enhancedEnum._enumName.ToString();
        this._category = enhancedEnum._category.ToString();
        _isUniqueIdentifierDirty = true;
    }
    
    protected EnhancedEnum(int value, string enumName, string categoryName)
    {
        this._value = value;
        this._enumName = enumName ?? throw new ArgumentNullException(nameof(enumName));
        this._category = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _isUniqueIdentifierDirty = true;
    }

    public override string ToString() => UniqueIdentifier;

    public override bool Equals(object obj) => obj is EnhancedEnum other && GetHashCode() == other.GetHashCode();
    public bool Equals(EnhancedEnum obj) => GetHashCode() == obj?.GetHashCode();
    
    public override int GetHashCode() => UniqueIdentifier.GetHashCode();
    
    public static explicit operator int(EnhancedEnum enhancedEnum) => enhancedEnum._value;
    
    public static bool operator ==(EnhancedEnum a, EnhancedEnum b) => a?.Equals(b) ?? b is null;
    
    public static bool operator !=(EnhancedEnum a, EnhancedEnum b) => !(a == b);
    
    #if UNITY_EDITOR
    public void SetDirty()
    {
        _isUniqueIdentifierDirty = true;
        _isUniqueIdentifierIntDirty = true;
    }
    #endif
}