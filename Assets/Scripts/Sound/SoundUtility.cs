using System;
using UnityEngine;

public static class SoundUtility
{
    public static void PlayRandomSound(SoundCollection collection, AudioSource source, bool preventOverlap = false, float overlapThreshold = 0f)
    {
        if (collection == null || source == null) return;

        if (preventOverlap && source.isPlaying && source.clip != null)
        {
            if (Array.IndexOf(collection.clips, source.clip) >= 0)
            {
                float pitch = source.pitch != 0f ? Mathf.Abs(source.pitch) : 1f;
                float timeRemaining = (source.clip.length - source.time) / pitch;

                if (timeRemaining > overlapThreshold)
                {
                    return; // Skip playback because a clip from this collection is still playing
                }
            }
        }

        AudioClip clipToPlay = collection.GetRandomClip();
        if (clipToPlay != null)
        {
            source.clip = clipToPlay;
            source.Play();
        }
    }
}