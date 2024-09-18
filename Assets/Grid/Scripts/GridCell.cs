using System;
using UnityEngine;

public class GridCell
{
    public Vector2Int Position { get; private set; }
    public bool IsOccupied { get; private set; }
    public bool IsOre { get; private set; }
    public OreType OreType { get; private set; }
    public GameObject PlacedObject { get; private set; }

    public GridCell(int x, int y, bool isOre = false, OreType oreType = OreType.None)
    {
        Position = new Vector2Int(x, y);
        IsOre = isOre;
        OreType = oreType;
    }

    public void SetAsOre(OreType type)
    {
        IsOre = true;
        OreType = type;
    }

    public bool PlaceObject(GameObject obj)
    {
        if (!IsOccupied)
        {
            PlacedObject = obj;
            IsOccupied = true;
            return true;
        }
        return false;
    }

    public void RemoveObject()
    {
        if (IsOccupied)
        {
            PlacedObject = null;
            IsOccupied = false;
        }
    }
}