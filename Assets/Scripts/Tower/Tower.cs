using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] protected TowerData towerData;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected Transform turretHead;
    [SerializeField] protected LayerMask enemyLayer;

    [Header("Animation (optional)")]
    [Tooltip("If set, Fire trigger is set when firing. Use Idle + Fire states in Animator.")]
    [SerializeField] private Animator towerAnimator;
    [Tooltip("Animator trigger name to play fire animation. Must exist in Animator Controller.")]
    [SerializeField] private string fireTriggerName = "Fire";
    [Tooltip("Animator trigger for Toreador ability (e.g. cape). Set when ability is used. Leave empty to skip.")]
    [SerializeField] private string abilityTriggerName = "Ability";

    protected float fireTimer = 0f;
    protected Enemy currentTarget;
    protected List<Enemy> enemiesInRange = new List<Enemy>();

    /// <summary>Upgrade path chosen so far (order matters for stat stacking).</summary>
    private List<TowerUpgradeNode> appliedUpgrades = new List<TowerUpgradeNode>();
    /// <summary>Cached effective stats after base + all applied upgrades.</summary>
    private float effectiveDamage, effectiveRange, effectiveFireRate, effectiveProjectileSpeed;
    private float effectiveDotDamage, effectiveDotDuration, effectiveSlowPct, effectiveSlowDuration;
    private GameObject effectiveProjectilePrefab;
    private bool effectiveBurstFire;
    private int effectiveBurstShotsPerBurst;
    private float effectiveBurstShotInterval;
    private float effectiveBurstCooldown;

    private float burstReloadRemaining;
    private float burstInnerRemaining;
    private int burstShotsRemainingInVolley;
    private int burstShotsFiredInVolley;

    private TowerTargetPriority effectiveTargetPriority;
    private TowerStatusAvoidMode effectiveAvoidStatus;
    private bool hasTargetPriorityOverride;
    private bool hasAvoidStatusOverride;
    private readonly List<Enemy> targetingScratch = new List<Enemy>();

    public TowerData Data => towerData;
    public bool IsPlaced { get; private set; } = false;
    public IReadOnlyList<TowerUpgradeNode> AppliedUpgrades => appliedUpgrades;

    public float EffectiveDamage => effectiveDamage;
    public float EffectiveRange => effectiveRange;
    public float EffectiveFireRate => effectiveFireRate;
    /// <summary>True when this tower uses burst firing (from Tower Data).</summary>
    public bool UsesBurstFire => effectiveBurstFire;
    public int EffectiveBurstShotsPerBurst => effectiveBurstShotsPerBurst;
    public float EffectiveBurstShotInterval => effectiveBurstShotInterval;
    public float EffectiveBurstCooldown => effectiveBurstCooldown;
    /// <summary>Average shots per second over a full burst cycle (for comparison with continuous towers).</summary>
    public float EffectiveSustainedFireRate
    {
        get
        {
            if (!effectiveBurstFire || effectiveBurstShotsPerBurst <= 0) return effectiveFireRate;
            float cycle = effectiveBurstCooldown + (effectiveBurstShotsPerBurst - 1) * effectiveBurstShotInterval;
            if (cycle <= 0f) return effectiveFireRate;
            return effectiveBurstShotsPerBurst / cycle;
        }
    }
    public float EffectiveProjectileSpeed => effectiveProjectileSpeed;
    public float EffectiveDotDamage => effectiveDotDamage;
    public float EffectiveDotDuration => effectiveDotDuration;
    public float EffectiveSlowPercentage => effectiveSlowPct;
    public float EffectiveSlowDuration => effectiveSlowDuration;
    public TowerTargetPriority EffectiveTargetPriority => effectiveTargetPriority;
    public TowerStatusAvoidMode EffectiveAvoidStatus => effectiveAvoidStatus;

    /// <summary>Override target priority for this specific tower instance (persists across upgrades).</summary>
    public void SetTargetPriority(TowerTargetPriority newPriority, bool persistOverride = true)
    {
        effectiveTargetPriority = newPriority;
        if (persistOverride)
            hasTargetPriorityOverride = true;

        // Force retargeting next frame.
        currentTarget = null;
    }

    /// <summary>Override status avoidance for this specific tower instance (persists across upgrades).</summary>
    public void SetAvoidStatus(TowerStatusAvoidMode newMode, bool persistOverride = true)
    {
        effectiveAvoidStatus = newMode;
        if (persistOverride)
            hasAvoidStatusOverride = true;

        // Force retargeting next frame.
        currentTarget = null;
    }

    private CircleCollider2D rangeCollider;
    private SpriteRenderer rangeIndicator;
    private SpriteRenderer towerSpriteRenderer;
    private RuntimeAnimatorController baseAnimatorController;
    private static TowerInfoUI towerInfoUI;

    // Toreador ability: cooldown and facing for front/behind
    private float nextToreadorAbilityTime;
    private float toreadorAbilityActiveUntil; // while Time.time < this, tower cannot fire (ability in progress)
    private HashSet<Enemy> toreadorMoveTowardEnemies = new HashSet<Enemy>(); // enemies already given move-toward this cast (so late-joiners can be added)
    private Vector2 lastFacing = Vector2.right;

    protected virtual void Awake()
    {
        SetupRangeIndicator();
        
        // Get or add SpriteRenderer for the tower
        towerSpriteRenderer = GetComponent<SpriteRenderer>();
        if (towerSpriteRenderer == null)
        {
            towerSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    protected virtual void Start()
    {
        if (towerData == null)
        {
            Debug.LogError("[Tower] TowerData is not assigned!");
            return;
        }

        SetupRangeCollider();
        if (towerAnimator != null)
            baseAnimatorController = towerData.baseAnimatorController != null ? towerData.baseAnimatorController : towerAnimator.runtimeAnimatorController;
        RefreshEffectiveStats();
    }

    protected virtual void Update()
    {
        if (!IsPlaced)
        {
            return;
        }
        
        if (towerData == null)
        {
            Debug.LogWarning("[Tower] TowerData is null - cannot fire!");
            return;
        }

        // Handle tower selection on click
        HandleTowerClick();

        UpdateTarget();

        // Toreador ability: use automatically when off cooldown and enemies in range
        if (HasToreadorAbility && GetToreadorCooldownRemaining() <= 0f)
        {
            enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
            if (enemiesInRange.Count > 0)
                TryUseToreadorAbility();
        }

        // While ability effect is active, apply slow + move-toward to any new enemies that enter range
        if (HasToreadorAbility && Time.time < toreadorAbilityActiveUntil)
            ApplyToreadorEffectToEnemiesInRange();

        // Don't fire while Toreador ability effect is active (cape out)
        if (currentTarget != null && Time.time >= toreadorAbilityActiveUntil)
        {
            RotateTowardsTarget();

            if (effectiveBurstFire)
                UpdateBurstFiring();
            else
            {
                fireTimer += Time.deltaTime;
                float cooldown = effectiveFireRate > 0f ? 1f / effectiveFireRate : 1f; // effectiveFireRate = shots per second
                if (fireTimer >= cooldown)
                {
                    Fire();
                    fireTimer = 0f;
                }
            }
        }
        else if (currentTarget == null)
        {
            ResetBurstStateOnLostTarget();
        }
    }

    private void ResetBurstStateOnLostTarget()
    {
        burstReloadRemaining = 0f;
        burstInnerRemaining = 0f;
        burstShotsRemainingInVolley = 0;
        burstShotsFiredInVolley = 0;
    }

    private void UpdateBurstFiring()
    {
        if (burstReloadRemaining > 0f)
            burstReloadRemaining -= Time.deltaTime;

        if (burstShotsRemainingInVolley > 0)
        {
            burstInnerRemaining -= Time.deltaTime;
            if (burstInnerRemaining <= 0f)
            {
                // Re-select before every burst shot so targeting can switch mid-burst.
                // When we already fired at the previous target in this volley, predict that the next
                // projectile will apply an avoided status (so we don't keep spamming the same target).
                bool predictCurrentTargetWillBeAvoided = burstShotsFiredInVolley > 0;
                SelectTargetForShot(predictCurrentTargetWillBeAvoided);

                Fire();
                burstShotsFiredInVolley++;
                burstShotsRemainingInVolley--;
                if (burstShotsRemainingInVolley > 0)
                    burstInnerRemaining = effectiveBurstShotInterval;
                else
                    burstReloadRemaining = effectiveBurstCooldown;
            }
            return;
        }

        if (burstReloadRemaining > 0f)
            return;

        // Start a new burst (first shot immediately). No prediction yet: the first projectile
        // hasn't been "queued" multiple times in this volley.
        SelectTargetForShot(false);

        Fire();
        burstShotsFiredInVolley = 1;
        burstShotsRemainingInVolley = effectiveBurstShotsPerBurst - 1;
        if (burstShotsRemainingInVolley > 0)
            burstInnerRemaining = effectiveBurstShotInterval;
        else
            burstReloadRemaining = effectiveBurstCooldown;
    }

    private void SetupRangeIndicator()
    {
        // Create a visual range indicator (optional)
        GameObject rangeObj = new GameObject("RangeIndicator");
        rangeObj.transform.SetParent(transform);
        rangeObj.transform.localPosition = Vector3.zero;
        
        rangeIndicator = rangeObj.AddComponent<SpriteRenderer>();
        rangeIndicator.sprite = CreateRangeSprite();
        rangeIndicator.color = new Color(1f, 1f, 1f, 0.1f);
        rangeIndicator.sortingOrder = -1;
        
        if (towerData != null)
        {
            float parentScale = rangeObj.transform.parent != null ? Mathf.Max(rangeObj.transform.parent.lossyScale.x, 0.001f) : 1f;
            rangeObj.transform.localScale = Vector3.one * (towerData.range * 2f) / parentScale;
        }
        
        rangeObj.SetActive(false); // Hidden by default, show on hover/selection
    }

    private Sprite CreateRangeSprite()
    {
        // Use higher resolution for smoother circle (512x512 instead of 64x64)
        int size = 512;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;
        float lineWidth = 4f; // Slightly thicker line for visibility

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius && distance >= radius - lineWidth)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        // Use pixelsPerUnit to maintain proper scaling
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void SetupRangeCollider()
    {
        // CRITICAL: For OnTriggerEnter2D to work, at least one GameObject needs a Rigidbody2D
        // Add a kinematic Rigidbody2D to the tower if it doesn't have one
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true; // Kinematic so it doesn't move
            rb.gravityScale = 0; // No gravity
        }
        else
        {
            // Make sure it's kinematic
            if (!rb.isKinematic)
            {
                rb.isKinematic = true;
            }
        }
        
        // Check if collider already exists
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider == null)
        {
            rangeCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        rangeCollider.radius = effectiveRange;
        rangeCollider.isTrigger = true;
        rangeCollider.enabled = true;
    }

    protected virtual void UpdateTarget()
    {
        SelectTargetForShot(false);
    }

    /// <summary>
    /// Selects the next target enemy.
    /// If <paramref name="predictCurrentTargetWillBeAvoided"/> is true, the currentTarget is temporarily
    /// treated as if it will gain an avoided status from the next queued shot (useful for burst timing
    /// where projectiles haven't hit yet).
    /// </summary>
    private void SelectTargetForShot(bool predictCurrentTargetWillBeAvoided)
    {
        // Remove null or destroyed enemies
        enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

        if (enemiesInRange.Count == 0)
        {
            currentTarget = null;
            return;
        }

        List<Enemy> pool = enemiesInRange;
        if (effectiveAvoidStatus != TowerStatusAvoidMode.None)
        {
            targetingScratch.Clear();
            for (int i = 0; i < enemiesInRange.Count; i++)
            {
                Enemy e = enemiesInRange[i];
                if (e == null) continue;
                if (!EnemyHasAvoidedStatusForFilter(e, predictCurrentTargetWillBeAvoided))
                    targetingScratch.Add(e);
            }

            if (targetingScratch.Count > 0)
                pool = targetingScratch;
        }

        currentTarget = PickEnemyByPriority(pool);
    }

    private bool WouldNextShotApplyAvoidedStatus()
    {
        bool wouldApplySlow = effectiveSlowDuration > 0f && effectiveSlowPct > 0f;
        bool wouldApplyDoT = effectiveDotDuration > 0f && effectiveDotDamage > 0f;

        switch (effectiveAvoidStatus)
        {
            case TowerStatusAvoidMode.PreferWithoutSlow:
                return wouldApplySlow;
            case TowerStatusAvoidMode.PreferWithoutDoT:
                return wouldApplyDoT;
            case TowerStatusAvoidMode.PreferWithoutSlowOrDoT:
                return wouldApplySlow || wouldApplyDoT;
            default:
                return false;
        }
    }

    /// <summary>True if enemy has a status this tower tries to avoid targeting (when filter is active).</summary>
    private bool EnemyHasAvoidedStatusForFilter(Enemy e, bool predictCurrentTargetWillBeAvoided)
    {
        bool hasStatus = false;
        switch (effectiveAvoidStatus)
        {
            case TowerStatusAvoidMode.PreferWithoutSlow:
                hasStatus = e.IsSlowActive;
                break;
            case TowerStatusAvoidMode.PreferWithoutDoT:
                hasStatus = e.HasActiveDoT;
                break;
            case TowerStatusAvoidMode.PreferWithoutSlowOrDoT:
                hasStatus = e.IsSlowActive || e.HasActiveDoT;
                break;
            default:
                hasStatus = false;
                break;
        }

        if (hasStatus) return true;

        // Prediction: if the queued shot would apply an avoided status to the current target, exclude it now.
        if (predictCurrentTargetWillBeAvoided && e == currentTarget && WouldNextShotApplyAvoidedStatus())
            return true;

        return false;
    }

    private Enemy PickEnemyByPriority(List<Enemy> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        Vector2 towerPos = transform.position;
        switch (effectiveTargetPriority)
        {
            case TowerTargetPriority.FirstOnPath:
                return PickBestByFloat(pool, towerPos, e => e.GetPathProgress01(), true);
            case TowerTargetPriority.MostHealth:
                return PickBestByFloat(pool, towerPos, e => e.CurrentHealth, true);
            case TowerTargetPriority.LeastHealth:
                return PickBestByFloat(pool, towerPos, e => e.CurrentHealth, false);
            case TowerTargetPriority.Closest:
            default:
                return PickClosest(pool, towerPos);
        }
    }

    private static Enemy PickClosest(List<Enemy> pool, Vector2 towerPos)
    {
        Enemy best = null;
        float bestSq = float.MaxValue;
        for (int i = 0; i < pool.Count; i++)
        {
            Enemy e = pool[i];
            if (e == null) continue;
            float sq = ((Vector2)e.transform.position - towerPos).sqrMagnitude;
            if (sq < bestSq)
            {
                bestSq = sq;
                best = e;
            }
        }
        return best;
    }

    private delegate float EnemyScoreFn(Enemy e);

    /// <param name="higherIsBetter">If true, pick max score; else min. Ties broken by closer to tower.</param>
    private static Enemy PickBestByFloat(List<Enemy> pool, Vector2 towerPos, EnemyScoreFn score, bool higherIsBetter)
    {
        Enemy best = null;
        float bestScore = 0f;
        float bestTieSq = float.MaxValue;
        bool have = false;

        for (int i = 0; i < pool.Count; i++)
        {
            Enemy e = pool[i];
            if (e == null) continue;
            float s = score(e);
            float sq = ((Vector2)e.transform.position - towerPos).sqrMagnitude;
            if (!have)
            {
                best = e;
                bestScore = s;
                bestTieSq = sq;
                have = true;
                continue;
            }

            if (higherIsBetter)
            {
                if (s > bestScore)
                {
                    best = e;
                    bestScore = s;
                    bestTieSq = sq;
                }
                else if (Mathf.Approximately(s, bestScore) && sq < bestTieSq)
                {
                    best = e;
                    bestTieSq = sq;
                }
            }
            else
            {
                if (s < bestScore)
                {
                    best = e;
                    bestScore = s;
                    bestTieSq = sq;
                }
                else if (Mathf.Approximately(s, bestScore) && sq < bestTieSq)
                {
                    best = e;
                    bestTieSq = sq;
                }
            }
        }
        return best;
    }

    protected virtual void RotateTowardsTarget()
    {
        if (turretHead != null && currentTarget != null)
        {
            Vector2 direction = (currentTarget.transform.position - turretHead.position).normalized;
            lastFacing = direction;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            turretHead.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    protected virtual void Fire()
    {
        if (currentTarget == null)
        {
            Debug.LogWarning($"[Tower] Cannot fire - no target. Enemies in range: {enemiesInRange.Count}");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("[Tower] Cannot fire - FirePoint is not assigned!");
            return;
        }

        GameObject prefab = effectiveProjectilePrefab != null ? effectiveProjectilePrefab : towerData?.projectilePrefab;
        if (towerData == null || prefab == null)
        {
            Debug.LogError("[Tower] Cannot fire - TowerData or projectilePrefab is null!");
            return;
        }

        GameObject projectileObj = Instantiate(prefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            HitInfo hit = new HitInfo
            {
                damage = Mathf.RoundToInt(effectiveDamage),
                dotDamagePerSecond = effectiveDotDamage,
                dotDuration = effectiveDotDuration,
                slowPercentage = effectiveSlowPct,
                slowDuration = effectiveSlowDuration,
                genre = towerData != null ? towerData.genre : MusicGenre.None
            };
            projectile.Initialize(currentTarget, effectiveProjectileSpeed, hit);
        }
        else
        {
            Debug.LogWarning("[Tower] Projectile GameObject created but Projectile component not found!");
        }

        if (towerAnimator != null && !string.IsNullOrEmpty(fireTriggerName))
            towerAnimator.SetTrigger(fireTriggerName);
    }

    public void PlaceTower()
    {
        IsPlaced = true;
    }

    public void SetTowerData(TowerData data)
    {
        towerData = data;
        appliedUpgrades.Clear();
        RefreshEffectiveStats();
        ApplyRangeIndicatorScale();
        if (rangeCollider != null)
            rangeCollider.radius = effectiveRange;
    }

    private void ApplyRangeIndicatorScale()
    {
        if (rangeIndicator == null) return;
        float parentScale = rangeIndicator.transform.parent != null ? Mathf.Max(rangeIndicator.transform.parent.lossyScale.x, 0.001f) : 1f;
        float localScale = (effectiveRange * 2f) / parentScale;
        rangeIndicator.transform.localScale = Vector3.one * localScale;
        float worldRadius = 0.5f * localScale * parentScale; // sprite radius 0.5 at scale 1
    }

    /// <summary>Returns (left option, right option). One or both can be null. Both null = max tier. When no upgrades applied yet, rootUpgrade is the first option if it has no next (one node = level 1 → 2); else options are root's left/right next (branch).</summary>
    public void GetNextUpgradeOptions(out TowerUpgradeNode left, out TowerUpgradeNode right)
    {
        left = null;
        right = null;
        if (appliedUpgrades.Count == 0 && towerData?.rootUpgrade != null)
        {
            TowerUpgradeNode root = towerData.rootUpgrade;
            if (root.leftNext != null && root.rightNext != null)
            {
                left = root.leftNext;
                right = root.rightNext;
            }
            else
            {
                left = root;
                right = null;
            }
            return;
        }
        if (appliedUpgrades.Count == 0) return;
        TowerUpgradeNode last = appliedUpgrades[appliedUpgrades.Count - 1];
        left = last.leftNext;
        right = last.rightNext;
    }

    private TowerUpgradeNode GetCurrentNodeNext()
    {
        if (towerData?.rootUpgrade == null || appliedUpgrades.Count == 0) return towerData?.rootUpgrade;
        TowerUpgradeNode last = appliedUpgrades[appliedUpgrades.Count - 1];
        return last.leftNext ?? last.rightNext;
    }

    /// <summary>Returns the next single node if path is linear (no branch).</summary>
    public TowerUpgradeNode GetNextUpgradeSingle()
    {
        TowerUpgradeNode current = appliedUpgrades.Count == 0 ? towerData?.rootUpgrade : GetCurrentNodeNext();
        if (current == null) return null;
        if (current.leftNext != null && current.rightNext == null) return current.leftNext;
        if (current.rightNext != null && current.leftNext == null) return current.rightNext;
        return null;
    }

    /// <summary>True if this tower can be upgraded (has options and player can afford one).</summary>
    public bool CanUpgrade(out TowerUpgradeNode left, out TowerUpgradeNode right)
    {
        GetNextUpgradeOptions(out left, out right);
        if (left == null && right == null) return false;
        if (CurrencyManager.Instance == null) return false;
        if (left != null && CurrencyManager.Instance.CanAfford(left.cost)) return true;
        if (right != null && CurrencyManager.Instance.CanAfford(right.cost)) return true;
        return false;
    }

    /// <summary>Apply an upgrade (spend currency, add to path, refresh stats and visual). Returns true if applied.</summary>
    public bool ApplyUpgrade(TowerUpgradeNode node)
    {
        if (node == null) return false;
        int countBefore = appliedUpgrades.Count;
        GetNextUpgradeOptions(out TowerUpgradeNode left, out TowerUpgradeNode right);
        if (node != left && node != right)
        {
            Debug.LogWarning($"[Tower.ApplyUpgrade] Rejected: node='{node?.displayName}' is not a valid next option (left='{left?.displayName}', right='{right?.displayName}')");
            return false;
        }
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.SpendCurrency(node.cost)) return false;
        appliedUpgrades.Add(node);
        RefreshEffectiveStats();
        ApplyRangeIndicatorScale();
        if (rangeCollider != null)
            rangeCollider.radius = effectiveRange;
        return true;
    }

    /// <summary>Base tower cost plus all upgrade costs paid on this instance.</summary>
    public int GetTotalInvestedCurrency()
    {
        if (towerData == null) return 0;
        int total = towerData.cost;
        foreach (TowerUpgradeNode u in appliedUpgrades)
        {
            if (u != null) total += u.cost;
        }
        return total;
    }

    /// <summary>Currency returned when selling this tower (fraction of total invested, floored).</summary>
    public int GetSellRefundAmount(float refundFraction = 0.5f)
    {
        return Mathf.FloorToInt(GetTotalInvestedCurrency() * Mathf.Clamp01(refundFraction));
    }

    private void RefreshEffectiveStats()
    {
        if (towerData == null) return;
        float d = towerData.damage, r = towerData.range, fr = towerData.fireRate, ps = towerData.projectileSpeed;
        float dotD = towerData.dotDamage, dotDur = towerData.dotDuration, slowP = towerData.slowPercentage, slowD = towerData.slowDuration;
        effectiveProjectilePrefab = towerData.projectilePrefab;

        foreach (TowerUpgradeNode u in appliedUpgrades)
        {
            d += u.damageAdd; r += u.rangeAdd; fr += u.fireRateAdd; ps += u.projectileSpeedAdd;
            dotD += u.dotDamageAdd; dotDur += u.dotDurationAdd; slowP += u.slowPercentageAdd; slowD += u.slowDurationAdd;
            if (u.projectilePrefabOverride != null) effectiveProjectilePrefab = u.projectilePrefabOverride;
        }

        // Meta upgrades (per tower type + save slot): percentage multipliers after base + in-level upgrades.
        string metaId = towerData.MetaId;
        float dmgM = MetaUpgradeState.GetDamageMultiplier(metaId);
        float rngM = MetaUpgradeState.GetRangeMultiplier(metaId);
        float frM = MetaUpgradeState.GetFireRateMultiplier(metaId);
        d *= dmgM;
        r *= rngM;
        fr *= frM;

        effectiveDamage = d;
        effectiveRange = Mathf.Max(0.1f, r);
        effectiveFireRate = Mathf.Max(0.01f, fr);
        effectiveProjectileSpeed = Mathf.Max(0.1f, ps);

        effectiveBurstFire = towerData.burstFire;
        effectiveBurstShotsPerBurst = Mathf.Max(1, towerData.burstShotsPerBurst);
        // Faster fire-rate meta shortens burst gaps and reload (continuous fire uses effectiveFireRate above).
        effectiveBurstShotInterval = Mathf.Max(0.02f, towerData.burstShotInterval / frM);
        effectiveBurstCooldown = Mathf.Max(0.05f, towerData.burstCooldown / frM);

        // Respect runtime overrides (e.g. from UI).
        if (!hasTargetPriorityOverride)
            effectiveTargetPriority = towerData.targetPriority;
        if (!hasAvoidStatusOverride)
            effectiveAvoidStatus = towerData.avoidTargetingIfStatus;
        effectiveDotDamage = dotD;
        effectiveDotDuration = dotDur;
        effectiveSlowPct = Mathf.Clamp01(slowP);
        effectiveSlowDuration = slowD;

        // Visual: last upgrade wins – Animator Controller override (new animations) or Sprite override (single sprite)
        RuntimeAnimatorController useController = null;
        Sprite useSprite = null;
        if (towerAnimator != null)
        {
            for (int i = appliedUpgrades.Count - 1; i >= 0; i--)
            {
                if (appliedUpgrades[i].animatorControllerOverride != null) { useController = appliedUpgrades[i].animatorControllerOverride; break; }
            }
        }
        for (int i = appliedUpgrades.Count - 1; i >= 0; i--)
        {
            if (appliedUpgrades[i].spriteOverride != null) { useSprite = appliedUpgrades[i].spriteOverride; break; }
        }
        if (towerAnimator != null && baseAnimatorController != null)
            towerAnimator.runtimeAnimatorController = useController != null ? useController : baseAnimatorController;
        if (towerSpriteRenderer != null && useController == null && useSprite != null)
            towerSpriteRenderer.sprite = useSprite;

        ApplyRangeIndicatorScale();
    }

    public void ShowRange(bool show)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.gameObject.SetActive(show);
        }
    }

    private void HandleTowerClick()
    {
        // Only handle clicks when not placing towers
        if (TowerPlacement.Instance != null && TowerPlacement.Instance.IsPlacing)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Check if clicking on UI
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Check if clicking on this tower
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            
            float distance = Vector2.Distance(transform.position, mouseWorldPos);
            float clickRadius = 0.5f; // Click detection radius
            
            if (distance <= clickRadius)
            {
                if (towerInfoUI == null)
                {
                    towerInfoUI = TowerInfoUI.Instance;
                    if (towerInfoUI == null)
                        towerInfoUI = FindObjectOfType<TowerInfoUI>();
                }
                if (towerInfoUI != null)
                    towerInfoUI.ShowTowerInfo(this);
                else
                {
                    Debug.LogWarning("[Tower] TowerInfoUI is null - cannot show tower info! Make sure TowerInfoUI component exists in the scene.");
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null)
        {
            return;
        }
        
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemiesInRange.Remove(enemy);
            if (currentTarget == enemy)
            {
                currentTarget = null;
            }
        }
    }

    // ----- Toreador ability -----

    /// <summary>True if this tower has the Toreador ability (last applied upgrade has specialAbility == Toreador).</summary>
    public bool HasToreadorAbility
    {
        get
        {
            if (appliedUpgrades.Count == 0) return false;
            TowerUpgradeNode last = appliedUpgrades[appliedUpgrades.Count - 1];
            return last != null && last.specialAbility == SpecialAbilityType.Toreador;
        }
    }

    /// <summary>Seconds until the Toreador ability can be used again (0 if ready).</summary>
    public float GetToreadorCooldownRemaining()
    {
        if (!HasToreadorAbility) return 0f;
        float remaining = nextToreadorAbilityTime - Time.time;
        return remaining > 0f ? remaining : 0f;
    }

    /// <summary>Use the Toreador ability if available and off cooldown. Enemies in front slow; enemies behind move toward tower then rejoin path.</summary>
    public bool TryUseToreadorAbility()
    {
        if (!HasToreadorAbility)
        {
            Debug.Log("[Tower] Toreador ability: not available (tower doesn't have it).");
            return false;
        }
        TowerUpgradeNode last = appliedUpgrades[appliedUpgrades.Count - 1];
        if (Time.time < nextToreadorAbilityTime)
        {
            Debug.Log($"[Tower] Toreador ability: on cooldown ({GetToreadorCooldownRemaining():F1}s left).");
            return false;
        }

        float duration = last.abilityEffectDuration > 0f ? last.abilityEffectDuration : 3f;
        float slowPct = Mathf.Clamp01(last.abilitySlowPercentage);
        float cooldown = last.abilityCooldownSeconds > 0f ? last.abilityCooldownSeconds : 15f;

        Vector2 towerPos = transform.position;
        Vector2 facing = lastFacing.sqrMagnitude > 0.01f ? lastFacing.normalized : Vector2.right;

        toreadorMoveTowardEnemies.Clear(); // new cast, track who we give move-toward to (including late-joiners in ApplyToreadorEffectToEnemiesInRange)
        enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
        int frontCount = 0, behindCount = 0;
        foreach (Enemy enemy in enemiesInRange)
        {
            Vector2 toEnemy = (Vector2)enemy.transform.position - towerPos;
            if (toEnemy.sqrMagnitude < 0.01f) continue;
            toEnemy.Normalize();
            float dot = Vector2.Dot(facing, toEnemy);
            if (dot >= 0f)
            {
                enemy.ApplySlow(slowPct, duration);
                frontCount++;
            }
            else
            {
                enemy.ApplySlow(slowPct, duration);
                enemy.StartMoveTowardPosition(towerPos, duration);
                toreadorMoveTowardEnemies.Add(enemy);
                behindCount++;
            }
        }

        nextToreadorAbilityTime = Time.time + cooldown;
        toreadorAbilityActiveUntil = Time.time + duration; // no firing while ability effect is active
        if (towerAnimator != null && !string.IsNullOrEmpty(abilityTriggerName))
            towerAnimator.SetTrigger(abilityTriggerName);
        Debug.Log($"[Tower] Toreador ability fired. Enemies: {frontCount} in front (slow), {behindCount} behind (slow + move to tower). Cooldown: {cooldown}s.");
        return true;
    }

    /// <summary>Called each frame while ability effect is active. Applies slow + move-toward to all enemies in range (including those that entered mid-ability).</summary>
    private void ApplyToreadorEffectToEnemiesInRange()
    {
        if (appliedUpgrades.Count == 0) return;
        TowerUpgradeNode last = appliedUpgrades[appliedUpgrades.Count - 1];
        float remaining = toreadorAbilityActiveUntil - Time.time;
        if (remaining <= 0f) return;
        float slowPct = Mathf.Clamp01(last.abilitySlowPercentage);
        Vector2 towerPos = transform.position;
        Vector2 facing = lastFacing.sqrMagnitude > 0.01f ? lastFacing.normalized : Vector2.right;

        enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
        toreadorMoveTowardEnemies.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy);
        foreach (Enemy enemy in enemiesInRange)
        {
            enemy.ApplySlow(slowPct, remaining);
            Vector2 toEnemy = (Vector2)enemy.transform.position - towerPos;
            if (toEnemy.sqrMagnitude < 0.01f) continue;
            toEnemy.Normalize();
            if (Vector2.Dot(facing, toEnemy) < 0f && !toreadorMoveTowardEnemies.Contains(enemy))
            {
                enemy.StartMoveTowardPosition(towerPos, remaining);
                toreadorMoveTowardEnemies.Add(enemy);
            }
        }
    }
}
