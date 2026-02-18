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
    
}
