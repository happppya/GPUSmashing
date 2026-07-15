using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaylistManager : MonoBehaviour
{
    public SoundCollection playlist;
   
    private AudioSource audioSource;
    private List<AudioClip> shuffledQueue = new List<AudioClip>();
    private int currentSongIndex = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        InitializeQueue();
        PlayNextSong();
    }

    private void Update()
    {
        if (!audioSource.isPlaying && audioSource.clip != null && shuffledQueue.Count > 0)
        {
            PlayNextSong();
        }
    }

    private void PlayNextSong()
    {
        if (shuffledQueue.Count == 0) return;

        // Assign and play the current song in the queue
        audioSource.clip = shuffledQueue[currentSongIndex];
        audioSource.Play();

        // Advance the index for the next time this is called
        currentSongIndex++;

        // If we reached the end of the queue, reshuffle and reset the index
        if (currentSongIndex >= shuffledQueue.Count)
        {
            InitializeQueue();
        }
    }

    private void InitializeQueue()
    {
        shuffledQueue.Clear();
        shuffledQueue.AddRange(playlist.Clips);
        currentSongIndex = 0;

        for (int i = 0; i < shuffledQueue.Count; i++)
        {
            AudioClip temp = shuffledQueue[i];
            int randomIndex = Random.Range(i, shuffledQueue.Count);
            shuffledQueue[i] = shuffledQueue[randomIndex];
            shuffledQueue[randomIndex] = temp;
        }
    }
}