using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0); // Position above enemy
    [SerializeField] private float healthBarScale = 0.005f; // Scale of the health bar (smaller = smaller bar)
    [SerializeField] private Vector2 healthBarSize = new Vector2(50, 10); // Width and height of health bar

    [Header("Sprites (optional)")]
    [Tooltip("Sprite for the fill (HP bar). Left-to-right full-width works best; fillAmount reveals it. If null, uses a simple colored quad.")]
    [SerializeField] private Sprite fillSprite;
    [Tooltip("Sprite for the background behind the fill. If null, uses a dark gray quad.")]
    [SerializeField] private Sprite backgroundSprite;

    private GameObject healthBarCanvas;
    private Image healthBarFill;
    private Enemy enemy;
    private Camera mainCamera;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("EnemyHealthBar requires an Enemy component!");
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        CreateHealthBar();
    }

    private void CreateHealthBar()
    {
        // Create Canvas for health bar
        healthBarCanvas = new GameObject("HealthBarCanvas");
        healthBarCanvas.transform.SetParent(transform);
        healthBarCanvas.transform.localPosition = offset;

        Canvas canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;

        CanvasScaler scaler = healthBarCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f; // Don't scale here, use transform scale instead

        GraphicRaycaster raycaster = healthBarCanvas.AddComponent<GraphicRaycaster>();

        // Set canvas size (configurable)
        RectTransform canvasRect = healthBarCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize;
        
        // Scale the entire canvas transform to control size
        healthBarCanvas.transform.localScale = Vector3.one * healthBarScale;

        // Create background (dark bar behind health)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarCanvas.transform);
        background.transform.localPosition = Vector3.zero;
        background.transform.localScale = Vector3.one;
        background.transform.SetAsFirstSibling(); // Render first (behind everything)

        Image bgImage = background.AddComponent<Image>();
        if (backgroundSprite != null)
        {
            bgImage.sprite = backgroundSprite;
            bgImage.color = Color.white;
        }
        else
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Create fill (health bar) - child of background so it renders on top
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = Vector3.one;

        healthBarFill = fill.AddComponent<Image>();
        if (fillSprite != null)
        {
            healthBarFill.sprite = fillSprite;
            healthBarFill.color = Color.white; // Tint with color in UpdateHealthBar
        }
        else
        {
            Texture2D fillTexture = new Texture2D(1, 1);
            fillTexture.SetPixel(0, 0, Color.white);
            fillTexture.Apply();
            healthBarFill.sprite = Sprite.Create(fillTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            healthBarFill.color = Color.green;
        }
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left; // Fill from left
        healthBarFill.fillAmount = 1f; // Start full

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        // Ensure the fill rect is properly set up
        fillRect.localScale = Vector3.one;

        UpdateHealthBar();
    }

    private void Update()
    {
        if (healthBarCanvas == null || enemy == null) return;

        // Make health bar face the camera
        if (mainCamera != null)
        {
            healthBarCanvas.transform.LookAt(healthBarCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (enemy == null || healthBarFill == null) return;

        float healthPercent = enemy.HealthPercentage;
        
        // Update the fill amount - this controls the bar size
        healthBarFill.fillAmount = healthPercent;
        
        // Make sure fill method is set correctly
        if (healthBarFill.type != Image.Type.Filled)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        }

        // Color tint: green when healthy, red when low (only applied if using procedural fill; sprite uses white + optional tint)
        if (fillSprite == null)
        {
            if (healthPercent > 0.5f)
            {
                float t = (healthPercent - 0.5f) / 0.5f;
                healthBarFill.color = Color.Lerp(Color.yellow, Color.green, t);
            }
            else
            {
                float t = healthPercent / 0.5f;
                healthBarFill.color = Color.Lerp(Color.red, Color.yellow, t);
            }
        }
        else
            healthBarFill.color = Color.white;
    }

    private void OnDestroy()
    {
        if (healthBarCanvas != null)
        {
            Destroy(healthBarCanvas);
        }
    }
}
