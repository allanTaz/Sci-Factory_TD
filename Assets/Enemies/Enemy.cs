using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;

    private float currentHealth;
    private List<Vector3> path;
    private int currentPathIndex;
    public int CurrentPathIndex => currentPathIndex;
    public delegate void EnemyEventHandler(Enemy enemy);
    public event EnemyEventHandler OnDestroyed;

    [SerializeField] private GameObject healthBarPrefab;
    private Image healthBarFillImage;
    private RectTransform healthBarRectTransform;
    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("Enemy Data not assigned to enemy!");
            return;
        }

        currentHealth = enemyData.maxHealth;
        StartCoroutine(WaitForPathAndMove());

        if (enemyData.canRegenerate)
        {
            StartCoroutine(RegenerateHealth());
        }
        CreateHealthBar();
    }
    void CreateHealthBar()
    {
        if (healthBarPrefab != null)
        {
            Canvas worldSpaceCanvas = FindObjectOfType<Canvas>();
            if (worldSpaceCanvas != null)
            {
                GameObject healthBarObj = Instantiate(healthBarPrefab, worldSpaceCanvas.transform);
                healthBarRectTransform = healthBarObj.GetComponent<RectTransform>();
                healthBarFillImage = healthBarObj.transform.Find("Fill").GetComponent<Image>();

                if (healthBarFillImage == null)
                {
                    Debug.LogError("Fill Image not found in health bar prefab");
                }

                UpdateHealthBarPosition();
                UpdateHealthBar();
            }
            else
            {
                Debug.LogError("World Space Canvas not found in the scene");
            }
        }
        else
        {
            Debug.LogError("Health Bar Prefab not assigned to enemy");
        }
    }
    IEnumerator WaitForPathAndMove()
    {
        while (path == null || path.Count == 0)
        {
            yield return new WaitForSeconds(0.5f);
            RequestPath();
        }
        StartCoroutine(MoveAlongPath());
    }

    void RequestPath()
    {
        Pathfinder pathfinder = FindObjectOfType<Pathfinder>();
        if (pathfinder != null)
        {
            path = pathfinder.GetPath();
            currentPathIndex = 0;
        }
        else
        {
            Debug.LogError("Pathfinder not found in the scene.");
        }
    }

    IEnumerator MoveAlongPath()
    {
        while (currentPathIndex < path.Count)
        {
            Vector3 targetPosition = path[currentPathIndex];
            while (transform.position != targetPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, enemyData.moveSpeed * Time.deltaTime);
                yield return null;
            }
            currentPathIndex++;
        }

        // Enemy reached the end of the path
        ReachedDestination();
    }

    void ReachedDestination()
    {
        Debug.Log($"{enemyData.enemyName} reached the destination and dealt {enemyData.damageToBase} damage!");
        // Here you would implement logic to damage the player's base
        Destroy(gameObject);
    }
    void UpdateHealthBarPosition()
    {
        if (healthBarRectTransform != null)
        {
            Vector3 worldPosition = transform.position + Vector3.up*0.5f; // Adjust this offset as needed
            healthBarRectTransform.position = worldPosition;
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = currentHealth / enemyData.maxHealth;
        }
    }

    void LateUpdate()
    {
        UpdateHealthBarPosition();
    }
    public void TakeDamage(float damage)
    {
        if (enemyData.isArmored)
        {
            damage *= 0.5f; // Armored enemies take half damage
        }

        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{enemyData.enemyName} defeated! Player gains {enemyData.currencyValue} currency.");
        // Here you would implement logic to give the player currency
        OnDestroyed?.Invoke(this);
        Destroy(healthBarRectTransform.gameObject);
        Destroy(gameObject);
    }
    void OnDestroy()
    {
        // This ensures that even if the enemy is destroyed by other means (e.g., reaching the end of the path),
        // the EnemySpawner will still be notified.
        OnDestroyed?.Invoke(this);
    }

    IEnumerator RegenerateHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (currentHealth < enemyData.maxHealth)
            {
                currentHealth = Mathf.Min(currentHealth + enemyData.regenerationRate, enemyData.maxHealth);
                UpdateHealthBar();
            }
        }
    }
}