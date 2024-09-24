using UnityEngine;

public class CollectionBuilding : MonoBehaviour
{
    private GridGenerator gridGenerator;
    private Vector2Int gridPosition;

    [System.Serializable]
    public class ResourceCurrencyMapping
    {
        public GameObject resourcePrefab;
        public string currencyType;
        public int currencyAmount = 1;
    }

    public ResourceCurrencyMapping[] resourceMappings;

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
        foreach (var mapping in resourceMappings)
        {
            if (mapping.resourcePrefab == resource)
            {
                CurrencyManager.Instance.AddCurrency(mapping.currencyType, mapping.currencyAmount);
                Destroy(resource);
                return;
            }
        }

        Debug.LogWarning($"No currency mapping found for resource: {resource.name}");
    }

    public Vector2Int GetInputPosition()
    {
        // Assuming the input is on the left side of the building
        return gridPosition;
    }
}