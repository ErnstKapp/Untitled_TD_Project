using UnityEngine;

/// <summary>
/// Call this from a scene to play a dialogue when the scene loads (e.g. Stadium intro).
/// Add this to a GameObject in the scene, assign the Dialogue Data, and ensure the scene has a DialogueUI (canvas with dialogue panel).
/// Do NOT add this scene to "Cutscenes Before Scenes" in DialogueManager — those play in the previous scene before loading.
/// </summary>
public class PlayDialogueOnLoad : MonoBehaviour
{
    [Tooltip("Dialogue to play when this scene loads (e.g. Stadium intro)")]
    [SerializeField] private DialogueData dialogue;

    [Tooltip("Optional delay in seconds before starting the dialogue (e.g. 0.5 to let the scene appear first)")]
    [SerializeField] private float delaySeconds;

    private void Start()
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
            return;
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[PlayDialogueOnLoad] DialogueManager.Instance is null. Is DialogueManager in the scene (or DontDestroyOnLoad)?");
            return;
        }
        if (delaySeconds > 0f)
            Invoke(nameof(Play), delaySeconds);
        else
            Play();
    }

    private void Play()
    {
        DialogueManager.Instance.StartDialogue(dialogue, null);
    }
}
