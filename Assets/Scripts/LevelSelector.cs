using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelSelect : MonoBehaviour
{
    public GameObject levelSelectUI;
    public Transform levelButtonParent;
    public SwitchLevel levelButtonPrefab;
    public LevelDefinitionList levelDefs;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Click1();
            levelSelectUI.SetActive(true);
            for (var i = 0; i < levelDefs.AllDefs.Length; i++)
            {
                var def = levelDefs.AllDefs[i];
                var newLevelButton = Instantiate(levelButtonPrefab, levelButtonParent);
                newLevelButton.levelIdChange = i;
                newLevelButton.GetComponentInChildren<TextMeshProUGUI>().text = def.LevelName;
            }
        });
    }
}