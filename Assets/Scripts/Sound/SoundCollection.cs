using UnityEngine;

[CreateAssetMenu(fileName = "Collection", menuName = "Sound/Sound Collection")]
public class SoundCollection : ScriptableObject
{
    public AudioClip[] clips;

    public AudioClip GetRandomClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }
}