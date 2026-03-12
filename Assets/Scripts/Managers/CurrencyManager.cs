using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Currency Settings")]
    [SerializeField] private int currentCurrency = 0;

    public event Action<int> OnCurrencyChanged;

    public int CurrentCurrency => currentCurrency;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Destroy only this component so other components on this object (e.g. WaveManager) stay
            Destroy(this);
        }
    }

    public void SetCurrency(int amount)
    {
        currentCurrency = amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public bool CanAfford(int cost)
    {
        return currentCurrency >= cost;
    }

    public bool SpendCurrency(int amount)
    {
        if (CanAfford(amount))
        {
            currentCurrency -= amount;
            OnCurrencyChanged?.Invoke(currentCurrency);
            return true;
        }
        return false;
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
    }
}
