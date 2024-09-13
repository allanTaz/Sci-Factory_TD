using UnityEngine;
using System.Collections.Generic;

public class ChunkExpansionSystem : MonoBehaviour
{
    [SerializeField] private GridGenerator gridGenerator;
    [SerializeField] private BuildingPlacer buildingPlacer;
    [SerializeField] private GameObject enemySpawnerPrefab;

    private WFCChunkGenerator chunkGenerator;
    private HashSet<Vector2Int> existingChunkPositions = new HashSet<Vector2Int>();
    private int chunkSize;

    private void Start()
    {
        chunkGenerator = new WFCChunkGenerator();
        chunkSize = WFCChunkGenerator.GetChunkSize();

        // Initialize with the first chunk
        AddChunk(Vector2Int.zero, true);
        TriggerExpansion();
    }

    public void TriggerExpansion()
    {
        Vector2Int newChunkPosition = GetRandomAdjacentChunkPosition();
        if (newChunkPosition != Vector2Int.one * -1)
        {
            AddChunk(newChunkPosition, false);
        }
        else
        {
            Debug.LogWarning("No valid positions for new chunks!");
        }
    }

    private Vector2Int GetRandomAdjacentChunkPosition()
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int existingChunk in existingChunkPositions)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int newPosition = existingChunk + direction;
                if (!existingChunkPositions.Contains(newPosition))
                {
                    possiblePositions.Add(newPosition);
                }
            }
        }

        if (possiblePositions.Count > 0)
        {
            return possiblePositions[Random.Range(0, possiblePositions.Count)];
        }

        return Vector2Int.one * -1; // No valid positions
    }

    private void AddChunk(Vector2Int chunkPosition, bool isInitialChunk)
    {
        WFCChunkGenerator.CellType[,] chunkData = chunkGenerator.GenerateChunk(isInitialChunk);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector2Int globalGridPosition = new Vector2Int(
                    chunkPosition.x * chunkSize + x,
                    chunkPosition.y * chunkSize + y
                );

                gridGenerator.EnsureGridCoverage(globalGridPosition);

                // Place objects based on cell type
                switch (chunkData[x, y])
                {
                    case WFCChunkGenerator.CellType.Ore:
                        gridGenerator.SetCellAsOre(globalGridPosition);
                        break;
                    case WFCChunkGenerator.CellType.EnemySpawner:
                        buildingPlacer.PlaceObjectAtPosition(globalGridPosition, enemySpawnerPrefab);
                        break;
                }
            }
        }

        existingChunkPositions.Add(chunkPosition);
    }
}