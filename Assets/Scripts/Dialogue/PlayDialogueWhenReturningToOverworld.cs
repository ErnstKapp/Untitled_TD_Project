using UnityEngine;

/// <summary>
/// Place this in the Overworld scene. When the player returns from a level (e.g. Stadium, Paris),
/// it plays the dialogue configured in DialogueManager → Dialogues When Returning To Overworld.
/// Configure that list in the DialogueManager: one entry per level (scene name + dialogue asset).
/// </summary>
public class PlayDialogueWhenReturningToOverworld : MonoBehaviour
{
    [Tooltip("Optional delay in seconds before starting the dialogue (e.g. 0.5 so overworld is visible first)")]
    [SerializeField] private float delaySeconds;

    private void Start()
    {
        Debug.Log("[PlayDialogueWhenReturningToOverworld] Start() – checking for overworld return dialogue.");
        if (DialogueManager.Instance == null)
        {
            Debug.Log("[PlayDialogueWhenReturningToOverworld] No DialogueManager, skipping.");
            return;
        }
        DialogueData dialogue = DialogueManager.Instance.GetAndClearDialogueForReturningFromOverworld();
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
        {
            if (dialogue == null)
                Debug.Log("[PlayDialogueWhenReturningToOverworld] No dialogue returned (not from a level or no entry for that scene).");
            else
                Debug.Log("[PlayDialogueWhenReturningToOverworld] Dialogue has no lines, skipping.");
            return;
        }
        if (delaySeconds > 0f)
        {
            Debug.Log($"[PlayDialogueWhenReturningToOverworld] Playing in {delaySeconds}s.");
            pendingDialogue = dialogue;
            Invoke(nameof(PlayPending), delaySeconds);
        }
        else
            Play(dialogue);
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

    private void Play(DialogueData dialogue)
    {
        if (DialogueManager.Instance == null || dialogue == null) return;
        Debug.Log($"[PlayDialogueWhenReturningToOverworld] Playing '{dialogue.dialogueTitle}' ({dialogue.lines.Length} lines).");
        DialogueManager.Instance.StartDialogue(dialogue, null);
    }
}
