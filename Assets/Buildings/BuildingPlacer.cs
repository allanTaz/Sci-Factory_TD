using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [SerializeField] private GridGenerator gridGenerator;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material specialTileMaterial;
    [SerializeField] private Material outputTileMaterial;
    [SerializeField] private LayerMask gridLayer;
    [SerializeField] private BuildingData buildingData;
    [SerializeField] private GameObject[] cantDestroy;

    private Building selectedBuildingProps;
    private float currentRotation = 0f;
    private GameObject selectedBuilding;
    private GameObject previewBuilding;
    private bool isPlacing = false;
    private TowerRangeIndicator rangeIndicator;
    private List<GameObject> highlightedCells = new List<GameObject>();
    private List<Vector2Int> rotatedOccupiedTiles = new List<Vector2Int>();
    private Vector2Int rotatedOutputTile;
    private Vector2Int rotatedSpecialTile;

    public Action<GameObject> OnBuildingPlaced;
    public Action<GameObject> OnBuildingDestroyed;

    private bool continuousPlacement = false;
    private int lastSelectedBuildingIndex = -1;

    private void Awake()
    {
        InitializeBuildingData();
    }
    void InitializeBuildingData()
    {
        if (buildingData == null)
        {
            Debug.LogError("BuildingData is not assigned in the inspector!");
            return;
        }

        buildingData = buildingData.DeepCopy();
    }
    void Update()
    {
        if (isPlacing)
        {
            UpdateBuildingPosition();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateBuilding(90f);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RotateBuilding(-90f);
            }

            if (Input.GetMouseButtonDown(0) || (continuousPlacement && Input.GetMouseButton(0)))
            {
                PlaceBuilding();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            DestroyBuildingAtMousePosition();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            continuousPlacement = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            continuousPlacement = false;
            if (!isPlacing)
            {
                CancelPlacement();
            }
        }
    }
    void RotateBuilding(float angle)
    {
        currentRotation += angle;
        currentRotation %= 360f;
        previewBuilding.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

        // Rotate occupied tiles and special tiles
        rotatedOccupiedTiles.Clear();
        foreach (Vector2Int tile in selectedBuildingProps.occupiedTiles)
        {
            rotatedOccupiedTiles.Add(RotateVector2(tile, currentRotation));
        }
        rotatedOutputTile = RotateVector2(selectedBuildingProps.outputTile, currentRotation);
        rotatedSpecialTile = RotateVector2(selectedBuildingProps.specialTile, currentRotation);
    }
    Vector2Int RotateVector2(Vector2Int v, float angle)
    {
        float rad = -angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2Int(
            Mathf.RoundToInt(v.x * cos - v.y * sin),
            Mathf.RoundToInt(v.x * sin + v.y * cos)
        );
    }

    public void SelectBuilding(int index)
    {
        if (index < 0 || index >= buildingData.buildings.Count) return;

        selectedBuildingProps = buildingData.buildings[index];
        selectedBuilding = selectedBuildingProps.buildingPrefab;

        if (previewBuilding != null)
        {
            Destroy(previewBuilding);
        }

        previewBuilding = Instantiate(selectedBuilding, Vector3.down * 4, selectedBuilding.transform.rotation);
        DisableScripts(previewBuilding, selectedBuildingProps.scriptsToDisableDuringPlacement);
        if (!continuousPlacement || lastSelectedBuildingIndex != index)
        {
            currentRotation = 0f;
        }
        previewBuilding.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

        // Initialize rotated tiles
        rotatedOccupiedTiles = new List<Vector2Int>(selectedBuildingProps.occupiedTiles);
        rotatedOutputTile = selectedBuildingProps.outputTile;
        rotatedSpecialTile = selectedBuildingProps.specialTile;

        // Apply current rotation to tiles
        if (currentRotation != 0f)
        {
            for (int i = 0; i < rotatedOccupiedTiles.Count; i++)
            {
                rotatedOccupiedTiles[i] = RotateVector2(rotatedOccupiedTiles[i], currentRotation);
            }
            rotatedOutputTile = RotateVector2(rotatedOutputTile, currentRotation);
            rotatedSpecialTile = RotateVector2(rotatedSpecialTile, currentRotation);
        }

        rangeIndicator = previewBuilding.GetComponent<TowerRangeIndicator>();
        if (rangeIndicator != null)
        {
            rangeIndicator.ShowRange(true);
        }

        isPlacing = true;
        lastSelectedBuildingIndex = index;
    }

    Vector2Int GetFinalGridPosition(RaycastHit hit)
    {
        Vector2 exactGridPosition = new Vector2(
            hit.point.x - gridGenerator.transform.position.x,
            hit.point.z - gridGenerator.transform.position.z
        );

        return new Vector2Int(
            Mathf.RoundToInt(exactGridPosition.x),
            Mathf.RoundToInt(exactGridPosition.y)
        );
    }

    void UpdateBuildingPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        ResetHighlightedCells();

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayer))
        {
            Vector2Int finalGridPosition = GetFinalGridPosition(hit);
            Vector3 snappedPosition = gridGenerator.GetWorldPosition(finalGridPosition);

            // Position the preview building at the center of its occupied tiles
            previewBuilding.transform.position = snappedPosition + Vector3.up * (previewBuilding.transform.localScale.y / 2f + 0.5f);

            bool canPlace = CanPlaceBuilding(finalGridPosition);
            SetPreviewTransparency(0.5f, canPlace ? Color.green : Color.red);
            if (rangeIndicator != null)
            {
                rangeIndicator.ShowRange(canPlace);
            }
            HighlightCells(finalGridPosition, canPlace);
        }
    }
    void HighlightCells(Vector2Int gridPosition, bool canPlace)
    {
        foreach (Vector2Int offset in rotatedOccupiedTiles)
        {
            Vector2Int cellPosition = gridPosition + offset;
            GameObject cellObject = gridGenerator.GetCellVisual(cellPosition);
            if (cellObject != null)
            {
                Renderer cellRenderer = cellObject.GetComponent<Renderer>();
                if (cellRenderer != null)
                {
                    Material highlightMat = new Material(highlightMaterial);
                    highlightMat.color = canPlace ? Color.green : Color.red;
                    cellRenderer.material = highlightMat;
                    highlightedCells.Add(cellObject);
                }
            }
        }

        // Highlight special tile if it exists
        if (selectedBuildingProps.HasSpecialTile)
        {
            Vector2Int specialPosition = gridPosition + rotatedSpecialTile;
            GameObject specialCellObject = gridGenerator.GetCellVisual(specialPosition);
            if (specialCellObject != null)
            {
                Renderer cellRenderer = specialCellObject.GetComponent<Renderer>();
                if (cellRenderer != null)
                {
                    cellRenderer.material = specialTileMaterial;
                    highlightedCells.Add(specialCellObject);
                }
            }
        }

        // Highlight output tile if it exists
        if (selectedBuildingProps.HasOutputTile)
        {
            Vector2Int outputPosition = gridPosition + rotatedOutputTile;
            GameObject outputCellObject = gridGenerator.GetCellVisual(outputPosition);
            if (outputCellObject != null)
            {
                Renderer cellRenderer = outputCellObject.GetComponent<Renderer>();
                if (cellRenderer != null)
                {
                    cellRenderer.material = outputTileMaterial;
                    highlightedCells.Add(outputCellObject);
                }
            }
        }
    }
    void ResetHighlightedCells()
    {
        foreach (GameObject cell in highlightedCells)
        {
            Renderer cellRenderer = cell.GetComponent<Renderer>();
            if (cellRenderer != null)
            {
                cellRenderer.material = gridGenerator.GetDefaultCellMaterial();
            }
        }
        highlightedCells.Clear();
    }

    bool CanPlaceBuilding(Vector2Int gridPosition)
    {
        if (selectedBuildingProps.singleInstance && selectedBuildingProps.instanceCount > 0)
        {
            return false;
        }
        foreach (var currency in selectedBuildingProps.Price)
        {
            if (CurrencyManager.Instance.GetCurrency(currency.Key) < currency.Value)
            {
                return false;
            }
        }
        foreach (Vector2Int offset in rotatedOccupiedTiles)
        {
            Vector2Int checkPosition = gridPosition + offset;
            GridCell cell = gridGenerator.GetCell(checkPosition);
            if (cell == null || cell.IsOccupied)
            {
                return false;
            }
        }

        // Check if special tile is valid
        if (selectedBuildingProps.HasSpecialTile)
        {
            Vector2Int specialPosition = gridPosition + rotatedSpecialTile;
            GridCell specialCell = gridGenerator.GetCell(specialPosition);
            if (specialCell == null || specialCell.IsOccupied || (selectedBuildingProps.digOre && !specialCell.IsOre))
            {
                return false;
            }
        }

        // Check if output tile is free
        if (selectedBuildingProps.HasOutputTile)
        {
            Vector2Int outputPosition = gridPosition + rotatedOutputTile;
            GridCell outputCell = gridGenerator.GetCell(outputPosition);
            if (outputCell == null || (outputCell.IsOccupied && (outputCell.PlacedObject.GetComponent<Belt>() == null)))
            {
                return false;
            }
        }

        return true;
    }
    private void PlaceBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayer))
        {
            Vector2Int gridPosition = GetFinalGridPosition(hit);

            if (CanPlaceBuilding(gridPosition))
            {
                foreach (var currency in selectedBuildingProps.Price)
                {
                    CurrencyManager.Instance.RemoveCurrency(currency.Key, currency.Value);
                }
                GameObject placedBuilding = gridGenerator.PlaceObject(gridPosition, selectedBuilding, previewBuilding.transform);
                if (placedBuilding != null)
                {
                    selectedBuildingProps.instanceCount++;
                    foreach (Vector2Int offset in rotatedOccupiedTiles)
                    {
                        Vector2Int occupyPosition = gridPosition + offset;
                        GridCell cell = gridGenerator.GetCell(occupyPosition);
                        if (cell != null)
                        {
                            cell.PlaceObject(placedBuilding);
                            OnBuildingPlaced?.Invoke(placedBuilding);
                        }
                    }

                    TowerRangeIndicator placedRangeIndicator = placedBuilding.GetComponent<TowerRangeIndicator>();
                    if (placedRangeIndicator != null)
                    {
                        placedRangeIndicator.ShowRange(false);
                    }

                    if (!continuousPlacement)
                    {
                        CancelPlacement();
                    }
                    else
                    {
                        // Reset the preview for the next placement, maintaining rotation
                        Destroy(previewBuilding);
                        previewBuilding = Instantiate(selectedBuilding, Vector3.down*3, Quaternion.Euler(0f, currentRotation, 0f));
                        DisableScripts(previewBuilding, selectedBuildingProps.scriptsToDisableDuringPlacement);
                        if (rangeIndicator != null)
                        {
                            rangeIndicator = previewBuilding.GetComponent<TowerRangeIndicator>();
                            rangeIndicator.ShowRange(true);
                        }
                    }
                }
            }
        }
        ResetHighlightedCells();
    }
    private void DisableScripts(GameObject building, List<string> scriptsToDisable)
    {
        foreach (string scriptName in scriptsToDisable)
        {
            Component component = building.GetComponent(scriptName);
            if (component != null && component is Behaviour behaviour)
            {
                behaviour.enabled = false;
            }
        }
    }

    private void EnableScripts(GameObject building, List<string> scriptsToEnable)
    {
        foreach (string scriptName in scriptsToEnable)
        {
            Component component = building.GetComponent(scriptName);
            if (component != null && component is Behaviour behaviour)
            {
                behaviour.enabled = true;
            }
        }
    }
    private void CancelPlacement()
    {
        isPlacing = false;
        if (previewBuilding != null)
        {
            Destroy(previewBuilding);
            previewBuilding = null;
        }
        rangeIndicator = null;
        ResetHighlightedCells();
        lastSelectedBuildingIndex = -1;
        currentRotation = 0f;
    }

    void SetPreviewTransparency(float alpha, Color? color = null)
    {
        Renderer[] renderers = previewBuilding.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                Color newColor = color ?? mat.color;
                newColor.a = alpha;
                mat.color = newColor;
            }
            renderer.materials = materials;
        }
    }

    public Building GetSelectedBuildingData()
    {
        return selectedBuildingProps;
    }
    private Building GetBuildingPropsFromObject(GameObject gameObject)
    {
        foreach(var building in buildingData.buildings)
        {
            if (gameObject.name.Contains(building.buildingPrefab.name))
            {
                return building;
            }
        }
        return null;
    }
    private void DestroyBuildingAtMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayer))
        {
            Vector2Int gridPosition = GetFinalGridPosition(hit);
            DestroyBuilding(gridPosition);
        }
    }
    private void DestroyBuilding(Vector2Int position)
    {
        GridCell cell = gridGenerator.GetCell(position);
        if (cell != null && cell.IsOccupied)
        {
            
            GameObject buildingToDestroy = cell.PlacedObject;
            foreach(var building in cantDestroy)
            {
                if (buildingToDestroy.name.Contains(building.name))
                {
                    return;
                }
            }
            Building buildingData = GetBuildingPropsFromObject(buildingToDestroy);
            if (buildingData != null) {
                foreach (var currency in buildingData.Price)
                {
                    CurrencyManager.Instance.AddCurrency(currency.Key, currency.Value);
                }
                buildingData.instanceCount--;
            }
            // Find all cells occupied by this building
            List<Vector2Int> occupiedPositions = new List<Vector2Int>();
            for (int x = gridGenerator.MinBounds.x; x <= gridGenerator.MaxBounds.x; x++)
            {
                for (int y = gridGenerator.MinBounds.y; y <= gridGenerator.MaxBounds.y; y++)
                {
                    Vector2Int xy = new Vector2Int(x, y);
                    GridCell checkCell = gridGenerator.GetCell(xy);
                    if (checkCell != null && checkCell.PlacedObject == buildingToDestroy)
                    {
                        occupiedPositions.Add(xy);
                    }
                }
            }

            // Clear all occupied cells
            foreach (Vector2Int pos in occupiedPositions)
            {
                GridCell occupiedCell = gridGenerator.GetCell(pos);
                if (occupiedCell != null)
                {
                    occupiedCell.RemoveObject();
                }
            }

            OnBuildingDestroyed?.Invoke(buildingToDestroy);
            Destroy(buildingToDestroy);
        }
    }
}