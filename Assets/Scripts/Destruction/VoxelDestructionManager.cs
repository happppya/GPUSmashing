using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Attach this to a lightweight GameObject on the Voxel (e.g., alongside the BoxCollider)
/// to report collisions back to the central manager.
/// </summary>
public class VoxelCollisionReporter : MonoBehaviour
{
    public VoxelDestructionManager Manager;
    public int GridIndex;

    private void OnCollisionEnter(Collision collision)
    {
        // Simple threshold to prevent breaking on gentle touches
        if (collision.relativeVelocity.sqrMagnitude > 20f)
        {
            Debug.Log($"Destroying self {GridIndex} with vel {collision.relativeVelocity}");
            Manager.DestroyVoxel(GridIndex, collision.relativeVelocity);
        }
    }
}

public class VoxelDestructionManager : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Drag the parent object containing all voxels here.")]
    public Transform VoxelRoot;
    [Tooltip("Drag the specific voxel GameObjects that act as unbreakable anchors.")]
    public List<GameObject> AnchorObjects;

    private NativeArray<VoxelData> _gridData;
    private GameObject[] _voxelInstances;
    private Rigidbody[] _voxelRigidbodies;

    private int3 _gridDimensions;
    private bool _jobScheduled = false;
    private bool _needsIslandDetection = false; // NEW: Flag to batch job scheduling
    private JobHandle _bfsJobHandle;
    private NativeList<int> _detachedIndices;

    // Phase 1: Unmanaged Voxel Struct
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
        int childCount = VoxelRoot.childCount;
        if (childCount == 0) return;

        // Extract voxel size from the first child's BoxCollider
        BoxCollider sampleCollider = VoxelRoot.GetChild(0).GetComponent<BoxCollider>();
        Vector3 voxelSize = sampleCollider.size;

        // Determine bounds to build the 3D grid
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        for (int i = 0; i < childCount; i++)
        {
            Vector3 pos = VoxelRoot.GetChild(i).GetComponent<BoxCollider>().center;
            minBounds = Vector3.Min(minBounds, pos);
            maxBounds = Vector3.Max(maxBounds, pos);
        }

        // Calculate grid dimensions
        _gridDimensions = new int3(
            Mathf.RoundToInt((maxBounds.x - minBounds.x) / voxelSize.x) + 1,
            Mathf.RoundToInt((maxBounds.y - minBounds.y) / voxelSize.y) + 1,
            Mathf.RoundToInt((maxBounds.z - minBounds.z) / voxelSize.z) + 1
        );

        int totalVoxels = _gridDimensions.x * _gridDimensions.y * _gridDimensions.z;

        _gridData = new NativeArray<VoxelData>(totalVoxels, Allocator.Persistent);
        _voxelInstances = new GameObject[totalVoxels];
        _voxelRigidbodies = new Rigidbody[totalVoxels];
        _detachedIndices = new NativeList<int>(Allocator.Persistent);

        // Populate Grid
        for (int i = 0; i < childCount; i++)
        {
            Transform voxelTransform = VoxelRoot.GetChild(i);
            Vector3 localPos = voxelTransform.GetComponent<BoxCollider>().center - minBounds;

            int3 gridPos = new int3(
                Mathf.RoundToInt(localPos.x / voxelSize.x),
                Mathf.RoundToInt(localPos.y / voxelSize.y),
                Mathf.RoundToInt(localPos.z / voxelSize.z)
            );

            // Phase 2: Mathematical 1D Flattening
            int index = gridPos.x + (gridPos.y * _gridDimensions.x) + (gridPos.z * _gridDimensions.x * _gridDimensions.y);

            bool isAnchor = AnchorObjects.Contains(voxelTransform.gameObject);

            _gridData[index] = new VoxelData
            {
                Index = index,
                GridPosition = gridPos,
                IsActive = true,
                IsAnchor = isAnchor
            };

            // Setup References and Physics
            _voxelInstances[index] = voxelTransform.gameObject;

            Rigidbody rb = voxelTransform.gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = voxelTransform.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Kinematic while attached
            _voxelRigidbodies[index] = rb;

            VoxelCollisionReporter reporter = voxelTransform.gameObject.AddComponent<VoxelCollisionReporter>();
            reporter.Manager = this;
            reporter.GridIndex = index;
        }
    }

    // Phase 3: High-Performance Impact
    public void DestroyVoxel(int index, Vector3 impactVelocity)
    {
        // FIX: Force complete any running job before mutating the NativeArray to prevent InvalidOperationException
        CompleteJobIfNeeded();

        if (!_gridData[index].IsActive) return;

        // Disable locally
        VoxelData data = _gridData[index];
        data.IsActive = false;
        _gridData[index] = data; // SAFE TO WRITE NOW

        // Visual destruction (Disable renderer/collider immediately)
        //_voxelInstances[index].SetActive(false);
        Destroy(_voxelInstances[index]);

        // FIX: Batch job scheduling. Don't schedule immediately, wait for LateUpdate.
        _needsIslandDetection = true;
    }

    // NEW: Helper method to safely sync the background thread with the main thread
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
        // 1. If a job is currently running and finished, complete it and process results
        if (_jobScheduled && _bfsJobHandle.IsCompleted)
        {
            CompleteJobIfNeeded();
        }

        // 2. Schedule a new job ONLY once per frame, even if multiple collisions occurred
        if (_needsIslandDetection && !_jobScheduled)
        {
            ScheduleIslandDetection();
            _needsIslandDetection = false;
        }
    }

    // Phase 5: Flat Hierarchy Separation & Physics
    private void ProcessDetachedVoxels()
    {
        for (int i = 0; i < _detachedIndices.Length; i++)
        {
            int index = _detachedIndices[i];

            // Mark as inactive in the grid so they are ignored by future BFS passes
            VoxelData data = _gridData[index];
            data.IsActive = false;
            _gridData[index] = data;

            // Apply separation physics
            Rigidbody rb = _voxelRigidbodies[index];
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            }
        }
    }

    private void OnDestroy()
    {
        if (_jobScheduled) _bfsJobHandle.Complete();
        if (_gridData.IsCreated) _gridData.Dispose();
        if (_detachedIndices.IsCreated) _detachedIndices.Dispose();
    }
}

// Phase 4: Asynchronous Island Detection (Burst Compiled)
[BurstCompile]
public struct FindIslandsJob : IJob
{
    [ReadOnly] public NativeArray<VoxelDestructionManager.VoxelData> Grid;
    [ReadOnly] public int3 GridSize;
    [WriteOnly] public NativeList<int> DetachedIndices;

    public void Execute()
    {
        int totalVoxels = GridSize.x * GridSize.y * GridSize.z;
        NativeArray<bool> visited = new NativeArray<bool>(totalVoxels, Allocator.Temp);
        NativeQueue<int> queue = new NativeQueue<int>(Allocator.Temp);

        // 1. Find all active anchors and enqueue them
        for (int i = 0; i < Grid.Length; i++)
        {
            if (Grid[i].IsActive && Grid[i].IsAnchor)
            {
                queue.Enqueue(i);
                visited[i] = true;
            }
        }

        // 6 neighbor directions (x±1, y±1, z±1)
        NativeArray<int3> directions = new NativeArray<int3>(6, Allocator.Temp);
        directions[0] = new int3(1, 0, 0); directions[1] = new int3(-1, 0, 0);
        directions[2] = new int3(0, 1, 0); directions[3] = new int3(0, -1, 0);
        directions[4] = new int3(0, 0, 1); directions[5] = new int3(0, 0, -1);

        // 2. BFS Traversal
        while (queue.TryDequeue(out int currentIndex))
        {
            int3 pos = Grid[currentIndex].GridPosition;

            for (int d = 0; d < 6; d++)
            {
                int3 neighborPos = pos + directions[d];

                // Bounds check
                if (neighborPos.x >= 0 && neighborPos.x < GridSize.x &&
                    neighborPos.y >= 0 && neighborPos.y < GridSize.y &&
                    neighborPos.z >= 0 && neighborPos.z < GridSize.z)
                {
                    int neighborIndex = neighborPos.x + (neighborPos.y * GridSize.x) + (neighborPos.z * GridSize.x * GridSize.y);

                    if (!visited[neighborIndex] && Grid[neighborIndex].IsActive)
                    {
                        visited[neighborIndex] = true;
                        queue.Enqueue(neighborIndex);
                    }
                }
            }
        }

        // 3. Find active unvisited voxels (these are the floating islands)
        for (int i = 0; i < Grid.Length; i++)
        {
            if (Grid[i].IsActive && !visited[i])
            {
                DetachedIndices.Add(i);
            }
        }

        visited.Dispose();
        queue.Dispose();
        directions.Dispose();
    }
}