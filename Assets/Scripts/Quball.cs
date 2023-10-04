using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Quball : MonoBehaviour
{
    [SerializeField] private Vector2 noiseScale = new(.2f, 0);
    [SerializeField] private float scalePower = 1;
    [SerializeField] private float baseHue;
    [SerializeField] private List<GameObject> detachOnDestroy;
    [SerializeField] private float detachLifetime;
    [SerializeField] private List<Image> imagesToColor;
    [SerializeField] private List<Renderer> renderersToColor;
    [SerializeField] private List<GameObject> variants;
    [SerializeField] private List<float> variantBaseHueOffsets;

    private Vector3 baseScale;

    public QData current { get; private set; } = new(0, Complex.One);
    public QData previous { get; private set; }

    public Vector3 currentPosNoise { get; private set; }
    public Vector3 previousPosNoise { get; private set; }
    public int variant { get; set; }

    private void Awake()
    {
        baseScale = transform.localScale;
        previousPosNoise = noiseScale * Random.insideUnitCircle;
        currentPosNoise = noiseScale * Random.insideUnitCircle;
        UpdateGraphics();
    }

    public void Step()
    {
        previousPosNoise = currentPosNoise;
        currentPosNoise = noiseScale * Random.insideUnitCircle;
        previous = current;
        UpdateGraphics();
    }

    public void Set(QData newData)
    {
        current = newData;
        previous ??= current;
        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
        var hue = baseHue + variantBaseHueOffsets[variant] + (float)current.Amplitude.Phase / (2 * Mathf.PI);
        foreach (var image in imagesToColor)
            image.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
        foreach (var r in renderersToColor)
            r.material.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
        transform.localScale = baseScale * Mathf.Pow(current.Magnitude, scalePower);
        for (var i = 0; i < variants.Count; i++) variants[i].SetActive(i == variant);
    }

    public void DestroyQuball()
    {
        foreach (var d in detachOnDestroy)
        {
            d.transform.parent = null;
            Destroy(d, detachLifetime);
        }

        Destroy(gameObject);
    }
}