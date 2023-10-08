using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelDefinition : MonoBehaviour
{
    [SerializeField] private LevelDefinitionList ldl;
    [SerializeField] private int numBits;
    [SerializeField] private int numRows = 4;
    [TextArea(1, 10)] [SerializeField] private string defGoals;
    [TextArea(1, 10)] [SerializeField] private string defPlaceable;
    [TextArea(1, 10)] [SerializeField] private string defBefore;
    [TextArea(1, 10)] [SerializeField] private string defAfter;
    [TextArea(1, 10)] [SerializeField] private string defSolution;

    [NonSerialized] public List<Gate> GatesAfter;
    [NonSerialized] public List<Gate> GatesBefore;
    [NonSerialized] public List<Gate> GatesPlaceable;
    [NonSerialized] public List<Gate> GatesSolution;
    [NonSerialized] public List<float> GoalPhases;
    [NonSerialized] public List<int> GoalStates;
    [NonSerialized] public List<int> StartStates;

    public int NumBits => numBits;
    public int NumRows => numRows;
    public string LevelName => name;

    public void Parse()
    {
        StartStates = new List<int>();
        GoalStates = new List<int>();
        GoalPhases = new List<float>();
        GatesBefore = new List<Gate>();
        GatesAfter = new List<Gate>();
        GatesPlaceable = new List<Gate>();
        GatesSolution = new List<Gate>();

        foreach (var variant in defGoals.Split("\n"))
        {
            var parts = variant.Split(" ");
            StartStates.Add(int.Parse(parts[0]));
            GoalStates.Add(int.Parse(parts[1]));
            GoalPhases.Add(parts.Length < 3 ? 0 : float.Parse(parts[2]));
        }

        foreach (var code in defPlaceable.Split(" ")) GatesPlaceable.Add(DecodeGate(code));

        foreach (var row in defBefore.Split("\n"))
        {
            if (row.Length == 0) continue;
            var parts = row.Split(" ");
            if (parts.Length != numBits) throw new InvalidOperationException("Bad parse");
            foreach (var code in parts) GatesBefore.Add(DecodeGate(code));
        }

        foreach (var row in defAfter.Split("\n"))
        {
            if (row.Length == 0) continue;
            var parts = row.Split(" ");
            if (parts.Length != numBits) throw new InvalidOperationException("Bad parse");
            foreach (var code in parts) GatesAfter.Add(DecodeGate(code));
        }

        GatesAfter.Reverse();

        foreach (var row in defSolution.Split("\n"))
        {
            if (row.Length == 0) continue;
            var parts = row.Split(" ");
            if (parts.Length != numBits) throw new InvalidOperationException("Bad parse");
            foreach (var code in parts) GatesSolution.Add(DecodeGate(code));
        }
    }

    private Gate DecodeGate(string code)
    {
        if (code == "_") return null;

        var gate = ldl.allGates.FirstOrDefault(g => g.code == code);
        if (gate != null) return gate;

        throw new InvalidOperationException($"Unknown gate code {code}");
    }

    public QData GoalData(int variant)
    {
        var goalState = GoalStates[variant];
        var goalAmplitude = Complex.FromPolarCoordinates(1, 2 * Mathf.PI * GoalPhases[variant]);
        return new QData(goalState, goalAmplitude);
    }

    public bool VerifySolution()
    {
        Parse();
        var allGates = new List<Gate>();
        allGates.AddRange(GatesBefore);
        allGates.AddRange(GatesSolution);
        allGates.AddRange(GatesAfter.AsEnumerable().Reverse());

        for (var trial = 0; trial < 1000; trial++)
        {
            var variant = Random.Range(0, StartStates.Count);
            var data = new List<QData> { new(StartStates[variant], Complex.One) };
            for (var row = 0; row < allGates.Count / NumBits; row++)
            {
                var gates = allGates.GetRange(row * NumBits, NumBits);

                for (var dim = 0; dim < gates.Count; dim++)
                {
                    if (gates[dim] == null) continue;
                    if (gates[dim].type != GateType.Measure && gates[dim].type != GateType.Reset) continue;
                    var prob1 = data.Sum(q => q.Bit(dim) ? q.Probability : 0);
                    var measure1 = Random.value < prob1;
                    data = data.Where(q => q.Bit(dim) == measure1).Select(q =>
                        new QData(q.State, q.Amplitude / Mathf.Sqrt(measure1 ? prob1 : 1 - prob1))).ToList();
                }

                data = data.SelectMany(q => q.ApplyGateRow(gates)).ToList();
            }

            var goalData = GoalData(variant);
            var fidelity = data.Sum(q => (float)q.Dot(goalData).Real);
            if (fidelity is < .9f or > 1.1f) return false;
        }

        return true;
    }
}