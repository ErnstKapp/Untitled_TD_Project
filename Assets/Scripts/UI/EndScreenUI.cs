using UnityEngine;
using TMPro;

/// <summary>
/// Displays the end screen popup after waves are completed.
/// Shows lives remaining and converts them to meta currency.
/// </summary>
public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI livesRemainingText;
    [SerializeField] private TextMeshProUGUI metaCurrencyEarnedText;
    [SerializeField] private TextMeshProUGUI totalMetaCurrencyText;

    [Header("Settings")]
    [Tooltip("How much meta currency is earned per life remaining")]
    [SerializeField] private int metaCurrencyPerLife = 10;

    private bool hasShownEndScreen = false;
    private bool isSubscribed = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[EndScreenUI] Multiple EndScreenUI instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        hasShownEndScreen = false;
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
        else
            Debug.LogError("[EndScreenUI] End screen panel is not assigned in the Inspector!");
        SubscribeToWaveCompletion();
    }
    
    private void OnEnable()
    {
        // Reset state when component is enabled (e.g., new scene)
        hasShownEndScreen = false;
        
        // Also try subscribing when enabled (in case component is enabled after WaveManager)
        SubscribeToWaveCompletion();
    }

    private void SubscribeToWaveCompletion()
    {
        if (isSubscribed)
        {
            return; // Already subscribed
        }
        
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnAllWavesCompleted += ShowEndScreen;
            isSubscribed = true;
        }
        else
            Debug.LogWarning("[EndScreenUI] WaveManager.Instance is null. Will try again in Update().");
    }

    private void Update()
    {
        // Try to subscribe if we haven't yet (in case WaveManager wasn't ready in Start)
        if (!isSubscribed && WaveManager.Instance != null)
        {
            SubscribeToWaveCompletion();
        }
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnAllWavesCompleted -= ShowEndScreen;
        }
    }

    /// <summary>
    /// Shows the end screen with lives remaining and meta currency earned.
    /// </summary>
    public void ShowEndScreen()
    {
        // If the player already died (common when the final enemy triggers both
        // "last wave complete" and "lose last life" in the same frame), don't show the victory screen.
        if (GameManager.Instance != null && GameManager.Instance.CurrentLives <= 0)
            return;

        if (hasShownEndScreen)
            return;
        hasShownEndScreen = true;
        if (endScreenPanel == null)
        {
            Debug.LogError("[EndScreenUI] End screen panel is not assigned!");
            return;
        }

        // Get lives remaining
        int livesRemaining = 0;
        if (GameManager.Instance != null)
        {
            livesRemaining = GameManager.Instance.CurrentLives;
        }

        // Calculate meta currency earned
        int metaCurrencyEarned = livesRemaining * metaCurrencyPerLife;

        // Ensure MetaCurrencyManager exists
        if (MetaCurrencyManager.Instance == null)
        {
            GameObject metaCurrencyObj = new GameObject("MetaCurrencyManager");
            metaCurrencyObj.AddComponent<MetaCurrencyManager>();
        }

        // Add meta currency
        if (MetaCurrencyManager.Instance != null)
        {
            MetaCurrencyManager.Instance.AddMetaCurrency(metaCurrencyEarned);
        }

        // Ensure LevelProgressionManager exists
        if (LevelProgressionManager.Instance == null)
        {
            GameObject levelProgressionObj = new GameObject("LevelProgressionManager");
            levelProgressionObj.AddComponent<LevelProgressionManager>();
        }

        // Mark level as completed so overworld can show next level button
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (LevelProgressionManager.Instance != null)
        {
            LevelProgressionManager.Instance.MarkLevelCompleted(currentSceneName);
            Debug.Log($"[EndScreenUI] Marked level completed: {currentSceneName}");
        }

        // Update UI text
        if (livesRemainingText != null)
        {
            livesRemainingText.text = $"{livesRemaining}";
        }

        if (metaCurrencyEarnedText != null)
        {
            metaCurrencyEarnedText.text = $"{metaCurrencyEarned}";
        }

        if (totalMetaCurrencyText != null && MetaCurrencyManager.Instance != null)
        {
            totalMetaCurrencyText.text = $"{MetaCurrencyManager.Instance.MetaCurrency}";
        }

        // Show panel
        endScreenPanel.SetActive(true);

        // Pause the game
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Hides the end screen and resumes time.
    /// Call this from button onClick events if needed.
    /// </summary>
    public void HideEndScreen()
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Resumes time before scene transition.
    /// Call this from button onClick events before loading a new scene.
    /// </summary>
    public void ResumeTime()
    {
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Test method to manually show the end screen (for debugging).
    /// </summary>
    [ContextMenu("Test Show End Screen")]
    public void TestShowEndScreen()
    {
        ShowEndScreen();
    }
}
