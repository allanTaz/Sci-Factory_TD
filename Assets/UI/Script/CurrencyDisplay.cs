using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI blueCurrencyText;
    [SerializeField] private TextMeshProUGUI YellowCurrencyText;
    [SerializeField] private TextMeshProUGUI redCurrencyText;
    private Dictionary<string, TextMeshProUGUI> currencyTexts = new Dictionary<string, TextMeshProUGUI>();
    private void Start()
    {
        currencyTexts = new Dictionary<string, TextMeshProUGUI> {
        {"Blue", blueCurrencyText},
        {"Yellow", YellowCurrencyText },
        {"Red", redCurrencyText }
    
        };
    
    
    }

    private void OnEnable()
    {
        CurrencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
    }

    private void UpdateCurrencyDisplay(string currencyType)
    {
        if (currencyTexts[currencyType] != null)
        {
            currencyTexts[currencyType].text = CurrencyManager.Instance.GetCurrency(currencyType).ToString();
        }
    }
}