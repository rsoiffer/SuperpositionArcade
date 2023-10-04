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

    [Header("General")] public Transform levelDefs;
    public int numRows = 2;
    public TextMeshProUGUI titleText;

    [Header("Buckets")] public Transform bucketsParent;
    public Bucket bucketYesPrefab;
    public Bucket bucketNoPrefab;
    public List<Transform> bucketTextParents;
    public TextMeshProUGUI bucketTextPrefab;
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

    private LevelDefinition _def;
    private Gate[,] gateGrid;
    private Peg[,] pegGrid;
    private (int, int) prevScreenSize;
    private bool scrollRectChanged;
    private GateSlot[,] slotGrid;

    public int NumBits => _def.numBits;

    public float VictoryPercent => Mathf.Clamp01(victoryProgress / victoryThreshold);

    private void Start()
    {
        LevelId = Mathf.Clamp(LevelId, 0, levelDefs.childCount - 1);
        _def = levelDefs.GetChild(LevelId).GetComponent<LevelDefinition>();
        titleText.text = _def.LevelName;

        slotGrid = new GateSlot[NumBits, numRows];
        pegGrid = new Peg[1 << NumBits, numRows];

        for (var state = 0; state < 1 << NumBits; state++)
        {
            var matchingGoal = _def.goalStates.FindIndex(s => s == state);
            var bucket = Instantiate(matchingGoal >= 0 ? bucketYesPrefab : bucketNoPrefab, bucketsParent);
            bucket.level = this;
            bucket.variant = matchingGoal;
            var phaseColorer = bucket.GetComponent<PhaseColorer>();
            phaseColorer.variant = matchingGoal < 0 ? 0 : matchingGoal;
            if (matchingGoal >= 0 && matchingGoal < _def.goalPhases.Count)
                phaseColorer.phase = _def.goalPhases[matchingGoal];

            var matchingStart = _def.startStates.FindIndex(s => s == state);
            var cannon = Instantiate(matchingStart >= 0 ? cannonYesPrefab : cannonNoPrefab, cannonsParent);
            cannon.level = this;
            cannon.variant = matchingStart;
            var phaseColorerCannon = cannon.GetComponent<PhaseColorer>();
            phaseColorerCannon.variant = matchingGoal < 0 ? 0 : matchingGoal;

            foreach (var bucketTextParent in bucketTextParents)
            {
                var bucketText = Instantiate(bucketTextPrefab, bucketTextParent);
                var stateChars = Convert.ToString(state, 2).PadLeft(NumBits, '0').ToCharArray();
                Array.Reverse(stateChars);
                var stateText = string.Join("",
                    stateChars.Select((c, idx) => $"<color={ToRGBHex(dimensionsColors[idx])}>{c}</color>"));
                bucketText.text = $"|{stateText}}}";
            }
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
            var newGateSlot = Instantiate(gateSlotPrefab, commandGrid.transform);
            slotGrid[dim, row] = newGateSlot;
        }

        for (var i = 0; i < _def.gatesBefore.Count; i++)
        {
            var gateSlot = commandGrid.transform.GetChild(NumBits + i);
            gateSlot.GetComponent<GateSlot>().BlockDragging();
            if (_def.gatesBefore[i] == null) continue;
            var newGate = Instantiate(_def.gatesBefore[i], gateSlot);
            newGate.BlockDragging();
        }

        for (var i = 0; i < _def.gatesAfter.Count; i++)
        {
            var gateSlot = commandGrid.transform.GetChild(commandGrid.transform.childCount - 1 - i);
            gateSlot.GetComponent<GateSlot>().BlockDragging();
            if (_def.gatesAfter[i] == null) continue;
            var newGate = Instantiate(_def.gatesAfter[i], gateSlot);
            newGate.BlockDragging();
        }

        foreach (var gate in _def.gatesPlaceable) Instantiate(gate, sourceGrid.transform);

        pegGridParent.constraintCount = 1 << NumBits;
        for (var state = 0; state < 1 << NumBits; state++) Instantiate(pegEmptyPrefab, pegGridParent.transform);
        for (var row = 0; row < numRows; row++)
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

        var newGateGrid = new Gate[NumBits, numRows];
        for (var dim = 0; dim < NumBits; dim++)
        for (var row = 0; row < numRows; row++)
            newGateGrid[dim, row] = slotGrid[dim, row].GetComponentInChildren<Gate>();

        var newScreenSize = (Screen.width, Screen.height);

        if (!SequenceEquals(newGateGrid, gateGrid) || newScreenSize != prevScreenSize || updateAfterFrames == 0 ||
            scrollRectChanged)
        {
            gateGrid = newGateGrid;
            prevScreenSize = newScreenSize;
            scrollRectChanged = false;

            for (var row = 0; row < numRows; row++)
            for (var state = 0; state < 1 << NumBits; state++)
                pegGrid[state, row].UpdateGraphics();
        }

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
                var variant = Random.Range(0, _def.startStates.Count);
                newState.level = this;
                newState.variant = variant;
                newState.ResetToState(_def.startStates[variant]);
            }

            yield return null;
        }
    }

    public bool QuballValid(Quball q)
    {
        var goalState = _def.goalStates[q.variant];
        return q.current.State == goalState;
    }

    public void StateHitBottom(State state)
    {
        var goalData = _def.GoalData(state.variant);
        var fidelity = state.quballs.Sum(q => (float)q.current.Dot(goalData).Real);
        victoryProgress += fidelity - (1 - fidelity) * wrongMultiplier;
        victoryProgress = Mathf.Clamp(victoryProgress, 0, victoryThreshold);
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
        var clampedRow = Mathf.Clamp(row, 0, numRows - 1);
        var offset = (clampedRow - row) * new Vector3(0, 200, 0);
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