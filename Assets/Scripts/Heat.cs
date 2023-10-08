using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Heat : MonoBehaviour
{
    public Level level;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(level.Heat);
    }
}