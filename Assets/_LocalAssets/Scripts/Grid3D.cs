using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Translates the global unity space to a grid based space for pathfinding purpose.
/// </summary>
public class Grid3D : MonoBehaviour
{
    public float CellMetric = 1f;
    public GameObject cubePrefab;
    public Dictionary<Vector3Int, NavBlock> placedCubes;
    public Dictionary<Vector3Int, int> NavNodes;

    public void Awake()
    {
        NavBlock[] startingBlocks = FindObjectsByType<NavBlock>(FindObjectsSortMode.None);
        foreach (NavBlock b in startingBlocks)
        {
            placedCubes.Add(WorldToGrid(b.transform.position),b);
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

            }
        }
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

                //Debug.Log(hit.point + "|" + WorldToGrid(hit.point));
                GameObject newCube = Instantiate(cubePrefab);
                newCube.transform.position = AlignToGrid(hit.point);
                newCube.GetComponent<NavBlock>().gridPos = WorldToGrid(hit.point);
                placedCubes.Add(newCube.GetComponent<NavBlock>().gridPos, newCube.GetComponent<NavBlock>());

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

                if (hit.collider.gameObject.CompareTag("PlayerCreatedCube")){
                    placedCubes.Remove(hit.collider.gameObject.GetComponent<NavBlock>().gridPos);
                    Destroy(hit.collider.gameObject);
                }

            }
        }
    }

}
