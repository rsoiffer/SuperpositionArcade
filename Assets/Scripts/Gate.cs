using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public enum GateType
{
    X,
    Z,
    H,
    Control
}

public class Gate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public Transform slot;
    public GateType type;
    public bool isDraggable = true;
    public Color notDraggableColor;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        slot = transform.parent;
        var gateSlot = slot.GetComponentInParent<GateSlot>();
        if (gateSlot != null && gateSlot.infiniteSource)
        {
            var copy = Instantiate(gameObject, transform.parent);
            copy.transform.SetSiblingIndex(transform.GetSiblingIndex());
            slot = null;
        }

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        // transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        if (slot == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        image.raycastTarget = true;
    }

    public void BlockDragging()
    {
        isDraggable = false;
        image.color = notDraggableColor;
    }
}