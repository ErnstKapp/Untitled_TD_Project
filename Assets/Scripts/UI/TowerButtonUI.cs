using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerButtonUI : MonoBehaviour
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

    public void Setup(TowerData data)
    {
        if (data != null)
            towerData = data;
        if (towerData == null) return;

        // Use only an Image that belongs to this button (this transform or a child), never a shared one
        Image imageToUse = null;
        if (iconImage != null && (iconImage.transform == transform || iconImage.transform.IsChildOf(transform)))
            imageToUse = iconImage;
        if (imageToUse == null)
        {
            imageToUse = GetComponentInChildren<Image>(true);
            if (imageToUse == null)
            {
                imageToUse = CreateIconImage();
            }
        }

        // Get sprite: Tower Data icon first, else from tower prefab (SpriteRenderer or Image)
        Sprite sprite = towerData.icon;
        if (sprite == null && towerData.towerPrefab != null)
            sprite = GetSpriteFromPrefab(towerData.towerPrefab);

        if (imageToUse != null)
        {
            if (sprite != null)
            {
                imageToUse.sprite = sprite;
                imageToUse.color = Color.white;
                imageToUse.enabled = true;
            }
            else
            {
                // No sprite: show a visible placeholder so the button isn't blank
                imageToUse.sprite = null;
                imageToUse.color = new Color(0.4f, 0.4f, 0.5f, 1f);
                imageToUse.enabled = true;
            }
        }

        if (nameText != null)
            nameText.text = towerData.towerName;
        if (costText != null)
            costText.text = $"${towerData.cost}";
        // If no name/cost refs assigned, show one fallback label so something is visible
        if (nameText == null && costText == null)
        {
            TextMeshProUGUI fallback = GetOrCreateLabel("FallbackLabel", 11);
            if (fallback != null)
                fallback.text = $"{towerData.towerName}\n${towerData.cost}";
        }

        // Update button interactability based on currency
        if (button != null && CurrencyManager.Instance != null)
        {
            UpdateButtonState(CurrencyManager.Instance.CurrentCurrency);
            CurrencyManager.Instance.OnCurrencyChanged += UpdateButtonState;
        }
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

    /// <summary>
    /// Finds or creates a TextMeshProUGUI child so name/cost can display when refs are not assigned.
    /// </summary>
    private TextMeshProUGUI GetOrCreateLabel(string childName, int fontSize)
    {
        var existing = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in existing)
            if (t.gameObject.name == childName) return t;
        GameObject go = new GameObject(childName);
        go.transform.SetParent(transform, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(4f, 2f);
        rect.offsetMax = new Vector2(-4f, -2f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>
    /// Creates an Image under this button at runtime if none is assigned.
    /// </summary>
    private Image CreateIconImage()
    {
        GameObject iconObj = new GameObject("TowerIcon");
        iconObj.transform.SetParent(transform, false);
        var rect = iconObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(64f, 64f);
        rect.anchoredPosition = Vector2.zero;
        var image = iconObj.AddComponent<Image>();
        image.color = Color.white;
        return image;
    }
}
