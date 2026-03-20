using UnityEngine;

/// <summary>
/// Manages level progression and tracks which levels are completed.
/// Levels are unlocked sequentially (level 2 unlocks when level 1 is complete, etc.)
/// </summary>
public class LevelProgressionManager : MonoBehaviour
{
    public static LevelProgressionManager Instance { get; private set; }

    // Ordered list of gameplay levels used for unlocking.
    // Make sure these names match your scenes' actual file names AND LevelButton.sceneNameToLoad values.
    private static readonly string[] ProgressionOrder =
    {
        "Stadium_Scene",
        "Paris_Scene",
        "Swamp_Scene",
        "Pop_Scene",
        "Lvl5_Scene",
        "ProtoLLM_Scene"
    };

    public static string[] GetProgressionOrder()
    {
        return (string[])ProgressionOrder.Clone();
    }

    /// <summary>Scene name of the level that was just completed (e.g. Stadium_Scene). Cleared when overworld reads it.</summary>
    public static string LastCompletedLevel { get; set; }

    private const string LEVEL_COMPLETED_KEY_PREFIX = "LevelCompleted_";
    private const string PROGRESSION_INITIALIZED_KEY = "LevelProgression_Initialized";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureCleanProgressionOnFirstRun();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// In builds, first run has no saved progression so only Stadium shows. Editor keeps existing PlayerPrefs.
    /// </summary>
    private void EnsureCleanProgressionOnFirstRun()
    {
#if !UNITY_EDITOR
        if (PlayerPrefs.GetInt(PROGRESSION_INITIALIZED_KEY, 0) == 0)
        {
            PlayerPrefs.SetInt(PROGRESSION_INITIALIZED_KEY, 1);
            foreach (string sceneName in ProgressionOrder)
            {
                if (string.IsNullOrEmpty(sceneName)) continue;
                PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY_PREFIX + sceneName);
            }
            PlayerPrefs.Save();
        }
#endif
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
        LastCompletedLevel = sceneName;
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
        if (string.IsNullOrEmpty(sceneName)) return false;
        if (sceneName.IndexOf("Stadium", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        // Hub/menu scenes are always loadable (New Game, etc.)
        if (sceneName.IndexOf("Overworld", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            sceneName.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            sceneName.IndexOf("Home", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        string previousSceneName = GetPreviousSceneName(sceneName);
        if (string.IsNullOrEmpty(previousSceneName))
        {
            Debug.LogWarning($"[LevelProgressionManager] Could not determine previous level for '{sceneName}', treating as locked.");
            return false;
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
        foreach (string sceneName in ProgressionOrder)
        {
            if (string.IsNullOrEmpty(sceneName)) continue;
            PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY_PREFIX + sceneName);
        }
        
        PlayerPrefs.Save();
        Debug.Log("[LevelProgressionManager] Progression reset. Refreshing all LevelButtons (including inactive). Reload overworld or re-enter Play to see only Stadium.");
        LevelButton[] buttons = FindObjectsOfType<LevelButton>(true);
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
        foreach (string sceneName in ProgressionOrder)
        {
            if (string.IsNullOrEmpty(sceneName)) continue;
            Debug.Log($"{sceneName} completed: {IsLevelCompleted(sceneName)}");
            Debug.Log($"{sceneName} unlocked: {IsLevelUnlocked(sceneName)}");
        }
        Debug.Log("================================");
    }

    /// <summary>
    /// Gets the previous scene name for a given scene.
    /// </summary>
    private string GetPreviousSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;

        // Prefer the explicit progression order first (handles non-numeric scene names like Stadium/Paris/Swamp).
        int idx = -1;
        for (int i = 0; i < ProgressionOrder.Length; i++)
        {
            if (string.Equals(ProgressionOrder[i], sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                idx = i;
                break;
            }
        }
        if (idx >= 0)
            return idx == 0 ? null : ProgressionOrder[idx - 1];

        // Map scenes to their previous scenes (order: Stadium → Paris → Swamp). Case-insensitive for builds.
        if (sceneName.IndexOf("Paris", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return "Stadium_Scene";
        if (sceneName.IndexOf("Swamp", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return "Paris_Scene";
        if(sceneName.IndexOf("Pop", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return "Swamp_Scene";
        if(sceneName.IndexOf("Lvl5", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return "Pop_Scene";
        if(sceneName.IndexOf("ProtoLLM", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return "Lvl5_Scene";
        // Add more mappings as you add more levels
        
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
