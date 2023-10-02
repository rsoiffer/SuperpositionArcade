using System.Collections.Generic;
using UnityEngine;

public class LevelDefinition : MonoBehaviour
{
    public int numBits;
    public int startState;
    public int goalState;
    public List<Gate> gatesBefore;
    public List<Gate> gatesAfter;
    public List<Gate> gatesPlaceable;
}