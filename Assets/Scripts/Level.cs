using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class Level : MonoBehaviour
{
    [Header("General")] public LevelDefinition def;
    public int numRows = 2;

    [Header("Buckets")] public Transform bucketsParent;
    public Bucket bucketYesPrefab;
    public Bucket bucketNoPrefab;

    [Header("Gate Slots")] public GridLayoutGroup commandGrid;
    public GameObject columnLabelPrefab;
    public GateSlot gateSlotPrefab;
    public string[] dimensionsAlphabet;
    public Color[] dimensionsColors;
    public GridLayoutGroup sourceGrid;

    [Header("Pegs")] public Transform pegParent;
    public GameObject pegPrefab;
    public GameObject pegPrefabX;
    public GameObject pegPrefabZ;
    public GameObject pegPrefabH;

    [Header("States")] public State statePrefab;
    public float spawnRate = 1;

    [Header("UI Fixes")] public int updateAfterFrames = 2;
    public ScrollRect scrollRect;

    [Header("Victory")] public float victoryProgress;
    public float victoryThreshold = 100;
    public float wrongMultiplier = 100;

    private Gate[,] gateGrid;
    private (int, int) prevScreenSize;
    private bool scrollRectChanged;
    private GateSlot[,] slotGrid;

    public int NumBits => def.numBits;

    public float VictoryPercent => Mathf.Clamp01(victoryProgress / victoryThreshold);

    private void Start()
    {
        slotGrid = new GateSlot[NumBits, numRows];

        for (var state = 0; state < 1 << NumBits; state++)
        {
            var bucket = Instantiate(state == def.goalState ? bucketYesPrefab : bucketNoPrefab, bucketsParent);
            bucket.level = this;
            var stateChars = Convert.ToString(state, 2).PadLeft(NumBits, '0').ToCharArray();
            Array.Reverse(stateChars);
            var stateText = string.Join("",
                stateChars.Select((c, idx) => $"<color={ToRGBHex(dimensionsColors[idx])}>{c}</color>"));
            bucket.tmp.text = $"|{stateText}}}";
        }

        commandGrid.constraintCount = NumBits;
        for (var dim = 0; dim < NumBits; dim++)
        {
            var columnLabel = Instantiate(columnLabelPrefab, commandGrid.transform);
            var tmp = columnLabel.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = $"<color={ToRGBHex(dimensionsColors[dim])}>{dimensionsAlphabet[dim]}</color>";
        }

        for (var row = 0; row < numRows; row++)
        for (var dim = 0; dim < NumBits; dim++)
        {
            var newGateSlow = Instantiate(gateSlotPrefab, commandGrid.transform);
            slotGrid[dim, row] = newGateSlow;
        }

        for (var i = 0; i < def.gatesBefore.Count; i++)
        {
            var gateSlot = commandGrid.transform.GetChild(i);
            gateSlot.GetComponent<GateSlot>().BlockDragging();
            if (def.gatesBefore[i] == null) continue;
            var newGate = Instantiate(def.gatesBefore[i], gateSlot);
            newGate.BlockDragging();
        }

        for (var i = 0; i < def.gatesAfter.Count; i++)
        {
            var gateSlot = commandGrid.transform.GetChild(commandGrid.transform.childCount - 1 - i);
            gateSlot.GetComponent<GateSlot>().BlockDragging();
            if (def.gatesAfter[i] == null) continue;
            var newGate = Instantiate(def.gatesAfter[i], gateSlot);
            newGate.BlockDragging();
        }

        foreach (var gate in def.gatesPlaceable) Instantiate(gate, sourceGrid.transform);

        StartCoroutine(SpawnCoroutine());

        scrollRect.onValueChanged.AddListener(_ => scrollRectChanged = true);
    }

    private void Update()
    {
        updateAfterFrames -= 1;

        var newGateGrid = new Gate[NumBits, numRows];
        for (var dim = 0; dim < NumBits; dim++)
        for (var row = 0; row < numRows; row++)
            newGateGrid[dim, row] = slotGrid[dim, row].GetComponentInChildren<Gate>();

        var newScreenSize = (Screen.width, Screen.height);

        if (SequenceEquals(newGateGrid, gateGrid) && newScreenSize == prevScreenSize && updateAfterFrames != 0 &&
            !scrollRectChanged) return;
        gateGrid = newGateGrid;
        prevScreenSize = newScreenSize;
        scrollRectChanged = false;

        if (pegParent != null) Destroy(pegParent.gameObject);
        pegParent = new GameObject("Peg Parent").transform;
        pegParent.SetParent(transform);

        for (var row = 0; row < numRows; row++)
        {
            var gates = Gates(row);
            for (var state = 0; state < 1 << NumBits; state++)
            {
                var newPeg = Instantiate(pegPrefab, pegParent);
                newPeg.transform.position = PegPos(state, row);

                if (gates.Any(g => g != null && g.type == GateType.X))
                {
                    var newPegX = Instantiate(pegPrefabX, pegParent);
                    newPegX.transform.position = PegPos(state, row);
                    var i2 = state;
                    for (var k = 0; k < NumBits; k++)
                        if (gates[k] != null && gates[k].type == GateType.X)
                            i2 ^= 1 << k;

                    if (i2 < state) newPegX.transform.localScale *= new Vector2(-1, 1);
                }

                if (gates.Select((g, k) => (g, k))
                        .Count(x => x.g != null && x.g.type == GateType.Z && (state & (1 << x.k)) != 0) % 2 == 1)
                {
                    var newPegZ = Instantiate(pegPrefabZ, pegParent);
                    newPegZ.transform.position = PegPos(state, row);
                }

                if (gates.Any(g => g != null && g.type == GateType.H))
                {
                    var newPegH = Instantiate(pegPrefabH, pegParent);
                    newPegH.transform.position = PegPos(state, row);
                }
            }
        }
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1 / spawnRate);
            var newState = Instantiate(statePrefab);
            newState.level = this;
            newState.ResetToState(def.startState);
        }
    }

    public void StateHitBottom(State state)
    {
        var fidelity = state.quballs.Sum(q => q.stateCurrent == def.goalState ? (float)q.Amplitude.Magnitude : 0);
        victoryProgress += fidelity - (1 - fidelity) * wrongMultiplier;
        victoryProgress = Mathf.Clamp(victoryProgress, 0, victoryThreshold);
    }

    public List<Gate> Gates(int row)
    {
        return Enumerable.Range(0, NumBits).Select(dim => gateGrid[dim, row]).ToList();
    }

    public Vector3 PegPos(int state, int row)
    {
        var mainCamera = Camera.main!;
        var slotPos = row < 0
            ? new Vector3(0, 5, 0)
            : row >= numRows
                ? new Vector3(0, -5, 0)
                : mainCamera.ScreenToWorldPoint(slotGrid[0, row].transform.position);
        var bucket = bucketsParent.GetChild(state);
        var bucketPos = mainCamera.ScreenToWorldPoint(bucket.position);
        return new Vector3(bucketPos.x, slotPos.y, 0);
    }

    private static bool SequenceEquals<T>(T[,] a, T[,] b) where T : Object
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Rank == b.Rank
               && Enumerable.Range(0, a.Rank).All(d => a.GetLength(d) == b.GetLength(d))
               && a.Cast<T>().Zip(b.Cast<T>(), (arg1, arg2) => arg1 == arg2).All(x => x);
    }

    public static string ToRGBHex(Color c)
    {
        return $"#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}";
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}