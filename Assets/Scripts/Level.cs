using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class Level : MonoBehaviour
{
    public int numBits;
    public int numRows = 2;

    public Transform bucketsParent;
    public GameObject bucketPrefab;

    public GridLayoutGroup commandGrid;
    public GameObject columnLabelPrefab;
    public GateSlot gateSlotPrefab;

    public Transform pegParent;
    public GameObject pegPrefab;
    public GameObject pegPrefabX;
    public GameObject pegPrefabZ;
    public GameObject pegPrefabH;

    public State statePrefab;
    public float spawnRate = 1;
    public int updateAfterFrames = 2;
    public ScrollRect scrollRect;

    public string[] dimensionsAlphabet;
    public Color[] dimensionsColors;
    public bool scrollRectChanged;

    public Gate[,] gateGrid;
    public (int, int) prevScreenSize;

    public GateSlot[,] slotGrid;

    private void Start()
    {
        slotGrid = new GateSlot[numBits, numRows];

        for (var i = 0; i < 1 << numBits; i++)
        {
            var bucket = Instantiate(bucketPrefab, bucketsParent);
            var tmp = bucket.GetComponentInChildren<TextMeshProUGUI>();
            var stateChars = Convert.ToString(i, 2).PadLeft(numBits, '0').ToCharArray();
            Array.Reverse(stateChars);
            var stateText = string.Join("",
                stateChars.Select((c, idx) => $"<color={ToRGBHex(dimensionsColors[idx])}>{c}</color>"));
            tmp.text = $"|{stateText}}}";
        }

        commandGrid.constraintCount = numBits;
        for (var i = 0; i < numBits; i++)
        {
            var columnLabel = Instantiate(columnLabelPrefab, commandGrid.transform);
            var tmp = columnLabel.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = $"<color={ToRGBHex(dimensionsColors[i])}>{dimensionsAlphabet[i]}</color>";
        }

        for (var j = 0; j < numRows; j++)
        for (var i = 0; i < numBits; i++)
        {
            var newGateSlow = Instantiate(gateSlotPrefab, commandGrid.transform);
            slotGrid[i, j] = newGateSlow;
        }

        StartCoroutine(SpawnCoroutine());

        scrollRect.onValueChanged.AddListener(_ => scrollRectChanged = true);
    }

    private void Update()
    {
        updateAfterFrames -= 1;

        var newGateGrid = new Gate[numBits, numRows];
        for (var i = 0; i < numBits; i++)
        for (var j = 0; j < numRows; j++)
            newGateGrid[i, j] = slotGrid[i, j].GetComponentInChildren<Gate>();

        var newScreenSize = (Screen.width, Screen.height);

        if (SequenceEquals(newGateGrid, gateGrid) && newScreenSize == prevScreenSize && updateAfterFrames != 0 &&
            !scrollRectChanged) return;
        gateGrid = newGateGrid;
        prevScreenSize = newScreenSize;
        scrollRectChanged = false;

        if (pegParent != null) Destroy(pegParent.gameObject);
        pegParent = new GameObject("Peg Parent").transform;
        pegParent.SetParent(transform);

        for (var j = 0; j < numRows; j++)
        {
            var gates = Enumerable.Range(0, numBits).Select(i => gateGrid[i, j]).ToList();
            for (var i = 0; i < 1 << numBits; i++)
            {
                var newPeg = Instantiate(pegPrefab, pegParent);
                newPeg.transform.position = PegPos(i, j);

                if (gates.Any(g => g != null && g.type == GateType.X))
                {
                    var newPegX = Instantiate(pegPrefabX, pegParent);
                    newPegX.transform.position = PegPos(i, j);
                    var i2 = i;
                    for (var k = 0; k < numBits; k++)
                        if (gates[k] != null && gates[k].type == GateType.X)
                            i2 ^= 1 << k;

                    if (i2 < i) newPegX.transform.localScale *= new Vector2(-1, 1);
                }

                if (gates.Select((g, k) => (g, k))
                        .Count(x => x.g != null && x.g.type == GateType.Z && (i & (1 << x.k)) != 0) % 2 == 1)
                {
                    var newPegZ = Instantiate(pegPrefabZ, pegParent);
                    newPegZ.transform.position = PegPos(i, j);
                }

                if (gates.Any(g => g != null && g.type == GateType.H))
                {
                    var newPegH = Instantiate(pegPrefabH, pegParent);
                    newPegH.transform.position = PegPos(i, j);
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
        }
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