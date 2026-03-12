using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerInfoUI : MonoBehaviour
{
    public static TowerInfoUI Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Image towerImage; // Image component to display tower sprite
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI fireRateText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI projectileSpeedText;
    [SerializeField] private TextMeshProUGUI dotDamageText; // Optional: DoT damage display
    [SerializeField] private TextMeshProUGUI dotDurationText; // Optional: DoT duration display
    [SerializeField] private TextMeshProUGUI slowPercentageText; // Optional: Slow percentage display
    [SerializeField] private TextMeshProUGUI slowDurationText; // Optional: Slow duration display
    [SerializeField] private Button closeButton;

    [Header("Ability (Toreador, optional)")]
    [Tooltip("Show when selected tower has Toreador ability (e.g. level 3 Opera).")]
    [SerializeField] private GameObject abilitySection;
    [SerializeField] private Button abilityButton;
    [SerializeField] private TextMeshProUGUI abilityCooldownText;

    [Header("Upgrade Buttons (optional)")]
    [Tooltip("Optional: show/hide container when tower has upgrades.")]
    [SerializeField] private GameObject upgradeSection;
    [Tooltip("Single upgrade button: use when tower has only one choice (linear path). Wire this and leave Left/Right empty for one-button setup.")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [Tooltip("Use when tower can have two choices (branch).")]
    [SerializeField] private Button upgradeLeftButton;
    [SerializeField] private TextMeshProUGUI upgradeLeftLabel;
    [SerializeField] private Button upgradeRightButton;
    [SerializeField] private TextMeshProUGUI upgradeRightLabel;
    
    [Header("Panel Toggle")]
    [SerializeField] private GameObject towerSelectionPanel; // The TowerSelectionUI panel to hide/show

    [Header("Debug")]
    [Tooltip("Log upgrade section/button state when panel opens (to fix missing buttons).")]
    [SerializeField] private bool debugUpgradeVisibility = true;

    private Tower selectedTower;
    private TowerSelectionUI towerSelectionUI;
    private bool justClickedTower = false; // Flag to prevent immediate close after tower click
    private float refreshTimer; // Refresh panel when open so next upgrade and afford state update

    private void Awake()
    {
        // Set singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[TowerInfoUI] Multiple TowerInfoUI instances found! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // If panel not assigned, try to find it
        if (infoPanel == null)
        {
            infoPanel = transform.Find("TowerInfoPanel")?.gameObject;
        }

        // Hide panel initially
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
            
            // Make sure it doesn't block raycasts when hidden
            CanvasGroup infoCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (infoCanvasGroup != null)
            {
                infoCanvasGroup.blocksRaycasts = false;
                infoCanvasGroup.interactable = false;
            }
        }

        // Find TowerSelectionUI if not assigned
        if (towerSelectionPanel == null)
        {
            towerSelectionUI = FindObjectOfType<TowerSelectionUI>();
            if (towerSelectionUI != null)
            {
                // Try to find the button parent or main panel
                towerSelectionPanel = towerSelectionUI.transform.Find("TowerSelectionPanel")?.gameObject;
                if (towerSelectionPanel == null)
                {
                    // Fallback: use the buttonParent if available, otherwise the GameObject itself
                    if (towerSelectionUI.ButtonParent != null)
                    {
                        towerSelectionPanel = towerSelectionUI.ButtonParent.gameObject;
                    }
                    else
                    {
                        towerSelectionPanel = towerSelectionUI.gameObject;
                    }
                }
            }
        }

        // Ensure tower selection panel is visible and clickable at start
        if (towerSelectionPanel != null)
        {
            towerSelectionPanel.SetActive(true);
            
            // Make sure the panel's Image component is enabled (for background)
            Image selectionPanelImage = towerSelectionPanel.GetComponent<Image>();
            if (selectionPanelImage != null)
            {
                selectionPanelImage.enabled = true;
            }
            
            // Make sure it's clickable
            CanvasGroup canvasGroup = towerSelectionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }

        // Set up close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInfoPanel);
        }

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeSingleClicked);
        if (upgradeLeftButton != null && upgradeLeftButton != upgradeButton)
            upgradeLeftButton.onClick.AddListener(OnUpgradeLeftClicked);
        if (upgradeRightButton != null && upgradeRightButton != upgradeButton)
            upgradeRightButton.onClick.AddListener(OnUpgradeRightClicked);
        if (abilityButton != null)
            abilityButton.onClick.AddListener(OnAbilityClicked);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(int newCurrency)
    {
        if (infoPanel != null && infoPanel.activeSelf && selectedTower != null)
            UpdateUI();
    }

    private void OnUpgradeSingleClicked()
    {
        if (selectedTower == null) return;
        selectedTower.GetNextUpgradeOptions(out TowerUpgradeNode left, out TowerUpgradeNode right);
        TowerUpgradeNode apply = left != null ? left : right;
        if (apply != null && selectedTower.ApplyUpgrade(apply))
            UpdateUI();
    }

    private void OnUpgradeLeftClicked()
    {
        if (selectedTower == null) return;
        selectedTower.GetNextUpgradeOptions(out TowerUpgradeNode left, out _);
        if (left != null && selectedTower.ApplyUpgrade(left))
            UpdateUI();
    }

    private void OnUpgradeRightClicked()
    {
        if (selectedTower == null) return;
        selectedTower.GetNextUpgradeOptions(out _, out TowerUpgradeNode right);
        if (right != null && selectedTower.ApplyUpgrade(right))
            UpdateUI();
    }

    private void OnAbilityClicked()
    {
        if (selectedTower == null)
        {
            return;
        }
        if (selectedTower.TryUseToreadorAbility())
            UpdateUI();
    }

    private void Update()
    {
        // Reset flag at start of frame
        if (justClickedTower)
        {
            justClickedTower = false;
        }
        
        // Handle clicks whether panel is open or closed
        if (Input.GetMouseButtonDown(0))
        {
            // Check if clicking on UI elements (buttons, etc.)
            bool clickedOnUI = IsPointerOverUI();
            
            // If clicking on UI, check if it's a button
            if (clickedOnUI)
            {
                UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
                if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
                {
                    // If clicking on a button, let it handle the click
                    if (eventSystem.currentSelectedGameObject.GetComponent<Button>() != null)
                    {
                        return; // Let button handle click
                    }
                }
            }
            
            // Check if clicking on a tower
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            
            // Check all towers to see if we clicked on one
            Tower clickedTower = null;
            Tower[] allTowers = FindObjectsOfType<Tower>();
            foreach (Tower tower in allTowers)
            {
                if (!tower.IsPlaced) continue;
                
                float distance = Vector2.Distance(tower.transform.position, mouseWorldPos);
                if (distance <= 0.5f) // Same click radius as Tower.cs
                {
                    clickedTower = tower;
                    break;
                }
            }
            
            if (clickedTower != null)
            {
                // Clicked on a tower - show its info (or keep current if same tower)
                if (clickedTower != selectedTower)
                {
                    ShowTowerInfo(clickedTower);
                }
                justClickedTower = true;
            }
            else if (infoPanel != null && infoPanel.activeSelf)
            {
                // Panel is open and clicked elsewhere (not on a tower, not on a button) - close info panel
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseInfoPanel();
                }
                else if (!justClickedTower && !clickedOnUI)
                {
                    CloseInfoPanel();
                }
            }
        }
        
        // Handle Escape key when panel is open
        if (infoPanel != null && infoPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInfoPanel();
        }

        // Refresh panel periodically when open so next upgrade shows and afford state updates
        if (infoPanel != null && infoPanel.activeSelf && selectedTower != null)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= 0.2f)
            {
                refreshTimer = 0f;
                UpdateUI();
            }
        }
        else
            refreshTimer = 0f;
    }

    public void ShowTowerInfo(Tower tower)
    {
        if (tower == null || tower.Data == null)
        {
            Debug.LogWarning("[TowerInfoUI] Cannot show info - tower or tower data is null");
            return;
        }

        // Just store the tower reference - the UI will pull what it needs
        selectedTower = tower;
        justClickedTower = true; // Set flag to prevent immediate close

        // Update the UI with info from the selected tower
        UpdateUI();

        // Hide tower selection panel first
        if (towerSelectionPanel != null)
        {
            towerSelectionPanel.SetActive(false);
            
            // Make sure it doesn't block raycasts when hidden
            CanvasGroup selectionCanvasGroup = towerSelectionPanel.GetComponent<CanvasGroup>();
            if (selectionCanvasGroup != null)
            {
                selectionCanvasGroup.blocksRaycasts = false;
                selectionCanvasGroup.interactable = false;
            }
        }
        // Show info panel
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            
            // Make sure the panel's Image component is enabled (for background)
            Image panelImage = infoPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.enabled = true;
            }
            
            // Make sure it can receive clicks
            CanvasGroup infoCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (infoCanvasGroup != null)
            {
                infoCanvasGroup.blocksRaycasts = true;
                infoCanvasGroup.interactable = true;
            }
        }
        // Show range indicator on tower
        tower.ShowRange(true);
    }

    private void UpdateUI()
    {
        if (selectedTower == null || selectedTower.Data == null)
        {
            return;
        }

        // Pull all info directly from the selected tower's data
        TowerData data = selectedTower.Data;

        // Display tower image/sprite
        if (towerImage != null)
        {
            // Try to get sprite from the tower's SpriteRenderer
            SpriteRenderer towerSpriteRenderer = selectedTower.GetComponent<SpriteRenderer>();
            if (towerSpriteRenderer != null && towerSpriteRenderer.sprite != null)
            {
                towerImage.sprite = towerSpriteRenderer.sprite;
                towerImage.enabled = true;
            }
            else if (data.icon != null)
            {
                // Fallback: Use icon from TowerData
                towerImage.sprite = data.icon;
                towerImage.enabled = true;
            }
            else
            {
                // If no sprite found, hide the image
                towerImage.enabled = false;
            }
        }

        if (towerNameText != null)
        {
            towerNameText.text = data.towerName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }

        if (costText != null)
        {
            costText.text = $"Cost: ${data.cost}";
        }

        // Use effective stats (base + upgrades)
        if (rangeText != null)
            rangeText.text = $"Range: {selectedTower.EffectiveRange:F1}";
        if (fireRateText != null)
            fireRateText.text = $"Fire Rate: {selectedTower.EffectiveFireRate:F2}/s";
        if (damageText != null)
            damageText.text = $"Damage: {selectedTower.EffectiveDamage:F0}";
        if (projectileSpeedText != null)
            projectileSpeedText.text = $"Projectile Speed: {selectedTower.EffectiveProjectileSpeed:F1}";

        if (dotDamageText != null)
        {
            if (selectedTower.EffectiveDotDamage > 0f && selectedTower.EffectiveDotDuration > 0f)
            {
                dotDamageText.text = $"DoT Damage: {selectedTower.EffectiveDotDamage:F1}/s";
                dotDamageText.gameObject.SetActive(true);
            }
            else
                dotDamageText.gameObject.SetActive(false);
        }
        if (dotDurationText != null)
        {
            if (selectedTower.EffectiveDotDamage > 0f && selectedTower.EffectiveDotDuration > 0f)
            {
                dotDurationText.text = $"DoT Duration: {selectedTower.EffectiveDotDuration:F1}s";
                dotDurationText.gameObject.SetActive(true);
            }
            else
                dotDurationText.gameObject.SetActive(false);
        }

        if (slowPercentageText != null)
        {
            if (selectedTower.EffectiveSlowPercentage > 0f && selectedTower.EffectiveSlowDuration > 0f)
            {
                slowPercentageText.text = $"Slow: {(selectedTower.EffectiveSlowPercentage * 100f):F0}%";
                slowPercentageText.gameObject.SetActive(true);
            }
            else
                slowPercentageText.gameObject.SetActive(false);
        }
        if (slowDurationText != null)
        {
            if (selectedTower.EffectiveSlowPercentage > 0f && selectedTower.EffectiveSlowDuration > 0f)
            {
                slowDurationText.text = $"Slow Duration: {selectedTower.EffectiveSlowDuration:F1}s";
                slowDurationText.gameObject.SetActive(true);
            }
            else
                slowDurationText.gameObject.SetActive(false);
        }

        // Upgrade section: one choice → single button; two choices → left/right buttons
        selectedTower.GetNextUpgradeOptions(out TowerUpgradeNode leftOpt, out TowerUpgradeNode rightOpt);
        bool hasUpgrades = leftOpt != null || rightOpt != null;
        bool singleChoice = (leftOpt != null && rightOpt == null) || (leftOpt == null && rightOpt != null);
        TowerUpgradeNode singleOpt = leftOpt != null ? leftOpt : rightOpt;

        if (hasUpgrades && upgradeSection == null && upgradeButton == null && upgradeLeftButton == null && upgradeRightButton == null)
            Debug.LogWarning("[TowerInfoUI] Tower has upgrades but no upgrade UI assigned. Assign Upgrade Section and/or Upgrade Button (or Left/Right buttons) on TowerInfoUI.");

        if (upgradeSection != null)
            upgradeSection.SetActive(hasUpgrades);

        if (singleChoice && upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(true);
            upgradeButton.interactable = singleOpt != null && CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(singleOpt.cost);
            if (upgradeLabel != null && singleOpt != null)
                upgradeLabel.text = $"{singleOpt.displayName} (${singleOpt.cost})";
            if (upgradeLeftButton != null && upgradeLeftButton != upgradeButton)
                upgradeLeftButton.gameObject.SetActive(false);
            if (upgradeRightButton != null && upgradeRightButton != upgradeButton)
                upgradeRightButton.gameObject.SetActive(false);
        }
        else
        {
            if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
            if (upgradeLeftButton != null)
            {
                upgradeLeftButton.gameObject.SetActive(leftOpt != null);
                upgradeLeftButton.interactable = leftOpt != null && CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(leftOpt.cost);
                if (upgradeLeftLabel != null && leftOpt != null)
                    upgradeLeftLabel.text = $"{leftOpt.displayName} (${leftOpt.cost})";
            }
            if (upgradeRightButton != null)
            {
                upgradeRightButton.gameObject.SetActive(rightOpt != null);
                upgradeRightButton.interactable = rightOpt != null && CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(rightOpt.cost);
                if (upgradeRightLabel != null && rightOpt != null)
                    upgradeRightLabel.text = $"{rightOpt.displayName} (${rightOpt.cost})";
            }
        }

        // Ability section (Toreador)
        bool hasAbility = selectedTower.HasToreadorAbility;
        if (abilitySection != null)
            abilitySection.SetActive(hasAbility);
        if (hasAbility && abilityButton != null)
        {
            float remaining = selectedTower.GetToreadorCooldownRemaining();
            abilityButton.interactable = remaining <= 0f;
            if (abilityCooldownText != null)
                abilityCooldownText.text = remaining > 0f ? $"{remaining:F1}s" : "Ready";
        }
    }

    public void CloseInfoPanel()
    {
        // Hide info panel first
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
            
            // Make sure it doesn't block raycasts when hidden
            CanvasGroup infoCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (infoCanvasGroup != null)
            {
                infoCanvasGroup.blocksRaycasts = false;
                infoCanvasGroup.interactable = false;
            }
        }

        // Hide range indicator
        if (selectedTower != null)
        {
            selectedTower.ShowRange(false);
        }

        selectedTower = null;
        
        // Show tower selection panel again
        if (towerSelectionPanel != null)
        {
            towerSelectionPanel.SetActive(true);
            
            // Make sure the panel's Image component is enabled (for background)
            Image selectionPanelImage = towerSelectionPanel.GetComponent<Image>();
            if (selectionPanelImage != null)
            {
                selectionPanelImage.enabled = true;
            }
            
            // Make sure it's clickable
            CanvasGroup canvasGroup = towerSelectionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
