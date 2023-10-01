using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class State : MonoBehaviour
{
    public Quball quballPrefab;
    public float timeScale = 1;
    public AnimationCurve bounceCurve;
    public AnimationCurve bounceCurveSide;
    public Level level;
    public float noiseScale = .1f;

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
            foreach (var q in quballs)
            {
                q.Set(q.stateCurrent, q.stateCurrent, q.Amplitude);
                q.previousPosNoise = q.currentPosNoise;
                q.currentPosNoise = noiseScale * Random.insideUnitCircle;
            }

            var rowId = Mathf.FloorToInt(time) - 1;
            if (rowId < level.numRows)
                for (var i = 0; i < level.numBits; i++)
                {
                    var gate = level.gateGrid[i, rowId];
                    if (gate != null)
                        switch (gate.type)
                        {
                            case GateType.X:
                                GateX(i);
                                break;
                            case GateType.Z:
                                GateZ(i);
                                break;
                            case GateType.H:
                                GateH(i);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                }
            else
                Destroy(gameObject);
        }

        var timeFrac = time - Mathf.FloorToInt(time);
        foreach (var q in quballs)
        {
            var rowId = Mathf.FloorToInt(time);
            var currentPos = level.PegPos(q.stateCurrent, rowId) + q.currentPosNoise;
            var previousPos = level.PegPos(q.statePrevious, rowId - 1) + q.previousPosNoise;
            q.transform.position = new Vector3(
                Mathf.LerpUnclamped(previousPos.x, currentPos.x, bounceCurveSide.Evaluate(timeFrac)),
                Mathf.LerpUnclamped(previousPos.y, currentPos.y, bounceCurve.Evaluate(timeFrac)),
                0
            );
        }
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
                totalAmplitude += q.Amplitude;
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
        foreach (var q in quballs) q.Set(q.stateCurrent ^ (1 << id), q.statePrevious, q.Amplitude);
    }

    public void GateZ(int id)
    {
        foreach (var q in quballs)
            q.Set(q.stateCurrent, q.statePrevious, q.Amplitude * ((q.stateCurrent & (1 << id)) == 0 ? 1 : -1));
    }

    public void GateH(int id)
    {
        foreach (var q in quballs.ToList())
        {
            var newQuball = Instantiate(quballPrefab, transform);
            newQuball.Set(q.stateCurrent ^ (1 << id), q.statePrevious, q.Amplitude / Mathf.Sqrt(2));
            quballs.Add(newQuball);
            q.Set(q.stateCurrent, q.stateCurrent,
                q.Amplitude * ((q.stateCurrent & (1 << id)) == 0 ? 1 : -1) / Mathf.Sqrt(2));
        }
    }
}