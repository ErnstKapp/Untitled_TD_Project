using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hover tooltip for tower buttons: shows icon + description near the cursor.
/// Put this in the same canvas as the tower selection UI and assign references in the Inspector.
/// </summary>
public class TowerHoverTooltipUI : MonoBehaviour
{
    public static TowerHoverTooltipUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [Tooltip("Large tower image shown in the tooltip (NOT the button icon).")]
    [SerializeField] private Image towerImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Positioning")]
    [Tooltip("If true, the tooltip follows the cursor. If false, it stays where you placed the panel in the UI.")]
    [SerializeField] private bool followCursor = false;
    [SerializeField] private Vector2 screenOffset = new Vector2(18f, -18f);
    [Tooltip("Padding from screen edges (pixels).")]
    [SerializeField] private Vector2 screenPadding = new Vector2(12f, 12f);

    private RectTransform _panelRect;
    private Canvas _rootCanvas;
    private bool _isShown;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (panel != null)
            _panelRect = panel.GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        Hide();
    }

    public void Show(TowerData data, Sprite towerSprite)
    {
        if (data == null || panel == null) return;

        if (titleText != null) titleText.text = data.towerName;
        if (descriptionText != null) descriptionText.text = data.description;

        if (towerImage != null)
        {
            towerImage.sprite = towerSprite;
            towerImage.enabled = towerSprite != null;
        }

        panel.SetActive(true);
        _isShown = true;
        if (followCursor)
            UpdatePosition(Input.mousePosition);
    }

    public void Hide()
    {
        _isShown = false;
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (!_isShown || !followCursor) return;
        UpdatePosition(Input.mousePosition);
    }

    private void UpdatePosition(Vector2 mouseScreenPos)
    {
        if (_panelRect == null || _rootCanvas == null) return;

        Vector2 screenPos = mouseScreenPos + screenOffset;

        // Clamp inside screen bounds (with padding). This is screen-space, works for Overlay and Camera canvas.
        Vector2 size = _panelRect.sizeDelta;
        float minX = screenPadding.x;
        float minY = screenPadding.y;
        float maxX = Screen.width - screenPadding.x - size.x;
        float maxY = Screen.height - screenPadding.y - size.y;

        screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
        screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

        if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _panelRect.position = screenPos;
            return;
        }

        Camera cam = _rootCanvas.worldCamera != null ? _rootCanvas.worldCamera : Camera.main;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_panelRect, screenPos, cam, out Vector3 world))
            _panelRect.position = world;
        else
            _panelRect.position = screenPos;
    }
}

