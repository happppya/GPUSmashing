using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundCollection", menuName = "Audio/Sound Collection")]
public class SoundCollection : ScriptableObject
{
    [SerializeField] public AudioClip[] Clips;
    [NonSerialized] private float nextAllowedPlayTime = -1f;

    public AudioClip GetRandomClip()
    {
        return Clips[UnityEngine.Random.Range(0, Clips.Length)];
    }

    public bool CanPlay(float overlapThreshold = 0f)
    {
        Debug.Log($"Can play? {Time.time}, {(nextAllowedPlayTime - overlapThreshold)}, {overlapThreshold}");
        return Time.time >= (nextAllowedPlayTime - overlapThreshold);
    }

    public void RegisterPlayback(AudioClip clip, float pitch)
    {
        if (clip == null) return;

        float safePitch = pitch != 0f ? Mathf.Abs(pitch) : 1f;
        float duration = clip.length / safePitch;

        nextAllowedPlayTime = Time.time + duration;
    }
}