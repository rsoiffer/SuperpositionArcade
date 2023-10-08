using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class Peg : MonoBehaviour
{
    public int state;
    public int row;
    public Level level;

    public Image imageBase;
    public Image imageX;
    public Image imageY;
    public Image imageZ;
    public Image imageH;

    public LineRenderer arrowPrefab;
    public RectTransform arrowHighlightArea;
    public GameObject showOnHighlight;
    public float arrowLerpStart = .1f;
    public float arrowLerpEnd = .9f;
    private readonly List<LineRenderer> _currentArrows = new();

    public void Update()
    {
        var selected = arrowHighlightArea.rect.Contains(arrowHighlightArea.InverseTransformPoint(Input.mousePosition));
        showOnHighlight.SetActive(selected);
        foreach (var a in _currentArrows) Destroy(a.gameObject);
        _currentArrows.Clear();
        if (selected)
        {
            var qData = new QData(state, Complex.One);
            foreach (var q2 in qData.ApplyGateRow(level.Gates(row)))
            {
                var startPos = Vector3.Lerp(level.PegPos(state, row), level.PegPos(q2.State, row + 1), arrowLerpStart);
                var endPos = Vector3.Lerp(level.PegPos(state, row), level.PegPos(q2.State, row + 1), arrowLerpEnd);
                var direction = endPos - startPos;

                var newArrow = Instantiate(arrowPrefab, transform);
                newArrow.transform.position = endPos;
                newArrow.transform.Rotate(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                newArrow.SetPositions(new[] { startPos, endPos });
                newArrow.positionCount = 2;
                newArrow.GetComponent<PhaseColorer>().phase = q2.Phase;

                _currentArrows.Add(newArrow);
            }
        }
    }

    public void UpdateGraphics()
    {
        var gates = level.Gates(row);

        imageBase.gameObject.SetActive(false);
        imageX.gameObject.SetActive(false);
        imageY.gameObject.SetActive(false);
        imageZ.gameObject.SetActive(false);
        imageH.gameObject.SetActive(false);

        if (!level.CheckControls(state, row))
        {
            imageBase.gameObject.SetActive(true);
            return;
        }

        if (gates.Any(g => g != null && g.type is GateType.H or GateType.Measure or GateType.Reset or GateType.RandomX))
        {
            imageH.gameObject.SetActive(true);
            return;
        }

        if (gates.Any(g => g != null && g.type == GateType.Y))
        {
            imageY.gameObject.SetActive(true);
            var (_, lastDim) = gates.Select((g, i) => (g, i)).Last(x => x.g != null && x.g.type == GateType.Y);
            imageY.transform.localScale = new Vector3((state & (1 << lastDim)) != 0 ? -1 : 1, 1, 1);
            return;
        }

        if (gates.Any(g => g != null && g.type == GateType.X))
        {
            imageX.gameObject.SetActive(true);
            var newState = state;
            for (var dim = 0; dim < gates.Count; dim++)
                if (gates[dim] != null && gates[dim].type == GateType.X)
                    newState ^= 1 << dim;
            imageX.transform.localScale = Mathf.Pow(Mathf.Abs(state - newState), .25f) *
                                          new Vector3(newState < state ? -1 : 1, 1, 1);
            return;
        }

        if (gates.Select((g, i) => (g, i))
                .Count(x => x.g != null && x.g.type == GateType.Z && (state & (1 << x.i)) != 0) % 2 == 1)
        {
            imageZ.gameObject.SetActive(true);
            return;
        }

        if (gates.Select((g, i) => (g, i))
                .Count(x => x.g != null && x.g.type is GateType.S or GateType.Sa or GateType.T or GateType.Ta &&
                            (state & (1 << x.i)) != 0) % 2 == 1)
        {
            imageZ.gameObject.SetActive(true);
            return;
        }

        imageBase.gameObject.SetActive(true);
    }
}