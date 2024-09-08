using UnityEngine;

public class CollectionBuilding : MonoBehaviour
{
    private GridGenerator gridGenerator;
    private Vector2Int gridPosition;

    private void Start()
    {
        gridGenerator = FindObjectOfType<GridGenerator>();
        if (gridGenerator == null)
        {
            Debug.LogError("GridGenerator not found in the scene. CollectionBuilding might not function correctly.");
            return;
        }

        gridPosition = GetGridPosition();
    }

    private Vector2Int GetGridPosition()
    {
        Vector3 worldPosition = transform.position;
        int x = Mathf.RoundToInt(worldPosition.x - gridGenerator.transform.position.x);
        int y = Mathf.RoundToInt(worldPosition.z - gridGenerator.transform.position.z);
        return new Vector2Int(x, y);
    }

    public void CollectResource(GameObject resource)
    {
        // Add to player's currency
        CurrencyManager.Instance.AddCurrency(1);

        // Destroy the resource object
        Destroy(resource);
    }

    public Vector2Int GetInputPosition()
    {
        // Assuming the input is on the left side of the building
        return gridPosition;
    }
}