using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{

    private enum EnumHeuristic
    {
        Chebyshev,
        NatChebyhev,
        Manhatan,
        Euclidean

    }

    [SerializeField]
    private EnumHeuristic heuristicSelection;
    private Dictionary<EnumHeuristic, Func<Vector3Int, Vector3Int, int>> HeuristicsDict;


    private void Awake()
    {
        HeuristicsDict = new()
        {
            [EnumHeuristic.Chebyshev] = ChebyshevDistance,
            [EnumHeuristic.NatChebyhev] = NaturalChebyshevDistance,
            [EnumHeuristic.Manhatan] = ManhatanDistance,
            [EnumHeuristic.Euclidean] = RoundedEuclideanDistance
        };

    }
    void Start()
    {
        
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

    int NaturalChebyshevDistance(Vector3Int origin,Vector3Int target)
    {
        int baseDistance = ChebyshevDistance(origin, target);
        if (!(origin.x == target.x || origin.z == target.z))
        {
            baseDistance++;
        } 
        return baseDistance;
    }

    private Stack<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> previousPath, Vector3Int currentNode)
    {
        Stack<Vector3Int> totalPath = new();
        totalPath.Push(currentNode);

        while (previousPath.ContainsKey(currentNode))
        {
            currentNode = previousPath[currentNode];
            totalPath.Push(currentNode);
        }
        return totalPath;
    }

    public Stack<Vector3Int> PathTo(Vector3Int start, Vector3Int end, Dictionary<Vector3Int, int> NavNodes)
    {
        //A* Algorithm


        Func<Vector3Int, Vector3Int, int> heuristic = HeuristicsDict[heuristicSelection];
        // Nodos que aun quedan por explorar
        HashSet<Vector3Int> NodeSet = new() { start };

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
           
            List<Vector3Int> neighbors = CheckValidNeighbors(currentNode,NavNodes);
            foreach (Vector3Int neighborNode in neighbors) {
                int TentativeCumulativeCost = baseCostMap[currentNode] + NavNodes[neighborNode] + (neighborNode.y > currentNode.y ? 10 : 0);
                
                if (!baseCostMap.ContainsKey(neighborNode) || TentativeCumulativeCost < baseCostMap[neighborNode] ) {
                    predecesorMap[neighborNode] = currentNode;
                    baseCostMap[neighborNode] = TentativeCumulativeCost;
                    CombinedCostMap[neighborNode] = TentativeCumulativeCost + heuristic(neighborNode,end);
                    if (!NodeSet.Contains(neighborNode)){
                        NodeSet.Add(neighborNode);
                    }
                }
            }
        }


        return null;
    }

    private List<Vector3Int> CheckValidNeighbors(Vector3Int gridPos, Dictionary<Vector3Int, int>  NavNodes)
    {
        List<Vector3Int> neighborsList = new();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // Skip the center point (0,0,0)
                    if (x == 0 && y == 0 && z == 0) continue;

                    Vector3Int neighborPos = gridPos + new Vector3Int(x, y, z);
                    if ( NavNodes.ContainsKey(neighborPos))
                    {
                        neighborsList.Add(neighborPos);

                    }
                    // Do your logic here (e.g., check if a tile exists at neighborPos)
                    
                }
            }
        }
        return neighborsList;
    }

}
