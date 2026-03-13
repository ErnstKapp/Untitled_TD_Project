using UnityEngine;

/// <summary>
/// Add this to a GameObject in Overworld_Scene. When the player returns after finishing a level,
/// it plays the matching dialogue (e.g. Stadium return). Uses LevelProgressionManager.LastCompletedLevel.
/// </summary>
[AddComponentMenu("Dialogue/Overworld Return Dialogue")]
public class OverworldReturnDialogue : MonoBehaviour
{
    [Header("Return dialogues")]
    [Tooltip("Dialogue to play when coming back after finishing Stadium_Scene")]
    [SerializeField] private DialogueData stadiumReturnDialogue;

    [Tooltip("Dialogue to play when coming back after finishing Paris_Scene (optional)")]
    [SerializeField] private DialogueData parisReturnDialogue;

    [Tooltip("Delay before showing dialogue (e.g. 0.3 so overworld is visible first)")]
    [SerializeField] private float delaySeconds = 0.3f;

    private void Start()
    {
        string last = LevelProgressionManager.LastCompletedLevel;
        Debug.Log($"[OverworldReturnDialogue] Start() – LastCompletedLevel='{last ?? "(null)"}', DialogueManager exists={DialogueManager.Instance != null}");
        if (DialogueManager.Instance == null)
            return;
        if (string.IsNullOrEmpty(last))
            return;

        DialogueData toPlay = null;
        if (last == "Stadium_Scene")
            toPlay = stadiumReturnDialogue;
        else if (last == "Paris_Scene")
            toPlay = parisReturnDialogue;

        if (toPlay == null || toPlay.lines == null || toPlay.lines.Length == 0)
        {
            Debug.Log($"[OverworldReturnDialogue] No dialogue to play for '{last}' (asset not assigned or empty). Assign Stadium Return Dialogue in Inspector.");
            return;
        }

        LevelProgressionManager.LastCompletedLevel = null;
        Debug.Log($"[OverworldReturnDialogue] Playing return dialogue for '{last}' ('{toPlay.dialogueTitle}', {toPlay.lines.Length} lines).");

        if (delaySeconds > 0f)
        {
            pendingDialogue = toPlay;
            Invoke(nameof(PlayPending), delaySeconds);
        }
        else
        {
            Debug.Log("[OverworldReturnDialogue] Calling DialogueManager.StartDialogue now.");
            Play(toPlay);
        }
    }

    private DialogueData pendingDialogue;

    private void PlayPending()
    {
        if (pendingDialogue != null)
        {
            Play(pendingDialogue);
            pendingDialogue = null;
        }
    }

    private void Play(DialogueData data)
    {
        if (DialogueManager.Instance == null || data == null)
        {
            Debug.LogWarning("[OverworldReturnDialogue] Play() skipped – DialogueManager or data null.");
            return;
        }
        Debug.Log("[OverworldReturnDialogue] Play() – calling DialogueManager.StartDialogue.");
        DialogueManager.Instance.StartDialogue(data, null);
    }
}
