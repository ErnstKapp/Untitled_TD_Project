using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place this on any GameObject in the Overworld scene (e.g. Canvas or an empty "OverworldSetup").
/// When the overworld loads (including when returning from a level), refreshes all level buttons
/// so newly unlocked levels (e.g. Paris after Stadium) appear.
/// </summary>
public class RefreshLevelButtonsOnOverworldLoad : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Overworld_Scene")
            return;

        LevelButton[] buttons = FindObjectsOfType<LevelButton>(true);
        foreach (LevelButton lb in buttons)
            lb.UpdateButtonState();
    }
}
