using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    public delegate void CurrencyChangedHandler();
    public static event CurrencyChangedHandler OnCurrencyChanged;

    private int _currency = 0;
    public int Currency
    {
        get { return _currency; }
        private set
        {
            _currency = value;
            OnCurrencyChanged?.Invoke();
        }
    }

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

    public void AddCurrency(int amount)
    {
        Currency += amount;
        Debug.Log($"Currency added. New total: {Currency}");
    }
    public bool RemoveCurrency(int amount)
    {
        if (Currency >= amount)
        {
            Currency -= amount;
            Debug.Log($"Currency removed. New total: {Currency}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough currency. Current total: {Currency}");
            return false;
        }
    }
    public int GetCurrency()
    {
        return Currency;
    }
}