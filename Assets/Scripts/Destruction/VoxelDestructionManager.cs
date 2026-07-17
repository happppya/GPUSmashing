using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class VoxelDestructionManager : MonoBehaviour
{
    
    public List<GameObject> AnchorObjects; // Starting points of island algorithm. Will explode if one is destroyed

    public float RequiredDestructionForce = 3.8f;
    [SerializeField] private DestructionConfig config;

    public event Action OnDamagedLight;
    public event Action OnDamagedCritical;
    public event Action OnExploded;

    private AudioSource audioSource;

    private NativeArray<VoxelData> gridData;
    private GameObject[] voxelInstances;
    private Dictionary<Collider, int> colliderToIndex;

    private int3 gridDimensions;
    private bool jobScheduled = false;
    private bool needsIslandDetection = false;
    private JobHandle bfsJobHandle;
    private NativeList<int> detachedIndices;

    // Health tracking
    private int totalInitialVoxels;
    private int currentActiveVoxels;
    private bool passedCritical = false;
    private bool passedLight = false;
    private bool passedExplode = false;

    public struct VoxelData
    {
        public int Index;
        public int3 GridPosition;
        public bool IsActive;
        public bool IsAnchor;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        InitializeGrid();
    }

    private float GetRandomDebrisLifetime()
    {
        return UnityEngine.Random.Range(config.DebrisLifetimeMin, config.DebrisLifetimeMax);
    }
    private void InitializeGrid()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        BoxCollider sampleCollider = transform.GetChild(0).GetComponent<BoxCollider>();
        Vector3 voxelSize = sampleCollider.size;

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        for (int i = 0; i < childCount; i++)
        {
            Vector3 pos = transform.GetChild(i).GetComponent<BoxCollider>().center;
            minBounds = Vector3.Min(minBounds, pos);
            maxBounds = Vector3.Max(maxBounds, pos);
        }

        gridDimensions = new int3(
            Mathf.RoundToInt((maxBounds.x - minBounds.x) / voxelSize.x) + 1,
            Mathf.RoundToInt((maxBounds.y - minBounds.y) / voxelSize.y) + 1,
            Mathf.RoundToInt((maxBounds.z - minBounds.z) / voxelSize.z) + 1
        );

        int totalGridSize = gridDimensions.x * gridDimensions.y * gridDimensions.z;

        gridData = new NativeArray<VoxelData>(totalGridSize, Allocator.Persistent);
        voxelInstances = new GameObject[totalGridSize];
        colliderToIndex = new Dictionary<Collider, int>(childCount);
        detachedIndices = new NativeList<int>(Allocator.Persistent);

        totalInitialVoxels = childCount;
        currentActiveVoxels = childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform voxelTransform = transform.GetChild(i);
            Collider voxelCollider = voxelTransform.GetComponent<BoxCollider>();
            Vector3 localPos = voxelTransform.GetComponent<BoxCollider>().center - minBounds;

            int3 gridPos = new int3(
                Mathf.RoundToInt(localPos.x / voxelSize.x),
                Mathf.RoundToInt(localPos.y / voxelSize.y),
                Mathf.RoundToInt(localPos.z / voxelSize.z)
            );

            int index = gridPos.x + (gridPos.y * gridDimensions.x) + (gridPos.z * gridDimensions.x * gridDimensions.y);
            bool isAnchor = AnchorObjects.Contains(voxelTransform.gameObject);

            gridData[index] = new VoxelData
            {
                Index = index,
                GridPosition = gridPos,
                IsActive = true,
                IsAnchor = isAnchor
            };

            voxelInstances[index] = voxelTransform.gameObject;
            colliderToIndex[voxelCollider] = index;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (passedExplode) return;

        if (collision.impulse.sqrMagnitude > RequiredDestructionForce)
        {
            Collider hitCollider = collision.GetContact(0).thisCollider;

            if (colliderToIndex.TryGetValue(hitCollider, out int index))
            {
                DestroyVoxel(index, collision.relativeVelocity);
            }

            // If not found, pick a random index from the existing dictionary values
            else if (colliderToIndex.Count > 0)
            {
                int randomIndex = colliderToIndex.Values.ElementAt(UnityEngine.Random.Range(0, colliderToIndex.Count));
                DestroyVoxel(randomIndex, collision.relativeVelocity);
            }

            SoundUtility.PlayRandomSound(config.ImpactHeavy, audioSource, true);

        }
        else if (collision.impulse.sqrMagnitude > 0.15f)
        {
            SoundUtility.PlayRandomSound(config.ImpactLight, audioSource, true);
        }
    }

    public void DestroyVoxel(int index, Vector3 impactVelocity)
    {
        CompleteJobIfNeeded();

        if (!gridData[index].IsActive) return;

        MarkVoxelInactive(index);

        // Convert the hit voxel to debris immediately
        MakeDebris(index, impactVelocity, false);

        if (gridData[index].IsAnchor)
        {
            ExplodeEverything();
            return;
        }

        needsIslandDetection = true;
        CheckHealthThresholds();
    }

    private void MarkVoxelInactive(int index)
    {
        VoxelData data = gridData[index];
        data.IsActive = false;
        gridData[index] = data;
        currentActiveVoxels--;
    }

    private void CompleteJobIfNeeded()
    {
        if (jobScheduled)
        {
            bfsJobHandle.Complete();
            jobScheduled = false;
            ProcessDetachedVoxels();
        }
    }

    private void ScheduleIslandDetection()
    {
        if (jobScheduled) return;

        detachedIndices.Clear();

        FindIslandsJob job = new FindIslandsJob
        {
            Grid = gridData,
            GridSize = gridDimensions,
            DetachedIndices = detachedIndices
        };

        bfsJobHandle = job.Schedule();
        jobScheduled = true;
    }

    private void LateUpdate()
    {
        if (jobScheduled && bfsJobHandle.IsCompleted)
        {
            CompleteJobIfNeeded();
        }

        if (needsIslandDetection && !jobScheduled)
        {
            ScheduleIslandDetection();
            needsIslandDetection = false;
        }
    }

    private void ProcessDetachedVoxels()
    {
        for (int i = 0; i < detachedIndices.Length; i++)
        {
            int index = detachedIndices[i];
            MarkVoxelInactive(index);

            // Floating islands drop with gravity
            MakeDebris(index, Vector3.down * 9.81f, false);
        }

        CheckHealthThresholds();
    }


    private void MakeDebris(int index, Vector3 force, bool isExplosion)
    {
        GameObject detachedVoxel = voxelInstances[index];
        if (detachedVoxel == null) return;

        Rigidbody rb = detachedVoxel.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = detachedVoxel.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
        }

        rb.isKinematic = false;

        if (isExplosion)
        {
            rb.AddExplosionForce(config.ExplosionForce, transform.position, config.ExplosionRadius, 0.5f, ForceMode.Impulse);
            rb.linearVelocity += UnityEngine.Random.insideUnitSphere * config.ExplosionRandomVelocity;
        }
        else
        {
            rb.AddForce(force, ForceMode.Impulse);
            rb.linearVelocity += UnityEngine.Random.insideUnitSphere * config.DebrisRandomVelocity;
        }

        // Clean up the debris after a delay
        Destroy(detachedVoxel, GetRandomDebrisLifetime());
    }

    private void CheckHealthThresholds()
    {
        if (passedExplode) return;

        float healthPercentage = (float)currentActiveVoxels / totalInitialVoxels;

        if (healthPercentage <= 0.8f && !passedLight)
        {
            passedLight = true;
            OnDamagedLight?.Invoke();
            SoundUtility.PlayRandomSound(config.Zap, audioSource, true);
        }

        if (healthPercentage <= 0.6f && !passedCritical)
        {
            passedCritical = true;
            OnDamagedCritical?.Invoke();
            SoundUtility.PlayRandomSound(config.Zap, audioSource, true);
        }

        if (healthPercentage <= 0.4f && !passedExplode)
        {
            passedExplode = true;
            ExplodeEverything();
        }
    }

    private void ExplodeEverything()
    {
        CompleteJobIfNeeded();
        OnExploded?.Invoke();

        SoundUtility.PlayRandomSound(config.Explode, audioSource, true);

        for (int i = 0; i < gridData.Length; i++)
        {
            if (gridData[i].IsActive)
            {
                MarkVoxelInactive(i);
                MakeDebris(i, Vector3.zero, true);
            }
        }

        // Disable the root's interactions
        Rigidbody mainRb = GetComponent<Rigidbody>();
        mainRb.isKinematic = true;
        gameObject.layer = LayerMask.NameToLayer("FractureChunk");
        
        Destroy(gameObject, config.DebrisLifetimeMax);
    }

    private void OnDestroy()
    {
        if (jobScheduled) bfsJobHandle.Complete();
        if (gridData.IsCreated) gridData.Dispose();
        if (detachedIndices.IsCreated) detachedIndices.Dispose();
    }
}