using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Quball : MonoBehaviour
{
    [SerializeField] private Vector2 noiseScale = new(.2f, 0);
    [SerializeField] private float scalePower = 1;
    [SerializeField] private float baseHue;
    [SerializeField] private List<GameObject> detachOnDestroy;
    [SerializeField] private float detachLifetime;

    private Vector3 baseScale;
    private Renderer[] renderers;

    public int stateCurrent { get; private set; }
    public Complex Amplitude { get; private set; } = Complex.One;

    public int statePrevious { get; private set; } = -1;
    public Vector3 currentPosNoise { get; private set; }
    public Vector3 previousPosNoise { get; private set; }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        baseScale = transform.localScale;
        previousPosNoise = noiseScale * Random.insideUnitCircle;
        currentPosNoise = noiseScale * Random.insideUnitCircle;
        UpdateGraphics();
    }

    public bool Bit(int i)
    {
        return (stateCurrent & (1 << i)) != 0;
    }

    public void Step()
    {
        previousPosNoise = currentPosNoise;
        currentPosNoise = noiseScale * Random.insideUnitCircle;
        statePrevious = stateCurrent;
        UpdateGraphics();
    }

    public void Set(int newState, Complex newAmplitude)
    {
        stateCurrent = newState;
        Amplitude = newAmplitude;
        if (statePrevious == -1) statePrevious = newState;
        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
        var hue = baseHue + (float)Amplitude.Phase / (2 * Mathf.PI);
        foreach (var r in renderers)
            r.material.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
        transform.localScale = baseScale * Mathf.Pow((float)Amplitude.Magnitude, scalePower);
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