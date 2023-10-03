using UnityEngine;
using UnityEngine.EventSystems;

public class GateSlot : MonoBehaviour, IDropHandler
{
    public bool infiniteSource;
    public bool acceptsGateDrops = true;

    public void OnDrop(PointerEventData eventData)
    {
        if (!acceptsGateDrops) return;
        if (!infiniteSource && GetComponentInChildren<Gate>() != null) return;
        var dropped = eventData.pointerDrag;
        var gate = dropped.GetComponent<Gate>();
        if (gate == null) return;
        gate.slot = transform;
        if (infiniteSource) Destroy(gate.gameObject);
    }

    public void BlockDragging()
    {
        acceptsGateDrops = false;
        foreach (Transform t in transform) t.gameObject.SetActive(false);
    }
}