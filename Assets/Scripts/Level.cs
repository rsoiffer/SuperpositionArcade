using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    public int numBits;

    public Transform bucketsParent;
    public GameObject bucketPrefab;

    public GridLayoutGroup commandGrid;
    public GameObject columnLabelPrefab;
    public GameObject gateSlotPrefab;

    private void Start()
    {
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

        for (var i = 0; i < 2 * numBits; i++) Instantiate(gateSlotPrefab, commandGrid.transform);
    }
}