using UnityEngine;

/// <summary>
/// Attach to a UI Button in the Overworld.
/// Saves meta progression (meta currency + unlocked/completed levels) into the chosen slot.
/// </summary>
public class MetaSaveSlotButton : MonoBehaviour
{
    [SerializeField] private int slotIndex = 1;

    public void OnClick()
    {
        SaveLoadManager.SaveSlot(slotIndex);
    }
}

