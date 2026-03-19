using UnityEngine;

/// <summary>
/// Put on the slow/DoT overlay child GameObject (the one toggled by EnemyStatusVisuals).
/// Animates only while the object is active.
/// </summary>
public class StatusEffectIconAnimator : MonoBehaviour
{
    public enum AnimationMode
    {
        PulseScale,
        BobVertical,
        RotateZ,
        PulseAlpha
    }

    [SerializeField] private AnimationMode mode = AnimationMode.PulseScale;
    [SerializeField] private float speed = 3f;
    [Tooltip("PulseScale: scale +/- this fraction. BobVertical: world units. RotateZ: max degrees sway.")]
    [SerializeField] private float amount = 0.12f;

    [Header("Pulse Alpha (mode = PulseAlpha)")]
    [SerializeField] private SpriteRenderer alphaTarget;
    [SerializeField] private float alphaMin = 0.5f;
    [SerializeField] private float alphaMax = 1f;

    private Vector3 _baseScale;
    private Vector3 _basePos;
    private Quaternion _baseRot;
    private Color _baseColor;
    private bool _captured;

    private void OnEnable()
    {
        CaptureBase();
    }

    private void CaptureBase()
    {
        _baseScale = transform.localScale;
        _basePos = transform.localPosition;
        _baseRot = transform.localRotation;
        if (alphaTarget == null)
            alphaTarget = GetComponent<SpriteRenderer>();
        if (alphaTarget != null)
            _baseColor = alphaTarget.color;
        _captured = true;
    }

    private void LateUpdate()
    {
        if (!_captured) CaptureBase();

        float t = Time.time * speed;
        float wave = Mathf.Sin(t);

        switch (mode)
        {
            case AnimationMode.PulseScale:
                transform.localScale = _baseScale * (1f + wave * amount);
                break;
            case AnimationMode.BobVertical:
                {
                    Vector3 p = _basePos;
                    p.y += wave * amount;
                    transform.localPosition = p;
                }
                break;
            case AnimationMode.RotateZ:
                transform.localRotation = _baseRot * Quaternion.Euler(0f, 0f, wave * amount);
                break;
            case AnimationMode.PulseAlpha:
                if (alphaTarget != null)
                {
                    float a = Mathf.Lerp(alphaMin, alphaMax, (wave + 1f) * 0.5f);
                    Color c = _baseColor;
                    c.a = a;
                    alphaTarget.color = c;
                }
                break;
        }
    }

    private void OnDisable()
    {
        if (!_captured) return;
        transform.localScale = _baseScale;
        transform.localPosition = _basePos;
        transform.localRotation = _baseRot;
        if (alphaTarget != null)
        {
            Color c = _baseColor;
            alphaTarget.color = c;
        }
    }
}
