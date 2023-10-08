using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelDefinitionList : MonoBehaviour
{
    public List<Gate> allGates;
    public LevelDefinition[] AllDefs => GetComponentsInChildren<LevelDefinition>();
}

[CustomEditor(typeof(LevelDefinitionList))]
public class LevelDefinitionListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var self = (LevelDefinitionList)target;
        if (GUILayout.Button("Verify Solutions"))
            foreach (var def in self.AllDefs)
                if (def.VerifySolution())
                    Debug.Log($"Verified solution for {def.name}");
                else
                    Debug.LogError($"Failed to verify solution for {def.name}");
    }
}