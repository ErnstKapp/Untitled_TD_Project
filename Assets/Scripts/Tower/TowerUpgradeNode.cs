using UnityEngine;

/// <summary>
/// One node in a tower upgrade tree. Stats are applied as flat adds only.
/// Use leftNext/rightNext for branching (pick one path). Leave both null for max tier.
/// </summary>
[CreateAssetMenu(fileName = "New Upgrade Node", menuName = "Tower Defense/Tower Upgrade Node")]
public class TowerUpgradeNode : ScriptableObject
{
    [Header("Display")]
    public string displayName = "Upgrade";
    [TextArea(2, 4)]
    public string description = "";

    [Header("Cost")]
    public int cost = 50;

    [Header("Stat Modifiers (flat add, 0 = no change)")]
    public float damageAdd = 0f;
    public float rangeAdd = 0f;
    public float fireRateAdd = 0f;
    public float projectileSpeedAdd = 0f;
    public float dotDamageAdd = 0f;
    public float dotDurationAdd = 0f;
    public float slowPercentageAdd = 0f;
    public float slowDurationAdd = 0f;

    [Header("Visual (optional)")]
    [Tooltip("If set, tower sprite changes to this after this upgrade (ignored if Animator Controller Override is set).")]
    public Sprite spriteOverride;
    [Tooltip("If set, tower uses this Animator Controller for Idle/Fire animations after this upgrade. Takes precedence over Sprite Override.")]
    public RuntimeAnimatorController animatorControllerOverride;
    [Tooltip("If set, tower uses this projectile prefab instead of base.")]
    public GameObject projectilePrefabOverride;

    [Header("Branching")]
    [Tooltip("Next upgrade option A. Leave both null for max tier.")]
    public TowerUpgradeNode leftNext;
    [Tooltip("Next upgrade option B. Set both for a choice; set only one for linear.")]
    public TowerUpgradeNode rightNext;

    [Header("Special Ability (optional)")]
    [Tooltip("If set to Toreador, this upgrade unlocks the cape ability: enemies in front slow, enemies behind move toward tower then rejoin path.")]
    public SpecialAbilityType specialAbility = SpecialAbilityType.None;
    [Tooltip("Duration in seconds the ability effect lasts (slow + move-toward-tower).")]
    public float abilityEffectDuration = 3f;
    [Tooltip("Slow percentage applied to enemies in front (0–1).")]
    [Range(0f, 1f)]
    public float abilitySlowPercentage = 0.5f;
    [Tooltip("Cooldown in seconds before the ability can be used again.")]
    public float abilityCooldownSeconds = 15f;

    /// <summary>True if this node has at least one next option (not max tier).</summary>
    public bool HasNext => leftNext != null || rightNext != null;

    /// <summary>True if the player must choose between two different upgrades.</summary>
    public bool IsBranch => leftNext != null && rightNext != null;
}

public enum SpecialAbilityType
{
    None,
    Toreador
}
