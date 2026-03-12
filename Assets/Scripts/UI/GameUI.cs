using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;

    private void Start()
    {
        UpdateUI();

        // Subscribe to events
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStarted += OnWaveStarted;
            WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;
        }
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update Currency
        if (currencyText != null && CurrencyManager.Instance != null)
        {
            currencyText.text = $"${CurrencyManager.Instance.CurrentCurrency}";
        }

        // Update Lives
        if (livesText != null && GameManager.Instance != null)
        {
            livesText.text = $"Lives: {GameManager.Instance.CurrentLives}";
        }

        // Update Wave
        if (waveText != null && WaveManager.Instance != null)
        {
            waveText.text = $"Wave: {WaveManager.Instance.CurrentWave}/{WaveManager.Instance.TotalWaves}";
        }

        // Update Enemies Remaining
        if (enemiesRemainingText != null && WaveManager.Instance != null)
        {
            enemiesRemainingText.text = $"Enemies: {WaveManager.Instance.RemainingEnemies}";
        }
    }

    private void OnCurrencyChanged(int newCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = $"Currency: ${newCurrency}";
        }
    }

    private void OnWaveStarted(int waveNumber)
    {
        UpdateUI();
    }

    private void OnWaveCompleted(int waveNumber)
    {
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStarted -= OnWaveStarted;
            WaveManager.Instance.OnWaveCompleted -= OnWaveCompleted;
        }
    }
}
