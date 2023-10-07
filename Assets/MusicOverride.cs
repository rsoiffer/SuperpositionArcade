using UnityEngine;

public class MusicOverride : MonoBehaviour
{
    public float newVolume;
    public float volumeDecay = 1;
    public AudioSource audioSource;
    private float _oldVolume;

    private void Awake()
    {
        _oldVolume = Music.Instance.audioSource.volume;
    }

    private void Update()
    {
        var targetVolume = audioSource.isPlaying ? newVolume : _oldVolume;
        Music.Instance.audioSource.volume =
            Mathf.Lerp(targetVolume, Music.Instance.audioSource.volume, Mathf.Exp(-Time.deltaTime * volumeDecay));
    }

    private void OnDisable()
    {
        Music.Instance.audioSource.volume = _oldVolume;
    }
}