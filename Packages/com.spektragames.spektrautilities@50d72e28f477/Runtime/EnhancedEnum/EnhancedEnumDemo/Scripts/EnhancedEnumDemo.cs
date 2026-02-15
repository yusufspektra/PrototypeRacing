using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Editor
{
    public class EnhancedEnumDemo : MonoBehaviour
    {
        [Title("Enhanced Enum", "Any Enhanced enum value can be chosen")]
        public EnhancedEnum enhancedEnum;
        
        [Title("Custom Type", "Only enemy type can be chosen")]
        public EnemyType enemyType;

        [Title("Assigned value", "Initial item type value is assigned")]
        public ItemType assignedItemType = new ItemType(ItemType.ManaPotion);

        [Title("Enhanced Enum Dictionary", "All Enhanced enums can be added to the dictionary")]
        public EnhancedEnumDictionary enhancedEnumDictionary;
        
        [Title("Item Dictionary", "Only item types can be added to the dictionary")]
        public ItemDictionary itemDictionary;
        
        [Title("Read Only Dictionary", "Dictionary keys can only be edited through code")]
        [DictionaryDrawerSettings(IsReadOnly = true)]
        public EnemyDictionary readOnlyEnemyDictionary;

        private void OnValidate()
        {
            PopulateReadOnlyEnemyDictionary();
        }

        private void PopulateReadOnlyEnemyDictionary()
        {
            if (readOnlyEnemyDictionary == null)
                readOnlyEnemyDictionary = new EnemyDictionary();
            if (readOnlyEnemyDictionary.Count != 0) 
                return;
            readOnlyEnemyDictionary[EnemyType.Zombie] = "This is a zombie";
            readOnlyEnemyDictionary[EnemyType.Vampire] = "This is a vampire";
            readOnlyEnemyDictionary[EnemyType.Cyborg] = "This is a cyborg";
        }

        [Button]
        private void ResetValues()
        {
            enhancedEnum = null;
            enemyType = null;
            assignedItemType = new ItemType(ItemType.ManaPotion);
            enhancedEnumDictionary = new EnhancedEnumDictionary();
            itemDictionary = new ItemDictionary();
            readOnlyEnemyDictionary = new EnemyDictionary();
        }
    }
    
    [Serializable]
    public class EnhancedEnumDictionary : SerializedDictionary<EnhancedEnum, int> { }
    [Serializable]
    public class ItemDictionary : SerializedDictionary<ItemType, int> { }
    [Serializable]
    public class EnemyDictionary : SerializedDictionary<EnemyType, string> { }
}
