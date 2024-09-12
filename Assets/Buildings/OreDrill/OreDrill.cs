using UnityEngine;
using System.Collections;

public class OreDrill : MonoBehaviour
{
    public float productionInterval = 5f;
    public float animationDuration = 3f;
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
    private Vector2Int GetOutputPosition()
    {
        BuildingPlacer buildingPlacer = FindObjectOfType<BuildingPlacer>();
        if (buildingPlacer != null)
        {
            Building buildingData = buildingPlacer.GetSelectedBuildingData();
            if (buildingData != null && buildingData.HasOutputTile)
            {
                //float rotation = transform.rotation.eulerAngles.y;
                Quaternion objectRotation = transform.rotation;
                Vector3 eulerAngles = objectRotation.eulerAngles;
                float rotation =  Mathf.Round(eulerAngles.y / 90) * 90;
                Vector2Int rotatedOutputTile = RotateVector2(buildingData.outputTile, rotation);
                return gridPosition + rotatedOutputTile;
            }
        }
        Debug.LogError("COULDNT FIND OUTPUT TILE");
        return gridPosition + Vector2Int.right;
    }

    private IEnumerator ProduceResources()
    {
        while (true)
        {
            //yield return new WaitForSeconds(productionInterval);
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
                    if (!CheckForIncomingItems(outputBelt))
                    {
                        Vector3 spawnPosition = gridGenerator.GetWorldPosition(outputPosition) + Vector3.up * 1.2f;
                        GameObject producedObject = Instantiate(resourcePrefab, spawnPosition, resourcePrefab.transform.rotation);
                        propBlock.SetFloat("_StartTime", Time.time);
                        producedObject.GetComponent<Renderer>().SetPropertyBlock(propBlock);

                        // Pause the belt and start the animation
                        outputBelt.PauseBelt();
                        outputBelt.PlaceItemOnBelt(producedObject);

                        // Wait for the animation duration
                        yield return new WaitForSeconds(animationDuration);

                        // Resume the belt movement
                        outputBelt.ResumeBelt();
                        yield break;
                    }
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    private bool CheckForIncomingItems(Belt targetBelt)
    {
        // Check all neighboring belts
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = outputPosition + dir;
            GridCell neighborCell = gridGenerator.GetCell(neighborPos.x, neighborPos.y);

            if (neighborCell != null && neighborCell.IsOccupied)
            {
                Belt neighborBelt = neighborCell.PlacedObject.GetComponent<Belt>();
                if (neighborBelt != null && neighborBelt.currentItem != null)
                {
                    // Check if this neighboring belt is pointing towards our target belt
                    if (neighborBelt.beltInSequence == targetBelt)
                    {
                        return true; // There's an item coming towards our target belt
                    }
                }
            }
        }

        return false; // No incoming items found
    }
}