using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Quball : MonoBehaviour
{
    [SerializeField] private Vector2 noiseScale = new(.2f, 0);
    [SerializeField] private float scalePower = 1;
    [SerializeField] private List<GameObject> detachOnDestroy;
    [SerializeField] private float detachLifetime;
    [SerializeField] private List<GameObject> variants;
    [SerializeField] private PhaseColorer phaseColorer;

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
        phaseColorer.variant = variant;
        phaseColorer.phase = current.Phase;
        transform.localScale = baseScale * Mathf.Pow(current.Magnitude, scalePower);
        for (var i = 0; i < variants.Count; i++) variants[i].SetActive(i == variant % variants.Count);
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