using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player Health UI")]
    public Slider playerHealthSlider;
    public TextMeshProUGUI playerHealthText;

    [Header("Core Health UI")]
    public Slider coreHealthSlider;
    public TextMeshProUGUI coreHealthText;

    [Header("Game Status UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;

    // --- [ 1. Burst 게이지 슬라이더 변수 추가 ] ---
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

        // --- [ 2. (선택) Burst 게이지가 있다면 0으로 초기화 ] ---
        if (burstGaugeSlider != null)
        {
            burstGaugeSlider.value = 0;
        }
    }

    // (기존 체력바 함수들...)
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

    // (기존 웨이브 텍스트 함수들...)
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

    // --- [ 3. Burst 게이지 관련 함수 2개 추가 ] ---

    /// <summary>
    /// Burst 게이지 슬라이더의 최대값을 설정합니다. (BurstSystem의 Start()에서 호출)
    /// </summary>
    public void InitializeBurstGauge(float maxGauge)
    {
        if (burstGaugeSlider != null)
        {
            burstGaugeSlider.maxValue = maxGauge;
            burstGaugeSlider.value = 0; // 시작은 0
        }
    }

    /// <summary>
    /// Burst 게이지 슬라이더의 현재값을 업데이트합니다.
    /// </summary>
    public void UpdateBurstGauge(float currentGauge)
    {
        if (burstGaugeSlider != null)
        {
            burstGaugeSlider.value = currentGauge;
        }
    }
}