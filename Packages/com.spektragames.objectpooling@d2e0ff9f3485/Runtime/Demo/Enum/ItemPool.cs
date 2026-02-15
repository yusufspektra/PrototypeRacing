using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public class ItemPool : PoolEnum
    {
        public static readonly ItemPool HealthPotion = new ItemPool(1, nameof(HealthPotion), nameof(ItemPool));
        public static readonly ItemPool ManaPotion = new ItemPool(2, nameof(ManaPotion), nameof(ItemPool));
        public static readonly ItemPool StaminaPotion = new ItemPool(3, nameof(StaminaPotion), nameof(ItemPool));
        
        public ItemPool(ItemPool itemType) : base(itemType) { }
        private ItemPool(int value, string enumName, string categoryName) : base(value, enumName, categoryName) { }
    }
}
