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

    private Dictionary<string, CurrencyType> currencyMappings = new Dictionary<string, CurrencyType> {
        { "BlueOrePrefab", CurrencyType.Blue },
        { "YellowOrePrefab", CurrencyType.Yellow },
        { "RedOrePrefab", CurrencyType.Red } 
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

    private CurrencyType GetCurrencyTypeForItem(string itemName)
    {
        var matchingPair = currencyMappings.FirstOrDefault(x => itemName.Contains(x.Key));
        if (!matchingPair.Equals(default(KeyValuePair<string, string>)))
        {
            return matchingPair.Value;
        }
        return CurrencyType.Blue;
    }
    private IEnumerator CollectItems()
    {
        while (true) {
            GetAdjacentBelts();
            adjacentBelts.RemoveAll(item => item == null || !item);
            foreach (var belt in adjacentBelts) {
                if (belt.isSpaceTaken) {
                    CurrencyType currencyType = GetCurrencyTypeForItem(belt.currentItem.name);
                    vortexAnimation.StartVortexAnimation(belt.currentItem.transform, currencyType);
                    belt.currentItem = null;
                    belt.isSpaceTaken = false;
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