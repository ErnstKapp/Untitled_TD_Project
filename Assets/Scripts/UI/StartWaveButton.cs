using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartWaveButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startWaveButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private bool lastInteractable = true;
    private int lastLoggedFrame = -1;

    private void Start()
    {
        // If button not assigned, try to get it from this GameObject
        if (startWaveButton == null)
        {
            startWaveButton = GetComponent<Button>();
        }

        // If button text not assigned, try to find it in children
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // Set up button click listener
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(OnStartWaveClicked);
        }
        else
        {
            Debug.LogError("[StartWaveButton] Button component not found!");
        }

        UpdateButtonState();
    }
    
    private void OnEnable()
    {
        UpdateButtonState();
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (startWaveButton == null)
        {
            if (Time.frameCount != lastLoggedFrame) { Debug.LogWarning("[StartWaveButton] startWaveButton is null - cannot update."); lastLoggedFrame = Time.frameCount; }
            return;
        }
        if (WaveManager.Instance == null)
        {
            if (Time.frameCount != lastLoggedFrame) { Debug.LogWarning("[StartWaveButton] WaveManager.Instance is null - button will stay disabled. Is there a WaveManager in this scene?"); lastLoggedFrame = Time.frameCount; }
            startWaveButton.interactable = false;
            if (buttonText != null) buttonText.text = "(No WaveManager)";
            return;
        }

        bool canStartWave = WaveManager.Instance.HasMoreWaves && !WaveManager.Instance.IsSpawning;
        startWaveButton.interactable = canStartWave;

        if (canStartWave != lastInteractable || Time.frameCount - lastLoggedFrame > 60)
        {
            lastInteractable = canStartWave;
            lastLoggedFrame = Time.frameCount;
        }

        // Update button text
        if (buttonText != null)
        {
            if (!WaveManager.Instance.HasMoreWaves)
            {
                buttonText.text = "All Waves Complete";
            }
            else if (WaveManager.Instance.IsSpawning)
            {
                buttonText.text = $"Wave {WaveManager.Instance.CurrentWave} In Progress...";
            }
            else
            {
                buttonText.text = $"Start Wave {WaveManager.Instance.CurrentWave}";
            }
        }
    }

    private void OnStartWaveClicked()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.HasMoreWaves && !WaveManager.Instance.IsSpawning)
            WaveManager.Instance.StartNextWave();
    }

    private void OnDestroy()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.RemoveListener(OnStartWaveClicked);
        }
    }
}
