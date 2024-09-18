using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Sci-Factory Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxHealth = 100f;
    public float moveSpeed = 1f;
    public int damageToBase = 1;
    public int currencyValue = 10;
    public GameObject prefab;

    // Define different abilities
    public bool canFly = false;
    public bool isArmored = false;
    public bool canRegenerate = false;
    public float regenerationRate = 0f;

    // Add more properties as needed for different enemy types
}