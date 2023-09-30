using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Quball : MonoBehaviour
{
    public int stateCurrent;
    public int statePrevious;

    private float _baseHue;
    private Vector3 _baseScale;

    private Renderer _renderer;

    public Complex amplitude = Complex.One;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        Color.RGBToHSV(_renderer.material.color, out var h, out var s, out var v);
        _baseHue = h;
        _baseScale = transform.localScale;
    }

    public void Set(int newStateCurrent, int newStatePrevious, Complex newAmplitude)
    {
        stateCurrent = newStateCurrent;
        statePrevious = newStatePrevious;
        amplitude = newAmplitude;

        var hue = _baseHue + (float)amplitude.Phase / (2 * Mathf.PI);
        _renderer.material.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
        transform.localScale = _baseScale * (float)amplitude.Magnitude;
    }
}