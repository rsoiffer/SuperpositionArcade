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

    public State statePrefab;

    public Gate[,] gateGrid;

    public GateSlot[,] slotGrid;

    private void Start()
    {
        slotGrid = new GateSlot[numBits, numRows];

        for (var i = 0; i < 1 << numBits; i++)
        {
            var bucket = Instantiate(bucketPrefab, bucketsParent);
            bucket.GetComponentInChildren<TextMeshProUGUI>().text =
                $"|{Convert.ToString(i, 2).PadLeft(numBits, '0')}}}";
        }

        commandGrid.constraintCount = numBits;
        for (var i = 0; i < numBits; i++)
        {
            var columnLabel = Instantiate(columnLabelPrefab, commandGrid.transform);
            columnLabel.GetComponentInChildren<TextMeshProUGUI>().text = $"{i}";
        }

        for (var j = 0; j < numRows; j++)
        for (var i = 0; i < numBits; i++)
        {
            var newGateSlow = Instantiate(gateSlotPrefab, commandGrid.transform);
            slotGrid[i, j] = newGateSlow;
        }

        StartCoroutine(SpawnCoroutine());
    }

    private void Update()
    {
        var newGateGrid = new Gate[numBits, numRows];
        for (var i = 0; i < numBits; i++)
        for (var j = 0; j < numRows; j++)
            newGateGrid[i, j] = slotGrid[i, j].GetComponentInChildren<Gate>();

        if (SequenceEquals(newGateGrid, gateGrid)) return;
        gateGrid = newGateGrid;

        if (pegParent != null) Destroy(pegParent.gameObject);
        pegParent = new GameObject("Peg Parent").transform;
        pegParent.SetParent(transform);

        for (var j = 0; j < numRows; j++)
            if (Enumerable.Range(0, numBits).Any(i => gateGrid[i, j] != null))
                // There is at least one gate in this row
                for (var i = 0; i < bucketsParent.childCount; i++)
                {
                    var newPeg = Instantiate(pegPrefab, pegParent);
                    newPeg.transform.position = PegPos(i, j);
                }
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
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
}