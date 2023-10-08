using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;

public class QData
{
    public QData(int state, Complex amplitude)
    {
        State = state;
        Amplitude = amplitude;
    }

    public int State { get; }
    public Complex Amplitude { get; }

    public float Magnitude => (float)Amplitude.Magnitude;
    public float Phase => (float)Amplitude.Phase / (2 * Mathf.PI);

    public float Probability => Magnitude * Magnitude;

    public bool Bit(int i)
    {
        return (State & (1 << i)) != 0;
    }

    public Complex Dot(QData other)
    {
        return State != other.State ? Complex.Zero : Amplitude * Complex.Conjugate(other.Amplitude);
    }

    public List<QData> ApplyGateRow(List<Gate> gates)
    {
        if (gates.Select((g, i) => (g, i))
            .Any(x => x.g != null && x.g.type == GateType.Control && !Bit(x.i)))
            return new List<QData> { this };

        var result = new List<QData> { this };
        for (var dim = 0; dim < gates.Count; dim++)
            if (gates[dim] != null)
                switch (gates[dim].type)
                {
                    case GateType.X:
                        result = result.SelectMany(q => q.GateX(dim)).ToList();
                        break;
                    case GateType.Y:
                        result = result.SelectMany(q => q.GateY(dim)).ToList();
                        break;
                    case GateType.Z:
                        result = result.SelectMany(q => q.GateZ(dim)).ToList();
                        break;
                    case GateType.S:
                        result = result.SelectMany(q => q.GateS(dim)).ToList();
                        break;
                    case GateType.Sa:
                        result = result.SelectMany(q => q.GateSa(dim)).ToList();
                        break;
                    case GateType.T:
                        result = result.SelectMany(q => q.GateT(dim)).ToList();
                        break;
                    case GateType.Ta:
                        result = result.SelectMany(q => q.GateTa(dim)).ToList();
                        break;
                    case GateType.H:
                        result = result.SelectMany(q => q.GateH(dim)).ToList();
                        break;
                    case GateType.Control:
                        break;
                    case GateType.Measure:
                        break;
                    case GateType.Reset:
                        result = result.Select(q => new QData(q.State & (-1 - (1 << dim)), q.Amplitude)).ToList();
                        break;
                    case GateType.RandomX:
                        if (Random.value < .5f) result = result.SelectMany(q => q.GateX(dim)).ToList();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

        return result;
    }

    public IEnumerable<QData> GateX(int dim)
    {
        yield return new QData(State ^ (1 << dim), Amplitude);
    }

    public IEnumerable<QData> GateY(int dim)
    {
        yield return new QData(State ^ (1 << dim),
            Amplitude * (Bit(dim) ? -Complex.ImaginaryOne : Complex.ImaginaryOne));
    }

    public IEnumerable<QData> GateZ(int dim)
    {
        yield return new QData(State, Amplitude * (Bit(dim) ? -1 : 1));
    }

    public IEnumerable<QData> GateS(int dim)
    {
        yield return new QData(State, Amplitude * (Bit(dim) ? Complex.ImaginaryOne : 1));
    }

    public IEnumerable<QData> GateSa(int dim)
    {
        yield return new QData(State, Amplitude * (Bit(dim) ? -Complex.ImaginaryOne : 1));
    }

    public IEnumerable<QData> GateT(int dim)
    {
        yield return new QData(State, Amplitude * (Bit(dim) ? Complex.Exp(Complex.ImaginaryOne * Mathf.PI / 4) : 1));
    }

    public IEnumerable<QData> GateTa(int dim)
    {
        yield return new QData(State, Amplitude * (Bit(dim) ? Complex.Exp(-Complex.ImaginaryOne * Mathf.PI / 4) : 1));
    }

    public IEnumerable<QData> GateH(int dim)
    {
        yield return new QData(State ^ (1 << dim), Amplitude / Mathf.Sqrt(2));
        yield return new QData(State, Amplitude * (Bit(dim) ? -1 : 1) / Mathf.Sqrt(2));
    }
}