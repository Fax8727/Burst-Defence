using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public RectTransform mainCanvasRect;

    [Header("Player Health UI")]
    public Slider playerHealthSlider;
    public TextMeshProUGUI playerHealthText;

    [Header("Core Health UI")]
    public Slider coreHealthSlider;
    public TextMeshProUGUI coreHealthText;

    [Header("Game Status UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;

    [Header("Burst Skill UI")]
    public Slider burstGaugeSlider;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (burstGaugeSlider != null)
        {
            burstGaugeSlider.value = 0;
        }
    }

    public void InitializeHealthBar(HealthSystem.EntityType type, float maxHealth)
    {
        Slider sliderToUse = null;
        TextMeshProUGUI textToUse = null;

        if (type == HealthSystem.EntityType.Player)
        {
            sliderToUse = playerHealthSlider;
            textToUse = playerHealthText;
        }
        else if (type == HealthSystem.EntityType.Core)
        {
            sliderToUse = coreHealthSlider;
            textToUse = coreHealthText;
        }

        if (sliderToUse != null)
        {
            sliderToUse.maxValue = maxHealth;
            sliderToUse.value = maxHealth;
            UpdateHealthText(textToUse, maxHealth, maxHealth);
        }
    }

    public void UpdateHealth(HealthSystem.EntityType type, float currentHealth, float maxHealth)
    {
        Slider sliderToUse = null;
        TextMeshProUGUI textToUse = null;

        if (type == HealthSystem.EntityType.Player)
        {
            sliderToUse = playerHealthSlider;
            textToUse = playerHealthText;
        }
        else if (type == HealthSystem.EntityType.Core)
        {
            sliderToUse = coreHealthSlider;
            textToUse = coreHealthText;
        }

        if (sliderToUse != null)
        {
            sliderToUse.value = currentHealth;
            UpdateHealthText(textToUse, currentHealth, maxHealth);
        }
    }

    private void UpdateHealthText(TextMeshProUGUI healthText, float current, float max)
    {
        if (healthText != null)
        {
            healthText.text = $"{current:F0} / {max:F0}";
        }
    }

    public void UpdateWaveText(int currentWave)
    {
        if (waveText != null)
        {
            waveText.text = "WAVE " + currentWave;
        }
    }

    public void UpdateWaveText(string message)
    {
        if (waveText != null)
        {
            waveText.text = message;
        }
    }

    public void UpdateEnemiesLeftText(int count)
    {
        if (enemiesLeftText != null)
        {
            enemiesLeftText.gameObject.SetActive(true);
            enemiesLeftText.text = "Enemies: " + count;
        }
    }

    public void UpdateEnemiesLeftText(string message, bool visible)
    {
        if (enemiesLeftText != null)
        {
            enemiesLeftText.gameObject.SetActive(visible);
            enemiesLeftText.text = message;
        }
    }

    public void InitializeBurstGauge(float maxGauge)
    {
        if (burstGaugeSlider != null)
        {
            burstGaugeSlider.maxValue = maxGauge;
            burstGaugeSlider.value = 0; // Ω√¿€¿∫ 0
        }
    }

    public void UpdateBurstGauge(float currentGauge)
    {
        if (burstGaugeSlider != null)
        {
            burstGaugeSlider.value = currentGauge;
        }
    }
}