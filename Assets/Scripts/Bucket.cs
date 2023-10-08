using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour
{
    public bool isYes;
    public Level level;

    public int variant;
    [SerializeField] private List<GameObject> variants;
    public RectTransform fill;
    public float maxFillHeight;

    private void Update()
    {
        if (!isYes) return;
        if (fill != null)
            fill.offsetMax = new Vector2(0, maxFillHeight * level.VictoryPercent);
        for (var i = 0; i < variants.Count; i++) variants[i].SetActive(i == variant % variants.Count);
    }
}