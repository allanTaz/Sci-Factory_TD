using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StartWaveButton : MonoBehaviour
{
    private Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(RunWaves);
    }
    public List<EnemySpawner> FindAllEnemySpawners()
    {
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        return new List<EnemySpawner>(spawners);
    }

    private void RunWaves()
    {
        List<EnemySpawner> spawners = FindAllEnemySpawners();
        foreach(EnemySpawner spawner in spawners)
        {
            spawner.StartWaves();
        }
    }
}
