using UnityEngine;

/// <summary>Which in-range enemy the tower shoots first.</summary>
public enum TowerTargetPriority
{
    Closest,
    /// <summary>Farthest along the path (nearest the goal) — classic “first” targeting.</summary>
    FirstOnPath,
    MostHealth,
    LeastHealth
}

/// <summary>Optional: prefer shooting enemies that do not have a given status. If every enemy has it, falls back to all in range.</summary>
public enum TowerStatusAvoidMode
{
    None,
    PreferWithoutSlow,
    PreferWithoutDoT,
    PreferWithoutSlowOrDoT
}

[CreateAssetMenu(fileName = "New Tower Data", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Tower Info")]
    public string towerName = "Basic Tower";
    public string description = "A basic tower";
    public Sprite icon;

    [Header("Tower Prefab")]
    public GameObject towerPrefab;

    [Header("Stats")]
    public int cost = 50;
    public float range = 3f;
    [Tooltip("Continuous mode: shots per second. Burst mode: still shown in UI; burst timing uses fields below.")]
    public float fireRate = 1f; // Shots per second (continuous only)
    public int damage = 10;
    public float projectileSpeed = 5f;
    
    [Header("Special Effects")]
    public float dotDamage = 0f; // Damage over time per second
    public float dotDuration = 0f; // How long DoT lasts (in seconds)
    public float slowPercentage = 0f; // Slow effect (0 = no slow, 1 = complete stop)
    public float slowDuration = 0f; // How long slow lasts (in seconds)

    [Header("Firing mode")]
    [Tooltip("If enabled, tower fires burstShotsPerBurst projectiles close together, then waits burstCooldown. If disabled, uses Fire Rate as continuous shots per second.")]
    public bool burstFire = false;
    [Tooltip("Projectiles each burst (minimum 1). First shot is immediate when a burst starts.")]
    [Min(1)]
    public int burstShotsPerBurst = 3;
    [Tooltip("Seconds between shots inside the same burst.")]
    public float burstShotInterval = 0.12f;
    [Tooltip("Seconds after the last shot of a burst before a new burst can begin.")]
    public float burstCooldown = 1.25f;

    [Header("Targeting")]
    [Tooltip("How this tower picks its target among enemies in range.")]
    public TowerTargetPriority targetPriority = TowerTargetPriority.FirstOnPath;
    [Tooltip("If not None, prefer enemies without this status when possible; if everyone has it, use Target Priority on all in range.")]
    public TowerStatusAvoidMode avoidTargetingIfStatus = TowerStatusAvoidMode.None;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Animation (optional)")]
    [Tooltip("Base Animator Controller for this tower (Idle/Fire). Used when no upgrade provides an override.")]
    public RuntimeAnimatorController baseAnimatorController;

    [Header("Upgrades (optional)")]
    [Tooltip("First upgrade node for this tower. Leave empty for no upgrades.")]
    public TowerUpgradeNode rootUpgrade;

    [Header("Genre (for weakness system)")]
    [Tooltip("This tower's music genre. Enemies weak to this genre take extra damage.")]
    public MusicGenre genre = MusicGenre.None;
}

/// <summary>Music genre for tower/enemy matchup. Enemies with weaknessGenre matching a tower's genre take bonus damage.</summary>
public enum MusicGenre
{
    None,
    Blues,
    Rock,
    Jazz,
    Classical,
    Opera,
    Electronic
}
