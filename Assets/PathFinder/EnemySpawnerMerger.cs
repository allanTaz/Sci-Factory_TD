using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;

public class EnemySpawnerMerger : MonoBehaviour
{
    [SerializeField] private Material pathLineMaterial;

    private Dictionary<GameObject, LineRenderer> spawnerPaths = new Dictionary<GameObject, LineRenderer>();
    private Dictionary<GameObject, GameObject> mergingPairs = new Dictionary<GameObject, GameObject>();
    private List<GameObject> enemySpawners = new List<GameObject>();
    private List<GameObject> eligibleSpawners = new List<GameObject>();
    private BuildingPlacer buildingPlacer;
    private const float MIN_CORE_DISTANCE = 8f;
    private GridGenerator gridGenerator;
    private float mergeInterval = 2f;
    private float scaleIncrease = 1.5f;
    private GameObject coreObject;

    void Start()
    {
        coreObject = GameObject.FindGameObjectWithTag("Core");
        buildingPlacer = FindAnyObjectByType<BuildingPlacer>();
        gridGenerator = FindObjectOfType<GridGenerator>();
        StartCoroutine(MergeSpawnersCoroutine());
    }

    void Update()
    {
        enemySpawners = new List<GameObject>(GameObject.FindGameObjectsWithTag("EnemySpawn"));
    }

    IEnumerator MergeSpawnersCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(mergeInterval);

            eligibleSpawners = enemySpawners.Where(s => GetCurrentWave(s) > 1).ToList();
            if (eligibleSpawners.Count > 2)
            {
                MergeSpawners(eligibleSpawners);
            }
        }
    }

    int GetCurrentWave(GameObject spawner)
    {
        EnemySpawner enemySpawnerComponent = spawner.GetComponent<EnemySpawner>();
        return enemySpawnerComponent != null ? enemySpawnerComponent.GetCurrentWave() : 0;
    }

    void MergeSpawners(List<GameObject> eligibleSpawners)
    {
        List<GameObject> availableSpawners = eligibleSpawners.Where(s => !mergingPairs.ContainsKey(s) && !mergingPairs.ContainsValue(s)).ToList();

        while (availableSpawners.Count >= 2)
        {
            GameObject spawner1 = availableSpawners[Random.Range(0, availableSpawners.Count)];
            availableSpawners.Remove(spawner1);

            GameObject spawner2 = GetClosestSpawnerWithPath(spawner1, availableSpawners);

            if (spawner2 != null)
            {
                availableSpawners.Remove(spawner2);
                mergingPairs[spawner1] = spawner2;
                StartCoroutine(MoveAndMergeSpawners(spawner1, spawner2));
            }
            else
            {
                // If no valid path is found, put spawner1 back in the list
                availableSpawners.Add(spawner1);
            }
        }
    }


    IEnumerator MoveAndMergeSpawners(GameObject spawner1, GameObject spawner2)
    {
        Vector2Int gridPos1 = GetGridPosition(spawner1);
        gridGenerator.GetCell(gridPos1).RemoveObject();

        Vector2Int gridPos2 = GetGridPosition(spawner2);

        List<Vector2Int> path = AStar(gridPos1, gridPos2);

        LineRenderer pathLine = GetOrCreateLineRenderer(spawner1);
        DisplayPath(pathLine, path);

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 nextWorldPos = GetWorldPosition(path[i + 1]) + (Vector3.up*0.5f);

            // Move spawner1 normally
            spawner1.transform.DOMove(nextWorldPos, mergeInterval).SetEase(Ease.Linear).OnComplete(() =>
            {
                spawner1.GetComponent<Pathfinder>().FindPath();
            });
            
            // Update path display
            DisplayPath(pathLine, path.GetRange(i + 1, path.Count - (i + 1)));

            // Destroy objects in the new cell
            DestroyObjectsInCell(path[i + 1]);

            yield return new WaitForSeconds(mergeInterval);

            // Check if spawners are adjacent
            if (Vector2Int.Distance(GetGridPosition(spawner1), GetGridPosition(spawner2)) <= 1)
            {
                MergeAdjacentSpawners(spawner1, spawner2);
                yield break;
            }
        }
    }

    void MergeAdjacentSpawners(GameObject spawner1, GameObject spawner2)
    {

        // Create a new, larger spawner
        GameObject mergedSpawner = Instantiate(spawner1, spawner2.transform.position, Quaternion.identity);
        float scale1 = spawner1.transform.localScale.x;
        float scale2 = spawner2.transform.localScale.x;
        float newScale = Mathf.Max(scale1, scale2) + (Mathf.Min(scale1, scale2) / 2f);
        mergedSpawner.transform.localScale = new Vector3(newScale, newScale, newScale);
        // Remove the original spawners and their path displays
        RemoveSpawnerAndPath(spawner1);
        RemoveSpawnerAndPath(spawner2);

        // Add the new spawner to the list
        enemySpawners.Add(mergedSpawner);

        // Remove from merging pairs
        mergingPairs.Remove(spawner1);
        mergingPairs.Remove(spawner2);
    }

    void RemoveSpawnerAndPath(GameObject spawner)
    {
        enemySpawners.Remove(spawner);
        if (spawnerPaths.ContainsKey(spawner))
        {
            Destroy(spawnerPaths[spawner].gameObject);
            spawnerPaths.Remove(spawner);
        }
        Destroy(spawner);
    }

    LineRenderer GetOrCreateLineRenderer(GameObject spawner)
    {
        if (!spawnerPaths.ContainsKey(spawner))
        {
            GameObject lineObj = new GameObject($"Path_{spawner.name}");
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = pathLineMaterial;
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            spawnerPaths[spawner] = line;
        }
        return spawnerPaths[spawner];
    }

    void DisplayPath(LineRenderer pathLine, List<Vector2Int> path)
    {
        pathLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            pathLine.SetPosition(i, GetWorldPosition(path[i]));
        }
    }

    Vector2Int GetGridPosition(GameObject obj)
    {
        Vector3 localPosition = obj.transform.position - gridGenerator.transform.position;
        return new Vector2Int(Mathf.RoundToInt(localPosition.x), Mathf.RoundToInt(localPosition.z));
    }

    Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x, 0.5f, gridPos.y) + gridGenerator.transform.position;
    }

    void DestroyObjectsInCell(Vector2Int gridPos)
    {
        GridCell cell = gridGenerator.GetCell(gridPos);
        if (cell != null && cell.PlacedObject != null)
        {
            buildingPlacer.DestroyBuilding(gridPos, false);
        }
    }

    // Implement a simplified version of A* pathfinding
    List<Vector2Int> AStar(Vector2Int start, Vector2Int goal)
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
                // Check if the neighbor is at least MIN_CORE_DISTANCE away from the core
                if (Vector3.Distance(GetWorldPosition(neighbor), coreObject.transform.position) < MIN_CORE_DISTANCE)
                {
                    continue; // Skip this neighbor if it's too close to the core
                }

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

        return new List<Vector2Int>();
    }

    float HeuristicCostEstimate(Vector2Int a, Vector2Int b)
    {
        float distanceToGoal = Vector2Int.Distance(a, b);
        float distanceToCore = Vector2.Distance(GetWorldPosition(a), coreObject.transform.position);

        // Heavily penalize positions closer than MIN_CORE_DISTANCE to the core
        if (distanceToCore < MIN_CORE_DISTANCE)
        {
            return float.MaxValue;
        }

        // Favor paths further from the Core, but within reasonable bounds
        return distanceToGoal - Mathf.Clamp(distanceToCore - MIN_CORE_DISTANCE, 0, 5);
    }
    GameObject GetClosestSpawnerWithPath(GameObject source, List<GameObject> spawners)
    {
        Vector2Int sourcePos = GetGridPosition(source);

        foreach (var spawner in spawners.OrderBy(s => Vector3.Distance(source.transform.position, s.transform.position)))
        {
            Vector2Int targetPos = GetGridPosition(spawner);
            List<Vector2Int> path = AStar(sourcePos, targetPos);

            if (path.Count > 0)
            {
                return spawner;
            }
        }

        return null;
    }
    List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y),
            new Vector2Int(pos.x, pos.y + 1),
            new Vector2Int(pos.x, pos.y - 1)
        };

        // Filter out neighbors that are too close to the core
        return neighbors
            .Where(p => gridGenerator.IsValidGridPosition(p) &&
                        Vector3.Distance(GetWorldPosition(p), coreObject.transform.position) >= MIN_CORE_DISTANCE)
            .OrderByDescending(p => Vector3.Distance(GetWorldPosition(p), coreObject.transform.position))
            .ToList();
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
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
}