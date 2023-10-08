using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Level : MonoBehaviour
{
    public static int LevelId;

    [Header("General")] public LevelDefinitionList levelDefs;
    public TextMeshProUGUI titleText;

    [Header("Buckets")] public Transform bucketsParent;
    public Bucket bucketYesPrefab;
    public Bucket bucketNoPrefab;
    public List<Transform> bucketTextParents;
    public GameObject bucketTextPrefab;
    public Transform cannonsParent;
    public Bucket cannonYesPrefab;
    public Bucket cannonNoPrefab;

    [Header("Gate Slots")] public GridLayoutGroup commandGrid;
    public GameObject columnLabelPrefab;
    public GateSlot gateSlotPrefab;
    public string[] dimensionsAlphabet;
    public Color[] dimensionsColors;
    public GridLayoutGroup sourceGrid;

    [Header("Pegs")] public GridLayoutGroup pegGridParent;
    public GameObject pegEmptyPrefab;
    public Peg pegPrefab;

    [Header("States")] public Transform objectsParent;
    public State statePrefab;
    public float spawnRate = 1;
    public float timeScale = 1;

    [Header("UI Fixes")] public int updateAfterFrames = 2;
    public ScrollRect scrollRect;

    [Header("Victory")] public float victoryProgress;
    public float victoryThreshold = 100;
    public float wrongMultiplier = 100;
    public GameObject victoryUI;

    [Header("Highlights")] public Transform commandHighlightsParent;
    public Image commandHighlightPrefab;
    public Transform bucketHighlightsParent;
    public Image bucketHighlightsPrefab;
    private readonly List<State> currentStates = new();

    private LevelDefinition _def;
    private Gate[,] gateGrid;
    private Peg[,] pegGrid;
    private (int, int) prevScreenSize;
    private bool scrollRectChanged;
    private GateSlot[,] slotGrid;

    public int NumBits => _def.NumBits;
    public int NumRows => _def.NumRows;

    public float VictoryPercent => Mathf.Clamp01(victoryProgress / victoryThreshold);

    private void Start()
    {
        LevelId = Mathf.Clamp(LevelId, 0, levelDefs.AllDefs.Length - 1);
        _def = levelDefs.AllDefs[LevelId];
        _def.Parse();
        titleText.text = _def.LevelName;

        slotGrid = new GateSlot[NumBits, NumRows];
        pegGrid = new Peg[1 << NumBits, NumRows];

        for (var state = 0; state < 1 << NumBits; state++)
        {
            var matchingGoal = _def.GoalStates.FindIndex(s => s == state);
            var bucket = Instantiate(matchingGoal >= 0 ? bucketYesPrefab : bucketNoPrefab, bucketsParent);
            bucket.level = this;
            bucket.variant = matchingGoal;
            var phaseColorer = bucket.GetComponent<PhaseColorer>();
            phaseColorer.variant = matchingGoal < 0 ? 0 : matchingGoal;
            if (matchingGoal >= 0) phaseColorer.phase = _def.GoalPhases[matchingGoal];

            var matchingStart = _def.StartStates.FindIndex(s => s == state);
            var cannon = Instantiate(matchingStart >= 0 ? cannonYesPrefab : cannonNoPrefab, cannonsParent);
            cannon.level = this;
            cannon.variant = matchingStart;
            var phaseColorerCannon = cannon.GetComponent<PhaseColorer>();
            phaseColorerCannon.variant = matchingStart < 0 ? 0 : matchingStart;

            foreach (var bucketTextParent in bucketTextParents)
            {
                var bucketText = Instantiate(bucketTextPrefab, bucketTextParent);
                var stateChars = Convert.ToString(state, 2).PadLeft(NumBits, '0').ToCharArray();
                Array.Reverse(stateChars);
                bucketText.GetComponentInChildren<TextMeshProUGUI>().text = string.Join("",
                    stateChars.Select((c, idx) => $"<color={ToRGBHex(dimensionsColors[idx])}>{c}</color>"));
            }
        }

        commandGrid.constraintCount = NumBits;
        for (var dim = 0; dim < NumBits; dim++)
        {
            var columnLabel = Instantiate(columnLabelPrefab, commandGrid.transform);
            var tmp = columnLabel.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = $"<color={ToRGBHex(dimensionsColors[dim])}>{dimensionsAlphabet[dim]}</color>";
        }

        for (var row = 0; row < NumRows; row++)
        for (var dim = 0; dim < NumBits; dim++)
        {
            var newGateSlot = Instantiate(gateSlotPrefab, commandGrid.transform);
            slotGrid[dim, row] = newGateSlot;
        }

        for (var i = 0; i < _def.GatesBefore.Count; i++)
        {
            var gateSlot = commandGrid.transform.GetChild(NumBits + i);
            gateSlot.GetComponent<GateSlot>().BlockDragging();
            if (_def.GatesBefore[i] == null) continue;
            var newGate = Instantiate(_def.GatesBefore[i], gateSlot);
            newGate.BlockDragging();
        }

        for (var i = 0; i < _def.GatesAfter.Count; i++)
        {
            var gateSlot = commandGrid.transform.GetChild(commandGrid.transform.childCount - 1 - i);
            gateSlot.GetComponent<GateSlot>().BlockDragging();
            if (_def.GatesAfter[i] == null) continue;
            var newGate = Instantiate(_def.GatesAfter[i], gateSlot);
            newGate.BlockDragging();
        }

        foreach (var gate in _def.GatesPlaceable) Instantiate(gate, sourceGrid.transform);

        pegGridParent.constraintCount = 1 << NumBits;
        for (var state = 0; state < 1 << NumBits; state++) Instantiate(pegEmptyPrefab, pegGridParent.transform);
        for (var row = 0; row < NumRows; row++)
        for (var state = 0; state < 1 << NumBits; state++)
        {
            var newPeg = Instantiate(pegPrefab, pegGridParent.transform);
            pegGrid[state, row] = newPeg;
            newPeg.state = state;
            newPeg.row = row;
            newPeg.level = this;
        }

        for (var dim = 0; dim < NumBits; dim++)
        {
            var commandHighlight = Instantiate(commandHighlightPrefab, commandHighlightsParent);
            var color = dimensionsColors[dim];
            color.a = commandHighlight.color.a;
            commandHighlight.color = color;
        }

        for (var state = 0; state < 1 << NumBits; state++)
        {
            var bucketHighlight = Instantiate(bucketHighlightsPrefab, bucketHighlightsParent);
            bucketHighlight.enabled = false;
        }

        StartCoroutine(SpawnCoroutine());

        scrollRect.onValueChanged.AddListener(_ => scrollRectChanged = true);
    }

    private void Update()
    {
        updateAfterFrames -= 1;

        UpdateGateGrid();

        var dimSelected = -1;
        for (var dim = 0; dim < NumBits; dim++)
        {
            var rect = commandHighlightsParent.GetChild(dim).GetComponent<RectTransform>();
            if (rect.rect.Contains(rect.InverseTransformPoint(Input.mousePosition))) dimSelected = dim;
        }

        for (var state = 0; state < 1 << NumBits; state++)
        {
            var image = bucketHighlightsParent.GetChild(state).GetComponent<Image>();
            image.enabled = dimSelected >= 0 && (state & (1 << dimSelected)) != 0;
            if (dimSelected < 0) continue;
            var newColor = dimensionsColors[dimSelected];
            newColor.a = image.color.a;
            image.color = newColor;
        }

        if (VictoryPercent >= 1) victoryUI.SetActive(true);
    }

    private void UpdateGateGrid(bool force = false)
    {
        var newGateGrid = new Gate[NumBits, NumRows];
        for (var dim = 0; dim < NumBits; dim++)
        for (var row = 0; row < NumRows; row++)
            newGateGrid[dim, row] = slotGrid[dim, row].GetComponentInChildren<Gate>();

        var newScreenSize = (Screen.width, Screen.height);

        if (SequenceEquals(newGateGrid, gateGrid)
            && newScreenSize == prevScreenSize
            && updateAfterFrames != 0
            && !scrollRectChanged
            && !force)
            return;

        gateGrid = newGateGrid;
        prevScreenSize = newScreenSize;
        scrollRectChanged = false;

        for (var row = 0; row < NumRows; row++)
        for (var state = 0; state < 1 << NumBits; state++)
            pegGrid[state, row].UpdateGraphics();
    }

    public void Clear()
    {
        for (var dim = 0; dim < NumBits; dim++)
        for (var row = 0; row < NumRows; row++)
        {
            var gate = slotGrid[dim, row].GetComponentInChildren<Gate>();
            if (gate is not { isDraggable: true }) continue;
            gate.transform.SetParent(null, true);
            Destroy(gate.gameObject);
        }

        UpdateGateGrid(true);
    }

    public void Cheat()
    {
        Clear();

        var cheatGates = _def.GatesSolution.ToList();
        var cheatGateId = 0;

        for (var row = 0; row < NumRows; row++)
        for (var dim = 0; dim < NumBits; dim++)
        {
            if (cheatGateId >= cheatGates.Count) continue;
            if (!slotGrid[dim, row].acceptsGateDrops) continue;
            var gatePrefab = cheatGates[cheatGateId];
            cheatGateId++;

            if (gatePrefab == null) continue;
            var copy = Instantiate(gatePrefab, slotGrid[dim, row].transform);
            copy.transform.localPosition = Vector3.zero;
        }

        UpdateGateGrid();
    }

    public void Heat()
    {
        currentStates.RemoveAll(s => s == null);
        foreach (var s in currentStates) s.Heat();
        currentStates.Clear();

        UpdateGateGrid();
    }

    private IEnumerator SpawnCoroutine()
    {
        var spawnCooldown = 1f;
        while (true)
        {
            spawnCooldown -= Time.deltaTime * spawnRate;
            if (spawnCooldown < 0)
            {
                spawnCooldown = 1;
                var newState = Instantiate(statePrefab, objectsParent);
                var variant = Random.Range(0, _def.StartStates.Count);
                newState.level = this;
                newState.variant = variant;
                newState.ResetToState(_def.StartStates[variant]);
                currentStates.Add(newState);
            }

            yield return null;
        }
    }

    public bool QuballValid(Quball q)
    {
        var goalData = _def.GoalData(q.variant);
        return q.current.Dot(goalData).Real > .9f;
    }

    public void StateHitBottom(State state)
    {
        var goalData = _def.GoalData(state.variant);
        var fidelity = state.quballs.Sum(q => (float)q.current.Dot(goalData).Real);
        victoryProgress += fidelity - (1 - fidelity) * wrongMultiplier;
        victoryProgress = Mathf.Clamp(victoryProgress, 0, victoryThreshold);
        currentStates.Remove(state);
    }

    public List<Gate> Gates(int row)
    {
        return Enumerable.Range(0, NumBits).Select(dim => gateGrid[dim, row]).ToList();
    }

    public bool CheckControls(int state, int row)
    {
        return Gates(row).Select((g, i) => (g, i))
            .Where(x => x.g != null && x.g.type == GateType.Control).All(x => (state & (1 << x.i)) != 0);
    }

    public Vector3 PegPos(int state, int row)
    {
        var clampedRow = Mathf.Clamp(row, 0, NumRows - 1);
        var offset = (clampedRow - row) * new Vector3(0, 200, 0) * Screen.width / 1200f;
        return pegGrid[state, clampedRow].transform.position + offset;
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