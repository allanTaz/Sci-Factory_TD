using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TowerAttackVFX : MonoBehaviour
{
    public GameObject vfxPrefab;
    public float projectileSpeed = 5f;
    public int maxChainCount = 3;
    public float chainRadius = 5f;
    public float maxRange = 10f; // Maximum range of the tower

    [Header("Curve Control")]
    public float pos2Speed = 1f;
    public float pos3Speed = 1.5f;
    public float pos2Randomness = 0.5f;
    public float pos3Randomness = 0.7f;

    public Transform tower { get; set; }
    private List<Enemy> affectedEnemies = new List<Enemy>();
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private List<LightningSegment> lightningSegments = new List<LightningSegment>();

    private class LightningSegment
    {
        public GameObject vfxObject;
        public Transform startPoint;
        public Transform endPoint;
        public Transform pos1, pos2, pos3, pos4;

        public LightningSegment(GameObject vfx, Transform start, Transform end)
        {
            vfxObject = vfx;
            startPoint = start;
            endPoint = end;
            pos1 = vfx.transform.Find("Pos1");
            pos2 = vfx.transform.Find("Pos2");
            pos3 = vfx.transform.Find("Pos3");
            pos4 = vfx.transform.Find("Pos4");
        }
    }

    private void Update()
    {
        UpdateLightningChain();
    }

    public void UpdateEnemiesInRange(List<Enemy> newEnemiesInRange)
    {
        enemiesInRange = newEnemiesInRange.Where(e => e != null).ToList();
    }

    public void StartAttack()
    {
        if (tower == null || enemiesInRange.Count == 0)
        {
            //Debug.LogWarning("Tower is not set or no enemies in range!");
            return;
        }

        UpdateLightningChain();
    }

    private void CreateLightningSegment(Transform start, Transform end)
    {
        GameObject vfxInstance = Instantiate(vfxPrefab, start.position, Quaternion.identity);
        LightningSegment segment = new LightningSegment(vfxInstance, start, end);
        lightningSegments.Add(segment);
        StartCoroutine(AnimateLightningSegment(segment));
    }

    private IEnumerator AnimateLightningSegment(LightningSegment segment)
    {
        while (segment.startPoint != null && segment.endPoint != null)
        {
            UpdateLightningPositions(segment);
            yield return null;
        }

        // If start or end point is destroyed, remove this segment
        RemoveLightningSegment(segment);
    }

    private void UpdateLightningPositions(LightningSegment segment)
    {
        if (segment.startPoint == null || segment.endPoint == null || segment.pos4 == null) return;

        segment.pos1.position = segment.startPoint.position;
        Vector3 targetPosition = segment.endPoint.position;

        // Move the lightning end position towards the target
        segment.pos4.position = Vector3.MoveTowards(
            segment.pos4.position,
            targetPosition,
            projectileSpeed * Time.deltaTime
        );

        // Update curve control points
        UpdateCurveControlPoints(segment);
    }

    private void UpdateCurveControlPoints(LightningSegment segment)
    {
        Vector3 currentDirection = (segment.pos4.position - segment.pos1.position).normalized;
        Vector3 perpendicular = Vector3.Cross(currentDirection, Vector3.up).normalized;

        float t = Vector3.Distance(segment.pos1.position, segment.pos4.position) /
                  Vector3.Distance(segment.pos1.position, segment.endPoint.position);

        Vector3 newPos2 = Vector3.Lerp(segment.pos1.position, segment.pos4.position, 0.33f) +
            perpendicular * (Mathf.Sin(Time.time * pos2Speed * 2 * Mathf.PI) * pos2Randomness);
        Vector3 newPos3 = Vector3.Lerp(segment.pos1.position, segment.pos4.position, 0.66f) +
            perpendicular * (Mathf.Sin((Time.time + 0.5f) * pos3Speed * 2 * Mathf.PI) * pos3Randomness);

        segment.pos2.position = Vector3.Lerp(segment.pos2.position, newPos2, Time.deltaTime * 5f);
        segment.pos3.position = Vector3.Lerp(segment.pos3.position, newPos3, Time.deltaTime * 5f);
    }

    private void UpdateLightningChain()
    {
        List<Transform> chainTargets = new List<Transform> { tower };
        Enemy currentEnemy = null;
        affectedEnemies.Clear();

        // Find the first enemy in range of the tower
        currentEnemy = enemiesInRange
            .Where(e => Vector3.Distance(tower.position, e.transform.position) <= maxRange)
            .OrderBy(e => Vector3.Distance(tower.position, e.transform.position))
            .FirstOrDefault();

        while (currentEnemy != null && chainTargets.Count < maxChainCount + 1)
        {
            chainTargets.Add(currentEnemy.transform);
            affectedEnemies.Add(currentEnemy);

            // Find next enemy in chain
            currentEnemy = enemiesInRange
                .Where(e => !chainTargets.Contains(e.transform) &&
                            Vector3.Distance(chainTargets.Last().position, e.transform.position) <= chainRadius)
                .OrderBy(e => Vector3.Distance(chainTargets.Last().position, e.transform.position))
                .FirstOrDefault();
        }

        // Update lightning segments
        for (int i = 0; i < chainTargets.Count - 1; i++)
        {
            if (i >= lightningSegments.Count)
            {
                CreateLightningSegment(chainTargets[i], chainTargets[i + 1]);
            }
            else
            {
                lightningSegments[i].startPoint = chainTargets[i];
                lightningSegments[i].endPoint = chainTargets[i + 1];
            }
        }

        // Remove excess segments
        while (lightningSegments.Count > chainTargets.Count - 1)
        {
            RemoveLightningSegment(lightningSegments.Last());
        }
    }

    public List<Enemy> GetAffectedEnemies()
    {
        return affectedEnemies;
    }
    private void RemoveLightningSegment(LightningSegment segment)
    {
        lightningSegments.Remove(segment);
        Destroy(segment.vfxObject);
    }

    public void StopAttack()
    {
        foreach (var segment in lightningSegments)
        {
            Destroy(segment.vfxObject);
        }
        lightningSegments.Clear();
    }
}