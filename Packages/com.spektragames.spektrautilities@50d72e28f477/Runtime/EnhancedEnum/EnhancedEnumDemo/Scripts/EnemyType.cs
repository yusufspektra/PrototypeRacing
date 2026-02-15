using System;

namespace SpektraGames.SpektraUtilities.Editor
{
    [Serializable]
    public class EnemyType : EnhancedEnum
    {
        public static readonly EnemyType Zombie = new EnemyType(1, nameof(Zombie), nameof(EnemyType));
        public static readonly EnemyType Vampire = new EnemyType(2, nameof(Vampire), nameof(EnemyType));
        public static readonly EnemyType Cyborg = new EnemyType(3, nameof(Cyborg), nameof(EnemyType));

        public EnemyType(EnemyType enemyType) : base(enemyType) { }
        private EnemyType(int value, string enumName, string categoryName) : base(value, enumName, categoryName) { }
    }   
}