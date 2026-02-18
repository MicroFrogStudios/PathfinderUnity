using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Translates the global unity space to a grid based space for pathfinding purpose.
/// </summary>
public class Grid3D : MonoBehaviour
{
    public float CellMetric = 1f;
    public GameObject cubePrefab;
    public GameObject IndicatorPrefab;
    public Dictionary<Vector3Int, NavBlock> placedCubes = new();
    public Dictionary<Vector3Int, int> NavNodes;
    private bool editing = true;
    private List<GameObject> pathIndicators = new();
    public Pathfinder pf;

    public void Awake()
    {
        NavBlock[] startingBlocks = FindObjectsByType<NavBlock>(FindObjectsSortMode.None);
        foreach (NavBlock b in startingBlocks)
        {
            placedCubes.Add(WorldToGrid(b.transform.position),b);
            b.gridPos = WorldToGrid(b.transform.position);
        }
    }
    public void BakeNavMesh()
    {
        NavNodes = new();
        foreach (Vector3Int key in placedCubes.Keys){
            foreach (Vector3Int pos in placedCubes[key].Neighbors)
            {
                if (NavNodes.ContainsKey(pos))
                    continue;
                if (placedCubes.ContainsKey(pos))
                    continue;
                
                NavNodes.Add(pos, ComputeCost(pos));
                
            }
        }
    }


    private int ComputeCost(Vector3Int pos)
    {
        if (placedCubes.ContainsKey(pos + Vector3Int.down))
            return 10;

        if (placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.forward) ||
               placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.back) ||
               placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.left) ||
               placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.right) 
              )
            return 60;

        if (placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.forward + Vector3Int.left) ||
               placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.forward + Vector3Int.right) ||
               placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.back + Vector3Int.left) ||
               placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.back + Vector3Int.right))
            return 120;

        if (placedCubes.ContainsKey(pos + Vector3Int.forward) || 
                placedCubes.ContainsKey(pos + Vector3Int.back) ||
                placedCubes.ContainsKey(pos + Vector3Int.left) ||
                placedCubes.ContainsKey(pos + Vector3Int.right) ||
                placedCubes.ContainsKey(pos + Vector3Int.forward + Vector3Int.left)  ||
                placedCubes.ContainsKey(pos + Vector3Int.forward + Vector3Int.right) ||
                placedCubes.ContainsKey(pos + Vector3Int.back + Vector3Int.left) ||
                placedCubes.ContainsKey(pos + Vector3Int.back + Vector3Int.right))
            return 180;


        return 10000;
    }


    

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        worldPos /= CellMetric;
        return new Vector3Int(Mathf.FloorToInt(worldPos.x),
                                Mathf.FloorToInt(worldPos.y), 
                                Mathf.FloorToInt(worldPos.z));
    }

    public Vector3 GridToWorld(Vector3Int GridPos)
    {
        return new Vector3(GridPos.x + 0.5f,GridPos.y + 0.5f,GridPos.z + 0.5f)*CellMetric;
    }

    public Vector3 AlignToGrid(Vector3 pos)
    {
        return GridToWorld(WorldToGrid(pos));
    }

    public void Update()
    {

        HandleLeftClick();
        HandleRightClick();



    }

    private void HandleLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {

                if (editing)
                {
                    //Debug.Log(hit.point + "|" + WorldToGrid(hit.point));
                    GameObject newCube = Instantiate(cubePrefab);
                    newCube.transform.position = AlignToGrid(hit.point);
                    newCube.GetComponent<NavBlock>().gridPos = WorldToGrid(hit.point);
                    placedCubes.Add(newCube.GetComponent<NavBlock>().gridPos, newCube.GetComponent<NavBlock>());
                }
                else
                {
                    pathIndicators.ForEach(i =>  Destroy(i) );
                    pathIndicators = new();
                    Stack<Vector3Int> path = pf.PathTo(WorldToGrid(pf.transform.position), WorldToGrid(hit.point), NavNodes);
                    if (path == null)
                    {
                        Debug.Log("No path found");
                        return;
                    }
                    Debug.Log(path.Count);
                    while (path.Count > 0)
                    {
                        Vector3Int nextNode = path.Pop();
                        GameObject indicator = Instantiate(IndicatorPrefab);
                        indicator.transform.position = GridToWorld(nextNode);
                        pathIndicators.Add(indicator);

                    }
                }
            }
        }
    }

    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (editing)
                {
                    if (hit.collider.gameObject.CompareTag("PlayerCreatedCube"))
                    {
                        placedCubes.Remove(hit.collider.gameObject.GetComponent<NavBlock>().gridPos);
                        Destroy(hit.collider.gameObject);
                    }
                }
                else
                {
                    // Move player to point
                }
            }
        }
    }

    public void OnEditToggle(bool toggle)
    {
       
        editing = toggle;
        if (!editing)
        {
            BakeNavMesh();
        }
        else
        {
            pathIndicators.ForEach(i => Destroy(i));
            pathIndicators = new();
        }
    }

}
