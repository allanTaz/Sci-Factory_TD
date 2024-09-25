using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private int initialCurrency = 5;
    [SerializeField] private TextMeshProUGUI blueCurrencyText;
    [SerializeField] private TextMeshProUGUI YellowCurrencyText;
    [SerializeField] private TextMeshProUGUI redCurrencyText;
    private Dictionary<CurrencyType, TextMeshProUGUI> currencyTexts = new Dictionary<CurrencyType, TextMeshProUGUI>();
    private void Start()
    {
        currencyTexts = new Dictionary<CurrencyType, TextMeshProUGUI> {
        {CurrencyType.Blue, blueCurrencyText},
        {CurrencyType.Yellow, YellowCurrencyText },
        {CurrencyType.Red, redCurrencyText }
        };
        CurrencyManager.Instance.AddCurrency(CurrencyType.Blue, initialCurrency);
    
    }

    private void OnEnable()
    {
        CurrencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
    }

    private void UpdateCurrencyDisplay(CurrencyType currencyType)
    {
        if (currencyTexts[currencyType] != null)
        {
            currencyTexts[currencyType].text = CurrencyManager.Instance.GetCurrency(currencyType).ToString();
        }
    }
}