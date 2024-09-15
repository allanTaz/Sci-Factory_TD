using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private WaveSystem waveSystem;

    private int currentWaveIndex = 0;
    private int currentEnemyIndex = 0;
    private int currentEnemyCount = 0;
    private int activeEnemies = 0;

    public delegate void WaveEventHandler(int waveNumber);
    public event WaveEventHandler OnWaveStart;
    public event WaveEventHandler OnWaveComplete;

    public delegate void SpawnEventHandler(Enemy spawnedEnemy);
    public event SpawnEventHandler OnEnemySpawned;

    public void StartWaves()
    {
        StartCoroutine(StartWaveSystem());
    }

    private IEnumerator StartWaveSystem()
    {
        while (currentWaveIndex < waveSystem.waves.Count)
        {
            Wave currentWave = waveSystem.waves[currentWaveIndex];
            OnWaveStart?.Invoke(currentWaveIndex + 1);
            yield return StartCoroutine(SpawnWave(currentWave));

            yield return new WaitUntil(() => activeEnemies == 0);
            OnWaveComplete?.Invoke(currentWaveIndex + 1);

            yield return new WaitForSeconds(currentWave.timeBeforeNextWave);
            currentWaveIndex++;
        }

        Debug.Log("All waves completed!");
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        Debug.Log($"Starting Wave {currentWaveIndex + 1}");

        currentEnemyIndex = 0;
        while (currentEnemyIndex < wave.enemies.Count)
        {
            EnemySpawn enemySpawn = wave.enemies[currentEnemyIndex];
            currentEnemyCount = 0;

            while (currentEnemyCount < enemySpawn.count)
            {
                SpawnEnemy(enemySpawn.enemyData);
                currentEnemyCount++;
                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }

            currentEnemyIndex++;
        }
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        Vector3 spawnPosition = transform.position;
        GameObject enemyObject = Instantiate(enemyData.prefab, spawnPosition, Quaternion.identity);

        Enemy enemyComponent = enemyObject.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.enemyData = enemyData;
            enemyComponent.OnDestroyed += HandleEnemyDestroyed;
            enemyComponent.pathfinder = GetComponent<Pathfinder>();
            activeEnemies++;
            OnEnemySpawned?.Invoke(enemyComponent);
        }
        else
        {
            Debug.LogError("Spawned enemy prefab does not have an Enemy component!");
        }
    }

    private void HandleEnemyDestroyed(Enemy destroyedEnemy)
    {
        activeEnemies--;
        destroyedEnemy.OnDestroyed -= HandleEnemyDestroyed;
    }

    public int GetCurrentWave()
    {
        return currentWaveIndex + 1;
    }

    public int GetTotalWaves()
    {
        return waveSystem.waves.Count;
    }
}