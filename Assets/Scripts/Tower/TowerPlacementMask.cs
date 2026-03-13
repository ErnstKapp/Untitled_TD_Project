using UnityEngine;

/// <summary>
/// Defines valid tower placement areas using a painted sprite overlay.
/// Paint white (or any opaque color) where towers can be placed; leave transparent or black where they can't.
/// Assign the same sprite to a SpriteRenderer so you can position/scale it over your map.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class TowerPlacementMask : MonoBehaviour
{
    [Header("Mask Sprite")]
    [Tooltip("SpriteRenderer that displays the placement mask. Leave empty to use this GameObject's Sprite Renderer (auto-assigned).")]
    [SerializeField] private SpriteRenderer maskRenderer;

    [Header("Sampling")]
    [Tooltip("Use alpha channel (opaque = valid). If false, uses red channel instead.")]
    [SerializeField] private bool useAlphaChannel = true;

    [Tooltip("Pixel value above this counts as valid (0–1).")]
    [Range(0f, 1f)]
    [SerializeField] private float validThreshold = 0.2f;

    [Tooltip("If green valid areas don't line up with where you painted white: try enabling this first (most image editors use top-left as origin).")]
    [SerializeField] private bool flipY = false;

    [Tooltip("If valid areas are mirrored left-right compared to your painting, enable this.")]
    [SerializeField] private bool flipX = false;

    [Header("Debug")]
    [Tooltip("When selected, draw a grid in the Scene view: green = valid placement, red = invalid.")]
    [SerializeField] private bool drawValidInvalidGrid = true;

    private const float DebugGridStep = 0.25f;

    private Texture2D _cachedTexture;
    private Sprite _cachedSprite;
    private bool _warnedReadable;

    private void Awake()
    {
        EnsureMaskRenderer();
    }

    private void OnValidate()
    {
        EnsureMaskRenderer();
    }

    private void EnsureMaskRenderer()
    {
        if (maskRenderer == null)
            maskRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Returns true if the world position is inside the mask bounds and the sampled pixel is valid.
    /// </summary>
    public bool IsValidPlacementPosition(Vector3 worldPos)
    {
        EnsureMaskRenderer();
        if (maskRenderer == null || maskRenderer.sprite == null)
            return true; // No mask = allow everywhere

        Sprite sprite = maskRenderer.sprite;
        Bounds bounds = maskRenderer.bounds;
        Vector2 w = new Vector2(worldPos.x, worldPos.y);
        Vector2 min = new Vector2(bounds.min.x, bounds.min.y);
        Vector2 size = new Vector2(bounds.size.x, bounds.size.y);
        if (size.x <= 0 || size.y <= 0)
            return false;

        Texture2D tex = GetReadableTexture(sprite);
        if (tex == null)
            return true;

        Rect r = sprite.textureRect;
        Vector2 norm = new Vector2((w.x - min.x) / size.x, (w.y - min.y) / size.y);
        if (norm.x < 0 || norm.x > 1 || norm.y < 0 || norm.y > 1)
            return false;

        // Match SpriteRenderer flip so sampled pixel = what's drawn
        if (maskRenderer.flipX) norm.x = 1f - norm.x;
        if (maskRenderer.flipY) norm.y = 1f - norm.y;
        if (flipX) norm.x = 1f - norm.x;
        if (flipY) norm.y = 1f - norm.y;

        // Map normalized (0,0)=bottom-left to texture UV
        float u = Mathf.Lerp(r.xMin / tex.width, r.xMax / tex.width, norm.x);
        float v = Mathf.Lerp(r.yMin / tex.height, r.yMax / tex.height, norm.y);
        Color pixel = tex.GetPixelBilinear(u, v);
        float value = useAlphaChannel ? pixel.a : pixel.r;
        return value >= validThreshold;
    }

    private Texture2D GetReadableTexture(Sprite sprite)
    {
        if (sprite == null) return null;
        Texture2D tex = sprite.texture;
        if (tex == null) return null;

        if (!tex.isReadable)
        {
            if (!_warnedReadable)
            {
                Debug.LogWarning("[TowerPlacementMask] Mask texture is not Read/Write Enabled. Enable it in the texture Import Settings so placement mask can be sampled.", tex);
                _warnedReadable = true;
            }
            return null;
        }
        return tex;
    }

    private void OnDrawGizmosSelected()
    {
        EnsureMaskRenderer();
        var sr = maskRenderer != null ? maskRenderer : GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Bounds b = sr.bounds;
        float z = transform.position.z;
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireCube(b.center, b.size);

        if (!drawValidInvalidGrid) return;

        float radius = Mathf.Max(DebugGridStep * 0.4f, 0.015f);
        for (float x = b.min.x; x <= b.max.x; x += DebugGridStep)
        {
            for (float y = b.min.y; y <= b.max.y; y += DebugGridStep)
            {
                Vector3 p = new Vector3(x, y, z);
                bool valid = IsValidPlacementPosition(p);
                Gizmos.color = valid ? new Color(0f, 1f, 0f, 0.85f) : new Color(1f, 0f, 0f, 0.7f);
                Gizmos.DrawSphere(p, radius);
            }
        }
    }
}
