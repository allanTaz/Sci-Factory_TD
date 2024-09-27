using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private WaveSystem waveSystem;
    [SerializeField] private float initialWait = 15f;
    private int currentWaveNumber = 0;
    private int activeEnemies = 0;

    public delegate void WaveEventHandler(int waveNumber);
    public event WaveEventHandler OnWaveStart;
    public event WaveEventHandler OnWaveComplete;

    public delegate void SpawnEventHandler(Enemy spawnedEnemy);
    public event SpawnEventHandler OnEnemySpawned;
    private ChunkExpansionSystem chunkExpansionSystem;
    private void Awake()
    {
        chunkExpansionSystem = FindAnyObjectByType<ChunkExpansionSystem>();
        StartCoroutine(StartInfiniteWaveSystem());

    }

    private IEnumerator StartInfiniteWaveSystem()
    {
        yield return new WaitForSeconds(initialWait);
        while (true) // Infinite loop for endless waves
        {
            currentWaveNumber++;
            Wave currentWave = waveSystem.GenerateWave(currentWaveNumber);
            OnWaveStart?.Invoke(currentWaveNumber);
            yield return StartCoroutine(SpawnWave(currentWave));

            yield return new WaitUntil(() => activeEnemies == 0);
            OnWaveComplete?.Invoke(currentWaveNumber);
            if (currentWaveNumber == 1)
                chunkExpansionSystem.TriggerExpansion();
            yield return new WaitForSeconds(currentWave.timeBeforeNextWave);
        }
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        //Debug.Log($"Starting Wave {currentWaveNumber}");

        foreach (EnemySpawn enemySpawn in wave.enemies)
        {
            for (int i = 0; i < enemySpawn.count; i++)
            {
                SpawnEnemy(enemySpawn.enemyData);
                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }
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
        return currentWaveNumber;
    }
}