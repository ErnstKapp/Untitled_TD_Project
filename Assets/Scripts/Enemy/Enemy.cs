using UnityEngine;
using System;

/// <summary>
/// Payload delivered when a projectile hits. Tower builds this from TowerData; Projectile carries it and passes to Enemy.
/// Keeps Projectile agnostic of DoT/slow - it just delivers the hit.
/// </summary>
public struct HitInfo
{
    public int damage;
    public float dotDamagePerSecond;
    public float dotDuration;
    public float slowPercentage;
    public float slowDuration;
    /// <summary>Tower's genre; if this matches enemy's weaknessGenre, enemy takes bonus damage.</summary>
    public MusicGenre genre;
}

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private float health;

    private EnemyPath path;
    private int currentWaypointIndex = 0;
    private float distanceTraveled = 0f;
    private float segmentProgress = 0f; // Progress along current segment (0 to 1)
    private bool isInitialized = false;
    
    // DoT and Slow effects
    private float dotDamage = 0f; // Current DoT damage per second
    private float dotTimer = 0f; // Time remaining for DoT
    private float dotDamageAccumulator = 0f; // Accumulates fractional damage so small per-frame DoT still applies
    private float slowPercentage = 0f; // Current slow percentage (0-1)
    private float slowTimer = 0f; // Time remaining for slow
    private float baseMoveSpeed; // Original move speed before slow

    // Toreador-style ability: move toward a position and ignore path, then rejoin
    private Vector2? moveTowardTarget;
    private float moveTowardEndTime;

    public event Action<Enemy> OnEnemyDestroyed;
    public event Action<Enemy> OnReachedEnd;

    public float CurrentHealth => health;
    public float MaxHealth => enemyData != null ? enemyData.maxHealth : 100f;
    public float HealthPercentage => MaxHealth > 0 ? health / MaxHealth : 0f;

    /// <summary>For UI/VFX: enemy is currently slowed.</summary>
    public bool IsSlowActive => slowTimer > 0f && slowPercentage > 0f;
    /// <summary>For UI/VFX: DoT is ticking.</summary>
    public bool HasActiveDoT => dotTimer > 0f && dotDamage > 0f;
    public bool IsInitialized => isInitialized;

    /// <summary>0 = path start, 1 = path end. Uses nearest point on path (works when briefly off-path, e.g. Toreador pull).</summary>
    public float PathProgress01 => GetPathProgress01();

    public event Action<float> OnHealthChanged;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void Initialize(EnemyData data)
    {
        Initialize(data, null);
    }
    
    public void Initialize(EnemyData data, EnemyPath assignedPath)
    {
        enemyData = data;
        health = data.maxHealth;
        baseMoveSpeed = data.moveSpeed; // Store original speed
        isInitialized = true;
        if (baseMoveSpeed <= 0f)
            Debug.LogWarning("[Enemy] moveSpeed is 0 or negative in EnemyData '" + (data != null ? data.name : "null") + "' - enemy will not move!");

        // Use assigned path, or find one automatically
        if (assignedPath != null)
        {
            path = assignedPath;
        }
        else
        {
            // Try PathManager first (for multiple paths)
            if (PathManager.Instance != null)
            {
                path = PathManager.Instance.GetPath();
            }
            else
            {
                // Fallback to old system (single path)
                path = FindObjectOfType<EnemyPath>();
            }
        }
        
        if (path == null)
        {
            Debug.LogError("[Enemy] No EnemyPath found! Make sure there's at least one EnemyPath in the scene or a PathManager.");
            return;
        }
        if (path.Waypoints.Count == 0)
        {
            Debug.LogError("[Enemy] Path has 0 waypoints! EnemyPath '" + path.gameObject.name + "' has no waypoints - add waypoints in the Inspector.");
            return;
        }
        if (path.Waypoints.Count < 2)
        {
            Debug.LogWarning("[Enemy] Path has only 1 waypoint - enemy will reach end immediately. Add at least 2 waypoints.");
        }

        // Set sprite if available
        if (spriteRenderer != null && data.enemySprite != null)
        {
            spriteRenderer.sprite = data.enemySprite;
        }

        // Reset position to start of path
        transform.position = path.GetWorldWaypoint(0);
        currentWaypointIndex = 0;
        distanceTraveled = 0f;
        segmentProgress = 0f;

        GetComponent<EnemyStatusVisuals>()?.CaptureBaseFromRenderer();
    }

    private void Update()
    {
        if (!isInitialized) return;
        if (path == null || path.Waypoints.Count == 0) return;

        // Update DoT and slow timers
        UpdateEffects();
        
        MoveAlongPath();
    }
    
    private void UpdateEffects()
    {
        // Apply DoT damage (accumulate fractional damage so small per-second values still apply)
        if (dotTimer > 0f && dotDamage > 0f)
        {
            dotTimer -= Time.deltaTime;
            dotDamageAccumulator += dotDamage * Time.deltaTime;
            int damageToApply = Mathf.FloorToInt(dotDamageAccumulator);
            if (damageToApply > 0)
            {
                dotDamageAccumulator -= damageToApply;
                TakeDamage(damageToApply);
            }
            if (dotTimer <= 0f)
            {
                dotDamage = 0f;
                dotDamageAccumulator = 0f;
            }
        }
        
        // Update slow timer
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowPercentage = 0f; // Slow expired
            }
        }
    }

    private void MoveAlongPath()
    {
        // Toreador-style override: move toward target until end time, then rejoin path
        if (moveTowardTarget.HasValue)
        {
            if (Time.time >= moveTowardEndTime)
            {
                RejoinPath();
                moveTowardTarget = null;
            }
            else
            {
                float moveSpeed = baseMoveSpeed * (1f - slowPercentage);
                float step = moveSpeed * Time.deltaTime;
                Vector2 pos = (Vector2)transform.position;
                Vector2 target = moveTowardTarget.Value;
                Vector2 newPos = Vector2.MoveTowards(pos, target, step);
                transform.position = newPos;
                Vector2 dir = (target - pos).normalized;
                if (spriteRenderer != null && dir.sqrMagnitude > 0.0001f)
                {
                    spriteRenderer.flipX = dir.x < 0;
                }
            }
            return;
        }

        if (currentWaypointIndex >= path.Waypoints.Count - 1)
        {
            ReachedEnd();
            return;
        }

        // Calculate distance to move this frame (apply slow effect)
        float currentMoveSpeed = baseMoveSpeed * (1f - slowPercentage); // slowPercentage 0 = no slow, 1 = complete stop
        float moveDistance = currentMoveSpeed * Time.deltaTime;
        
        // Get segment start and end positions
        Vector2 segmentStart = path.GetWorldWaypoint(currentWaypointIndex);
        Vector2 segmentEnd = path.GetWorldWaypoint(currentWaypointIndex + 1);
        Vector2 currentPos = transform.position;
        
        // Check if we've reached the current target waypoint
        float distanceToWaypoint = Vector2.Distance(currentPos, segmentEnd);
        
        if (distanceToWaypoint < 0.1f)
        {
            // Reached the waypoint, move to next
            currentWaypointIndex++;
            segmentProgress = 0f;
            
            // Check if we've reached the end
            if (currentWaypointIndex >= path.Waypoints.Count - 1)
            {
                ReachedEnd();
                return;
            }
            
            // Update for next segment
            segmentStart = path.GetWorldWaypoint(currentWaypointIndex);
            segmentEnd = path.GetWorldWaypoint(currentWaypointIndex + 1);
            currentPos = transform.position;
        }
        
        // Calculate progress along current segment
        // For curved paths, we increment progress based on movement
        float totalSegmentDistance = Vector2.Distance(segmentStart, segmentEnd);
        
        if (totalSegmentDistance > 0.01f)
        {
            // Increment progress based on how far we're moving
            float progressIncrement = moveDistance / totalSegmentDistance;
            segmentProgress += progressIncrement;
            segmentProgress = Mathf.Clamp01(segmentProgress);
        }
        else
        {
            // Segment is too short, just move directly to end
            segmentProgress = 1f;
        }
        
        // Get position along path (handles both straight and curved)
        Vector2 targetPosition = path.GetPositionAlongPath(currentWaypointIndex, segmentProgress);
        Vector2 direction = (targetPosition - currentPos).normalized;
        
        // Move towards the calculated position
        transform.position = Vector2.MoveTowards(currentPos, targetPosition, moveDistance);
        
        // Rotate sprite to face movement direction
        if (spriteRenderer != null && direction.magnitude > 0.01f)
        {
            if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (direction.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    /// <summary>
    /// Force this enemy to move toward a world position and ignore the path for a duration (e.g. Toreador ability).
    /// When duration ends, the enemy rejoins the path at the nearest point.
    /// </summary>
    public void StartMoveTowardPosition(Vector2 worldPosition, float durationSeconds)
    {
        moveTowardTarget = worldPosition;
        moveTowardEndTime = Time.time + durationSeconds;
    }

    /// <summary>Normalized distance along the path (0–1) for targeting “first” enemies.</summary>
    public float GetPathProgress01()
    {
        if (path == null || path.Waypoints.Count < 2 || !isInitialized)
            return 0f;

        path.GetNearestPointOnPath(transform.position, out int segmentIndex, out float segT);
        return ComputeNormalizedDistanceAlongPath(segmentIndex, segT);
    }

    private float ComputeNormalizedDistanceAlongPath(int segmentIndex, float t)
    {
        float total = GetTotalPathWorldLength();
        if (total < 0.0001f)
            return 0f;

        segmentIndex = Mathf.Clamp(segmentIndex, 0, Mathf.Max(0, path.Waypoints.Count - 2));
        t = Mathf.Clamp01(t);

        float dist = 0f;
        for (int i = 0; i < segmentIndex; i++)
            dist += Vector2.Distance(path.GetWorldWaypoint(i), path.GetWorldWaypoint(i + 1));

        if (segmentIndex < path.Waypoints.Count - 1)
        {
            float segLen = Vector2.Distance(path.GetWorldWaypoint(segmentIndex), path.GetWorldWaypoint(segmentIndex + 1));
            dist += segLen * t;
        }

        return Mathf.Clamp01(dist / total);
    }

    private float GetTotalPathWorldLength()
    {
        if (path == null || path.Waypoints.Count < 2)
            return 0f;
        float len = 0f;
        for (int i = 0; i < path.Waypoints.Count - 1; i++)
            len += Vector2.Distance(path.GetWorldWaypoint(i), path.GetWorldWaypoint(i + 1));
        return len;
    }

    private void RejoinPath()
    {
        if (path == null || path.Waypoints.Count < 2) return;
        Vector2 pos = transform.position;
        path.GetNearestPointOnPath(pos, out int seg, out float t);
        currentWaypointIndex = seg;
        segmentProgress = t;
        // If we're past the last waypoint, clamp to end
        if (currentWaypointIndex >= path.Waypoints.Count - 1)
        {
            currentWaypointIndex = path.Waypoints.Count - 2;
            segmentProgress = 1f;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Max(0, health);

        OnHealthChanged?.Invoke(health);

        if (health <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Applies damage over time effect to the enemy.
    /// </summary>
    /// <param name="damagePerSecond">Damage dealt per second</param>
    /// <param name="duration">How long the DoT lasts in seconds</param>
    public void ApplyDoT(float damagePerSecond, float duration)
    {
        dotDamage = damagePerSecond;
        dotTimer = duration;
    }
    
    /// <summary>
    /// Applies slow effect to the enemy.
    /// </summary>
    /// <param name="slowPct">Slow percentage (0 = no slow, 1 = complete stop)</param>
    /// <param name="duration">How long the slow lasts in seconds</param>
    public void ApplySlow(float slowPct, float duration)
    {
        // Use the stronger slow if already slowed
        if (slowPct > slowPercentage)
        {
            slowPercentage = Mathf.Clamp01(slowPct);
            slowTimer = duration;
        }
        else if (slowPct == slowPercentage)
        {
            // Refresh duration if same slow strength
            slowTimer = Mathf.Max(slowTimer, duration);
        }
    }

    /// <summary>
    /// Apply a hit from a projectile. Tower builds HitInfo from TowerData; Projectile just delivers it.
    /// If the hit's genre matches this enemy's weakness, damage and DoT are multiplied.
    /// </summary>
    public void ApplyHit(HitInfo hit)
    {
        float damageMult = GetGenreDamageMultiplier(hit.genre);
        int damage = Mathf.RoundToInt(hit.damage * damageMult);
        float dotDps = hit.dotDamagePerSecond * damageMult;
        TakeDamage(damage);
        if (hit.dotDuration > 0f && dotDps > 0f)
            ApplyDoT(dotDps, hit.dotDuration);
        if (hit.slowDuration > 0f && hit.slowPercentage > 0f)
            ApplySlow(hit.slowPercentage, hit.slowDuration);
    }

    /// <summary>Returns 1f or weaknessDamageMultiplier if hit genre matches this enemy's weakness.</summary>
    private float GetGenreDamageMultiplier(MusicGenre hitGenre)
    {
        if (enemyData == null || hitGenre == MusicGenre.None || enemyData.weaknessGenre == MusicGenre.None)
            return 1f;
        if (hitGenre == enemyData.weaknessGenre)
            return Mathf.Max(1f, enemyData.weaknessDamageMultiplier);
        return 1f;
    }

    private void Die()
    {
        // Award currency
        if (CurrencyManager.Instance != null && enemyData != null)
        {
            CurrencyManager.Instance.AddCurrency(enemyData.currencyReward);
        }

        OnEnemyDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    private void ReachedEnd()
    {
        // Player loses a life
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(enemyData.damageToPlayer);
        }

        OnReachedEnd?.Invoke(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnEnemyDestroyed?.Invoke(this);
    }
}
