using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public List<AudioClip> musicClips;
    public AudioSource musicSource;

    public AudioClip click1;
    public AudioClip click2;

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
        if (!musicSource.isPlaying)
        {
            musicSource.clip = musicClips[Random.Range(0, musicClips.Count)];
            musicSource.Play();
        }
    }

    private IEnumerator PlayClip(AudioClip clip)
    {
        var child = new GameObject(clip.name);
        child.transform.SetParent(transform);
        var source = child.AddComponent<AudioSource>();
        source.clip = clip;
        source.Play();
        while (source.isPlaying) yield return null;
        Destroy(child);
    }

    public static void Click1()
    {
        Instance.StartCoroutine(Instance.PlayClip(Instance.click1));
    }

    public static void Click2()
    {
        Instance.StartCoroutine(Instance.PlayClip(Instance.click2));
    }
}