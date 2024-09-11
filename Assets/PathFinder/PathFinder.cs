using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private GridGenerator gridGenerator;
    [SerializeField] private Material pathLineMaterial;
    [SerializeField] private BuildingPlacer buildingPlacer;
    private LineRenderer pathLine;

    private GameObject startObject;
    private GameObject endObject;

    private List<Vector2Int> path = new List<Vector2Int>();
    private Vector3 gridOffset;

    void Start()
    {
        buildingPlacer.OnBuildingPlaced += FindPath;
        buildingPlacer.OnBuildingDestroyed += FindPath;
        pathLine = gameObject.AddComponent<LineRenderer>();
        pathLine.startWidth = 0.1f;
        pathLine.endWidth = 0.1f;
        pathLine.material = pathLineMaterial;

        gridOffset = gridGenerator.transform.position;
        //Debug.Log($"Grid offset: {gridOffset}");

        StartCoroutine(FindPathWhenReady());
    }

    public List<Vector3> GetPath()
    {
        List<Vector3> worldPath = new List<Vector3>();
        foreach (Vector2Int gridPos in path)
        {
            Vector3 worldPos = new Vector3(gridPos.x, 1f, gridPos.y) + gridOffset;
            worldPath.Add(worldPos);
        }
        return worldPath;
    }

    IEnumerator FindPathWhenReady()
    {
        //Debug.Log("Waiting for grid to initialize...");
        yield return new WaitForSeconds(0.5f);
       //Debug.Log("Finding start and end objects...");
        FindStartAndEndObjects();
        if (startObject != null && endObject != null)
        {
            //Debug.Log("Start and end objects found. Finding path...");
            FindPath();
            pathLine.material.SetVector("_Tiling", new Vector2(17, 1));
        }
        else
        {
            Debug.LogError("Start or end object not found. Cannot find path.");
        }
    }

    void FindStartAndEndObjects()
    {
        startObject = GameObject.FindGameObjectWithTag("EnemySpawn");
        endObject = GameObject.FindGameObjectWithTag("Core");

        if (startObject == null) Debug.LogError("EnemySpawn not found");
        if (endObject == null) Debug.LogError("Core not found");
    }

    public void FindPath(GameObject gameObject = null)
    {
        if (startObject == null || endObject == null)
        {
            Debug.LogError("Start or end object is null. Cannot find path.");
            return;
        }

        Vector2Int startPos = GetGridPosition(startObject);
        Vector2Int endPos = GetGridPosition(endObject);

        //Debug.Log($"Finding path from {startPos} to {endPos}");
        path = AStar(startPos, endPos);
        DisplayPath();
    }

    private Vector2Int GetGridPosition(GameObject obj)
    {
        Vector3 localPosition = obj.transform.position - gridOffset;
        return new Vector2Int(Mathf.RoundToInt(localPosition.x), Mathf.RoundToInt(localPosition.z));
    }

    private List<Vector2Int> AStar(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { { start, 0 } };
        var fScore = new Dictionary<Vector2Int, float> { { start, HeuristicCostEstimate(start, goal) } };

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(pos => fScore.GetValueOrDefault(pos, float.MaxValue)).First();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            foreach (var neighbor in GetNeighbors(current))
            {
                var tentativeGScore = gScore[current] + 1;
                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning($"No path found from {start} to {goal}.");
        return new List<Vector2Int>();
    }

    private float HeuristicCostEstimate(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y),
            new Vector2Int(pos.x, pos.y + 1),
            new Vector2Int(pos.x, pos.y - 1)
        };

        return neighbors.Where(n => IsValidPosition(n) && IsWalkable(n)).ToList();
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return gridGenerator.IsValidGridPosition(pos);
    }

    private bool IsWalkable(Vector2Int pos)
    {
        GridCell cell = gridGenerator.GetCell(pos.x, pos.y);
        if (cell == null)
        {
            return false;
        }
        if (cell.IsOccupied)
        {
            // Allow walking through start and end objects
            if (cell.PlacedObject == startObject || cell.PlacedObject == endObject)
            {
                return true;
            }
            return false;
        }
        return true;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();
        return totalPath;
    }

    private void DisplayPath()
    {
        if (path.Count == 0)
        {
            Debug.LogWarning("No path found to display!");
            pathLine.positionCount = 0;
            return;
        }

        //Debug.Log($"Displaying path with {path.Count} points");
        pathLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = new Vector3(path[i].x, 0.5f, path[i].y) + gridOffset;
            pathLine.SetPosition(i, worldPos);
        }
    }
}