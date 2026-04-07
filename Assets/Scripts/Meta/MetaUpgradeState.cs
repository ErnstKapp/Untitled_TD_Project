using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Per-save-slot meta upgrades, tracked per tower type (<see cref="TowerData.MetaId"/>).
/// Towers without an explicit entry use legacy default ranks (the three ints on SaveSlotData — old saves / shared baseline).
/// </summary>
public static class MetaUpgradeState
{
    public const int MaxRank = 10;
    public const float PercentPerRank = 5f;

    /// <summary>Cost for rank 0→1; then +StepCostAdd per rank.</summary>
    public const int BasePurchaseCost = 50;
    public const int StepCostAdd = 25;

    private static readonly Dictionary<string, Vector3Int> _perTower = new Dictionary<string, Vector3Int>(StringComparer.Ordinal);
    private static int _legacyDamageRank;
    private static int _legacyRangeRank;
    private static int _legacyFireRateRank;

    public static void ImportFromSaveData(SaveSlotData data)
    {
        _perTower.Clear();
        if (data == null)
        {
            _legacyDamageRank = _legacyRangeRank = _legacyFireRateRank = 0;
            return;
        }

        _legacyDamageRank = Mathf.Clamp(data.metaTowerDamageRank, 0, MaxRank);
        _legacyRangeRank = Mathf.Clamp(data.metaTowerRangeRank, 0, MaxRank);
        _legacyFireRateRank = Mathf.Clamp(data.metaTowerFireRateRank, 0, MaxRank);

        if (data.metaPerTowerUpgrades == null) return;

        foreach (MetaPerTowerUpgradeEntry e in data.metaPerTowerUpgrades)
        {
            if (e == null || string.IsNullOrEmpty(e.towerId)) continue;
            _perTower[e.towerId] = new Vector3Int(
                Mathf.Clamp(e.damageRank, 0, MaxRank),
                Mathf.Clamp(e.rangeRank, 0, MaxRank),
                Mathf.Clamp(e.fireRateRank, 0, MaxRank));
        }
    }

    public static void ExportToSaveData(SaveSlotData data)
    {
        if (data == null) return;

        data.metaTowerDamageRank = _legacyDamageRank;
        data.metaTowerRangeRank = _legacyRangeRank;
        data.metaTowerFireRateRank = _legacyFireRateRank;

        data.metaPerTowerUpgrades ??= new List<MetaPerTowerUpgradeEntry>();
        data.metaPerTowerUpgrades.Clear();

        foreach (KeyValuePair<string, Vector3Int> kv in _perTower)
        {
            Vector3Int v = kv.Value;
            if (v.x == 0 && v.y == 0 && v.z == 0) continue;
            data.metaPerTowerUpgrades.Add(new MetaPerTowerUpgradeEntry
            {
                towerId = kv.Key,
                damageRank = v.x,
                rangeRank = v.y,
                fireRateRank = v.z
            });
        }
    }

    public static void ResetAllRanks()
    {
        _perTower.Clear();
        _legacyDamageRank = _legacyRangeRank = _legacyFireRateRank = 0;
    }

    private static Vector3Int GetRanks(string towerId)
    {
        if (!string.IsNullOrEmpty(towerId) && _perTower.TryGetValue(towerId, out Vector3Int o))
            return o;
        return new Vector3Int(_legacyDamageRank, _legacyRangeRank, _legacyFireRateRank);
    }

    private static void SetRanks(string towerId, Vector3Int ranks)
    {
        ranks.x = Mathf.Clamp(ranks.x, 0, MaxRank);
        ranks.y = Mathf.Clamp(ranks.y, 0, MaxRank);
        ranks.z = Mathf.Clamp(ranks.z, 0, MaxRank);

        if (string.IsNullOrEmpty(towerId))
        {
            _legacyDamageRank = ranks.x;
            _legacyRangeRank = ranks.y;
            _legacyFireRateRank = ranks.z;
            return;
        }

        if (ranks.x == _legacyDamageRank && ranks.y == _legacyRangeRank && ranks.z == _legacyFireRateRank)
            _perTower.Remove(towerId);
        else
            _perTower[towerId] = ranks;
    }

    public static int GetDamageRank(string towerId) => GetRanks(towerId).x;
    public static int GetRangeRank(string towerId) => GetRanks(towerId).y;
    public static int GetFireRateRank(string towerId) => GetRanks(towerId).z;

    public static float GetDamageMultiplier(string towerId) => 1f + GetDamageRank(towerId) * (PercentPerRank / 100f);
    public static float GetRangeMultiplier(string towerId) => 1f + GetRangeRank(towerId) * (PercentPerRank / 100f);
    public static float GetFireRateMultiplier(string towerId) => 1f + GetFireRateRank(towerId) * (PercentPerRank / 100f);

    public static int GetCostForNextRank(int currentRank)
    {
        if (currentRank >= MaxRank) return 0;
        return BasePurchaseCost + currentRank * StepCostAdd;
    }

    public static bool TryPurchaseDamage(string towerId)
    {
        if (string.IsNullOrEmpty(towerId)) return false;
        Vector3Int r = GetRanks(towerId);
        if (r.x >= MaxRank) return false;
        int cost = GetCostForNextRank(r.x);
        if (MetaCurrencyManager.Instance == null || !MetaCurrencyManager.Instance.SpendMetaCurrency(cost)) return false;
        r.x++;
        SetRanks(towerId, r);
        return true;
    }

    public static bool TryPurchaseRange(string towerId)
    {
        if (string.IsNullOrEmpty(towerId)) return false;
        Vector3Int r = GetRanks(towerId);
        if (r.y >= MaxRank) return false;
        int cost = GetCostForNextRank(r.y);
        if (MetaCurrencyManager.Instance == null || !MetaCurrencyManager.Instance.SpendMetaCurrency(cost)) return false;
        r.y++;
        SetRanks(towerId, r);
        return true;
    }

    public static bool TryPurchaseFireRate(string towerId)
    {
        if (string.IsNullOrEmpty(towerId)) return false;
        Vector3Int r = GetRanks(towerId);
        if (r.z >= MaxRank) return false;
        int cost = GetCostForNextRank(r.z);
        if (MetaCurrencyManager.Instance == null || !MetaCurrencyManager.Instance.SpendMetaCurrency(cost)) return false;
        r.z++;
        SetRanks(towerId, r);
        return true;
    }
}
