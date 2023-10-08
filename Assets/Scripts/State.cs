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
    public GameObject explosionPrefab;
    public GameObject successPrefab;
    public GameObject bouncePrefab;

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
            var currentPos = level.PegPos(q.current.State, rowId) + q.currentPosNoise;
            var previousPos = level.PegPos(q.previous.State, rowId - 1) + q.previousPosNoise;
            q.transform.position = new Vector3(
                Mathf.LerpUnclamped(previousPos.x, currentPos.x, bounceCurveSide.Evaluate(timeFrac)),
                Mathf.LerpUnclamped(previousPos.y, currentPos.y, bounceCurve.Evaluate(timeFrac)),
                q.transform.position.z
            );
        }
    }

    private Quball NewQuball()
    {
        var newQuball = Instantiate(quballPrefab, transform);
        newQuball.variant = variant;
        quballs.Add(newQuball);
        return newQuball;
    }

    public void ResetToState(int state)
    {
        quballs.RemoveAll(q => q == null);
        foreach (var q in quballs)
            q.DestroyQuball();

        var newQuball = NewQuball();
        newQuball.Set(new QData(state, Complex.One));
    }

    public void Collapse()
    {
        foreach (var state in quballs.Select(q => q.current.State).ToHashSet())
        {
            var qs = quballs.Where(q => q.current.State == state).ToList();
            if (qs.Count <= 1) continue;

            var totalAmplitude = Complex.Zero;
            foreach (var q in qs)
            {
                totalAmplitude += q.current.Amplitude;
                q.DestroyQuball();
            }

            if (totalAmplitude.Magnitude < 1e-3)
            {
                var newExplosion = Instantiate(explosionPrefab);
                newExplosion.transform.position = qs[0].transform.position;
                SoundManager.Explosion();
                continue;
            }

            var newQuball = NewQuball();
            newQuball.Set(new QData(state, totalAmplitude));
        }

        quballs.RemoveAll(q => q == null);
    }

    public void Step()
    {
        foreach (var q in quballs) q.Step();

        var row = Mathf.FloorToInt(time) - 1;
        if (row < level.NumRows)
        {
            var gates = level.Gates(row);

            for (var dim = 0; dim < gates.Count; dim++)
            {
                if (gates[dim] == null) continue;
                if (gates[dim].type != GateType.Measure && gates[dim].type != GateType.Reset) continue;
                var prob1 = quballs.Sum(q => q.current.Bit(dim) ? q.current.Probability : 0);
                var measure1 = Random.value < prob1;
                foreach (var q in quballs.ToList())
                    if (q.current.Bit(dim) != measure1)
                    {
                        q.DestroyQuball();
                        var newExplosion = Instantiate(explosionPrefab);
                        newExplosion.transform.position = q.transform.position;
                        SoundManager.Explosion();
                    }
                    else
                    {
                        q.Set(new QData(q.current.State,
                            q.current.Amplitude / Mathf.Sqrt(measure1 ? prob1 : 1 - prob1)));
                    }
            }

            foreach (var q in quballs.ToList())
            {
                var newQData = q.current.ApplyGateRow(gates);
                q.Set(newQData.First());
                for (var i = 1; i < newQData.Count; i++)
                {
                    var newQuball = NewQuball();
                    newQuball.Set(q.previous);
                    newQuball.Set(newQData[i]);
                }

                var newBounce = Instantiate(bouncePrefab);
                newBounce.transform.position = q.transform.position;
            }

            SoundManager.Impact4();
        }
        else
        {
            level.StateHitBottom(this);
            foreach (var q in quballs)
            {
                q.DestroyQuball();
                if (level.QuballValid(q))
                {
                    SoundManager.Coin1();
                    var newSuccess = Instantiate(successPrefab);
                    newSuccess.transform.position = q.transform.position;
                }
                else
                {
                    var newExplosion = Instantiate(explosionPrefab);
                    newExplosion.transform.position = q.transform.position;
                    SoundManager.Explosion();
                }
            }

            Destroy(gameObject);
        }
    }

    public void Heat()
    {
        foreach (var q in quballs)
        {
            var newExplosion = Instantiate(explosionPrefab);
            newExplosion.transform.position = q.transform.position;
        }

        SoundManager.Explosion();
        Destroy(gameObject);
    }
}