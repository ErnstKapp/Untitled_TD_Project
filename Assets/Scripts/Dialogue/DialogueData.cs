using UnityEngine;

/// <summary>
/// One line of dialogue: who is speaking, their portrait, and the text.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Display name of the speaker (e.g. 'Coach', 'Referee')")]
    public string speakerName = "";
    [TextArea(2, 4)]
    public string text = "";
    [Tooltip("Portrait sprite for this line (who is 'on screen' talking). Leave empty to keep previous.")]
    public Sprite portrait;
    [Tooltip("True = speaker on left side, False = speaker on right side")]
    public bool speakerOnLeft = true;
}

/// <summary>
/// A sequence of dialogue lines (e.g. Paris intro cutscene).
/// Create via: Right-click in Project → Create → Tower Defense → Dialogue Data
/// </summary>
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Tower Defense/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Optional title for this dialogue (e.g. 'Paris Intro')")]
    public string dialogueTitle = "";
    public DialogueLine[] lines = new DialogueLine[0];
}
