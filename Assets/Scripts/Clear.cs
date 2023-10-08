using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Clear : MonoBehaviour
{
    public Level level;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(level.Clear);
    }
}