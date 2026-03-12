using UnityEngine;

/// <summary>
/// Manages level progression and tracks which levels are completed.
/// Levels are unlocked sequentially (level 2 unlocks when level 1 is complete, etc.)
/// </summary>
public class LevelProgressionManager : MonoBehaviour
{
    public static LevelProgressionManager Instance { get; private set; }

    private const string LEVEL_COMPLETED_KEY_PREFIX = "LevelCompleted_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Marks a level as completed by scene name.
    /// </summary>
    /// <param name="sceneName">Name of the scene/level that was completed</param>
    public void MarkLevelCompleted(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[LevelProgressionManager] Cannot mark level complete - scene name is null or empty");
            return;
        }

        PlayerPrefs.SetInt(LEVEL_COMPLETED_KEY_PREFIX + sceneName, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Checks if a level is completed by scene name.
    /// </summary>
    public bool IsLevelCompleted(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }
        
        string key = LEVEL_COMPLETED_KEY_PREFIX + sceneName;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    /// <summary>
    /// Checks if a level is unlocked (previous level is completed).
    /// Level 1 (Stadium_Scene) is always unlocked.
    /// </summary>
    /// <param name="sceneName">Name of the scene to check</param>
    public bool IsLevelUnlocked(string sceneName)
    {
        if (sceneName.Contains("Stadium"))
            return true;
        string previousSceneName = GetPreviousSceneName(sceneName);
        if (string.IsNullOrEmpty(previousSceneName))
        {
            Debug.LogWarning($"[LevelProgressionManager] Could not determine previous level for '{sceneName}', assuming unlocked");
            return true;
        }
        return IsLevelCompleted(previousSceneName);
    }

    /// <summary>
    /// Resets all level progression (for testing/debugging).
    /// Call this from Unity Editor or add a debug button.
    /// Refreshes all LevelButtons so the UI updates immediately.
    /// </summary>
    [ContextMenu("Reset All Progression")]
    public void ResetProgression()
    {
        // Clear known level keys
        PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY_PREFIX + "Stadium_Scene");
        PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY_PREFIX + "Paris_Scene");
        // Add more as needed when you add levels
        
        PlayerPrefs.Save();
        LevelButton[] buttons = FindObjectsOfType<LevelButton>();
        foreach (LevelButton lb in buttons)
            lb.UpdateButtonState();
    }
    
    /// <summary>
    /// Debug method to check current progression status.
    /// </summary>
    [ContextMenu("Debug: Check Progression Status")]
    public void DebugCheckProgression()
    {
        Debug.Log("=== LEVEL PROGRESSION STATUS ===");
        Debug.Log($"Stadium_Scene completed: {IsLevelCompleted("Stadium_Scene")}");
        Debug.Log($"Paris_Scene completed: {IsLevelCompleted("Paris_Scene")}");
        Debug.Log($"Stadium_Scene unlocked: {IsLevelUnlocked("Stadium_Scene")}");
        Debug.Log($"Paris_Scene unlocked: {IsLevelUnlocked("Paris_Scene")}");
        Debug.Log("================================");
    }

    /// <summary>
    /// Gets the previous scene name for a given scene.
    /// </summary>
    private string GetPreviousSceneName(string sceneName)
    {
        // Map scenes to their previous scenes
        if (sceneName.Contains("Paris"))
        {
            return "Stadium_Scene";
        }
        // Add more mappings as you add more levels
        // e.g., if you add "Level3_Scene", map it to "Paris_Scene"
        
        // Try to extract number and decrement
        string numberStr = System.Text.RegularExpressions.Regex.Match(sceneName, @"\d+").Value;
        if (!string.IsNullOrEmpty(numberStr) && int.TryParse(numberStr, out int levelNum) && levelNum > 1)
        {
            // Try to construct previous scene name
            string previousName = sceneName.Replace(levelNum.ToString(), (levelNum - 1).ToString());
            return previousName;
        }
        
        return null;
    }
}
