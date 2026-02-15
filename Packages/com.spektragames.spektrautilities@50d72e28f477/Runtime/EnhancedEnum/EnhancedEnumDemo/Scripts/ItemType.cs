using System;

namespace SpektraGames.SpektraUtilities.Editor
{
    [Serializable]
    public class ItemType : EnhancedEnum
    {
        public static readonly ItemType HealthPotion = new ItemType(1, nameof(HealthPotion), nameof(ItemType));
        public static readonly ItemType ManaPotion = new ItemType(2, nameof(ManaPotion), nameof(ItemType));
        public static readonly ItemType StaminaPotion = new ItemType(3, nameof(StaminaPotion), nameof(ItemType));

        public ItemType(ItemType type) : base(type) { }
        private ItemType(int value, string enumName, string categoryName) : base(value, enumName, categoryName) { }
    }
}
