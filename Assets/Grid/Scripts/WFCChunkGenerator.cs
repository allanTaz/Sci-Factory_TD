using UnityEngine;
using System.Collections.Generic;

public class WFCChunkGenerator
{
    private const int CHUNK_SIZE = 10;
    private List<CellType> cellTypes;
    private CellType[,] grid;

    public enum CellType
    {
        Empty,
        Ore,
        EnemySpawner
    }

    public WFCChunkGenerator()
    {
        this.cellTypes = new List<CellType> { CellType.Empty, CellType.Ore, CellType.EnemySpawner };
        this.grid = new CellType[CHUNK_SIZE, CHUNK_SIZE];
    }

    public CellType[,] GenerateChunk(bool isInitialChunk)
    {
        ResetGrid();

        if (isInitialChunk)
        {
            // Initial chunk only has empty cells
            return grid;
        }

        // Place one ore and one enemy spawner
        PlaceCell(CellType.Ore);
        PlaceCell(CellType.EnemySpawner);

        return grid;
    }

    private void ResetGrid()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                grid[x, y] = CellType.Empty;
            }
        }
    }

    private void PlaceCell(CellType cellType)
    {
        int x = Random.Range(1, CHUNK_SIZE-1);
        int y = Random.Range(1, CHUNK_SIZE-1);

        if (grid[x, y] == CellType.Empty)
        {
            grid[x, y] = cellType;
        }
        else {
            PlaceCell(cellType);
        }
    }

    public static int GetChunkSize()
    {
        return CHUNK_SIZE;
    }
}