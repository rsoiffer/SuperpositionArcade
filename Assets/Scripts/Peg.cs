using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Peg : MonoBehaviour
{
    public int state;
    public int row;
    public Level level;

    public Image imageBase;
    public Image imageX;
    public Image imageZ;
    public Image imageH;

    public void UpdateGraphics()
    {
        var gates = level.Gates(row);

        imageBase.gameObject.SetActive(false);
        imageX.gameObject.SetActive(false);
        imageZ.gameObject.SetActive(false);
        imageH.gameObject.SetActive(false);

        if (gates.Any(g => g != null && g.type == GateType.H))
        {
            imageH.gameObject.SetActive(true);
            return;
        }

        if (gates.Any(g => g != null && g.type == GateType.X))
        {
            imageX.gameObject.SetActive(true);
            var (_, lastDim) = gates.Select((g, i) => (g, i)).Last(x => x.g != null && x.g.type == GateType.X);
            imageX.transform.localScale = new Vector3((state & (1 << lastDim)) != 0 ? -1 : 1, 1, 1);
            return;
        }

        if (gates.Select((g, i) => (g, i))
                .Count(x => x.g != null && x.g.type == GateType.Z && (state & (1 << x.i)) != 0) % 2 == 1)
        {
            imageZ.gameObject.SetActive(true);
            return;
        }

        imageBase.gameObject.SetActive(true);
    }
}