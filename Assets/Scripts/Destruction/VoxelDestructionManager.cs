using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class VoxelDestructionManager : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Drag the specific voxel GameObjects that act as unbreakable anchors.")]
    public List<GameObject> AnchorObjects;

    [Header("Debris Settings")]
    [Tooltip("How many seconds before a detached voxel disappears?")]
    public float DebrisLifetime = 3f;
    public float ExplosionForce = 5f;
    public float ExplosionRadius = 2f;
    [Tooltip("Amount of random velocity applied to standard broken voxels.")]
    public float DebrisRandomVelocity = 2f;
    [Tooltip("Amount of random velocity applied during the final 30% explosion.")]
    public float ExplosionRandomVelocity = 7f;

    [Header("Damage Events")]
    public UnityEvent OnDamagedLight;
    public UnityEvent OnDamagedCritical;
    public UnityEvent OnExploded;

    private NativeArray<VoxelData> _gridData;
    private GameObject[] _voxelInstances;
    private Dictionary<Collider, int> _colliderToIndex;

    private int3 _gridDimensions;
    private bool _jobScheduled = false;
    private bool _needsIslandDetection = false;
    private JobHandle _bfsJobHandle;
    private NativeList<int> _detachedIndices;

    // Health tracking
    private int _totalInitialVoxels;
    private int _currentActiveVoxels;
    private bool _passedCritical = false;
    private bool _passedLight = false;
    private bool _passedExplode = false;

    public struct VoxelData
    {
        public int Index;
        public int3 GridPosition;
        public bool IsActive;
        public bool IsAnchor;
    }

    void Start()
    {
        InitializeGrid();
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

        _gridDimensions = new int3(
            Mathf.RoundToInt((maxBounds.x - minBounds.x) / voxelSize.x) + 1,
            Mathf.RoundToInt((maxBounds.y - minBounds.y) / voxelSize.y) + 1,
            Mathf.RoundToInt((maxBounds.z - minBounds.z) / voxelSize.z) + 1
        );

        int totalGridSize = _gridDimensions.x * _gridDimensions.y * _gridDimensions.z;

        _gridData = new NativeArray<VoxelData>(totalGridSize, Allocator.Persistent);
        _voxelInstances = new GameObject[totalGridSize];
        _colliderToIndex = new Dictionary<Collider, int>(childCount);
        _detachedIndices = new NativeList<int>(Allocator.Persistent);

        _totalInitialVoxels = childCount;
        _currentActiveVoxels = childCount;

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

            int index = gridPos.x + (gridPos.y * _gridDimensions.x) + (gridPos.z * _gridDimensions.x * _gridDimensions.y);
            bool isAnchor = AnchorObjects.Contains(voxelTransform.gameObject);

            _gridData[index] = new VoxelData
            {
                Index = index,
                GridPosition = gridPos,
                IsActive = true,
                IsAnchor = isAnchor
            };

            _voxelInstances[index] = voxelTransform.gameObject;
            _colliderToIndex[voxelCollider] = index;
        }
    }

    // Integrated Collision Routing
    private void OnCollisionEnter(Collision collision)
    {
        // Stop calculating if we are already dead
        if (_passedExplode) return;

        if (collision.impulse.sqrMagnitude > 10f)
        {
            Collider hitCollider = collision.GetContact(0).thisCollider;

            if (_colliderToIndex.TryGetValue(hitCollider, out int index))
            {
                DestroyVoxel(index, collision.relativeVelocity);
            }
        }
    }

    public void DestroyVoxel(int index, Vector3 impactVelocity)
    {
        CompleteJobIfNeeded();

        if (!_gridData[index].IsActive) return;

        MarkVoxelInactive(index);

        // Convert the hit voxel to debris immediately
        MakeDebris(index, impactVelocity, false);

        if (_gridData[index].IsAnchor)
        {
            ExplodeEverything();
            return;
        }

        _needsIslandDetection = true;
        CheckHealthThresholds();
    }

    private void MarkVoxelInactive(int index)
    {
        VoxelData data = _gridData[index];
        data.IsActive = false;
        _gridData[index] = data;
        _currentActiveVoxels--;
    }

    private void CompleteJobIfNeeded()
    {
        if (_jobScheduled)
        {
            _bfsJobHandle.Complete();
            _jobScheduled = false;
            ProcessDetachedVoxels();
        }
    }

    private void ScheduleIslandDetection()
    {
        if (_jobScheduled) return;

        _detachedIndices.Clear();

        FindIslandsJob job = new FindIslandsJob
        {
            Grid = _gridData,
            GridSize = _gridDimensions,
            DetachedIndices = _detachedIndices
        };

        _bfsJobHandle = job.Schedule();
        _jobScheduled = true;
    }

    private void LateUpdate()
    {
        if (_jobScheduled && _bfsJobHandle.IsCompleted)
        {
            CompleteJobIfNeeded();
        }

        if (_needsIslandDetection && !_jobScheduled)
        {
            ScheduleIslandDetection();
            _needsIslandDetection = false;
        }
    }

    private void ProcessDetachedVoxels()
    {
        for (int i = 0; i < _detachedIndices.Length; i++)
        {
            int index = _detachedIndices[i];
            MarkVoxelInactive(index);

            // Floating islands drop with gravity
            MakeDebris(index, Vector3.down * 9.81f, false);
        }

        CheckHealthThresholds();
    }

    // --- Debris & Destruction Logic ---

    private void MakeDebris(int index, Vector3 force, bool isExplosion)
    {
        GameObject detachedVoxel = _voxelInstances[index];
        if (detachedVoxel == null) return;

        Rigidbody rb = detachedVoxel.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = detachedVoxel.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
        }

        if (isExplosion)
        {
            rb.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius, 0.5f, ForceMode.Impulse);
            // Inject heavy random velocity for maximum spread
            rb.linearVelocity += UnityEngine.Random.insideUnitSphere * ExplosionRandomVelocity;
        }
        else
        {
            rb.AddForce(force, ForceMode.Impulse);
            // Inject slight random velocity so cascading pieces don't fall perfectly flat
            rb.linearVelocity += UnityEngine.Random.insideUnitSphere * DebrisRandomVelocity;
        }

        // Clean up the debris after a delay
        Destroy(detachedVoxel, DebrisLifetime);
    }

    private void CheckHealthThresholds()
    {
        if (_passedExplode) return;

        float healthPercentage = (float)_currentActiveVoxels / _totalInitialVoxels;

        if (healthPercentage <= 0.8f && !_passedCritical)
        {
            _passedCritical = true;
            OnDamagedCritical.Invoke();
        }

        if (healthPercentage <= 0.6f && !_passedLight)
        {
            _passedLight = true;
            OnDamagedLight.Invoke();
        }

        if (healthPercentage <= 0.3f && !_passedExplode)
        {
            _passedExplode = true;
            ExplodeEverything();
        }
    }

    private void ExplodeEverything()
    {
        CompleteJobIfNeeded();
        OnExploded.Invoke();

        for (int i = 0; i < _gridData.Length; i++)
        {
            if (_gridData[i].IsActive)
            {
                MarkVoxelInactive(i);
                MakeDebris(i, Vector3.zero, true);
            }
        }

        // Disable the root's interactions
        Rigidbody mainRb = GetComponent<Rigidbody>();
        if (mainRb != null) mainRb.isKinematic = true;

        Destroy(gameObject, DebrisLifetime);
    }

    private void OnDestroy()
    {
        if (_jobScheduled) _bfsJobHandle.Complete();
        if (_gridData.IsCreated) _gridData.Dispose();
        if (_detachedIndices.IsCreated) _detachedIndices.Dispose();
    }
}