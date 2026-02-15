using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        public static List<T> FindChildObjectsOfType<T>(this Component thisComponent) where T : Component
        {
            var parent = thisComponent.transform;
            List<Transform> allChilds = null;
            allChilds = parent.GetAllChilds(ref allChilds);

            var result = new List<T>();
            for (var i = 0; i < allChilds.Count; i++)
            {
                var component = allChilds[i].GetComponent<T>();
                if (component != null) result.Add(component);
            }

            return result;
        }

        public static GameObject GetFirstParentWithComponent<T>(this GameObject gameObject)
        {
            GameObject result = null;
            var tempGameObject = gameObject.transform.parent.gameObject;
            while (result == null && tempGameObject != null)
                if (tempGameObject.GetComponent<T>() != null)
                    result = tempGameObject;
                else
                    try
                    {
                        tempGameObject = tempGameObject.transform.parent.gameObject;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("!!!", gameObject);

                        if (tempGameObject != null)
                            Debug.LogError(tempGameObject.name);

                        if (tempGameObject == null)
                            Debug.LogError("aaa");
                        else if (tempGameObject.transform.parent == null)
                            Debug.LogError("bbb");

                        throw e;
                    }

            return result;
        }

        public static void DestroyAllInstancedMaterials(this Renderer renderer)
        {
            if (renderer == null || !Application.isPlaying)
                return;

            for (var i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] != null &&
                    renderer.sharedMaterials[i].GetInstanceID() < 0)
                {
                    GameObject.Destroy(renderer.sharedMaterials[i]);
                    renderer.sharedMaterials[i] = null;
                }
            }
        }
    }
}