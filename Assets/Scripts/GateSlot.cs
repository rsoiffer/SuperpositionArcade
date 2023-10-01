using UnityEngine;
using UnityEngine.EventSystems;

public class GateSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (GetComponentInChildren<Gate>() != null) return;
        var dropped = eventData.pointerDrag;
        var gate = dropped.GetComponent<Gate>();
        gate.slot = transform;
    }
}