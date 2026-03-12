using UnityEngine;
using UnityEngine.UI;

public class TowerSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent object that contains your tower buttons. Each child with TowerButtonUI will be wired to its assigned Tower Data.")]
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TowerPlacement towerPlacement;

    public Transform ButtonParent => buttonParent;

    private void Start()
    {
        if (towerPlacement == null)
            towerPlacement = FindObjectOfType<TowerPlacement>();

        WireTowerButtons();
    }

    private void WireTowerButtons()
    {
        if (buttonParent == null)
        {
            Debug.LogWarning("[TowerSelectionUI] Assign Button Parent (the object that contains your tower buttons).");
            return;
        }

        var buttonUIs = buttonParent.GetComponentsInChildren<TowerButtonUI>(true);
        foreach (TowerButtonUI buttonUI in buttonUIs)
        {
            TowerData data = buttonUI.TowerData;
            if (data == null)
            {
                Debug.LogWarning($"[TowerSelectionUI] {buttonUI.gameObject.name} has no Tower Data assigned. Assign it in the Inspector.", buttonUI);
                continue;
            }

            buttonUI.Setup(data);

            Button btn = buttonUI.GetComponent<Button>();
            if (btn == null)
                btn = buttonUI.GetComponentInChildren<Button>();
            if (btn != null)
            {
                // Use the TowerButtonUI on this button (same object or children only – never parent, or we get a shared one)
                Button capturedBtn = btn;
                TowerButtonUI capturedUI = buttonUI;
                btn.onClick.AddListener(() =>
                {
                    TowerButtonUI ui = capturedBtn.GetComponent<TowerButtonUI>();
                    if (ui == null) ui = capturedBtn.GetComponentInChildren<TowerButtonUI>(true);
                    if (ui == null) ui = capturedUI;
                    if (ui != null && ui.TowerData != null)
                        SelectTower(ui.TowerData);
                });
            }
            else
                Debug.LogWarning($"[TowerSelectionUI] No Button component on '{buttonUI.gameObject.name}' - click won't work.", buttonUI);
        }
    }

    private void SelectTower(TowerData towerData)
    {
        if (towerPlacement != null)
            towerPlacement.StartPlacement(towerData);
    }
}
