using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Data", menuName = "Sci-Factory Data/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public float damage;
    public float fireRate;
    public float range;
    public int cost;
    public GameObject prefab;

    // Add more properties as needed for different tower types
    public bool canTargetAir = false;
    public bool hasAreaOfEffect = false;
    public float aoeRadius = 0f;

    // Upgrade-related properties
    public int maxUpgradeLevel = 3;
    public float damageIncreasePerLevel = 5f;
    public float rangeIncreasePerLevel = 0.5f;
    public int upgradeCost = 50;
}