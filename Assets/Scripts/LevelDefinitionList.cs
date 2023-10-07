using System.Collections.Generic;
using UnityEngine;

public class LevelDefinitionList : MonoBehaviour
{
    public List<Gate> allGates;
    public LevelDefinition[] AllDefs => GetComponentsInChildren<LevelDefinition>();
}