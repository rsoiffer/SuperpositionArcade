using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Turbo : MonoBehaviour
{
    public Button button;
    public Level level;
    public float turboSpeedMul = 10;
    public float turboDuration = 2;

    private bool activeNow;

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (activeNow) return;
            SoundManager.Click1();
            StartCoroutine(RunTurbo());
        });
    }

    private IEnumerator RunTurbo()
    {
        activeNow = true;
        level.spawnRate *= turboSpeedMul;
        yield return new WaitForSeconds(turboDuration);
        level.spawnRate /= turboSpeedMul;
        activeNow = false;
    }
}