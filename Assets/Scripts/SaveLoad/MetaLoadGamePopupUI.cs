using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup UI for selecting a save slot.
/// Works in two modes:
/// - LoadOnly: empty slots are disabled; clicking loads an existing save.
/// - NewOnly: empty or full slots both start a fresh new game (overwriting meta progression for that slot).
/// Attach this to a GameObject in your Menu scene.
/// Wire a menu button to call <see cref="OpenPopup"/>.
/// </summary>
public class MetaLoadGamePopupUI : MonoBehaviour
{
    public enum PopupMode
    {
        LoadOnly,
        NewOnly
    }

    [Header("Popup")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button closeButton;

    [Header("Behavior")]
    [SerializeField] private PopupMode popupMode = PopupMode.LoadOnly;

    [Header("After Selection")]
    [SerializeField] private string sceneToLoadAfterLoad = "Overworld_Scene";

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

        RefreshSlotAvailability();
    }

    public void OpenPopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        RefreshSlotAvailability();
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void RefreshSlotAvailability()
    {
        SetupSlotButton(slot1Button, slot1Text, 1);
        SetupSlotButton(slot2Button, slot2Text, 2);
        SetupSlotButton(slot3Button, slot3Text, 3);
    }

    private void SetupSlotButton(Button slotButton, TextMeshProUGUI slotText, int slotIndex)
    {
        bool hasSave = SaveSystem.HasSave(slotIndex);
        if (slotButton != null)
            slotButton.interactable = popupMode == PopupMode.NewOnly ? true : hasSave;

        if (slotText != null)
        {
            if (hasSave)
                slotText.text = $"Slot {slotIndex} (Load)";
            else
                slotText.text = $"Slot {slotIndex} (Empty)";

            if (popupMode == PopupMode.NewOnly)
            {
                slotText.text = hasSave ? $"Slot {slotIndex} (Overwrite)" : $"Slot {slotIndex} (New)";
            }
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        bool hasSave = SaveSystem.HasSave(slotIndex);

        if (popupMode == PopupMode.LoadOnly)
        {
            if (!hasSave) return;
            bool ok = SaveLoadManager.LoadSlot(slotIndex);
            if (!ok)
            {
                RefreshSlotAvailability();
                return;
            }
        }
        else // NewOnly
        {
            SaveLoadManager.StartNewGameInSlot(slotIndex);
        }

        ClosePopup();
        if (!string.IsNullOrEmpty(sceneToLoadAfterLoad))
            SceneManager.LoadScene(sceneToLoadAfterLoad);
    }
}

