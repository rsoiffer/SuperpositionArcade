using UnityEngine;

public class MusicOverride : MonoBehaviour
{
    public float newVolume;
    public float volumeDecay = 1;
    public AudioSource audioSource;
    private float _oldVolume;

    private void Awake()
    {
        _oldVolume = SoundManager.Instance.musicSource.volume;
    }

    private void Update()
    {
        var targetVolume = audioSource.isPlaying ? newVolume : _oldVolume;
        SoundManager.Instance.musicSource.volume =
            Mathf.Lerp(targetVolume, SoundManager.Instance.musicSource.volume,
                Mathf.Exp(-Time.deltaTime * volumeDecay));
    }

    private void OnDisable()
    {
        SoundManager.Instance.musicSource.volume = _oldVolume;
    }
}