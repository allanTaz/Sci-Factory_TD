using UnityEngine;
using System;
using System.Collections.Generic;

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

    public Building(GameObject prefab, List<Vector2Int> tiles, Vector2Int? output = null, Vector2Int? special = null, bool dig = false, bool belt = false, List<string> scripts = null, List<string> scriptsToDisable = null)
    {
        buildingPrefab = prefab;
        occupiedTiles = new List<Vector2Int>(tiles);
        outputTile = output ?? NoTile;
        specialTile = special ?? NoTile;
        digOre = dig;
        isBelt = belt;
        scriptsToDisableDuringPlacement = scriptsToDisable ?? new List<string>();
    }

    public bool HasSpecialTile => specialTile != NoTile;
    public bool HasOutputTile => outputTile != NoTile;
}

[CreateAssetMenu(fileName = "New Buildings", menuName = "Sci-Factory Data/Building Data")]
public class BuildingData : ScriptableObject
{
    public List<Building> buildings = new List<Building>();
}