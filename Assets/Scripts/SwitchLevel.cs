using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SwitchLevel : MonoBehaviour
{
    public int levelIdChange = 1;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            Level.LevelId += levelIdChange;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }
}