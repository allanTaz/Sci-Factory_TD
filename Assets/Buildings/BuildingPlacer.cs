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

    private bool continuousPlacement = false;
    private int lastSelectedBuildingIndex = -1;

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

        previewBuilding = Instantiate(selectedBuilding);
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
            previewBuilding.transform.position = snappedPosition + Vector3.up*(previewBuilding.transform.localScale.y/2f + 0.5f);

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
            GameObject cellObject = gridGenerator.GetCellVisual(cellPosition.x, cellPosition.y);
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
            GameObject specialCellObject = gridGenerator.GetCellVisual(specialPosition.x, specialPosition.y);
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
            GameObject outputCellObject = gridGenerator.GetCellVisual(outputPosition.x, outputPosition.y);
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
        foreach (Vector2Int offset in rotatedOccupiedTiles)
        {
            Vector2Int checkPosition = gridPosition + offset;
            GridCell cell = gridGenerator.GetCell(checkPosition.x, checkPosition.y);
            if (cell == null || cell.IsOccupied)
            {
                return false;
            }
        }

        // Check if special tile is valid
        if (selectedBuildingProps.HasSpecialTile)
        {
            Vector2Int specialPosition = gridPosition + rotatedSpecialTile;
            GridCell specialCell = gridGenerator.GetCell(specialPosition.x, specialPosition.y);
            if (specialCell == null || specialCell.IsOccupied || (selectedBuildingProps.digOre && !specialCell.IsOre))
            {
                return false;
            }
        }

        // Check if output tile is free
        if (selectedBuildingProps.HasOutputTile)
        {
            Vector2Int outputPosition = gridPosition + rotatedOutputTile;
            GridCell outputCell = gridGenerator.GetCell(outputPosition.x, outputPosition.y);
            if (outputCell == null || (outputCell.IsOccupied && outputCell.PlacedObject.GetComponent<Belt>() != null))
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
                GameObject placedBuilding = gridGenerator.PlaceObject(gridPosition, selectedBuilding, previewBuilding.transform);

                if (placedBuilding != null)
                {
                    foreach (Vector2Int offset in rotatedOccupiedTiles)
                    {
                        Vector2Int occupyPosition = gridPosition + offset;
                        GridCell cell = gridGenerator.GetCell(occupyPosition.x, occupyPosition.y);
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
                        previewBuilding = Instantiate(selectedBuilding);
                        previewBuilding.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
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
}