using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricTower : MonoBehaviour
{
    private TowerAttackVFX attackVFX;
    public float attackRange = 10f;
    public float damagePerTick = 10f;
    public float tickInterval = 0.5f;

    private float lastTickTime;

    private void Start()
    {
        attackVFX = GetComponentInChildren<TowerAttackVFX>();
        attackVFX.tower = gameObject.transform.GetChild(0);
        attackVFX.maxRange = attackRange;
        lastTickTime = Time.time;
    }

    private void Update()
    {
        List<Enemy> enemiesInRange = Physics.OverlapSphere(transform.position, attackRange)
            .Select(c => c.GetComponent<Enemy>())
            .Where(e => e != null)
            .ToList();

        attackVFX.UpdateEnemiesInRange(enemiesInRange);
        attackVFX.StartAttack();

        // Apply damage at regular intervals
        if (Time.time - lastTickTime >= tickInterval)
        {
            ApplyDamageToEnemies();
            lastTickTime = Time.time;
        }
    }

    private void ApplyDamageToEnemies()
    {
        List<Enemy> affectedEnemies = attackVFX.GetAffectedEnemies();
        foreach (Enemy enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerTick);
            }
        }
    }
}