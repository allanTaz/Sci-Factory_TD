using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "OreData", menuName = "Sci-Factory Data/OreData", order = 1)]
public class OreData : ScriptableObject
{

    [System.Serializable]
    public class OreInfo
    {
        public OreType oreType;
        public GameObject oreCell;
        public GameObject orePrefab;
    }

    [SerializeField] private List<OreInfo> ores = new List<OreInfo>();

    private Dictionary<OreType, OreInfo> oreDictionary = new Dictionary<OreType, OreInfo>();

    private void OnEnable()
    {
        oreDictionary = ores.ToDictionary(o => o.oreType, o => o);
    }

    public OreInfo GetOreInfo(OreType type)
    {
        return oreDictionary.TryGetValue(type, out OreInfo info) ? info : null;
    }
}
public enum OreType
{
    Blue,
    Yellow,
    Red,
    None
}