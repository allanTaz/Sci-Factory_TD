using UnityEngine;
using System.Collections;

public class TowerAttackVFX : MonoBehaviour
{
    public GameObject vfxPrefab;
    public float projectileSpeed = 5f;

    [Header("Curve Control")]
    public float pos2Speed = 1f;
    public float pos3Speed = 1.5f;
    public float pos2Randomness = 0.5f;
    public float pos3Randomness = 0.7f;

    public Transform tower;
    public Transform enemy;

    private GameObject vfxInstance;
    private Transform pos1, pos2, pos3, pos4;
    private Vector3 initialPos2, initialPos3;
    private bool isAttacking = false;

    public void StartAttack()
    {
        if (tower == null || enemy == null)
        {
            Debug.LogError("Tower or Enemy transform is not set!");
            return;
        }

        if (isAttacking)
        {
            // If already attacking, just update the VFX
            StopAllCoroutines();
            StartCoroutine(AnimateAttack());
            return;
        }

        isAttacking = true;
        vfxInstance = Instantiate(vfxPrefab, tower.position, Quaternion.identity);

        pos1 = vfxInstance.transform.Find("Pos1");
        pos2 = vfxInstance.transform.Find("Pos2");
        pos3 = vfxInstance.transform.Find("Pos3");
        pos4 = vfxInstance.transform.Find("Pos4");

        pos1.position = tower.position;
        pos4.position = tower.position;

        Vector3 direction = (enemy.position - tower.position).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

        initialPos2 = tower.position + direction * 0.33f + perpendicular * Random.Range(-pos2Randomness, pos2Randomness);
        initialPos3 = tower.position + direction * 0.66f + perpendicular * Random.Range(-pos3Randomness, pos3Randomness);

        pos2.position = initialPos2;
        pos3.position = initialPos3;

        StartCoroutine(AnimateAttack());
    }

    public void StopAttack()
    {
        isAttacking = false;
        StopAllCoroutines();
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }
    }

    private IEnumerator AnimateAttack()
    {
        float elapsedDistance = 0f;
        Vector3 startPosition = pos4.position;

        while (isAttacking && enemy != null)
        {
            float totalDistance = Vector3.Distance(startPosition, enemy.position);

            // Calculate the current position based on elapsed distance
            float t = elapsedDistance / totalDistance;
            Vector3 currentPos = Vector3.Lerp(startPosition, enemy.position, t);

            // Update pos4 position
            pos4.position = currentPos;

            // Update positions of pos2 and pos3
            UpdateCurveControlPoints(t);

            // Check if pos4 has reached the enemy
            if (Vector3.Distance(pos4.position, enemy.position) < 0.1f)
            {
                // Pos4 has reached the enemy, keep it there
                pos4.position = enemy.position;
            }
            else
            {
                // Continue moving pos4 towards the enemy
                elapsedDistance += projectileSpeed * Time.deltaTime;
                elapsedDistance = Mathf.Min(elapsedDistance, totalDistance);
            }

            yield return null;
        }

        // If the loop ended because the enemy is null, clean up
        if (enemy == null)
        {
            StopAttack();
        }
    }

    private void UpdateCurveControlPoints(float t)
    {
        if (tower == null || enemy == null) return;

        Vector3 currentDirection = (enemy.position - tower.position).normalized;
        Vector3 perpendicular = Vector3.Cross(currentDirection, Vector3.up).normalized;

        // Calculate new positions for pos2 and pos3
        Vector3 newPos2 = Vector3.Lerp(tower.position, enemy.position, 0.33f) +
            perpendicular * (Mathf.Sin(Time.time * pos2Speed * 2 * Mathf.PI) * pos2Randomness);
        Vector3 newPos3 = Vector3.Lerp(tower.position, enemy.position, 0.66f) +
            perpendicular * (Mathf.Sin((Time.time + 0.5f) * pos3Speed * 2 * Mathf.PI) * pos3Randomness);

        // Smoothly move pos2 and pos3
        pos2.position = Vector3.Lerp(pos2.position, newPos2, Time.deltaTime * 5f);
        pos3.position = Vector3.Lerp(pos3.position, newPos3, Time.deltaTime * 5f);
    }
}