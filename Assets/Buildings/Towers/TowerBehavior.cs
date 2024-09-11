using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class TowerBehavior : MonoBehaviour
{
    public TowerData towerData;
    public GameObject projectilePrefab;
    private Transform turretRotationPart;
    private Transform firePoint;

    private float fireCountdown = 0f;
    private Enemy targetEnemy;
    private List<Enemy> enemiesInRange = new List<Enemy>();

    private void Start()
    {
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        turretRotationPart = transform.GetChild(0).GetChild(0);
        firePoint = turretRotationPart.GetChild(0);
    }

    private void Update()
    {
        if (targetEnemy == null)
        {
            return;
        }

        // Rotate towards the target
        Vector3 dir = targetEnemy.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(turretRotationPart.rotation, lookRotation, Time.deltaTime * 5f).eulerAngles;
        turretRotationPart.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / towerData.fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        enemiesInRange = Physics.OverlapSphere(transform.position, towerData.range)
            .Select(c => c.GetComponent<Enemy>())
            .Where(e => e != null)
            .ToList();

        if (enemiesInRange.Count == 0)
        {
            targetEnemy = null;
            return;
        }

        // Find the enemy that has progressed furthest along the path
        targetEnemy = enemiesInRange
            .OrderByDescending(e => e.CurrentPathIndex)
            .First();
    }

    void Shoot()
    {
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Seek(targetEnemy.transform);
            projectile.damage = towerData.damage;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, towerData.range);
    }
}