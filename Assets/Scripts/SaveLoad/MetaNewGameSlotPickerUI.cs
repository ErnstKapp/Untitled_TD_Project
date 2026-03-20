using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to a GameObject in your Menu scene.
/// Use it to pick which save slot to start a new game in.
///
/// Wiring:
/// - Menu "New Game" button -> call OpenPopup()
/// - slot1/slot2/slot3 buttons -> assign in inspector
/// </summary>
public class MetaNewGameSlotPickerUI : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button closeButton;

    [Header("After selection")]
    [SerializeField] private string sceneToLoadAfterSelection = "Overworld_Scene";

    [Header("Slot Buttons")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;

    [Tooltip("Optional: text objects next to each slot button.")]
    [SerializeField] private TextMeshProUGUI slot1Text;
    [SerializeField] private TextMeshProUGUI slot2Text;
    [SerializeField] private TextMeshProUGUI slot3Text;

    private void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (slot1Button != null) slot1Button.onClick.AddListener(() => OnSlotClicked(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => OnSlotClicked(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => OnSlotClicked(3));

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);

        RefreshSlotLabels();
    }

    public void OpenPopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);
        RefreshSlotLabels();
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    private void RefreshSlotLabels()
    {
        SetupSlotButtonLabel(slot1Text, 1);
        SetupSlotButtonLabel(slot2Text, 2);
        SetupSlotButtonLabel(slot3Text, 3);
    }

    private void SetupSlotButtonLabel(TextMeshProUGUI txt, int slotIndex)
    {
        if (txt == null) return;
        bool hasSave = SaveSystem.HasSave(slotIndex);
        txt.text = hasSave ? $"Slot {slotIndex} (Load)" : $"Slot {slotIndex} (New)";
    }

    private void OnSlotClicked(int slotIndex)
    {
        SaveLoadManager.StartNewGameInSlot(slotIndex);
        ClosePopup();

        if (!string.IsNullOrEmpty(sceneToLoadAfterSelection))
            SceneManager.LoadScene(sceneToLoadAfterSelection);
    }
}

