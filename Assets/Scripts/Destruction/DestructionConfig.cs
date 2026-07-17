using UnityEngine;

[CreateAssetMenu(menuName = "Destruction/DestructionConfig")]
public class DestructionConfig : ScriptableObject
{
    public SoundCollection ImpactLight;
    public SoundCollection ImpactHeavy;
    public SoundCollection Explode;
    public SoundCollection Zap;

    public float DebrisLifetimeMin = 3f;
    public float DebrisLifetimeMax = 5f;
    public float ExplosionForce = 5f;
    public float ExplosionRadius = 2f;
    public float DebrisRandomVelocity = 2f;
    public float ExplosionRandomVelocity = 7f;
}