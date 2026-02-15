using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class GameObjectExtensions
    {
        public static List<GameObject> GetAllGameObjectsInSceneDeep(this Scene scene,
            List<string> excludeGameObjectsWithThisNames = null)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            var list = new List<GameObject>();

            for (var i = 0; i < rootGameObjects.Length; i++)
            {
                var objectName = rootGameObjects[i].name;
                if (excludeGameObjectsWithThisNames != null && excludeGameObjectsWithThisNames.Contains(objectName))
                    continue;

                Helpers.GameObject.GetAllChildGameObjectsDeep(rootGameObjects[i], ref list,
                    excludeGameObjectsWithThisNames);
            }

            return list;
        }

        public static List<GameObject> GetAllChildGameObjectsDeep(this GameObject gameObject,
            List<string> excludeGameObjectsWithThisNames = null)
        {
            var list = new List<GameObject>();
            Helpers.GameObject.GetAllChildGameObjectsDeep(gameObject, ref list, excludeGameObjectsWithThisNames);
            return list;
        }

        public static void ChangeLayerWithChildren(this GameObject obj, string layerName)
        {
            if (string.IsNullOrEmpty(layerName)) layerName = "Default";

            var layerIndex = LayerMask.NameToLayer(layerName);

            obj.layer = layerIndex;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(obj);
#endif
            List<Transform> allChilds = null;
            allChilds = obj.transform.GetAllChilds(ref allChilds);
            if (allChilds != null && allChilds.Count > 0)
                for (var i = 0; i < allChilds.Count; i++)
                {
                    allChilds[i].gameObject.layer = layerIndex;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        EditorUtility.SetDirty(allChilds[i].gameObject);
#endif
                }
        }
    }
}