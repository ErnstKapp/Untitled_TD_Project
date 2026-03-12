using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class TowerPlacement : MonoBehaviour
{
    public static TowerPlacement Instance { get; private set; }
    public bool IsPlacing => isPlacing;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private LayerMask blockedLayer;
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;

    private Camera mainCamera;
    private TowerData selectedTowerData;
    private GameObject previewTower;
    private SpriteRenderer previewRenderer;
    private GameObject rangeIndicator;
    private SpriteRenderer rangeIndicatorRenderer;
    private bool isPlacing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }

    private void Update()
    {
        if (isPlacing && selectedTowerData != null)
        {
            UpdatePreviewPosition();
            HandlePlacementInput();
        }
    }

    public void StartPlacement(TowerData towerData)
    {
        if (towerData == null || towerData.towerPrefab == null)
        {
            if (towerData != null && towerData.towerPrefab == null)
                Debug.LogWarning($"[TowerPlacement] Cannot start placement for '{towerData.towerName}': Tower Data has no Tower Prefab assigned. Assign it in the Inspector.", towerData);
            return;
        }

        selectedTowerData = towerData;
        isPlacing = true;

        // Recreate preview when switching to a different tower so the sprite matches
        if (previewTower != null)
        {
            Destroy(previewTower);
            previewTower = null;
            previewRenderer = null;
        }

        // Create preview from current tower prefab
        previewTower = Instantiate(towerData.towerPrefab);
        previewTower.name = "TowerPreview";

        // Disable components that shouldn't work in preview
        Tower tower = previewTower.GetComponent<Tower>();
        if (tower != null)
        {
            tower.enabled = false;
        }

        // Make preview semi-transparent
        previewRenderer = previewTower.GetComponent<SpriteRenderer>();
        if (previewRenderer == null)
        {
            previewRenderer = previewTower.GetComponentInChildren<SpriteRenderer>();
        }

        if (previewRenderer != null)
        {
            Color color = previewRenderer.color;
            color.a = 0.5f;
            previewRenderer.color = color;
        }

        // Disable colliders
        Collider2D[] colliders = previewTower.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        previewTower.SetActive(true);
        
        // Create range indicator
        CreateRangeIndicator();
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        selectedTowerData = null;
        
        if (previewTower != null)
        {
            previewTower.SetActive(false);
        }
        
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }

    private void UpdatePreviewPosition()
    {
        if (previewTower == null || mainCamera == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        previewTower.transform.position = mouseWorldPos;

        // Update range indicator position
        if (rangeIndicator != null)
        {
            rangeIndicator.transform.position = mouseWorldPos;
        }

        // Update color based on placement validity
        if (previewRenderer != null)
        {
            bool canPlace = CanPlaceAtPosition(mouseWorldPos);
            Color color = previewRenderer.color;
            color = canPlace ? validPlacementColor : invalidPlacementColor;
            color.a = 0.5f;
            previewRenderer.color = color;
        }
        
        // Update range indicator color based on placement validity
        if (rangeIndicatorRenderer != null)
        {
            bool canPlace = CanPlaceAtPosition(mouseWorldPos);
            Color rangeColor = rangeIndicatorRenderer.color;
            rangeColor = canPlace ? new Color(0.2f, 0.5f, 1f, 0.3f) : new Color(1f, 0.2f, 0.2f, 0.3f); // Blue if valid, red if invalid
            rangeIndicatorRenderer.color = rangeColor;
        }
    }
    
    private void CreateRangeIndicator()
    {
        if (selectedTowerData == null) return;
        
        if (rangeIndicator == null)
        {
            rangeIndicator = new GameObject("RangeIndicator");
            rangeIndicatorRenderer = rangeIndicator.AddComponent<SpriteRenderer>();
            rangeIndicatorRenderer.sprite = CreateRangeSprite();
            rangeIndicatorRenderer.sortingOrder = -1;
        }
        
        // Set size based on tower range
        rangeIndicator.transform.localScale = Vector3.one * selectedTowerData.range * 2f;
        rangeIndicatorRenderer.color = new Color(0.2f, 0.5f, 1f, 0.3f); // Transparent blue
        rangeIndicator.SetActive(true);
    }
    
    private Sprite CreateRangeSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Vector2 center = new Vector2(32, 32);
        float radius = 32f;

        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
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
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
    }

    private void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (mainCamera == null)
            {
                Debug.LogError("[TowerPlacement] Main camera is null! Cannot convert mouse position.");
                return;
            }

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            bool canPlace = CanPlaceAtPosition(mouseWorldPos);

            if (canPlace)
                PlaceTower(mouseWorldPos);
            else
                Debug.LogWarning("[TowerPlacement] Cannot place tower here. Check: blocked area, currency, or layers.");
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            CancelPlacement();
    }

    private bool CanPlaceAtPosition(Vector3 position)
    {
        // Check if position is blocked (only if blockedLayer is set)
        if (blockedLayer.value != 0)
        {
            Collider2D blocked = Physics2D.OverlapCircle(position, 0.5f, blockedLayer);
            if (blocked != null)
            {
                return false;
            }
        }

        // Check if player can afford the tower
        if (CurrencyManager.Instance == null)
        {
            return false;
        }

        if (!CurrencyManager.Instance.CanAfford(selectedTowerData.cost))
        {
            return false;
        }

        return true;
    }

    private void PlaceTower(Vector3 position)
    {
        if (selectedTowerData == null)
        {
            Debug.LogError("[TowerPlacement] Cannot place tower - selectedTowerData is null!");
            return;
        }
        if (selectedTowerData.towerPrefab == null)
        {
            Debug.LogError("[TowerPlacement] Cannot place tower - towerPrefab is null!");
            return;
        }
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("[TowerPlacement] Cannot place tower - CurrencyManager.Instance is null!");
            return;
        }
        if (!CurrencyManager.Instance.SpendCurrency(selectedTowerData.cost))
        {
            Debug.LogError($"[TowerPlacement] Not enough currency! Cost: ${selectedTowerData.cost}, Have: ${CurrencyManager.Instance.CurrentCurrency}");
            return;
        }

        GameObject towerObj = null;
        try
        {
            towerObj = Instantiate(selectedTowerData.towerPrefab, position, Quaternion.identity);
            if (towerObj == null)
            {
                Debug.LogError("[TowerPlacement] Instantiate returned null! Tower was not created.");
                return;
            }
            if (!towerObj.activeSelf)
                towerObj.SetActive(true);
            Tower towerCheck = towerObj.GetComponent<Tower>();
            if (towerCheck != null && !towerCheck.enabled)
                towerCheck.enabled = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TowerPlacement] Exception during instantiation: {e.Message}");
            return;
        }

        if (towerObj == null)
        {
            Debug.LogError("[TowerPlacement] Tower object is null after instantiation!");
            return;
        }

        Tower tower = towerObj.GetComponent<Tower>();
        if (tower != null)
        {
            tower.SetTowerData(selectedTowerData);
            tower.PlaceTower();
        }
        else
            Debug.LogWarning("[TowerPlacement] Tower prefab instantiated but Tower component not found!");

        CancelPlacement();
    }
}
