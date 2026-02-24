using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{

    public Stack<Vector3Int> currentPath = new();
    public Vector3 currentObjective = Vector3.negativeInfinity;

    public int LastNodeSearchCount = 0;
    public bool pathingCorrections = true;
    public enum EnumHeuristic
    {
        NoHeuristic = 0,
        Chebyshev = 1,
        Manhatan = 2,
        Euclidean = 3

    }

    [SerializeField]
    public EnumHeuristic heuristicSelection;
    private Dictionary<EnumHeuristic, Func<Vector3Int, Vector3Int, int>> HeuristicsDict;


    private void Awake()
    {
        HeuristicsDict = new()
        {
            [EnumHeuristic.NoHeuristic] = NoHeuristic,
            [EnumHeuristic.Chebyshev] = ChebyshevDistance,
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

    int NoHeuristic(Vector3Int origin, Vector3Int target)
    {
        //Equates Djistra algortihm
        return 0;
    }

    int ChebyshevDistance(Vector3Int origin, Vector3Int target)
    {
        // Chebyshev distance
        return Mathf.Max(Mathf.Abs(target.x - origin.x), Mathf.Abs(target.y - origin.y), Mathf.Abs(target.z - origin.z))*10;
    }

    int ManhatanDistance(Vector3Int origin, Vector3Int target)
    {
        //Manhatan Distance
        return Mathf.Abs(target.x - origin.x) + Mathf.Abs(target.y - origin.y) + Mathf.Abs(target.z - origin.z)*10;
    }
    int RoundedEuclideanDistance(Vector3Int origin, Vector3Int target)
    {
        return Mathf.FloorToInt(Vector3Int.Distance(origin, target)*10);
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
        LastNodeSearchCount = 0;
        while (NodeSet.Count > 0)
        {
            LastNodeSearchCount++;
            Vector3Int currentNode = NodeSet.Aggregate((acc, x) => CombinedCostMap[x] < CombinedCostMap[acc] ? x : acc);
            if (currentNode == end)
            {
                return ReconstructPath(predecesorMap, currentNode);
            }

            NodeSet.Remove(currentNode);
           
            List<Vector3Int> neighbors = CheckValidNeighbors(currentNode,NavNodes);
            foreach (Vector3Int neighborNode in neighbors) {
                int TentativeCumulativeCost = baseCostMap[currentNode] + NavNodes[neighborNode];


                if (pathingCorrections)
                {
                    if (heuristicSelection == EnumHeuristic.Manhatan)
                    {
                        //make manhatan zigzag
                        int endDiagonal = Mathf.Abs(Mathf.Abs(end.x- neighborNode.x) - Mathf.Abs(end.z- neighborNode.z));
                        TentativeCumulativeCost += endDiagonal;
                        Debug.Log(TentativeCumulativeCost);
                    }
                    else if (heuristicSelection != EnumHeuristic.NoHeuristic)
                    {
                        //correct zigzag
                        if (neighborNode.y > currentNode.y)
                        {
                            TentativeCumulativeCost += 10;
                            if (neighborNode.x != currentNode.x)
                                TentativeCumulativeCost += 1;
                            if (neighborNode.z != currentNode.z)
                                TentativeCumulativeCost += 1;

                        }
                        TentativeCumulativeCost += !(neighborNode.x == currentNode.x || neighborNode.z == currentNode.z) ? 10 : 0;
                    }
                }
                
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


        return new();
    }

    private List<Vector3Int> CheckValidNeighbors(Vector3Int gridPos, Dictionary<Vector3Int, int>  NavNodes)
    {
        if (heuristicSelection == EnumHeuristic.Manhatan)
        {
            //check only orthogonal neighbors
            List<Vector3Int> neighbors = new()
                {
                gridPos + Vector3Int.up,
                gridPos + Vector3Int.down,
                gridPos + Vector3Int.left,
                gridPos + Vector3Int.right,
                gridPos + Vector3Int.forward,
                gridPos + Vector3Int.back,
                 };
            return neighbors.FindAll((x)=> NavNodes.ContainsKey(x));
        }

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
                }
            }
        }
        return neighborsList;
    }

}
