using UnityEngine;

/// <summary>
/// DEPRECATED: Use OverworldReturnDialogue instead (assign Stadium/Paris return dialogues on that component).
/// This script no longer does anything; remove it and add OverworldReturnDialogue to a GameObject in Overworld_Scene.
/// </summary>
public class PlayDialogueWhenReturningToOverworld : MonoBehaviour
{
    private void Start()
    {
        // Replaced by OverworldReturnDialogue + LevelProgressionManager.LastCompletedLevel
    }
}
