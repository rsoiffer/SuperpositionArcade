using System.Collections.Generic;
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
}