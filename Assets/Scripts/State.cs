using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class State : MonoBehaviour
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
            var randomGate = Random.Range(0, 3);
            switch (randomGate)
            {
                case 0:
                    GateX(0);
                    break;
                case 1:
                    GateZ(0);
                    break;
                case 2:
                    GateH(0);
                    break;
            }
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
            newQuball.Set(kvp.Key, kvp.Key, kvp.Value);
            quballs.Add(newQuball);
        }
    }

    public void GateX(int id)
    {
        foreach (var q in quballs) q.Set(q.stateCurrent ^ (1 << id), q.stateCurrent, q.amplitude);
    }

    public void GateZ(int id)
    {
        foreach (var q in quballs)
            q.Set(q.stateCurrent, q.stateCurrent, q.amplitude * ((q.stateCurrent & (1 << id)) == 0 ? 1 : -1));
    }

    public void GateH(int id)
    {
        foreach (var q in quballs.ToList())
        {
            var newQuball = Instantiate(quballPrefab, transform);
            newQuball.Set(q.stateCurrent ^ (1 << id), q.stateCurrent, q.amplitude / Mathf.Sqrt(2));
            quballs.Add(newQuball);
            q.Set(q.stateCurrent, q.stateCurrent,
                q.amplitude * ((q.stateCurrent & (1 << id)) == 0 ? 1 : -1) / Mathf.Sqrt(2));
        }
    }
}