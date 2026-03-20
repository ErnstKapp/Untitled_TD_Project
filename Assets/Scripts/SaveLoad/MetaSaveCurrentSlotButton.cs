using UnityEngine;

/// <summary>
/// Attach this to your in-game / overworld "Save" button.
/// Saves meta progression into the currently selected save slot.
/// </summary>
public class MetaSaveCurrentSlotButton : MonoBehaviour
{
    public void OnClick()
    {
        int slot = SaveLoadManager.SelectedSlotIndex;
        SaveLoadManager.SaveSlot(slot);
    }
}

