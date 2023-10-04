using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelSelect : MonoBehaviour
{
    public GameObject levelSelectUI;
    public Transform levelButtonParent;
    public SwitchLevel levelButtonPrefab;
    public Transform levelDefs;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            levelSelectUI.SetActive(true);
            for (var i = 0; i < levelDefs.childCount; i++)
            {
                var def = levelDefs.GetChild(i).GetComponent<LevelDefinition>();
                var newLevelButton = Instantiate(levelButtonPrefab, levelButtonParent);
                newLevelButton.levelIdChange = i;
                newLevelButton.GetComponentInChildren<TextMeshProUGUI>().text = def.LevelName;
            }
        });
    }
}