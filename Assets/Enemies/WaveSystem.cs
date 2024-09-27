using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class EnemySpawn
{
    public EnemyData enemyData;
    public int count;
}

[Serializable]
public class Wave
{
    public List<EnemySpawn> enemies = new List<EnemySpawn>();
    public float timeBetweenSpawns;
    public float timeBeforeNextWave;
}

[Serializable]
public class EnemySpawnChance
{
    public EnemyData enemyData;
    public float spawnWeight;
}

[CreateAssetMenu(fileName = "New Dynamic Wave System", menuName = "Wave System/Dynamic")]
public class WaveSystem : ScriptableObject
{
    public List<EnemySpawnChance> possibleEnemies = new List<EnemySpawnChance>();
    
    [Header("Wave Generation Settings")]
    public int minEnemyCount = 3;
    public int maxEnemyCount = 6;
    public float enemyCountIncreasePerWave = 2f;
    public float minTimeBetweenSpawns = 0.5f;
    public float maxTimeBetweenSpawns = 2f;
    public float minTimeBeforeNextWave = 3f;
    public float maxTimeBeforeNextWave = 10f;

    public Wave GenerateWave(int waveNumber)
    {
        Wave wave = new Wave();
        
        // Calculate total enemies for this wave
        int totalEnemies = UnityEngine.Random.Range(minEnemyCount, maxEnemyCount);
        
        // Distribute enemies
        while (totalEnemies > 0)
        {
            EnemySpawnChance selectedEnemy = ChooseRandomEnemy();
            int count = Mathf.Min(UnityEngine.Random.Range(1, 5), totalEnemies);
            
            wave.enemies.Add(new EnemySpawn { enemyData = selectedEnemy.enemyData, count = count });
            totalEnemies -= count;
        }
        
        // Set random times
        wave.timeBetweenSpawns = UnityEngine.Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns);
        wave.timeBeforeNextWave = UnityEngine.Random.Range(minTimeBeforeNextWave, maxTimeBeforeNextWave);
        
        return wave;
    }

    private EnemySpawnChance ChooseRandomEnemy()
    {
        float totalWeight = 0f;
        foreach (var enemy in possibleEnemies)
        {
            totalWeight += enemy.spawnWeight;
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var enemy in possibleEnemies)
        {
            currentWeight += enemy.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return enemy;
            }
        }
        
        // Fallback to the last enemy if something goes wrong
        return possibleEnemies[possibleEnemies.Count - 1];
    }
}