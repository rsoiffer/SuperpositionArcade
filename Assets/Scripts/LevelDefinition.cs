using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class LevelDefinition : MonoBehaviour
{
    public int numBits;
    public List<int> startStates;
    public List<int> goalStates;
    public List<float> goalPhases;
    public List<Gate> gatesBefore;
    public List<Gate> gatesAfter;
    public List<Gate> gatesPlaceable;

    public QData GoalData(int variant)
    {
        var goalState = goalStates[variant];
        var goalAmplitude = goalPhases.Count == 0
            ? Complex.One
            : Complex.FromPolarCoordinates(1, 2 * Mathf.PI * goalPhases[variant]);
        return new QData(goalState, goalAmplitude);
    }
}