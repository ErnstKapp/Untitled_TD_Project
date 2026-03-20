using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows a panel at level start (e.g. a tip). Assign the panel and a "Continue" button;
/// the panel is shown on Start and hidden when the button is clicked.
/// </summary>
public class LevelStartTipPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button continueButton;
    [Tooltip("Optional: set tip text at runtime. If null, use whatever you set in the Inspector on the panel's text.")]
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Tip content (optional)")]
    [TextArea(2, 4)]
    [SerializeField] private string tipMessage = "Enemies take extra damage from towers that match their weakness genre (e.g. Blues enemies from Blues towers).";

    [Header("Dialogue Timing")]
    [Tooltip("If true, waits until the current dialogue finishes before showing the tip panel.")]
    [SerializeField] private bool waitForDialogueToFinish = true;

    [Tooltip("How long to wait after scene start for dialogue to begin before showing the tip (in seconds).")]
    [SerializeField] private float dialogueStartGraceSeconds = 0.25f;

    private bool isSubscribedToDialogue;
    private bool hasShown;

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        if (tipText != null && !string.IsNullOrEmpty(tipMessage))
            tipText.text = tipMessage;

        if (panel == null)
            return;

        if (waitForDialogueToFinish)
        {
            panel.SetActive(false);
            hasShown = false;

            // If dialogue is already active, wait until it ends.
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                SubscribeToDialogueEnded();
                return;
            }

            // Otherwise, wait a short grace period in case the dialogue starts right after Start().
            StartCoroutine(ShowIfDialogueDoesNotStartCoroutine());
            return;
        }

        panel.SetActive(true);
        hasShown = true;
    }

    private System.Collections.IEnumerator ShowIfDialogueDoesNotStartCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < dialogueStartGraceSeconds)
        {
            // If dialogue becomes active during the grace window, wait for it to end.
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                SubscribeToDialogueEnded();
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Dialogue didn't start within the grace period -> show tip now.
        if (panel != null)
            panel.SetActive(true);
        hasShown = true;
    }

    private void SubscribeToDialogueEnded()
    {
        if (isSubscribedToDialogue) return;
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        isSubscribedToDialogue = true;
    }

    private void OnDialogueEnded()
    {
        if (hasShown) return;
        hasShown = true;
        if (panel != null)
            panel.SetActive(true);
        UnsubscribeFromDialogueEnded();
    }

    private void UnsubscribeFromDialogueEnded()
    {
        if (!isSubscribedToDialogue) return;
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        isSubscribedToDialogue = false;
    }

    private void OnContinueClicked()
    {
        if (panel != null)
            panel.SetActive(false);

        // If the user dismisses the tip while waiting, prevent it from re-showing later.
        hasShown = true;
        UnsubscribeFromDialogueEnded();

        // Also hide the button's parent so the whole tip goes away if Panel was assigned to the button by mistake
        if (continueButton != null && continueButton.transform.parent != null)
            continueButton.transform.parent.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeFromDialogueEnded();
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }
}
