using System;
using System.Collections.Generic;

[Serializable]
public class MetaPerTowerUpgradeEntry
{
    public string towerId;
    public int damageRank;
    public int rangeRank;
    public int fireRateRank;
}

[Serializable]
public class SaveSlotData
{
    public int version = 1;
    public int metaCurrency;
    public string lastCompletedLevel;
    public List<string> completedScenes = new List<string>();

    /// <summary>Default meta ranks for any tower type with no entry in <see cref="metaPerTowerUpgrades"/> (also loads old saves).</summary>
    public int metaTowerDamageRank;
    public int metaTowerRangeRank;
    public int metaTowerFireRateRank;

    /// <summary>Per-tower-type overrides; key matches <see cref="TowerData.MetaId"/>.</summary>
    public List<MetaPerTowerUpgradeEntry> metaPerTowerUpgrades = new List<MetaPerTowerUpgradeEntry>();
}

