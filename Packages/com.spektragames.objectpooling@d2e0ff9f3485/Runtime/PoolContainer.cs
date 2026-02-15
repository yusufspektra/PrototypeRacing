using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    [CreateAssetMenu(fileName = "PoolContainer", menuName = "Object Pooling/PoolContainer", order = 1)]
    public class PoolContainer : SingletonScriptableObject<PoolContainer>
    {
        public List<PoolCategory> poolCategoryList = new();
        [DictionaryDrawerSettings(IsReadOnly = true)]
        public PoolObjectDictionary poolObjectDictionary = new();

        [Button]
        public void ReCreatePoolObjectDictionary()
        {
            poolObjectDictionary = new PoolObjectDictionary();

            for (var i = 0; i < poolCategoryList.Count; i++)
            {
                for (var j = 0; j < poolCategoryList[i].poolItemList.Count; j++)
                {
                    var item = poolCategoryList[i].poolItemList[j];
                    poolObjectDictionary[new PoolEnum(item.type)] = item;
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    [Serializable]
    public class PoolObjectDictionary : SerializedDictionary<PoolEnum, PoolItem>
    {
    }
}