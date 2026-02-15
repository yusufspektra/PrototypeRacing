using System.Collections.Generic;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class GameObject
        {
            public static void GetAllChildGameObjectsDeep(UnityEngine.GameObject gameObject, ref List<UnityEngine.GameObject> listReference,
                List<string> excludeGameObjectsWithThisNames = null)
            {
                if (gameObject == null)
                    return;

                listReference.Add(gameObject);

                var childCount = gameObject.transform.childCount;
                if (childCount > 0)
                    for (var i = 0; i < childCount; i++)
                    {
                        var childGameObject = gameObject.transform.GetChild(i).gameObject;

                        var objectName = childGameObject.name;
                        if (excludeGameObjectsWithThisNames != null && excludeGameObjectsWithThisNames.Contains(objectName))
                            continue;

                        GetAllChildGameObjectsDeep(childGameObject, ref listReference, excludeGameObjectsWithThisNames);
                    }
            }
        }
    }
}