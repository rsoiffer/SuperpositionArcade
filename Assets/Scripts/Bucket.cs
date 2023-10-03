using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bucket : MonoBehaviour
{
    public bool isYes;
    public Level level;
    public float phase;

    public float baseHue;
    public int variant;
    [SerializeField] private List<GameObject> variants;
    public List<float> variantBaseHueOffsets;
    public RectTransform fill;
    public float maxFillHeight;
    public List<Image> imagesToColor;

    private void Update()
    {
        if (!isYes) return;
        if (fill != null)
            fill.offsetMax = new Vector2(0, maxFillHeight * level.VictoryPercent);
        var hue = baseHue + variantBaseHueOffsets[variant] + phase;
        foreach (var image in imagesToColor)
            image.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
        for (var i = 0; i < variants.Count; i++) variants[i].SetActive(i == variant);
    }
}