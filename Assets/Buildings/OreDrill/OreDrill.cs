using UnityEngine;
using System.Collections;

public class OreDrill : MonoBehaviour
{
    public float productionInterval = 5f;
    private GridGenerator gridGenerator;
    private Vector2Int gridPosition;
    private Vector2Int outputPosition;
    public GameObject resourcePrefab;
    private MaterialPropertyBlock propBlock;

    private void OnEnable()
    {
        propBlock = new MaterialPropertyBlock();

        gridGenerator = FindObjectOfType<GridGenerator>();
        if (gridGenerator == null)
        {
            Debug.LogError("GridGenerator not found in the scene. OreDrill might not function correctly.");
            return;
        }

        gridPosition = GetGridPosition();
        outputPosition = GetOutputPosition();
        StartCoroutine(ProduceResources());
    }

    private Vector2Int GetGridPosition()
    {
        Vector3 worldPosition = transform.position;
        int x = Mathf.RoundToInt(worldPosition.x - gridGenerator.transform.position.x);
        int y = Mathf.RoundToInt(worldPosition.z - gridGenerator.transform.position.z);
        return new Vector2Int(x, y);
    }

    private Vector2Int GetOutputPosition()
    {
        BuildingPlacer buildingPlacer = FindObjectOfType<BuildingPlacer>();
        if (buildingPlacer != null)
        {
            Building buildingData = buildingPlacer.GetSelectedBuildingData();
            if (buildingData != null && buildingData.HasOutputTile)
            {
                // The output tile is relative to the building's position
                return gridPosition + buildingData.outputTile;
            }
        }
        Debug.LogError("COULDNT FIND OUTPUT TILE");
        // Fallback: If we can't get the data, assume output is one tile to the right
        return gridPosition + Vector2Int.right;
    }

    private IEnumerator ProduceResources()
    {
        while (true)
        {
            // Wait for the production interval
            yield return new WaitForSeconds(productionInterval);

            // Try to place the resource
            yield return StartCoroutine(TryPlaceResource());
        }
    }
    private IEnumerator TryPlaceResource()
    {
        while (true)
        {
            GridCell outputCell = gridGenerator.GetCell(outputPosition.x, outputPosition.y);
            if (outputCell != null && outputCell.IsOccupied)
            {
                Belt outputBelt = outputCell.PlacedObject.GetComponent<Belt>();
                if (outputBelt != null && !outputBelt.isSpaceTaken)
                {
                    Vector3 spawnPosition = gridGenerator.GetWorldPosition(outputPosition) + Vector3.up * 1.2f;
                    GameObject producedObject = Instantiate(resourcePrefab, spawnPosition, resourcePrefab.transform.rotation);
                    propBlock.SetFloat("_StartTime", Time.time);
                    producedObject.GetComponent<Renderer>().SetPropertyBlock(propBlock);
                    outputBelt.PlaceItemOnBelt(producedObject);
                    yield break; // Exit the coroutine after successfully placing the resource
                }
            }
            // If we couldn't place the resource, wait a short time before trying again
            yield return new WaitForSeconds(0.01f);
        }
    }
}