using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Quball : MonoBehaviour
{
    public int stateCurrent;
    public int statePrevious;

    public Vector3 currentPosNoise;
    public Vector3 previousPosNoise;

    public float debugReal;
    public float debugImaginary;

    public float baseHue;
    public Vector3 baseScale;
    public Renderer renderer;

    public Complex Amplitude = Complex.One;

    private void Awake()
    {
        renderer = GetComponentInChildren<Renderer>();
        baseScale = transform.localScale;
        Set(stateCurrent, statePrevious, Amplitude);
    }

    public void Set(int newStateCurrent, int newStatePrevious, Complex newAmplitude)
    {
        stateCurrent = newStateCurrent;
        statePrevious = newStatePrevious;
        Amplitude = newAmplitude;

        var hue = baseHue + (float)Amplitude.Phase / (2 * Mathf.PI);
        renderer.material.color = Color.HSVToRGB(hue - Mathf.FloorToInt(hue), 1, 1);
        transform.localScale = baseScale * (float)Amplitude.Magnitude;

        debugReal = (float)Amplitude.Real;
        debugImaginary = (float)Amplitude.Imaginary;
    }
}