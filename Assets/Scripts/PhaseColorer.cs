using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseColorer : MonoBehaviour
{
    private static readonly float baseHue = .55f;
    private static readonly float[] variantBaseHueOffsets = { 0, -.06f, -0.12f, .06f, .12f, .18f, -.18f, -.24f };

    [SerializeField] private List<Image> imagesToColor;
    [SerializeField] private List<Renderer> renderersToColor;

    public int variant;
    public float phase;

    private void LateUpdate()
    {
        var hue = baseHue + variantBaseHueOffsets[variant] + phase;
        foreach (var image in imagesToColor)
            image.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
        foreach (var r in renderersToColor)
            r.material.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
    }
}