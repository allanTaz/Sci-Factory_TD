using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricTower : MonoBehaviour
{
    public TowerAttackVFX attackVFX;
    public float attackRange = 10f;

    private void Start()
    {
        attackVFX.tower = this.transform;
        attackVFX.maxRange = attackRange;
    }

    private void Update()
    {
        List<Enemy> enemiesInRange = Physics.OverlapSphere(transform.position, attackRange)
            .Select(c => c.GetComponent<Enemy>())
            .Where(e => e != null)
            .ToList();

        attackVFX.UpdateEnemiesInRange(enemiesInRange);
        attackVFX.StartAttack();
    }
}