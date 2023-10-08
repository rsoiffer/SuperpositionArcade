using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public enum GateType
{
    X = 0,
    Y = 1,
    Z = 2,
    S = 3,
    Sa = 10,
    T = 4,
    Ta = 11,
    H = 5,
    Control = 6,
    Measure = 7,
    Reset = 8,
    RandomX = 9
}

public class Gate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public Transform slot;
    public GateType type;
    public bool isDraggable = true;
    public Color notDraggableColor;
    public string code;

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
        SoundManager.Click1();
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
            SoundManager.Click2();
            Destroy(gameObject);
            return;
        }

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        image.raycastTarget = true;
        SoundManager.Click2();
    }

    public void BlockDragging()
    {
        isDraggable = false;
        image.color = notDraggableColor;
    }
}