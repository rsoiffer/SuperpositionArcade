using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class State : MonoBehaviour
{
    public Quball quballPrefab;
    public AnimationCurve bounceCurve;
    public AnimationCurve bounceCurveSide;
    public Level level;
    public float collapsePreempt = .2f;

    public int variant;
    public float time;
    public List<Quball> quballs;

    private void Update()
    {
        quballs.RemoveAll(q => q == null);

        var timePrev = time;
        time += level.timeScale * Time.deltaTime;

        if (Mathf.FloorToInt(time + collapsePreempt) > Mathf.FloorToInt(timePrev + collapsePreempt)) Collapse();

        if (Mathf.FloorToInt(time) > Mathf.FloorToInt(timePrev)) Step();
    }

    private void LateUpdate()
    {
        quballs.RemoveAll(q => q == null);

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

    public void ResetToState(int state)
    {
        quballs.RemoveAll(q => q == null);
        foreach (var q in quballs)
            q.DestroyQuball();

        var newQuball = Instantiate(quballPrefab, transform);
        newQuball.Set(state, Complex.One);
        quballs.Add(newQuball);
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
                q.DestroyQuball();
            }

            if (totalAmplitude.Magnitude < 1e-3) continue;
            var newQuball = Instantiate(quballPrefab, transform);
            newQuball.Set(state, totalAmplitude);
            quballs.Add(newQuball);
        }

        quballs.RemoveAll(q => q == null);
    }

    public void Step()
    {
        foreach (var q in quballs) q.Step();

        var rowId = Mathf.FloorToInt(time) - 1;
        if (rowId < level.numRows)
        {
            var gates = level.Gates(rowId);
            for (var dim = 0; dim < level.NumBits; dim++)
                if (gates[dim] != null)
                    switch (gates[dim].type)
                    {
                        case GateType.X:
                            GateX(dim);
                            break;
                        case GateType.Z:
                            GateZ(dim);
                            break;
                        case GateType.H:
                            GateH(dim);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
        }
        else
        {
            level.StateHitBottom(this);
            foreach (var q in quballs) q.DestroyQuball();
            Destroy(gameObject);
        }
    }

    public void GateX(int dim)
    {
        foreach (var q in quballs) q.Set(q.stateCurrent ^ (1 << dim), q.Amplitude);
    }

    public void GateZ(int dim)
    {
        foreach (var q in quballs)
            q.Set(q.stateCurrent, q.Amplitude * (q.Bit(dim) ? -1 : 1));
    }

    public void GateH(int dim)
    {
        foreach (var q in quballs.ToList())
        {
            var newQuball = Instantiate(quballPrefab, transform);
            newQuball.Set(q.statePrevious, q.Amplitude / Mathf.Sqrt(2));
            newQuball.Set(q.stateCurrent ^ (1 << dim), q.Amplitude / Mathf.Sqrt(2));
            quballs.Add(newQuball);
            q.Set(q.stateCurrent, (q.Bit(dim) ? -1 : 1) * q.Amplitude / Mathf.Sqrt(2));
        }
    }
}