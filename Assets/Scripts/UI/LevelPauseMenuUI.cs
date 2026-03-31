using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// In-level pause/settings popup.
/// - Toggle with ESC
/// - Can also be opened from a UI cogwheel button via OpenMenu()
/// - Exiting the stage loads the overworld immediately (no cutscene).
/// </summary>
public class LevelPauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel;

    [Header("Scene")]
    [SerializeField] private string overworldSceneName = "Overworld_Scene";

    [Header("Input")]
    [SerializeField] private bool allowEscToggle = true;

    private bool isOpen;

    private void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
        isOpen = false;
    }

    private void Update()
    {
        if (!allowEscToggle) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen) CloseMenu();
            else OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (menuPanel == null) return;
        menuPanel.SetActive(true);
        isOpen = true;
        Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        if (menuPanel == null) return;
        menuPanel.SetActive(false);
        isOpen = false;
        Time.timeScale = 1f;
    }

    public void ExitStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(overworldSceneName);
    }
}

