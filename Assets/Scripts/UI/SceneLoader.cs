using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple script to load scenes. Attach to a GameObject and connect buttons to the methods.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
    
    /// <summary>
    /// Test method to verify button clicks are working. Connect this first to test.
    /// </summary>
    public void TestButtonClick() { }
    
    /// <summary>
    /// Test method with no parameters - easier to connect.
    /// </summary>
    public void OnButtonClicked() { }
    /// <summary>
    /// Loads a scene by name.
    /// Checks if the level is unlocked before loading.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load (must be in Build Settings)</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneLoader] Scene name is empty! Cannot load scene.");
            return;
        }

        if (LevelProgressionManager.Instance == null)
        {
            GameObject managerObj = new GameObject("LevelProgressionManager");
            managerObj.AddComponent<LevelProgressionManager>();
        }
        
        if (!LevelProgressionManager.Instance.IsLevelUnlocked(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' is locked! Cannot load.");
            return;
        }
        
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                sceneExists = true;
                break;
            }
        }
        
        if (!sceneExists)
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' NOT FOUND in Build Settings!");
            return;
        }

        if (sceneName == "Overworld_Scene" && DialogueManager.Instance != null)
        {
            string from = SceneManager.GetActiveScene().name;
            Debug.Log($"[SceneLoader] Loading Overworld_Scene – setting overworld return from '{from}'.");
            DialogueManager.SetReturningFromScene(from);
        }

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.PlayCutsceneForSceneIfAny(sceneName, () => DoLoadScene(sceneName));
        else
            DoLoadScene(sceneName);
    }
    
    private void DoLoadScene(string sceneName)
    {
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneLoader] Exception loading scene '{sceneName}': {e.Message}");
        }
    }

    /// <summary>
    /// Loads a scene by build index.
    /// </summary>
    /// <param name="sceneIndex">Index of the scene in Build Settings</param>
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[SceneLoader] Invalid scene index: {sceneIndex} (total scenes in build: {SceneManager.sceneCountInBuildSettings})");
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Loads the next scene in Build Settings.
    /// </summary>
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentIndex + 1;
        
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[SceneLoader] No next scene available!");
            return;
        }
        LoadScene(nextSceneIndex);
    }

    /// <summary>
    /// Reloads the current scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Quits the game (doesn't work in Editor, only in builds).
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
}
