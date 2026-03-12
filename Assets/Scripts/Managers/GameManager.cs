using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isGamePaused = false;

    [Header("Game Settings")]
    public int startingLives = 20;
    public int startingCurrency = 100;

    private int currentLives;
    private CurrencyManager currencyManager;
    private WaveManager waveManager;

    public int CurrentLives => currentLives;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroy only our components so this scene's WaveManager stays and can become Instance
            Destroy(GetComponent<GameManager>());
            var other = GetComponent<CurrencyManager>();
            if (other != null) Destroy(other);
            return;
        }

        currencyManager = GetComponent<CurrencyManager>();
        waveManager = GetComponent<WaveManager>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// When a level scene loads, reset lives and currency so each level starts fresh.
    /// Hub scenes (overworld, menu, home) do not trigger a reset.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isHubScene = scene.name == "Overworld_Scene" || scene.name == "Menu_Scene" || scene.name == "Home_Scene";
        if (!isHubScene)
            InitializeGame();
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        currentLives = startingLives;
        currencyManager?.SetCurrency(startingCurrency);
        isGameActive = true;
        isGamePaused = false;
        Time.timeScale = 1f; // Ensure game runs (fixes enemies not moving if we loaded level after end screen had paused time)
    }

    public void LoseLife(int amount = 1)
    {
        currentLives -= amount;
        currentLives = Mathf.Max(0, currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public void AddLives(int amount)
    {
        currentLives += amount;
    }

    public void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;
    }

    public void Victory()
    {
        isGameActive = false;
    }

    public void PauseGame()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
