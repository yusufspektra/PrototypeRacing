using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static RuntimeInspectorNamespace.PointerEventListener;

namespace RuntimeInspectorNamespace
{
    public abstract class DragableWindow : SkinnedWindow
    {
        [Header("Dragable Window")]
        [SerializeField] private PointerEventListener pointEventListener;

        private RectTransform dragAreaRectTransform = null;
        private RectTransform DragAreaRectTransform
        {
            get
            {
                if (dragAreaRectTransform == null)
                {
                    dragAreaRectTransform = pointEventListener.GetComponent<RectTransform>();
                }

                return dragAreaRectTransform;
            }
        }

        private RectTransform mainRectTransform = null;
        private RectTransform MainRectTransform
        {
            get
            {
                if (mainRectTransform == null)
                {
                    mainRectTransform = gameObject.GetComponent<RectTransform>();
                }

                return mainRectTransform;
            }
        }

        private Canvas canvas = null;
        private Canvas Canvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = gameObject.GetComponentInParent<Canvas>();
                }

                return canvas;
            }
        }

        private Vector2 offset;

        protected override void OnEnable()
        {
            base.OnEnable();

            pointEventListener.BeginDrag += BeginDrag;
            pointEventListener.OnDrag += OnDrag;
            pointEventListener.EndDrag += EndDrag;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            pointEventListener.BeginDrag -= BeginDrag;
            pointEventListener.OnDrag -= OnDrag;
            pointEventListener.EndDrag -= EndDrag;
        }

        private void BeginDrag(PointerEventData eventData)
        {
            Vector2 pointerPosition = eventData.position;
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(MainRectTransform, pointerPosition, eventData.pressEventCamera, out localPointerPosition))
            {
                offset = localPointerPosition;
            }
        }

        private void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Canvas.transform as RectTransform, eventData.position,
                canvas.worldCamera, out localPoint);

            MainRectTransform.anchoredPosition = localPoint - offset;
        }

        private void EndDrag(PointerEventData eventData)
        {

        }
    }
}