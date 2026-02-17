using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NavBlock : MonoBehaviour
{

    public Vector3Int gridPos;

    private List<Vector3Int> neighbors = null;

    public List<Vector3Int> Neighbors
    {
        get
        {
            if (neighbors == null)
                ComputeNeighbors();

            return neighbors;

        }
    }

    public void ComputeNeighbors()
    {
        neighbors = new()
        {
        gridPos + Vector3Int.up,
        gridPos + Vector3Int.down,
        gridPos + Vector3Int.left,
        gridPos + Vector3Int.right,
        gridPos + Vector3Int.forward,
        gridPos + Vector3Int.back,

        gridPos + new Vector3Int(1, 1, 0),   gridPos + new Vector3Int(1, -1, 0),
        gridPos + new Vector3Int(-1, 1, 0),  gridPos + new Vector3Int(-1, -1, 0),
        gridPos + new Vector3Int(1, 0, 1),   gridPos + new Vector3Int(1, 0, -1),
        gridPos + new Vector3Int(-1, 0, 1),  gridPos + new Vector3Int(-1, 0, -1),
        gridPos + new Vector3Int(0, 1, 1),   gridPos + new Vector3Int(0, 1, -1),
        gridPos + new Vector3Int(0, -1, 1),  gridPos + new Vector3Int(0, -1, -1),

        // 8 Corners
        gridPos + new Vector3Int(1, 1, 1),   gridPos + new Vector3Int(1, 1, -1),
        gridPos + new Vector3Int(1, -1, 1),  gridPos + new Vector3Int(1, -1, -1),
        gridPos + new Vector3Int(-1, 1, 1),  gridPos + new Vector3Int(-1, 1, -1),
        gridPos + new Vector3Int(-1, -1, 1), gridPos + new Vector3Int(-1, -1, -1)


        };
    }
    void CheckNeighbors(Vector3Int gridPos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // Skip the center point (0,0,0)
                    if (x == 0 && y == 0 && z == 0) continue;

                    Vector3Int neighborPos = gridPos + new Vector3Int(x, y, z);

                    // Do your logic here (e.g., check if a tile exists at neighborPos)
                    Debug.Log($"Checking neighbor at: {neighborPos}");
                }
            }
        }
    }
}
