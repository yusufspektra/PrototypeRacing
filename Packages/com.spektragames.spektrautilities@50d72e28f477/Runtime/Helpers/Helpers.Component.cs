using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class Component
        {
            public static T FindComponentInScene<T>(string path) where T : UnityEngine.Component
        {
            //For example for path : MainCamera/CameraPivot
            if (string.IsNullOrEmpty(path))
                throw new Exception("The path is null");

            path = path.Replace("\\", "/");

            var allRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var pathArray = new Queue<string>();

            if (path.Contains("/"))
            {
                var paths = path.Split('/');
                for (var i = 0; i < paths.Length; i++)
                    if (!string.IsNullOrEmpty(paths[i]))
                        pathArray.Enqueue(paths[i]);
            }
            else
            {
                pathArray.Enqueue(path);
            }

            if (pathArray.Count <= 0)
                throw new Exception("The path is corrupt");

            UnityEngine.Transform founded = null;

            while (pathArray.Count > 0)
            {
                var nextPath = pathArray.Dequeue();
                var foundedPath = false;

                if (founded == null)
                {
                    for (var i = 0; i < allRootObjects.Length; i++)
                        if (allRootObjects[i].name == nextPath)
                        {
                            founded = allRootObjects[i].transform;
                            foundedPath = true;
                            break;
                        }

                    if (!foundedPath)
                        throw new Exception("The path is corrupt");
                }
                else
                {
                    var childCount = founded.childCount;
                    for (var i = 0; i < childCount; i++)
                        if (founded.GetChild(i).gameObject.name == nextPath)
                        {
                            foundedPath = true;
                            founded = founded.GetChild(i).transform;
                            break;
                        }

                    if (!foundedPath)
                        throw new Exception("The path is corrupt");
                }
            }

            if (founded == null)
                throw new Exception("The object is not founded");
            return founded.GetComponent<T>();
        }
        }
    }
}