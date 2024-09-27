using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class CurrencyAmount
{
    public CurrencyType currencyType;
    public int amount;
    public CurrencyAmount(CurrencyType type, int amt)
    {
        currencyType = type;
        amount = amt;
    }
}
[Serializable]
public class Building
{
    public static readonly Vector2Int NoTile = new Vector2Int(-99, -99);

    public GameObject buildingPrefab;
    public List<Vector2Int> occupiedTiles = new List<Vector2Int>();
    public Vector2Int specialTile = NoTile;
    public Vector2Int outputTile = NoTile;
    public bool digOre = false;
    public bool isBelt = false;
    public List<string> scriptsToDisableDuringPlacement = new List<string>();
    
    public bool singleInstance = false;
    public int instanceCount = 0;
    [SerializeField]
    private List<CurrencyAmount> priceList = new List<CurrencyAmount>();
    public Dictionary<CurrencyType, int> Price
    {
        get
        {
            Dictionary<CurrencyType, int> priceDict = new Dictionary<CurrencyType, int>();
            foreach (var item in priceList)
            {
                priceDict[item.currencyType] = item.amount;
            }
            return priceDict;
        }
    }
    public Building(GameObject prefab, List<Vector2Int> tiles, List<CurrencyAmount> buildingPrice, Vector2Int? output = null, bool isSingleInstance = false, Vector2Int? special = null, bool dig = false, bool belt = false, List<string> scriptsToDisable = null)
    {
        buildingPrefab = prefab;
        occupiedTiles = new List<Vector2Int>(tiles);
        priceList = buildingPrice;
        singleInstance = isSingleInstance;
        outputTile = output ?? NoTile;
        specialTile = special ?? NoTile;
        digOre = dig;
        isBelt = belt;
        scriptsToDisableDuringPlacement = new List<string>(scriptsToDisable);
    }


    public Building DeepCopy()
    {
        Building copy = new Building(
            buildingPrefab,
            new List<Vector2Int>(occupiedTiles),
            new List<CurrencyAmount>(priceList.ConvertAll(ca => new CurrencyAmount(ca.currencyType, ca.amount))),
            outputTile,
            singleInstance,
            specialTile,
            digOre,
            isBelt,
            new List<string>(scriptsToDisableDuringPlacement)
        );
        copy.instanceCount = 0; // Reset instance count
        return copy;
    }


    public bool HasSpecialTile => specialTile != NoTile;
    public bool HasOutputTile => outputTile != NoTile;
}

[CreateAssetMenu(fileName = "New Buildings", menuName = "Sci-Factory Data/Building Data")]
public class BuildingData : ScriptableObject
{
    public List<Building> buildings = new List<Building>();

    public BuildingData DeepCopy()
    {
        BuildingData copy = CreateInstance<BuildingData>();
        copy.buildings = new List<Building>(buildings.ConvertAll(b => b.DeepCopy()));
        return copy;
    }
}