using UnityEngine;

public static class SoundUtility
{
    private static AudioSource globalSource;

    public static void PlayRandomSound(SoundCollection collection, AudioSource source, bool preventOverlap = false, float overlapThreshold = 0f)
    {
        if (globalSource == null)
        {
            globalSource = new GameObject("SoundPlayer").AddComponent<AudioSource>();
            globalSource.playOnAwake = false;
            globalSource.loop = false;
            globalSource.minDistance = 10000;
            globalSource.maxDistance = 100000;
            UnityEngine.Object.DontDestroyOnLoad(globalSource.gameObject);
        }

        Debug.Log($"called {preventOverlap}, {collection.CanPlay(overlapThreshold)}");
        if (preventOverlap && !collection.CanPlay(overlapThreshold))
        {
            return;
        }

        AudioClip clipToPlay = collection.GetRandomClip();

        if (source == null)
        {
            source = globalSource;
        }

        source.PlayOneShot(clipToPlay, source.volume);

        if (preventOverlap)
        {
            collection.RegisterPlayback(clipToPlay, source.pitch);
        }
    }
}