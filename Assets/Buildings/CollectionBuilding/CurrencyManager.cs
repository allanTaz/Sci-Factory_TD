using UnityEngine;
using System.Collections.Generic;

public enum CurrencyType
{
    Blue,
    Yellow,
    Red
}
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    public delegate void CurrencyChangedHandler(CurrencyType currencyType);
    public static event CurrencyChangedHandler OnCurrencyChanged;

    private Dictionary<CurrencyType, int> currencies = new Dictionary<CurrencyType, int>
    {
        { CurrencyType.Blue, 0 },
        { CurrencyType.Yellow, 0 },
        { CurrencyType.Red, 0 }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        if (currencies.ContainsKey(currencyType))
        {
            currencies[currencyType] += amount;
            OnCurrencyChanged?.Invoke(currencyType);
            Debug.Log($"{currencyType} currency added. New total: {currencies[currencyType]}");
        }
        else
        {
            Debug.LogError($"Invalid currency type: {currencyType}");
        }
    }

    public bool RemoveCurrency(CurrencyType currencyType, int amount)
    {
        if (currencies.ContainsKey(currencyType) && currencies[currencyType] >= amount)
        {
            currencies[currencyType] -= amount;
            OnCurrencyChanged?.Invoke(currencyType);
            Debug.Log($"{currencyType} currency removed. New total: {currencies[currencyType]}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough {currencyType} currency. Current total: {currencies[currencyType]}");
            return false;
        }
    }

    public int GetCurrency(CurrencyType currencyType)
    {
        if (currencies.ContainsKey(currencyType))
        {
            return currencies[currencyType];
        }
        else
        {
            Debug.LogError($"Invalid currency type: {currencyType}");
            return 0;
        }
    }
}