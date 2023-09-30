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
    public AnimationCurve bounceCurveSide;

    public float time;
    public List<Quball> quballs;

    private void Update()
    {
        quballs.RemoveAll(q => q == null);

        var timePrev = time;
        time += timeScale * Time.deltaTime;

        if (Mathf.FloorToInt(time + .3f) > Mathf.FloorToInt(timePrev + .3f)) Collapse();

        if (Mathf.FloorToInt(time) > Mathf.FloorToInt(timePrev))
        {
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
                Mathf.Lerp(q.statePrevious, q.stateCurrent, bounceCurveSide.Evaluate(timeFrac)),
                -1 * (Mathf.FloorToInt(time) + bounceCurve.Evaluate(timeFrac)),
                0
            );
    }

    public void Collapse()
    {
        foreach (var state in quballs.Select(q => q.stateCurrent).ToHashSet())
        {
            var qs = quballs.Where(q => q.stateCurrent == state).ToList();
            if (qs.Count <= 1) continue;

            var totalAmplitude = Complex.Zero;
            foreach (var q in qs)
            {
                totalAmplitude += q.amplitude;
                Destroy(q.gameObject);
            }

            if (totalAmplitude.Magnitude < 1e-3) continue;
            var newQuball = Instantiate(quballPrefab, transform);
            newQuball.Set(state, state, totalAmplitude);
            quballs.Add(newQuball);
        }

        quballs.RemoveAll(q => q == null);
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