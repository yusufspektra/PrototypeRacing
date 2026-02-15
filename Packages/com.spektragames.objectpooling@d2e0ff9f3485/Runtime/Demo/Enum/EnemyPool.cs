using System;

namespace SpektraGames.ObjectPooling.Runtime
{
    [Serializable]
    public class EnemyPool : PoolEnum
    {
        public static readonly EnemyPool Zombie = new EnemyPool(1, nameof(Zombie), nameof(EnemyPool));
        public static readonly EnemyPool Vampire = new EnemyPool(2, nameof(Vampire), nameof(EnemyPool));
        public static readonly EnemyPool Cyborg = new EnemyPool(3, nameof(Cyborg), nameof(EnemyPool));

        public EnemyPool(EnemyPool enemyType) : base(enemyType) { }
        private EnemyPool(int value, string enumName, string categoryName) : base(value, enumName, categoryName) { }
    }
}
