using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Tower Defense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Info")]
    public string enemyName = "Basic Enemy";
    public Sprite enemySprite;

    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public int currencyReward = 10;
    public int damageToPlayer = 1;

    [Header("Genre weakness")]
    [Tooltip("If a tower's genre matches this, the enemy takes extra damage from that tower.")]
    public MusicGenre weaknessGenre = MusicGenre.None;
    [Tooltip("Damage multiplier when hit by a tower whose genre matches weaknessGenre (e.g. 1.5 = 50% more damage).")]
    public float weaknessDamageMultiplier = 1.5f;
}
