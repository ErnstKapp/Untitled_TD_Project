using UnityEngine;

/// <summary>
/// Optional standalone buy buttons. Assign <see cref="tower"/> so purchases apply to that tower type's meta id.
/// Prefer using <see cref="MetaUpgradeShopPanelUI"/> for the full tabbed shop.
/// </summary>
public class MetaUpgradePurchaseUI : MonoBehaviour
{
    [Tooltip("Tower type these purchases upgrade (uses TowerData.MetaId).")]
    [SerializeField] private TowerData tower;

    private string Id => tower != null ? tower.MetaId : string.Empty;

    public void BuyTowerDamageMeta()
    {
        if (string.IsNullOrEmpty(Id)) { Debug.LogWarning("[MetaUpgradePurchaseUI] Assign Tower Data."); return; }
        MetaUpgradeState.TryPurchaseDamage(Id);
    }

    public void BuyTowerRangeMeta()
    {
        if (string.IsNullOrEmpty(Id)) { Debug.LogWarning("[MetaUpgradePurchaseUI] Assign Tower Data."); return; }
        MetaUpgradeState.TryPurchaseRange(Id);
    }

    public void BuyTowerFireRateMeta()
    {
        if (string.IsNullOrEmpty(Id)) { Debug.LogWarning("[MetaUpgradePurchaseUI] Assign Tower Data."); return; }
        MetaUpgradeState.TryPurchaseFireRate(Id);
    }
}
