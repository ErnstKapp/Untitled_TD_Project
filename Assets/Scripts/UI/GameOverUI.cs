using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows a Game Over panel when lives hit 0.
/// Hook Retry and Back to Overworld buttons to the public methods below.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scenes")]
    [SerializeField] private string overworldSceneName = "Overworld_Scene";

    private bool hasShown;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        hasShown = false;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += Show;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= Show;
    }

    private void Show()
    {
        if (hasShown) return;
        hasShown = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToOverworld()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(overworldSceneName);
    }
}

