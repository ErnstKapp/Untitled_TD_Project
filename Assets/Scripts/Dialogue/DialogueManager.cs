using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Runs dialogue sequences. Use PlayDialogueOnLoad in a scene for intro dialogue; overworld return dialogue is configured here (OverworldReturnDialogue is optional/legacy).
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Overworld return dialogue (play when returning from a level)")]
    [Tooltip("Assign in the scene that has DialogueManager. Plays when Overworld loads after finishing Stadium.")]
    [SerializeField] private DialogueData stadiumReturnDialogue;
    [Tooltip("Optional: plays when returning from Paris_Scene.")]
    [SerializeField] private DialogueData parisReturnDialogue;
    [Tooltip("Optional: plays when returning from Swamp_Scene.")]
    [SerializeField] private DialogueData swampReturnDialogue;
    [Tooltip("Optional: plays when returning from ProtoLLM_Scene.")]
    [SerializeField] private DialogueData protoLLMReturnDialogue;
    [Tooltip("Optional: plays when returning from LLM_Scene.")]
    [SerializeField] private DialogueData llmReturnDialogue;

    private DialogueData currentDialogue;
    private int currentIndex;
    private Action onComplete;

    /// <summary>True while a dialogue sequence is currently active.</summary>
    public bool IsDialogueActive => currentDialogue != null;

    /// <summary>Fired when the current dialogue ends (after the last line is advanced).</summary>
    public event Action OnDialogueEnded;

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
    /// When the active scene changes, clear cached UI. When returning to Overworld, re-acquire its Dialogue UI so we have a valid reference.
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        dialogueUI = null;
        if (scene.name == "Overworld_Scene")
        {
            Debug.Log("[DialogueManager] OnSceneLoaded: entered Overworld_Scene block.");
            dialogueUI = FindObjectOfType<DialogueUI>(true);
            if (dialogueUI != null)
                Debug.Log($"[DialogueManager] Overworld loaded – re-assigned Dialogue UI to '{dialogueUI.gameObject.name}'.");
            else
                Debug.LogWarning("[DialogueManager] Overworld loaded but no DialogueUI found in scene! Add a DialogueUI component to your dialogue panel in Overworld_Scene.");

            // Play return dialogue from here so it works even if OverworldReturnDialogue isn't in the scene
            string last = LevelProgressionManager.LastCompletedLevel;
            Debug.Log($"[DialogueManager] Overworld return check – LastCompletedLevel='{last ?? "(null)"}', stadium={stadiumReturnDialogue != null}, paris={parisReturnDialogue != null}, swamp={swampReturnDialogue != null}, protoLLM={protoLLMReturnDialogue != null}, llm={llmReturnDialogue != null}.");
            if (string.IsNullOrEmpty(last))
            {
                Debug.Log("[DialogueManager] Overworld return: LastCompletedLevel is null or empty, skipping (did you finish a level before returning?).");
            }
            else
            {
                DialogueData toPlay = null;
                if (last == "Stadium_Scene") toPlay = stadiumReturnDialogue;
                else if (last == "Paris_Scene") toPlay = parisReturnDialogue;
                else if (last == "Swamp_Scene") toPlay = swampReturnDialogue;
                else if (last == "ProtoLLM_Scene") toPlay = protoLLMReturnDialogue;
                else if (last == "LLM_Scene") toPlay = llmReturnDialogue;

                bool hasDialogue = toPlay != null && toPlay.lines != null && toPlay.lines.Length > 0;
                if (!hasDialogue)
                {
                    Debug.Log($"[DialogueManager] Overworld return: no return dialogue configured for '{last}'.");
                    LevelProgressionManager.LastCompletedLevel = null;
                    return;
                }

                LevelProgressionManager.LastCompletedLevel = null;
                StartDialogue(toPlay, null);
            }
        }
    }

    /// <summary>
    /// Start a dialogue sequence. onComplete is called when the player advances past the last line.
    /// </summary>
    public void StartDialogue(DialogueData data, Action onCompleteCallback)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            onCompleteCallback?.Invoke();
            OnDialogueEnded?.Invoke();
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

        Debug.Log($"[DialogueManager] StartDialogue: showing '{data.dialogueTitle}' ({data.lines.Length} lines), dialogueUI='{dialogueUI?.gameObject?.name}'.");
        dialogueUI.Show();
        dialogueUI.ShowLine(data.lines[0]);
        Debug.Log("[DialogueManager] Show() and ShowLine() done.");
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
        OnDialogueEnded?.Invoke();
        callback?.Invoke();
    }
}
