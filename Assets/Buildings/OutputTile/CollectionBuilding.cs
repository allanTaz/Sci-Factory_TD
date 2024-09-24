using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectionBuilding : MonoBehaviour
{
    [SerializeField] private float inputDelay = 0.5f;
    private GridGenerator gridGenerator;
    private GameObject[] objectsToInput;
    private List<Belt> adjacentBelts;
    private VortexAnimation vortexAnimation;

    private Dictionary<string, string> currencyMappings = new Dictionary<string, string> {
        { "BlueOrePrefab", "Blue" },
        { "YellowOrePrefab", "Yellow" },
        { "RedOrePrefab", "Red" } 
    };
    private void Start()
    {
        adjacentBelts = new List<Belt>();
        vortexAnimation = GetComponent<VortexAnimation>();
        gridGenerator = FindObjectOfType<GridGenerator>();
        if (gridGenerator == null)
        {
            Debug.LogError("GridGenerator not found in the scene. CollectionBuilding might not function correctly.");
            return;
        }
        StartCoroutine(CollectItems());
    }

    private string GetCurrencyTypeForItem(string itemName)
    {
        var matchingPair = currencyMappings.FirstOrDefault(x => itemName.Contains(x.Key));
        if (!matchingPair.Equals(default(KeyValuePair<string, string>)))
        {
            return matchingPair.Value;
        }
        return null;
    }
    private IEnumerator CollectItems()
    {
        while (true) {
            GetAdjacentBelts();
            adjacentBelts.RemoveAll(item => item == null || !item);
            foreach (var belt in adjacentBelts) {
                if (belt.isSpaceTaken) {
                    string currencyType = GetCurrencyTypeForItem(belt.currentItem.name);
                    if (currencyType != null)
                    {
                        CurrencyManager.Instance.AddCurrency(currencyType, 1);
                        vortexAnimation.StartVortexAnimation(belt.currentItem.transform);
                        belt.currentItem = null;
                        belt.isSpaceTaken = false;
                    }
                    else
                    {
                        Debug.LogWarning($"No currency mapping found for item: {belt.currentItem.name}");
                    }
                }
            }
            yield return new WaitForSeconds(inputDelay);
        }
    }


    private void GetAdjacentBelts()
    {
        GameObject[] adjacentObjects = GridUtility.GetAdjacentObjects(transform, gridGenerator);
        foreach (GameObject obj in adjacentObjects) {
            if (obj != null)
            {
                Belt belt = obj.GetComponent<Belt>();
                if (belt != null && GridUtility.IsObjectFacing(obj.transform, transform))
                {
                    if (!adjacentBelts.Contains(belt))
                    {
                        adjacentBelts.Add(belt);
                        print(adjacentObjects);
                    }
                }
            }
        }
    }
}