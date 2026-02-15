using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    [Serializable]
    public class PoolEnum : EnhancedEnum
    {
        public PoolEnum(EnhancedEnum enhancedEnum) : base(enhancedEnum) { }
        protected PoolEnum(int value, string enumName, string categoryName) : base(value, enumName, categoryName) { }
    }
}
