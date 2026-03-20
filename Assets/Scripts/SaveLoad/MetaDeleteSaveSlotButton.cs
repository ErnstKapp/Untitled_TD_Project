using UnityEngine;

/// <summary>
/// Optional helper: wire this to a "Delete" button for a slot.
/// After deletion it refreshes the MetaLoadGamePopupUI so the slot label/interactive state updates.
/// </summary>
public class MetaDeleteSaveSlotButton : MonoBehaviour
{
    [SerializeField] private int slotIndex = 1;

    [Tooltip("Optional: if assigned, refreshes this popup after delete.")]
    [SerializeField] private MetaLoadGamePopupUI popupUI;

    public void OnClick()
    {
        SaveLoadManager.DeleteSlot(slotIndex);

        if (popupUI != null)
            popupUI.RefreshSlotAvailability();
        else
            FindObjectOfType<MetaLoadGamePopupUI>(true)?.RefreshSlotAvailability();
    }
}

