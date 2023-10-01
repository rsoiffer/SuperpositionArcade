using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public enum GateType
{
    X,
    Z,
    H
}

public class Gate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public Transform slot;
    public GateType type;

    public void OnBeginDrag(PointerEventData eventData)
    {
        slot = transform.parent;
        var gateSlot = slot.GetComponentInParent<GateSlot>();
        if (gateSlot != null && gateSlot.infiniteSource)
        {
            var copy = Instantiate(gameObject, transform.parent);
            copy.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        image.raycastTarget = true;
    }
}