using UnityEngine;

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
    public float fireRate = 1f; // Shots per second
    public int damage = 10;
    public float projectileSpeed = 5f;
    
    [Header("Special Effects")]
    public float dotDamage = 0f; // Damage over time per second
    public float dotDuration = 0f; // How long DoT lasts (in seconds)
    public float slowPercentage = 0f; // Slow effect (0 = no slow, 1 = complete stop)
    public float slowDuration = 0f; // How long slow lasts (in seconds)

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
