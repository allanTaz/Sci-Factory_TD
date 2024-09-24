using UnityEngine;
using System.Collections.Generic;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    public delegate void CurrencyChangedHandler(string currencyType);
    public static event CurrencyChangedHandler OnCurrencyChanged;

    private Dictionary<string, int> currencies = new Dictionary<string, int>
    {
        { "Blue", 0 },
        { "Yellow", 0 },
        { "Red", 0 }
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

    public void AddCurrency(string currencyType, int amount)
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

    public bool RemoveCurrency(string currencyType, int amount)
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

    public int GetCurrency(string currencyType)
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