using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{

    // An array of all the different music clips
    public AudioClip[] sounds;

    // Reference to the audio source
    private AudioSource audioSource;

    void Start()
    {
        // Get the second audio source component (the first one is playing the "background" sound)
        audioSource = GetComponents<AudioSource>()[1];
    }

    void Update()
    {
        // Do nothing if the audio source is already playing a clip
        if (audioSource.isPlaying) return;

        // If it isn't playing something, get a random sound from the array and play it
        audioSource.clip = sounds[Random.Range(0, sounds.Length)];
        audioSource.Play();
    }
}
