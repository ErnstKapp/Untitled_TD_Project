using System;
using UnityEngine;

/// <summary>
/// Manages meta currency that persists between levels.
/// This is separate from in-game currency and used for meta upgrades.
/// </summary>
public class MetaCurrencyManager : MonoBehaviour
{
    public static MetaCurrencyManager Instance { get; private set; }

    private const string META_CURRENCY_KEY = "MetaCurrency";

    [Header("Meta Currency Settings")]
    [SerializeField] private int metaCurrency = 0;

    public int MetaCurrency => metaCurrency;

    /// <summary>Fired after meta currency changes (amount is the new total).</summary>
    public event Action<int> OnMetaCurrencyChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMetaCurrency();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Adds meta currency and saves it.
    /// </summary>
    public void AddMetaCurrency(int amount)
    {
        metaCurrency += amount;
        SaveMetaCurrency();
        OnMetaCurrencyChanged?.Invoke(metaCurrency);
    }

    /// <summary>
    /// Sets meta currency and saves it.
    /// </summary>
    public void SetMetaCurrency(int amount)
    {
        metaCurrency = amount;
        SaveMetaCurrency();
        OnMetaCurrencyChanged?.Invoke(metaCurrency);
    }

    /// <summary>
    /// Spends meta currency if available.
    /// </summary>
    public bool SpendMetaCurrency(int amount)
    {
        if (CanAfford(amount))
        {
            metaCurrency -= amount;
            SaveMetaCurrency();
            OnMetaCurrencyChanged?.Invoke(metaCurrency);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if player can afford the amount.
    /// </summary>
    public bool CanAfford(int amount)
    {
        return metaCurrency >= amount;
    }

    /// <summary>
    /// Saves meta currency to PlayerPrefs.
    /// </summary>
    private void SaveMetaCurrency()
    {
        PlayerPrefs.SetInt(META_CURRENCY_KEY, metaCurrency);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads meta currency from PlayerPrefs.
    /// </summary>
    private void LoadMetaCurrency()
    {
        metaCurrency = PlayerPrefs.GetInt(META_CURRENCY_KEY, 0);
    }

    /// <summary>
    /// Resets meta currency (for testing/debugging).
    /// </summary>
    public void ResetMetaCurrency()
    {
        metaCurrency = 0;
        SaveMetaCurrency();
    }
}
