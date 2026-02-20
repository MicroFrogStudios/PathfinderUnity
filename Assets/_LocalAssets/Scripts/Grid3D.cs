using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
    private bool allowClimbing = true;
    private bool allowCorners = true;
    
    private List<GameObject> pathIndicators = new();
    

    public Pathfinder pf;
    public Toggle editToggle, ClimbToggle, CornerToggle, correctionsToggle;
    public TMP_Text heurLabel, editControlUI,pathControlUI,diagUI;

    [Header("Costs")]
    public int floorCost = 10;
    public int wallCost = 80;

    public void Awake()
    {
        NavBlock[] startingBlocks = FindObjectsByType<NavBlock>(FindObjectsSortMode.None);
        foreach (NavBlock b in startingBlocks)
        {
            placedCubes.Add(WorldToGrid(b.transform.position),b);
            b.gridPos = WorldToGrid(b.transform.position);
        }
        
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
                int cost = ComputeCost(pos);
                if (cost > 0)
                    NavNodes.Add(pos,cost);
                
            }
        }
    }


    private int ComputeCost(Vector3Int pos)
    {
        // has floor straight down
        if (placedCubes.ContainsKey(pos + Vector3Int.down))
            return floorCost;
        // floor down in the corners
        //if (placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.forward) ||
        //       placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.back) ||
        //       placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.left) ||
        //       placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.right) 
        //      )
        //    return 60;

        // 
        //if (placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.forward + Vector3Int.left) ||
        //       placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.forward + Vector3Int.right) ||
        //       placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.back + Vector3Int.left) ||
        //       placedCubes.ContainsKey(pos + Vector3Int.down + Vector3Int.back + Vector3Int.right))
        //    return 120;

        if(!allowClimbing) return -1;

        // Lateral orthogonal walls
        if (placedCubes.ContainsKey(pos + Vector3Int.forward) || 
                placedCubes.ContainsKey(pos + Vector3Int.back) ||
                placedCubes.ContainsKey(pos + Vector3Int.left) ||
                placedCubes.ContainsKey(pos + Vector3Int.right))
            return wallCost;

        if (!allowCorners) return -1;

        // Lateral corner walls
        if (placedCubes.ContainsKey(pos + Vector3Int.forward + Vector3Int.left) ||
                placedCubes.ContainsKey(pos + Vector3Int.forward + Vector3Int.right) ||
                placedCubes.ContainsKey(pos + Vector3Int.back + Vector3Int.left) ||
                placedCubes.ContainsKey(pos + Vector3Int.back + Vector3Int.right))
            return wallCost;

        return -1;
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

        
        HandleRightClick();
       

        if (pf.currentPath.Count > 0)
        {

            int currentNodeCost = NavNodes[pf.currentPath.Peek()];
            pf.transform.position = Vector3.MoveTowards(pf.transform.position, GridToWorld(pf.currentPath.Peek()), Mathf.Clamp(5f*floorCost/ currentNodeCost,2f,5f) * Time.deltaTime);

            if (Vector3.Distance(pf.transform.position, GridToWorld(pf.currentPath.Peek())) < 0.01)
            {
                GridToWorld(pf.currentPath.Pop());

            }
            return;
        }

        HandleLeftClick();
        HandleToggles();
    }

    private void HandleToggles()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
            Application.Quit();
            

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnEditToggle(!editing);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnClimbToggle(!allowClimbing);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnCornerToggle(!allowCorners);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnCorrectionsToggle();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnHeuristicToggle();
        }
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
                    ProduceIndicators(hit.point);
                }
            }
        }
    }

    /// <summary>
    /// Calcula un camino hacia el objetivo y muestra una visualización del camino si es válido
    /// </summary>
    /// <param name="objective"></param>
    private void ProduceIndicators( Vector3 objective)
    {
        pathIndicators.ForEach(i => Destroy(i));
        pathIndicators = new();
        long startCalc = System.Diagnostics.Stopwatch.GetTimestamp();
        Stack<Vector3Int> path = pf.PathTo(WorldToGrid(pf.transform.position), WorldToGrid(objective), NavNodes);
        long calcTime = System.Diagnostics.Stopwatch.GetTimestamp() - startCalc;
        //double milsec = (calcTime / System.Diagnostics.Stopwatch.Frequency) * 1000000;
        if (path.Count <= 0)
        {
            Debug.Log("No path found");
            diagUI.text = "No path found";
            return;
        }
        Debug.Log(pf.heuristicSelection.ToString() +" pathfinding took " + calcTime.ToString() + " ticks and searched " + pf.LastNodeSearchCount + " nodes");
        diagUI.text = "Searched " + pf.LastNodeSearchCount + " nodes in " + calcTime.ToString() + " ticks.";
        while (path.Count > 0)
        {
            Vector3Int nextNode = path.Pop();
            GameObject indicator = Instantiate(IndicatorPrefab);
            indicator.transform.position = GridToWorld(nextNode);
            pathIndicators.Add(indicator);

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
                    ProduceIndicators(hit.point);
                    pf.currentPath = pf.PathTo(WorldToGrid(pf.transform.position), WorldToGrid(hit.point), NavNodes);
                }
            }
        }
    }

    public void OnEditToggle(bool toggle)
    {
       
        editing = toggle;
        editToggle.isOn = toggle;
        if (!editing)
        {
            BakeNavMesh();
            pathControlUI.gameObject.SetActive(true);
            editControlUI.gameObject.SetActive(false);
        }
        else
        {
            pathControlUI.gameObject.SetActive(false);
            editControlUI.gameObject.SetActive(true);
            pathIndicators.ForEach(i => Destroy(i));
            pathIndicators = new();
        }
    }

    public void OnClimbToggle(bool toggle)
    {

        allowClimbing = toggle;
        ClimbToggle.isOn = toggle;
        if (!editing)
        {
            BakeNavMesh();
        }
        
    }

    public void OnCornerToggle(bool toggle)
    {

        allowCorners = toggle;
        CornerToggle.isOn = toggle;
        if (!editing)
        {
            BakeNavMesh();
        }
        
    }

    public void OnHeuristicToggle()
    {

        pf.heuristicSelection = (Pathfinder.EnumHeuristic)(((int)pf.heuristicSelection + 1) % 4);
        heurLabel.text = pf.heuristicSelection.ToString();
        if (!editing)
        {
            BakeNavMesh();
        }
        
    }
    public void OnCorrectionsToggle()
    {
        pf.pathingCorrections = !pf.pathingCorrections;
        correctionsToggle.isOn = pf.pathingCorrections;
        if (!editing)
        {
            BakeNavMesh();
        }
    }

}
