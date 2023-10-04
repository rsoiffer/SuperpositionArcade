using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SwitchLevel : MonoBehaviour
{
    public bool resetLevelId;
    public int levelIdChange = 1;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (resetLevelId) Level.LevelId = 0;
            Level.LevelId += levelIdChange;
            SceneManager.LoadScene("Level");
        });
    }
}