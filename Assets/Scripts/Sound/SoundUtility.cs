using UnityEngine;

public static class SoundUtility
{
    public static void PlayRandomSound(SoundCollection collection, AudioSource source, bool preventOverlap = false, float overlapThreshold = 0f)
    {
        if (preventOverlap && !collection.CanPlay(overlapThreshold))
        {
            return;
        }

        AudioClip clipToPlay = collection.GetRandomClip();
        if (clipToPlay != null)
        {
            source.clip = clipToPlay;
            source.Play();

            if (preventOverlap)
            {
                collection.RegisterPlayback(clipToPlay, source.pitch);
            }
        }
    }
}