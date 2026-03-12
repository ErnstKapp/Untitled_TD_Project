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

    private void Start()
    {
        if (panel != null)
            panel.SetActive(true);
        if (tipText != null && !string.IsNullOrEmpty(tipMessage))
            tipText.text = tipMessage;
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnContinueClicked()
    {
        if (panel != null)
            panel.SetActive(false);
        // Also hide the button's parent so the whole tip goes away if Panel was assigned to the button by mistake
        if (continueButton != null && continueButton.transform.parent != null)
            continueButton.transform.parent.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }
}
