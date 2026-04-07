using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pop-up meta shop: pick a tower (tabs), then upgrade damage / range / fire rate for that tower type only.
/// Assign one entry per tab in <see cref="towerTabs"/> (tower + optional tab button + highlight Graphic).
/// </summary>
public class MetaUpgradeShopPanelUI : MonoBehaviour
{
    [Serializable]
    public class TowerTab
    {
        public TowerData tower;
        [Tooltip("Optional: button that selects this tower tab.")]
        public Button tabButton;
        [Tooltip("Optional: shown when this tab is selected (e.g. Image color or separate highlight object).")]
        public Graphic tabHighlight;
        [Tooltip("Optional: label on this tab — tower name is filled automatically when the shop opens.")]
        public TextMeshProUGUI tabLabelText;
    }

    [Serializable]
    public class UpgradeRow
    {
        [Tooltip("Which stat this row controls.")]
        public UpgradeRowKind kind = UpgradeRowKind.Damage;

        [Tooltip("Optional label in the UI. If empty, a default name is used.")]
        public string titleOverride;

        public TextMeshProUGUI titleText;

        [Tooltip("Rank and current bonus for the selected tower.")]
        public TextMeshProUGUI infoText;

        [Tooltip("Next purchase cost or MAX.")]
        public TextMeshProUGUI costText;

        public Button buyButton;
    }

    public enum UpgradeRowKind
    {
        Damage,
        Range,
        FireRate
    }

    [Header("Panel")]
    [Tooltip("The shop panel root (this object or a child). Disabled when shop is closed.")]
    [SerializeField] private GameObject panelRoot;

    [Header("Tower tabs")]
    [Tooltip("One entry per tower line in the shop. Order = tab order left-to-right.")]
    [SerializeField] private TowerTab[] towerTabs = Array.Empty<TowerTab>();

    [Header("Selected tower (main menu readout)")]
    [Tooltip("Large title, e.g. \"Upgrading: Opera\"")]
    [SerializeField] private TextMeshProUGUI selectedTowerTitleText;
    [Tooltip("Optional: tower icon from Tower Data.")]
    [SerializeField] private Image selectedTowerIcon;
    [Tooltip("Optional: Tower Data description for the selected type.")]
    [SerializeField] private TextMeshProUGUI selectedTowerDescriptionText;
    [Tooltip("Optional: shows internal upgrade id (MetaId) so the exact tower type is unambiguous.")]
    [SerializeField] private TextMeshProUGUI selectedTowerMetaIdText;
    [Tooltip("Format for MetaId line; {0} = MetaId. Example: \"Type ID: {0}\"")]
    [SerializeField] private string selectedTowerMetaIdFormat = "Type ID: {0}";

    [Header("Rows (assign one per upgrade line)")]
    [SerializeField] private UpgradeRow[] rows = Array.Empty<UpgradeRow>();

    [Header("Meta currency")]
    [SerializeField] private TextMeshProUGUI metaCurrencyText;
    [SerializeField] private string metaCurrencyFormat = "Meta: {0}";

    [Header("Optional")]
    [SerializeField] private Button closeButton;

    private bool _listenersWired;
    private int _selectedTabIndex;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        WireIfNeeded();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void WireIfNeeded()
    {
        if (_listenersWired) return;
        _listenersWired = true;

        for (int i = 0; i < towerTabs.Length; i++)
        {
            int idx = i;
            TowerTab tab = towerTabs[i];
            if (tab?.tabButton != null)
                tab.tabButton.onClick.AddListener(() => SelectTab(idx));
        }

        foreach (UpgradeRow row in rows)
        {
            if (row?.buyButton == null) continue;
            UpgradeRowKind captured = row.kind;
            row.buyButton.onClick.AddListener(() => OnBuyClicked(captured));
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        if (MetaCurrencyManager.Instance != null)
            MetaCurrencyManager.Instance.OnMetaCurrencyChanged += OnMetaCurrencyChanged;

        if (panelRoot != null && panelRoot.activeInHierarchy)
        {
            RefreshTabLabels();
            RefreshAllRows();
        }
    }

    private void OnDisable()
    {
        if (MetaCurrencyManager.Instance != null)
            MetaCurrencyManager.Instance.OnMetaCurrencyChanged -= OnMetaCurrencyChanged;
    }

    private void OnMetaCurrencyChanged(int _)
    {
        if (panelRoot != null && panelRoot.activeInHierarchy)
            RefreshAllRows();
    }

    public void TogglePanel()
    {
        if (panelRoot == null) return;
        WireIfNeeded();
        bool show = !panelRoot.activeSelf;
        panelRoot.SetActive(show);
        if (show)
        {
            EnsureValidTabSelection();
            RefreshTabLabels();
            RefreshAllRows();
        }
    }

    public void OpenPanel()
    {
        if (panelRoot == null) return;
        WireIfNeeded();
        panelRoot.SetActive(true);
        EnsureValidTabSelection();
        RefreshTabLabels();
        RefreshAllRows();
    }

    public void ClosePanel()
    {
        if (panelRoot == null) return;
        panelRoot.SetActive(false);
    }

    /// <summary>Select tower tab by index (0-based). Can wire UI buttons here if you prefer dynamic lists.</summary>
    public void SelectTab(int index)
    {
        if (towerTabs == null || towerTabs.Length == 0) return;
        _selectedTabIndex = Mathf.Clamp(index, 0, towerTabs.Length - 1);
        UpdateTabHighlights();
        RefreshAllRows();
    }

    private void EnsureValidTabSelection()
    {
        if (towerTabs == null || towerTabs.Length == 0)
        {
            _selectedTabIndex = 0;
            return;
        }
        if (_selectedTabIndex < 0 || _selectedTabIndex >= towerTabs.Length)
            _selectedTabIndex = 0;
        UpdateTabHighlights();
        RefreshTabLabels();
    }

    private void UpdateTabHighlights()
    {
        if (towerTabs == null) return;
        for (int i = 0; i < towerTabs.Length; i++)
        {
            TowerTab tab = towerTabs[i];
            bool selected = i == _selectedTabIndex;
            if (tab?.tabHighlight != null)
                tab.tabHighlight.gameObject.SetActive(selected);
            if (tab?.tabLabelText != null)
                tab.tabLabelText.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    /// <summary>Fills tab labels from <see cref="TowerData.towerName"/> (call when panel opens or towers change).</summary>
    private void RefreshTabLabels()
    {
        if (towerTabs == null) return;
        foreach (TowerTab tab in towerTabs)
        {
            if (tab?.tabLabelText == null || tab.tower == null) continue;
            tab.tabLabelText.text = tab.tower.towerName;
        }
    }

    private string GetSelectedTowerId()
    {
        if (towerTabs == null || towerTabs.Length == 0) return string.Empty;
        TowerTab tab = towerTabs[_selectedTabIndex];
        if (tab?.tower == null) return string.Empty;
        return tab.tower.MetaId;
    }

    private void OnBuyClicked(UpgradeRowKind kind)
    {
        string id = GetSelectedTowerId();
        if (string.IsNullOrEmpty(id)) return;

        bool ok = kind switch
        {
            UpgradeRowKind.Damage => MetaUpgradeState.TryPurchaseDamage(id),
            UpgradeRowKind.Range => MetaUpgradeState.TryPurchaseRange(id),
            UpgradeRowKind.FireRate => MetaUpgradeState.TryPurchaseFireRate(id),
            _ => false
        };
        if (ok)
            RefreshAllRows();
    }

    public void RefreshAllRows()
    {
        RefreshMetaCurrencyText();
        RefreshSelectedTowerDetails();

        string towerId = GetSelectedTowerId();

        if (rows == null) return;
        foreach (UpgradeRow row in rows)
        {
            if (row == null) continue;
            RefreshRow(row, towerId);
        }
    }

    private void RefreshSelectedTowerDetails()
    {
        if (towerTabs == null || towerTabs.Length == 0 || towerTabs[_selectedTabIndex]?.tower == null)
        {
            if (selectedTowerTitleText != null)
                selectedTowerTitleText.text = "Select a tower";
            if (selectedTowerIcon != null)
            {
                selectedTowerIcon.sprite = null;
                selectedTowerIcon.enabled = false;
            }
            if (selectedTowerDescriptionText != null)
                selectedTowerDescriptionText.text = string.Empty;
            if (selectedTowerMetaIdText != null)
                selectedTowerMetaIdText.text = string.Empty;
            return;
        }

        TowerData t = towerTabs[_selectedTabIndex].tower;

        if (selectedTowerTitleText != null)
            selectedTowerTitleText.text = $"Upgrading: {t.towerName}";

        if (selectedTowerIcon != null)
        {
            selectedTowerIcon.sprite = t.icon;
            selectedTowerIcon.enabled = t.icon != null;
        }

        if (selectedTowerDescriptionText != null)
            selectedTowerDescriptionText.text = t.description ?? string.Empty;

        if (selectedTowerMetaIdText != null)
            selectedTowerMetaIdText.text = string.Format(selectedTowerMetaIdFormat, t.MetaId);
    }

    private void RefreshMetaCurrencyText()
    {
        if (metaCurrencyText == null) return;
        int amount = MetaCurrencyManager.Instance != null ? MetaCurrencyManager.Instance.MetaCurrency : 0;
        metaCurrencyText.text = string.Format(metaCurrencyFormat, amount);
    }

    private static void RefreshRow(UpgradeRow row, string towerId)
    {
        int rank;
        float mult;
        int nextCost;
        string defaultTitle;

        switch (row.kind)
        {
            case UpgradeRowKind.Damage:
                rank = string.IsNullOrEmpty(towerId) ? 0 : MetaUpgradeState.GetDamageRank(towerId);
                mult = string.IsNullOrEmpty(towerId) ? 1f : MetaUpgradeState.GetDamageMultiplier(towerId);
                nextCost = string.IsNullOrEmpty(towerId) ? 0 : MetaUpgradeState.GetCostForNextRank(rank);
                defaultTitle = "Damage";
                break;
            case UpgradeRowKind.Range:
                rank = string.IsNullOrEmpty(towerId) ? 0 : MetaUpgradeState.GetRangeRank(towerId);
                mult = string.IsNullOrEmpty(towerId) ? 1f : MetaUpgradeState.GetRangeMultiplier(towerId);
                nextCost = string.IsNullOrEmpty(towerId) ? 0 : MetaUpgradeState.GetCostForNextRank(rank);
                defaultTitle = "Range";
                break;
            case UpgradeRowKind.FireRate:
                rank = string.IsNullOrEmpty(towerId) ? 0 : MetaUpgradeState.GetFireRateRank(towerId);
                mult = string.IsNullOrEmpty(towerId) ? 1f : MetaUpgradeState.GetFireRateMultiplier(towerId);
                nextCost = string.IsNullOrEmpty(towerId) ? 0 : MetaUpgradeState.GetCostForNextRank(rank);
                defaultTitle = "Fire rate";
                break;
            default:
                return;
        }

        int max = MetaUpgradeState.MaxRank;
        bool maxed = rank >= max;
        int bonusPct = Mathf.RoundToInt((mult - 1f) * 100f);

        if (row.titleText != null)
            row.titleText.text = string.IsNullOrEmpty(row.titleOverride) ? defaultTitle : row.titleOverride;

        if (row.infoText != null)
        {
            if (string.IsNullOrEmpty(towerId))
                row.infoText.text = "—";
            else
                row.infoText.text = $"Rank {rank} / {max}\n+{bonusPct}%";
        }

        if (row.costText != null)
            row.costText.text = string.IsNullOrEmpty(towerId) ? "—" : (maxed ? "MAX" : $"Next: {nextCost} meta");

        if (row.buyButton != null)
        {
            bool canAfford = !string.IsNullOrEmpty(towerId) && !maxed && MetaCurrencyManager.Instance != null &&
                             MetaCurrencyManager.Instance.CanAfford(nextCost);
            row.buyButton.interactable = canAfford;
        }
    }
}
