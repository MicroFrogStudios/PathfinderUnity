using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{

    Tilemap map;
    void Start()
    {
        map = GetComponent<Tilemap>();
        TileBase tile = map.GetTile(new Vector3Int(1, -1, 0));
        PathTo(Vector3Int.zero, new Vector3Int(1, -1, 0), ChebyshevDistance);
    }

    // Update is called once per frame
    void Update()
    {

    }

    int ChebyshevDistance(Vector3Int origin, Vector3Int target)
    {
        // Chebyshev distance
        return Mathf.Max(Mathf.Abs(target.x - origin.x), Mathf.Abs(target.y - origin.y), Mathf.Abs(target.z - origin.z));
    }

    int ManhatanDistance(Vector3Int origin, Vector3Int target)
    {
        //Manhatan Distance
        return Mathf.Abs(target.x - origin.x) + Mathf.Abs(target.y - origin.y) + Mathf.Abs(target.z - origin.z);
    }
    int RoundedEuclideanDistance(Vector3Int origin, Vector3Int target)
    {
        return Mathf.FloorToInt(Vector3Int.Distance(origin, target));
    }

    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> previousPath, Vector3Int currentNode)
    {
        List<Vector3Int> totalPath = new() { currentNode };

        while (previousPath.ContainsKey(currentNode))
        {
            currentNode = previousPath[currentNode];
            totalPath.Prepend(currentNode);
        }
        return totalPath;
    }

    List<Vector3Int> PathTo(Vector3Int start, Vector3Int end, Func<Vector3Int, Vector3Int, int> heuristic)
    {
        //A* Algorithm

        // Nodos que aun quedan por explorar
        List<Vector3Int> NodeSet = new() { start };

        // mapa asociando nodos con el nodo desde mas barato desde el que es accesible.
        Dictionary<Vector3Int, Vector3Int> predecesorMap = new();

        // Mapa de coste base
        Dictionary<Vector3Int, int> baseCostMap = new() { [start] = 0 };

        // Mapa de coste base + Heuristica
        Dictionary<Vector3Int, int> CombinedCostMap = new() { [start] = heuristic(start, end) };

        while (NodeSet.Count > 0)
        {

            Vector3Int currentNode = NodeSet.Aggregate((acc, x) => CombinedCostMap[x] < CombinedCostMap[acc] ? x : acc);
            if (currentNode == end)
            {
                return ReconstructPath(predecesorMap, currentNode);
            }

            NodeSet.Remove(currentNode);
            map.GetTilesBlock(new BoundsInt(currentNode, Vector3Int.one));
            TileBase tile = map.GetTile(currentNode);
            List<TileBase> neighbors = new() { };


        }


        return null;
    }
}
