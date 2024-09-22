using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject gridCellVisualPrefab;
    [SerializeField] private GameObject baseCorePrefab;
    [SerializeField] private GameObject enemySpawnerPrefab;
    [SerializeField] public OreData oreData;
    [SerializeField] private float cellAnimationDuration = 0.5f;
    [SerializeField] private Ease cellAnimationEase = Ease.OutBack;
    private Material defaultCellMaterial;
    private GameObject enemySpawner;
    private Dictionary<Vector2Int, GridCell> grid = new Dictionary<Vector2Int, GridCell>();
    private Dictionary<Vector2Int, GameObject> visualCells = new Dictionary<Vector2Int, GameObject>();
    public Vector2Int MinBounds { get; private set; }
    public Vector2Int MaxBounds { get; private set; }
    private void Awake()
    {
        MinBounds = Vector2Int.zero;
        MaxBounds = new Vector2Int(9, 9);  // Initial 10x10 grid
        GenerateInitialGrid();
        PlaceObject(Vector2Int.zero, baseCorePrefab);
    }

    private void GenerateInitialGrid()
    {
        for (int x = MinBounds.x; x <= MaxBounds.x; x++)
        {
            for (int y = MinBounds.y; y <= MaxBounds.y; y++)
            {
                CreateCell(new Vector2Int(x, y));
            }
        }
    }

    private void CreateCell(Vector2Int position)
    {
        if (!grid.ContainsKey(position))
        {
            GridCell newCell = new GridCell(position.x, position.y);
            grid[position] = newCell;

            Vector3 worldPosition = GetWorldPosition(position);
            GameObject cellVisual = Instantiate(gridCellVisualPrefab, worldPosition, Quaternion.identity, transform);
            cellVisual.transform.localScale = Vector3.zero;
            visualCells[position] = cellVisual;

            if (defaultCellMaterial == null)
            {
                Renderer renderer = cellVisual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    defaultCellMaterial = renderer.material;
                }
            }

            // Animate the cell scaling
            cellVisual.transform.DOScale(Vector3.one, cellAnimationDuration)
                .SetEase(cellAnimationEase)
                .OnComplete(() => {
                    // You can add any post-animation logic here if needed
                });
        }
    }

    public void EnsureGridCoverage(Vector2Int position)
    {
        MinBounds = Vector2Int.Min(MinBounds, position);
        MaxBounds = Vector2Int.Max(MaxBounds, position);
        CreateCell(position);
    }
    private enum OreVisuals
    {
        
    }
    public void SetCellAsOre(Vector2Int position, OreType oreType)
    {
        EnsureGridCoverage(position);

        GridCell cell = grid[position];
        if (cell != null && !cell.IsOre)
        {
            cell.SetAsOre(oreType);

            // Replace the visual with ore prefab
            if (visualCells.ContainsKey(position))
            {
                Destroy(visualCells[position]);
            }
            Vector3 worldPosition = GetWorldPosition(position);
            visualCells[position] = Instantiate(oreData.GetOreInfo(oreType).oreCell, worldPosition, Quaternion.identity, transform);
        }
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return transform.position + new Vector3(gridPosition.x, 0, gridPosition.y);
    }

    public bool IsValidGridPosition(Vector2Int position)
    {
        return grid.ContainsKey(position);
    }

    public GridCell GetCell(Vector2Int position)
    {
        if (grid.TryGetValue(position, out GridCell cell))
        {
            return cell;
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
                EnsureGridCoverage(checkPosition);
                GridCell cell = GetCell(checkPosition);
                if (cell == null || cell.IsOccupied)
                {
                    return null; // Cannot place the object
                }
            }
        }

        GameObject placedObj = null;
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
                GridCell cell = GetCell(occupyPosition);
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
        GridCell cell = GetCell(position);
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
        return GetWorldPosition(MaxBounds);
    }

    public Vector3 GetBaseCorePosition()
    {
        return GetWorldPosition(Vector2Int.zero);
    }

    public Material GetDefaultCellMaterial()
    {
        return defaultCellMaterial;
    }

    public GameObject GetCellVisual(Vector2Int position)
    {
        if (visualCells.TryGetValue(position, out GameObject visual))
        {
            return visual;
        }
        return null;
    }
}