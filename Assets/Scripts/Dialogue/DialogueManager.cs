using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Runs a dialogue sequence (e.g. Paris intro). When done, can run a callback (e.g. load scene).
/// Register cutscenes per scene so SceneLoader can play them before loading.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Cutscenes before scenes")]
    [Tooltip("If the player loads this scene name, play this dialogue first; when done, load the scene.")]
    [SerializeField] private SceneCutscene[] cutscenesBeforeScenes = new SceneCutscene[0];

    [Header("Dialogue when returning to overworld")]
    [Tooltip("When the player returns to the overworld after completing a level, play this dialogue. One entry per level (e.g. Stadium_Scene → stadium complete dialogue, Paris_Scene → Paris complete dialogue).")]
    [SerializeField] private SceneCutscene[] dialoguesWhenReturningToOverworld = new SceneCutscene[0];

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;

    private static string returningFromScene;

    [Serializable]
    public struct SceneCutscene
    {
        public string sceneName;
        public DialogueData dialogue;
    }

    private DialogueData currentDialogue;
    private int currentIndex;
    private Action onComplete;

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

    private void Start()
    {
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// When the active scene changes, clear cached UI so we use the new scene's DialogueUI (e.g. Stadium has its own panel).
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        dialogueUI = null;
    }

    /// <summary>
    /// Call before loading the overworld scene so we know which level we're returning from. Used by SceneLoader when loading Overworld_Scene.
    /// </summary>
    public static void SetReturningFromScene(string sceneName)
    {
        returningFromScene = sceneName;
        Debug.Log($"[DialogueManager] Overworld return: set returning-from = '{sceneName}'.");
    }

    /// <summary>
    /// Get the dialogue to play when returning to overworld (based on which scene we came from), and clear the state so we only play once.
    /// Returns null if no dialogue is configured for that scene.
    /// </summary>
    public DialogueData GetAndClearDialogueForReturningFromOverworld()
    {
        Debug.Log($"[DialogueManager] Overworld return: GetAndClear – returningFromScene='{returningFromScene ?? "(null)"}', list length={dialoguesWhenReturningToOverworld?.Length ?? 0}");
        if (string.IsNullOrEmpty(returningFromScene))
        {
            Debug.Log("[DialogueManager] Overworld return: no returning-from scene set, skipping dialogue.");
            return null;
        }
        if (dialoguesWhenReturningToOverworld == null || dialoguesWhenReturningToOverworld.Length == 0)
        {
            Debug.Log("[DialogueManager] Overworld return: Dialogues When Returning To Overworld is empty. Add entries in Inspector.");
            returningFromScene = null;
            return null;
        }
        for (int i = 0; i < dialoguesWhenReturningToOverworld.Length; i++)
        {
            var entry = dialoguesWhenReturningToOverworld[i];
            if (entry.sceneName == returningFromScene && entry.dialogue != null)
            {
                DialogueData data = entry.dialogue;
                Debug.Log($"[DialogueManager] Overworld return: playing dialogue for '{returningFromScene}' → '{data.dialogueTitle}' ({data.lines?.Length ?? 0} lines).");
                returningFromScene = null;
                return data;
            }
        }
        Debug.Log($"[DialogueManager] Overworld return: no dialogue for '{returningFromScene}'. Registered: [{string.Join(", ", System.Array.ConvertAll(dialoguesWhenReturningToOverworld, e => "'" + e.sceneName + "'"))}]");
        returningFromScene = null;
        return null;
    }

    /// <summary>
    /// If there is a cutscene for this scene, play it and call onComplete when done; otherwise call onComplete immediately.
    /// </summary>
    public void PlayCutsceneForSceneIfAny(string sceneName, Action thenLoadScene)
    {
        DialogueData data = GetCutsceneForScene(sceneName);
        if (data != null && data.lines != null && data.lines.Length > 0)
            StartDialogue(data, thenLoadScene);
        else
            thenLoadScene?.Invoke();
    }

    public DialogueData GetCutsceneForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || cutscenesBeforeScenes == null)
            return null;
        for (int i = 0; i < cutscenesBeforeScenes.Length; i++)
        {
            var entry = cutscenesBeforeScenes[i];
            if (entry.sceneName == sceneName && entry.dialogue != null)
                return entry.dialogue;
        }
        return null;
    }

    /// <summary>
    /// Start a dialogue sequence. onComplete is called when the player advances past the last line.
    /// </summary>
    public void StartDialogue(DialogueData data, Action onCompleteCallback)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            onCompleteCallback?.Invoke();
            return;
        }

        // Use assigned reference if still valid, otherwise find in current scene (e.g. after loading back to overworld).
        // includeInactive: true so we find DialogueUI even if the panel is currently inactive (it starts hidden).
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>(true);
        if (dialogueUI == null)
        {
            Debug.LogWarning("[DialogueManager] No DialogueUI found! Assign the Dialogue Panel to DialogueManager's 'Dialogue UI' field (Inspector), or add a GameObject with DialogueUI in this scene. Skipping dialogue.");
            onCompleteCallback?.Invoke();
            return;
        }

        currentDialogue = data;
        currentIndex = 0;
        onComplete = onCompleteCallback;

        dialogueUI.Show();
        dialogueUI.ShowLine(data.lines[0]);
    }

    /// <summary>
    /// Advance to the next line, or end dialogue and run the completion callback.
    /// </summary>
    public void Advance()
    {
        if (currentDialogue == null || dialogueUI == null) return;

        currentIndex++;
        if (currentIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueUI.ShowLine(currentDialogue.lines[currentIndex]);
    }

    private void EndDialogue()
    {
        dialogueUI.Hide();
        currentDialogue = null;
        Action callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }
}
