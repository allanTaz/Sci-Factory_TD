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
    public float timeBetweenSpawns = 1f;
    public float timeBeforeNextWave = 5f;
}

[CreateAssetMenu(fileName = "New Wave System", menuName = "Wave System")]
public class WaveSystem : ScriptableObject
{
    public List<Wave> waves = new List<Wave>();
}