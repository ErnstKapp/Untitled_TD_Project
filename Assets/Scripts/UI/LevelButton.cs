using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages a level button's state (locked/unlocked) based on level progression.
/// Attach this to level buttons in the overworld.
/// </summary>
public class LevelButton : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Scene name to load when button is clicked (also used for progression checking)")]
    [SerializeField] private string sceneNameToLoad = "";

    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private GameObject lockedOverlay; // Optional: visual indicator for locked state

    [Header("Visual Settings")]
    [Tooltip("If true, this GameObject is hidden when locked (only appears after previous stage is completed). Put LevelButton on the root of each button so the whole button hides.")]
    [SerializeField] private bool hideWhenLocked = true;

    [Tooltip("Log unlock state to Console when UpdateButtonState runs (for debugging).")]
    [SerializeField] private bool debugLog = false;
    
    [Tooltip("Color when level is locked (used when hideWhenLocked is false)")]
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Tooltip("Color when level is unlocked")]
    [SerializeField] private Color unlockedColor = Color.white;

    private bool isUnlocked = false;

    private void Awake()
    {
        // Get components if not assigned
        if (button == null)
            button = GetComponent<Button>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null)
            button.onClick.AddListener(OnButtonIntercepted);

        // Apply lock/hide as early as possible (manager may be created by first button)
        EnsureLevelProgressionManager();
        UpdateButtonState();
    }
    
    /// <summary>
    /// Intercepts button clicks to check if level is unlocked.
    /// This runs before the persistent SceneLoader listener.
    /// </summary>
    private void OnButtonIntercepted()
    {
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogWarning($"[LevelButton] sceneNameToLoad is empty! Set it in the Inspector. (buttonObject='{gameObject.name}', component='{nameof(LevelButton)}')");
            return;
        }
        EnsureLevelProgressionManager();
        if (LevelProgressionManager.Instance != null)
            isUnlocked = LevelProgressionManager.Instance.IsLevelUnlocked(sceneNameToLoad);
        else
            Debug.LogWarning("[LevelButton] LevelProgressionManager.Instance is null after Ensure!");
        if (!isUnlocked)
        {
            button.interactable = false;
            return;
        }
    }

    private void Start()
    {
        // Ensure LevelProgressionManager exists
        EnsureLevelProgressionManager();
        
        // Update button state
        UpdateButtonState();
    }
    
    private void EnsureLevelProgressionManager()
    {
        if (LevelProgressionManager.Instance == null)
        {
            GameObject managerObj = new GameObject("LevelProgressionManager");
            managerObj.AddComponent<LevelProgressionManager>();
        }
    }

    private void OnEnable()
    {
        // Update state when enabled (in case progression changed)
        UpdateButtonState();
    }

    [ContextMenu("Refresh Button State")]
    private void RefreshButtonState() => UpdateButtonState();

    /// <summary>
    /// Updates the button's visual state and interactability based on level progression.
    /// </summary>
    public void UpdateButtonState()
    {
        // Ensure manager exists
        EnsureLevelProgressionManager();
        
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogWarning($"[LevelButton] Scene name is not set! Cannot check unlock status. (buttonObject='{gameObject.name}')");
            isUnlocked = false;
        }
        else if (LevelProgressionManager.Instance == null)
        {
            isUnlocked = !string.IsNullOrEmpty(sceneNameToLoad) && sceneNameToLoad.IndexOf("Stadium", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (debugLog)
                Debug.Log($"[LevelButton] '{gameObject.name}' ({sceneNameToLoad}): no manager, isUnlocked={isUnlocked}");
        }
        else
        {
            isUnlocked = LevelProgressionManager.Instance.IsLevelUnlocked(sceneNameToLoad);
            if (debugLog)
                Debug.Log($"[LevelButton] '{gameObject.name}' ({sceneNameToLoad}): isUnlocked={isUnlocked}, hideWhenLocked={hideWhenLocked}, willBeActive={isUnlocked}");
        }

        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button b in allButtons)
        {
            b.interactable = isUnlocked;
            Navigation nav = b.navigation;
            nav.mode = isUnlocked ? Navigation.Mode.Automatic : Navigation.Mode.None;
            b.navigation = nav;
        }
        if (allButtons.Length == 0)
            Debug.LogWarning($"[LevelButton] No Button component found for scene '{sceneNameToLoad}' – lock state won't block clicks!");

        // Keep cached reference in sync for other code (e.g. OnButtonIntercepted)
        if (button != null)
            button.interactable = isUnlocked;

        // Update visual appearance
        if (buttonImage != null)
        {
            buttonImage.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // Show/hide entire button when locked (so buttons "appear" after previous stage)
        if (hideWhenLocked)
            gameObject.SetActive(isUnlocked);
        else
        {
            gameObject.SetActive(true);
            if (lockedOverlay != null)
                lockedOverlay.SetActive(!isUnlocked);
            if (buttonText != null)
                buttonText.text = !isUnlocked ? $"{sceneNameToLoad}\n(Locked)" : sceneNameToLoad;
        }
    }

    /// <summary>
    /// Called when the button is clicked. Only works if level is unlocked.
    /// You can connect this to the button's OnClick event, or let the existing SceneLoader handle it.
    /// </summary>
    public void OnButtonClicked()
    {
        if (!isUnlocked || string.IsNullOrEmpty(sceneNameToLoad))
        {
            if (string.IsNullOrEmpty(sceneNameToLoad))
                Debug.LogWarning("[LevelButton] Scene name is not set! Assign sceneNameToLoad in Inspector.");
            return;
        }
        SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
        if (sceneLoader != null)
            sceneLoader.LoadScene(sceneNameToLoad);
        else
        {
            Debug.LogError($"[LevelButton] SceneLoader not found in scene! Cannot load scene '{sceneNameToLoad}'.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneNameToLoad);
        }
    }
    
    /// <summary>
    /// Checks if the button should be interactable based on level progression.
    /// Call this from button's OnClick if you want to prevent clicks on locked levels.
    /// </summary>
    public bool CanInteract()
    {
        return isUnlocked;
    }

    /// <summary>
    /// Sets the scene name to load.
    /// </summary>
    public void SetSceneName(string sceneName)
    {
        sceneNameToLoad = sceneName;
        UpdateButtonState();
    }
}
