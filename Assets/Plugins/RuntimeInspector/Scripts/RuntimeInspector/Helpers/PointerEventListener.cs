using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeInspectorNamespace
{
    public class PointerEventListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public delegate void PointerEvent(PointerEventData eventData);

        public event PointerEvent PointerDown, PointerUp, PointerClick, BeginDrag, OnDrag, EndDrag;
        public Action pointerClickAction;

        public bool isPressedNow = false;

        private void Start()
        {
            isPressedNow = false;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            isPressedNow = true;
            if (PointerDown != null)
                PointerDown(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            isPressedNow = false;
            if (PointerUp != null)
                PointerUp(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (PointerClick != null)
                PointerClick(eventData);
            pointerClickAction?.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (OnDrag != null)
                OnDrag(eventData);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (BeginDrag != null)
                BeginDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (EndDrag != null)
                EndDrag(eventData);
        }
    }
}