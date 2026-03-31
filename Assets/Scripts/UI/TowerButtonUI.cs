using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TowerButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tower")]
    [Tooltip("Assign the Tower Data asset this button should place. Create via: Right-click in Project → Create → Tower Defense → Tower Data")]
    [SerializeField] private TowerData towerData;

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button button;

    /// <summary> The tower this button is connected to. Set in Inspector or via Setup. </summary>
    public TowerData TowerData => towerData;

    private Sprite _tooltipTowerSprite;

    public void Setup(TowerData data)
    {
        if (data != null)
            towerData = data;
        if (towerData == null) return;

        // Button visuals are now user-authored (no auto-wiring).
        // If you want an icon on the button, assign iconImage explicitly in the Inspector.
        if (iconImage != null)
        {
            iconImage.sprite = towerData.icon;
            iconImage.enabled = iconImage.sprite != null;
        }

        // Tooltip image: prefer the actual tower sprite from the prefab, fall back to icon.
        _tooltipTowerSprite = null;
        if (towerData.towerPrefab != null)
            _tooltipTowerSprite = GetSpriteFromPrefab(towerData.towerPrefab);
        if (_tooltipTowerSprite == null)
            _tooltipTowerSprite = towerData.icon;

        // Update button interactability based on currency
        if (button == null)
            button = GetComponent<Button>();
        if (button != null && CurrencyManager.Instance != null)
        {
            UpdateButtonState(CurrencyManager.Instance.CurrentCurrency);
            CurrencyManager.Instance.OnCurrencyChanged += UpdateButtonState;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (towerData == null) return;
        if (TowerHoverTooltipUI.Instance != null)
            TowerHoverTooltipUI.Instance.Show(towerData, _tooltipTowerSprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TowerHoverTooltipUI.Instance != null)
            TowerHoverTooltipUI.Instance.Hide();
    }

    private void UpdateButtonState(int newCurrency)
    {
        // Update button state when currency changes
        if (button != null && towerData != null && CurrencyManager.Instance != null)
        {
            button.interactable = CurrencyManager.Instance.CanAfford(towerData.cost);
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateButtonState;
        }
    }

    /// <summary>
    /// Gets a sprite from the tower prefab (SpriteRenderer or UI Image).
    /// Lets you skip assigning icons in Tower Data.
    /// </summary>
    private Sprite GetSpriteFromPrefab(GameObject prefab)
    {
        if (prefab == null) return null;
        var sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null && sr.sprite != null) return sr.sprite;
        var img = prefab.GetComponentInChildren<Image>(true);
        if (img != null && img.sprite != null) return img.sprite;
        return null;
    }

}
