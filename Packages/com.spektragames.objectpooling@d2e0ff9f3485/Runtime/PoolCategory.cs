using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    [InlineEditor]
    public class PoolCategory : ScriptableObject// where T : PoolEnum
    {
        [ReadOnly]
        public string categoryType;
        [InlineEditor(Expanded = false)]
        public List<PoolItem> poolItemList;
    }
}
