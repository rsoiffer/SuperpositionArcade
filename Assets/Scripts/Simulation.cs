using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Simulation : MonoBehaviour
{
    public Quball quballPrefab;
    public float timeScale = 1;
    public AnimationCurve bounceCurve;

    public float time;
    public List<Quball> quballs;

    private void Update()
    {
        var timePrev = time;
        time += timeScale * Time.deltaTime;
        if (Mathf.FloorToInt(time) > Mathf.FloorToInt(timePrev))
        {
            Collapse();
            GateX(0);
        }

        var timeFrac = time - Mathf.FloorToInt(time);
        foreach (var q in quballs)
            q.transform.position = new Vector3(
                Mathf.Lerp(q.statePrevious, q.stateCurrent, timeFrac),
                -1 * (Mathf.FloorToInt(time) + bounceCurve.Evaluate(timeFrac)),
                0
            );
    }

    public void Collapse()
    {
        var stateDict = new Dictionary<int, Complex>();
        foreach (var q in quballs)
        {
            stateDict.TryAdd(q.stateCurrent, Complex.Zero);
            stateDict[q.stateCurrent] += q.amplitude;
            Destroy(q.gameObject);
        }

        quballs.Clear();

        foreach (var kvp in stateDict)
        {
            if (kvp.Value.Magnitude < 1e-3) continue;
            var newQuball = Instantiate(quballPrefab, transform);
            newQuball.stateCurrent = newQuball.statePrevious = kvp.Key;
            newQuball.amplitude = kvp.Value;
            quballs.Add(newQuball);
        }
    }

    public void GateX(int id)
    {
        foreach (var q in quballs)
        {
            q.statePrevious = q.stateCurrent;
            q.stateCurrent ^= 1 << id;
        }
    }
}