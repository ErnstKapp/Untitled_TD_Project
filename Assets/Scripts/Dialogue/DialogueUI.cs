using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ren'Py-style dialogue UI: dimmed background (map visible but darkened), two character slots,
/// one speaker highlighted per line, text and optional Next button.
/// Use AdvanceDialogue() for Next or a full-screen "click anywhere" button.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel & dim background")]
    [Tooltip("Full-screen panel that holds everything. Should have a semi-transparent black Image so the map shows but darkened.")]
    [SerializeField] private GameObject panelRoot;
    [Tooltip("The Image used to dim the background (black with alpha ~0.5–0.7). Assign if different from panelRoot's Image.")]
    [SerializeField] private Image dimOverlay;

    [Header("Character portraits (left and right)")]
    [SerializeField] private Image portraitLeft;
    [SerializeField] private Image portraitRight;
    [Tooltip("Alpha for the portrait that is NOT speaking this line (0 = invisible, 1 = full)")]
    [Range(0f, 1f)]
    [SerializeField] private float inactivePortraitAlpha = 0.4f;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;

    [Header("Advance")]
    [Tooltip("Button to go to next line. For 'click anywhere to continue', add a full-screen Button and assign its OnClick to AdvanceDialogue().")]
    [SerializeField] private Button nextButton;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            Debug.LogWarning("[DialogueUI] Panel Root is not assigned! Panel will never show. Assign it in the Inspector.");
        // Do NOT add AdvanceDialogue here – wire the button in the Inspector (On Click → DialogueUI.AdvanceDialogue).
        // Adding it in code as well would double-fire on one click and skip the second line.
        if (nextButton == null)
            Debug.LogWarning("[DialogueUI] Next Button is not assigned. Assign in Inspector and wire On Click → AdvanceDialogue().");
    }

    /// <summary>
    /// Call from Next button or a full-screen "click anywhere" button. Public so you can wire multiple buttons in the Inspector.
    /// </summary>
    public void AdvanceDialogue()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.Advance();
    }

    /// <summary>
    /// Show the dialogue panel with dimmed background.
    /// </summary>
    public void Show()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
        else
            Debug.LogWarning("[DialogueUI] Show() called but panelRoot is null! Assign Panel Root in the Inspector.");
        if (dimOverlay != null)
            dimOverlay.raycastTarget = true;
    }

    /// <summary>
    /// Hide the dialogue panel.
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    /// <summary>
    /// Display one line: set speaker portrait (left or right), dim the other, set text and speaker name.
    /// </summary>
    public void ShowLine(DialogueLine line)
    {
        if (line.portrait != null)
        {
            if (line.speakerOnLeft)
            {
                SetPortrait(portraitLeft, line.portrait, true);
                SetPortrait(portraitRight, null, false);
            }
            else
            {
                SetPortrait(portraitLeft, null, false);
                SetPortrait(portraitRight, line.portrait, true);
            }
        }
        else
        {
            SetPortraitAlpha(portraitLeft, inactivePortraitAlpha);
            SetPortraitAlpha(portraitRight, inactivePortraitAlpha);
        }

        if (dialogueText != null)
            dialogueText.text = line.text;
        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;
    }

    private void SetPortrait(Image img, Sprite sprite, bool speaking)
    {
        if (img == null) return;
        img.sprite = sprite;
        img.enabled = sprite != null;
        if (sprite != null)
        {
            Color c = img.color;
            c.a = speaking ? 1f : inactivePortraitAlpha;
            img.color = c;
        }
    }

    private void SetPortraitAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    public bool IsVisible => panelRoot != null && panelRoot.activeSelf;
}
