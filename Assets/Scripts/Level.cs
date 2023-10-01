using System;
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

        var mainCamera = Camera.main!;
        for (var j = 0; j < numRows; j++)
        {
            var slotPos = mainCamera.ScreenToWorldPoint(slotGrid[0, j].transform.position);
            if (Enumerable.Range(0, numBits).Any(i => gateGrid[i, j] != null))
                // There is at least one gate in this row
                foreach (var bucket in bucketsParent.Cast<Transform>())
                {
                    var worldPos = mainCamera.ScreenToWorldPoint(bucket.position);
                    var pegPos = new Vector3(worldPos.x, slotPos.y, 0);
                    var newPeg = Instantiate(pegPrefab, pegParent);
                    newPeg.transform.position = pegPos;
                }
        }
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