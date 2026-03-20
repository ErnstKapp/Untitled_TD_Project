using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a UI Button in the Menu.
/// Loads meta progression from the chosen slot, then goes to the overworld.
/// </summary>
public class MetaLoadSlotButton : MonoBehaviour
{
    [SerializeField] private int slotIndex = 1;
    [SerializeField] private string sceneToLoadAfterLoad = "Overworld_Scene";

    public void OnClick()
    {
        bool ok = SaveLoadManager.LoadSlot(slotIndex);
        if (!ok)
        {
            Debug.LogWarning($"[MetaLoadSlotButton] No save in slot {slotIndex}.");
            return;
        }

        if (!string.IsNullOrEmpty(sceneToLoadAfterLoad))
            SceneManager.LoadScene(sceneToLoadAfterLoad);
    }
}

