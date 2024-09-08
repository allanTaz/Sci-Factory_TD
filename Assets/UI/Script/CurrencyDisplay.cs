using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;

    private void Start()
    {
        UpdateCurrencyDisplay();
    }

    private void OnEnable()
    {
        CurrencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
    }

    private void UpdateCurrencyDisplay()
    {
        if (currencyText != null)
        {
            currencyText.text = CurrencyManager.Instance.GetCurrency().ToString();
        }
    }
}