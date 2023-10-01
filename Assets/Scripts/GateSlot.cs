using UnityEngine;
using UnityEngine.EventSystems;

public class GateSlot : MonoBehaviour, IDropHandler
{
    public bool infiniteSource;

    public void OnDrop(PointerEventData eventData)
    {
        if (!infiniteSource && GetComponentInChildren<Gate>() != null) return;
        var dropped = eventData.pointerDrag;
        var gate = dropped.GetComponent<Gate>();
        gate.slot = transform;
        if (infiniteSource) Destroy(gate.gameObject);
    }
}