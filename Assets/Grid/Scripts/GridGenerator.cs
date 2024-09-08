using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int width = 18;
    [SerializeField] private int height = 9;
    [SerializeField] private GameObject gridCellVisualPrefab;
    [SerializeField] private GameObject orePrefab;
    [SerializeField] private GameObject baseCorePrefab;
    [SerializeField] private GameObject enemySpawnerPrefab;
    private Material defaultCellMaterial;
    private GameObject enemySpawner;
    private GridCell[,] grid;
    private GameObject[,] visualCells;
    public int gridWidth { get { return width; } set { width = value; } }
    public int gridHeight { get { return height; } set { height = value; } }
    private void Awake()
    {
        GenerateGrid();
        PlaceObject(new Vector2Int(0, 0), baseCorePrefab);
        enemySpawner = PlaceObject(new Vector2Int(width - 1, height - 1), enemySpawnerPrefab);
    }

    void GenerateGrid()
    {
        grid = new GridCell[width, height];
        visualCells = new GameObject[width, height];
        Vector3 startPosition = transform.position;
        Vector2Int orePosition = new Vector2Int(Random.Range(2, width - 2), Random.Range(2, height - 2));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isOre = (x == orePosition.x && y == orePosition.y);
                grid[x, y] = new GridCell(x, y, isOre);

                Vector3 position = GetWorldPosition(new Vector2Int(x, y));
                GameObject cellVisual = Instantiate(isOre ? orePrefab : gridCellVisualPrefab, position, Quaternion.identity, transform);
                visualCells[x, y] = cellVisual;
                if (!isOre && defaultCellMaterial == null)
                {
                    Renderer renderer = cellVisual.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        defaultCellMaterial = renderer.material;
                    }
                }
            }
        }

        //Debug.Log($"Grid generated with dimensions {width}x{height}");
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return transform.position + new Vector3(gridPosition.x, 0, gridPosition.y);
    }

    public bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }

    public GridCell GetCell(int x, int y)
    {
        if (IsValidGridPosition(new Vector2Int(x, y)))
        {
            return grid[x, y];
        }
        return null;
    }

    public GameObject PlaceObject(Vector2Int position, GameObject obj, Transform objTransform = null, Vector2Int size = default)
    {
        if (size == default) size = Vector2Int.one;

        // Check if all required cells are available
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkPosition = position + new Vector2Int(x, y);
                GridCell cell = GetCell(checkPosition.x, checkPosition.y);
                if (cell == null || cell.IsOccupied)
                {
                    return null; // Cannot place the object
                }
            }
        }
        GameObject placedObj = null;
        // If all cells are available, place the object
        Vector3 worldPosition = GetWorldPosition(position);
        if (objTransform != null)
        {
            placedObj = Instantiate(obj, worldPosition, objTransform.rotation);
        }
        else
        {
            placedObj = Instantiate(obj, worldPosition, Quaternion.identity);
        }
        float objectHeight = CalculateObjectHeight(placedObj);

        if (objTransform == null)
        {
            placedObj.transform.position = new Vector3(
                worldPosition.x + (size.x - 1) * 0.5f,
                worldPosition.y + (objectHeight / 2f) + 0.5f,
                worldPosition.z + (size.y - 1) * 0.5f
            );
        }
        else
        {
            placedObj.transform.position = objTransform.position;
            placedObj.transform.rotation = objTransform.rotation;
        }

        // Occupy all required cells
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int occupyPosition = position + new Vector2Int(x, y);
                GridCell cell = GetCell(occupyPosition.x, occupyPosition.y);
                cell.PlaceObject(placedObj);
            }
        }

        return placedObj;
    }
    private float CalculateObjectHeight(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }

        // If there's no renderer on the main object, check children
        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
        if (childRenderers.Length > 0)
        {
            float maxHeight = 0f;
            foreach (Renderer childRenderer in childRenderers)
            {
                maxHeight = Mathf.Max(maxHeight, childRenderer.bounds.size.y);
            }
            return maxHeight;
        }

        // If no renderers found, return a default height
        Debug.LogWarning("No renderer found on object or its children. Using default height.");
        return 1f; // Default height
    }
    public void RemoveObject(Vector2Int position)
    {
        GridCell cell = GetCell(position.x, position.y);
        if (cell != null && cell.IsOccupied)
        {
            GameObject objToRemove = cell.PlacedObject;
            cell.RemoveObject();
            if (objToRemove != null)
            {
                Destroy(objToRemove);
            }
        }
    }

    public Vector3 GetEnemySpawnPosition()
    {
        return GetWorldPosition(new Vector2Int(width - 1, height - 1));
    }

    public Vector3 GetBaseCorePosition()
    {
        return GetWorldPosition(new Vector2Int(0, 0));
    }
    public Material GetDefaultCellMaterial()
    {
        return defaultCellMaterial;
    }
    public GameObject GetCellVisual(int x, int y)
    {
        if (IsValidGridPosition(new Vector2Int(x, y)))
        {
            return visualCells[x, y];
        }
        return null;
    }
}