using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

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
                    case GateType.Z:
                        result = result.SelectMany(q => q.GateZ(dim)).ToList();
                        break;
                    case GateType.H:
                        result = result.SelectMany(q => q.GateH(dim)).ToList();
                        break;
                    case GateType.Control:
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

    public IEnumerable<QData> GateZ(int dim)
    {
        yield return new QData(State, Amplitude * (Bit(dim) ? -1 : 1));
    }

    public IEnumerable<QData> GateH(int dim)
    {
        yield return new QData(State ^ (1 << dim), Amplitude / Mathf.Sqrt(2));
        yield return new QData(State, Amplitude * (Bit(dim) ? -1 : 1) / Mathf.Sqrt(2));
    }
}