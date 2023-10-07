using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class LevelDefinition : MonoBehaviour
{
    [SerializeField] private LevelDefinitionList ldl;
    [SerializeField] private int numBits;
    [TextArea(1, 10)] [SerializeField] private string defGoals;
    [TextArea(1, 10)] [SerializeField] private string defPlaceable;
    [TextArea(1, 10)] [SerializeField] private string defBefore;
    [TextArea(1, 10)] [SerializeField] private string defAfter;

    [NonSerialized] public List<Gate> GatesAfter;
    [NonSerialized] public List<Gate> GatesBefore;
    [NonSerialized] public List<Gate> GatesPlaceable;
    [NonSerialized] public List<float> GoalPhases;
    [NonSerialized] public List<int> GoalStates;
    [NonSerialized] public List<int> StartStates;

    public int NumBits => numBits;
    public string LevelName => name;

    public void Parse()
    {
        if (StartStates != null) return;

        StartStates = new List<int>();
        GoalStates = new List<int>();
        GoalPhases = new List<float>();
        GatesBefore = new List<Gate>();
        GatesAfter = new List<Gate>();
        GatesPlaceable = new List<Gate>();

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
}