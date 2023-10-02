using TMPro;
using UnityEngine;

public class Bucket : MonoBehaviour
{
    public bool isYes;
    public Level level;

    public TextMeshProUGUI tmp;
    public RectTransform fill;
    public float maxFillHeight;

    private void Update()
    {
        if (isYes) fill.offsetMax = new Vector2(0, maxFillHeight * level.VictoryPercent);
    }
}