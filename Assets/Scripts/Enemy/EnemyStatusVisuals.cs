using UnityEngine;

/// <summary>
/// Agnostic status VFX for any enemy: tints the main sprite for slow / DoT and optionally toggles
/// shared overlay objects (icons, particles) under this enemy. Add to the same GameObject as Enemy.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyStatusVisuals : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Sprite to tint; defaults to SpriteRenderer on this object.")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Tint (multiply base color)")]
    [SerializeField] private bool enableTints = true;
    [SerializeField] private Color slowTint = new Color(0.75f, 0.9f, 1.15f, 1f);
    [SerializeField] private Color dotTint = new Color(1.15f, 0.65f, 0.65f, 1f);

    [Header("Optional overlays (same for all enemies)")]
    [Tooltip("Child object with a small slow icon / ring; toggled on while slowed.")]
    [SerializeField] private GameObject slowOverlay;
    [Tooltip("Child object with a DoT / poison icon; toggled on while DoT is active.")]
    [SerializeField] private GameObject dotOverlay;

    private Enemy _enemy;
    private Color _baseColor = Color.white;
    private bool _hasBaseColor;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>Call after the enemy sprite/color is set (Enemy.Initialize does this automatically).</summary>
    public void CaptureBaseFromRenderer()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
        if (targetRenderer != null)
        {
            _baseColor = targetRenderer.color;
            _hasBaseColor = true;
        }
    }

    private void LateUpdate()
    {
        if (_enemy == null || !enableTints || targetRenderer == null)
        {
            UpdateOverlaysOnly();
            return;
        }

        if (!_hasBaseColor && _enemy.IsInitialized)
            CaptureBaseFromRenderer();

        Color c = _baseColor;
        if (_enemy.IsSlowActive)
        {
            c = new Color(
                Mathf.Clamp01(c.r * slowTint.r),
                Mathf.Clamp01(c.g * slowTint.g),
                Mathf.Clamp01(c.b * slowTint.b),
                c.a * slowTint.a);
        }
        if (_enemy.HasActiveDoT)
        {
            c = new Color(
                Mathf.Clamp01(c.r * dotTint.r),
                Mathf.Clamp01(c.g * dotTint.g),
                Mathf.Clamp01(c.b * dotTint.b),
                c.a * dotTint.a);
        }
        targetRenderer.color = c;

        UpdateOverlaysOnly();
    }

    private void UpdateOverlaysOnly()
    {
        if (_enemy == null) return;
        if (slowOverlay != null)
            slowOverlay.SetActive(_enemy.IsSlowActive);
        if (dotOverlay != null)
            dotOverlay.SetActive(_enemy.HasActiveDoT);
    }

    private void OnDisable()
    {
        if (targetRenderer != null && _hasBaseColor)
            targetRenderer.color = _baseColor;
        if (slowOverlay != null) slowOverlay.SetActive(false);
        if (dotOverlay != null) dotOverlay.SetActive(false);
    }
}
