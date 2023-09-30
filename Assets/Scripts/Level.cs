using UnityEngine;

public class Level : MonoBehaviour
{
    public int numBits;
    public GameObject bucketPrefab;

    private void Start()
    {
        for (var i = 0; i < 1 << numBits; i++)
        {
            var newBucket = Instantiate(bucketPrefab, transform);
            newBucket.transform.position = new Vector3(i, 0, 0);
        }
    }
}