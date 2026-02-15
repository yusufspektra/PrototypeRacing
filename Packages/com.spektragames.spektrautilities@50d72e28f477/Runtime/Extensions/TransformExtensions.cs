using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Transform = UnityEngine.Transform;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class TransformExtensions
    {
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            if (aParent != null)
            {
                var result = aParent.Find(aName);
                if (result != null)
                    return result;

                foreach (Transform child in aParent)
                {
                    result = child.FindDeepChild(aName);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        public static T FindDeepChild<T>(this Transform aParent, string aName) where T : Object
        {
            var result = default(T);

            var transform = aParent.FindDeepChild(aName);

            if (transform != null)
                result = typeof(T) == typeof(GameObject)
                    ? (T)Convert.ChangeType(transform.gameObject, typeof(T))
                    : transform.GetComponent<T>();

            if (result == null)
                Debug.LogError($"FindDeepChild didn't find: '{aName}' on GameObject: '{aParent.name}'",
                    aParent.gameObject);

            return result;
        }

        public static void DestroyAllChildren(this Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--) Object.Destroy(parent.GetChild(i).gameObject);
        }

        public static void SetSizeDeltaX(this RectTransform transform, float value)
        {
            Vector3 v = transform.sizeDelta;
            v.x = value;
            transform.sizeDelta = v;
        }

        public static void SetSizeDeltaY(this RectTransform transform, float value)
        {
            Vector3 v = transform.sizeDelta;
            v.y = value;
            transform.sizeDelta = v;
        }

        public static void SetAnchorPosX(this RectTransform transform, float value)
        {
            Vector3 v = transform.anchoredPosition;
            v.x = value;
            transform.anchoredPosition = v;
        }

        public static void SetAnchorPosY(this RectTransform transform, float value)
        {
            Vector3 v = transform.anchoredPosition;
            v.y = value;
            transform.anchoredPosition = v;
        }

        public static void SetPivotPosX(this RectTransform transform, float value)
        {
            var v = transform.pivot;
            v.x = value;
            transform.pivot = v;
        }

        public static void SetPivotPosY(this RectTransform transform, float value)
        {
            var v = transform.pivot;
            v.y = value;
            transform.pivot = v;
        }

        public static List<Transform> GetAllChilds(this Transform parent, ref List<Transform> result)
        {
            if (result == null)
                result = new List<Transform>();

            var childCount = parent.childCount;
            if (childCount > 0)
                for (var i = 0; i < childCount; i++)
                {
                    result.Add(parent.GetChild(i));
                    if (parent.GetChild(i).childCount > 0)
                        GetAllChilds(parent.GetChild(i), ref result);
                }

            return result;
        }

        public static void SetLocalEulerAnglesX(this Transform transform, float value)
        {
            var v = transform.localEulerAngles;
            v.x = value;
            transform.localEulerAngles = v;
        }

        public static void SetLocalEulerAnglesY(this Transform transform, float value)
        {
            var v = transform.localEulerAngles;
            v.y = value;
            transform.localEulerAngles = v;
        }

        public static void SetLocalEulerAnglesZ(this Transform transform, float value)
        {
            var v = transform.localEulerAngles;
            v.z = value;
            transform.localEulerAngles = v;
        }

        public static void SetLRTB(this RectTransform rt, Vector4 poss)
        {
            SetLeft(rt, poss.x);
            SetRight(rt, poss.y);
            SetTop(rt, poss.z);
            SetBottom(rt, poss.w);
        }

        public static Vector4 GetLRTB(this RectTransform rt)
        {
            var result = Vector4.zero;
            result.x = rt.offsetMin.x;
            result.y = -rt.offsetMax.x;
            result.z = -rt.offsetMax.y;
            result.w = rt.offsetMin.y;
            return result;
        }

        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
        {
            if (rectTransform == null) return;

            var size = rectTransform.rect.size;
            var deltaPivot = rectTransform.pivot - pivot;
            var deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }

        /// <summary>
        ///     Set pivot without changing the position of the element
        /// </summary>
        public static void SetPivotWithoutChangingPosition(this RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot; // get change in pivot
            deltaPosition.Scale(rectTransform.rect.size); // apply sizing
            deltaPosition.Scale(rectTransform.localScale); // apply scaling
            deltaPosition = rectTransform.rotation * deltaPosition; // apply rotation

            rectTransform.pivot = pivot; // change the pivot
            rectTransform.localPosition -= deltaPosition; // reverse the position change
        }

        public static Quaternion InverseTransformRotation(this Transform targetTransform, Quaternion worldRotation)
        {
            var localRotation = Quaternion.Inverse(targetTransform.rotation) * worldRotation;
            return localRotation;
        }

        public static Quaternion TransformRotation(this Transform targetTransform, Quaternion localRotation)
        {
            var worldRotation = targetTransform.rotation * localRotation;
            return worldRotation;
        }
    }
}