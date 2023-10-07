using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public static Music Instance;

    public List<AudioClip> clips;
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clips[Random.Range(0, clips.Count)];
            audioSource.Play();
        }
    }
}