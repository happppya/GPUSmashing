using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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

        for (int i = 0; i < Grid.Length; i++)
        {
            if (Grid[i].IsActive && Grid[i].IsAnchor)
            {
                queue.Enqueue(i);
                visited[i] = true;
            }
        }

        NativeArray<int3> directions = new NativeArray<int3>(6, Allocator.Temp);
        directions[0] = new int3(1, 0, 0); directions[1] = new int3(-1, 0, 0);
        directions[2] = new int3(0, 1, 0); directions[3] = new int3(0, -1, 0);
        directions[4] = new int3(0, 0, 1); directions[5] = new int3(0, 0, -1);

        while (queue.TryDequeue(out int currentIndex))
        {
            int3 pos = Grid[currentIndex].GridPosition;

            for (int d = 0; d < 6; d++)
            {
                int3 neighborPos = pos + directions[d];

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