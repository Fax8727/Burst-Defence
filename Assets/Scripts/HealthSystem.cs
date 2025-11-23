using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Slider를 사용하기 위해 필요

public class HealthSystem : MonoBehaviour
{
    // ... (이벤트, Enum, 헤더 변수들은 기존과 동일) ...
    public static event System.Action OnEnemyDied;
    public static event System.Action OnPlayerDied;
    public static event System.Action OnPlayerRespawned;
    public enum EntityType { Player, Core, Enemy }
    public EntityType entityType = EntityType.Player;
    public float maxHealth = 100f;
    private float currentHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;
    public float respawnTime = 5.0f;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Rigidbody2D rb;
    public static Transform PlayerTransform { get; private set; }
    public static Transform CoreTransform { get; private set; }
    public static bool IsPlayerDead { get; private set; }

    [Header("Enemy Health Bar (UI)")]
    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset = new Vector3(0, 1.2f, 0);

    private Slider myHealthBarInstance;

    // --- [ 1. (추가) 체력바의 '원본' 스케일을 저장할 변수 ] ---
    private Vector3 initialHealthBarScale;


    void Awake()
    {
        // (Awake 함수는 기존과 동일)
        if (entityType == EntityType.Player)
        {
            PlayerTransform = transform;
            IsPlayerDead = false;
            playerController = GetComponent<PlayerController>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerCollider = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
        }
        else if (entityType == EntityType.Core)
        {
            CoreTransform = transform;
        }
    }

    // --- [ 2. (수정) Start 함수 ] ---
    void Start()
    {
        currentHealth = maxHealth;

        if (UIManager.Instance != null && (entityType == EntityType.Player || entityType == EntityType.Core))
        {
            UIManager.Instance.InitializeHealthBar(entityType, maxHealth);
        }

        if (entityType == HealthSystem.EntityType.Enemy)
        {
            if (UIManager.Instance != null && UIManager.Instance.mainCanvasRect != null && healthBarPrefab != null)
            {
                GameObject hb_GO = Instantiate(healthBarPrefab, UIManager.Instance.mainCanvasRect);
                myHealthBarInstance = hb_GO.GetComponent<Slider>();

                // --- [ (추가) 체력바의 '초기 스케일'을 저장 ] ---
                if (myHealthBarInstance != null)
                {
                    // 프리팹의 원본 스케일(아마 (1,1,1))을 저장
                    initialHealthBarScale = myHealthBarInstance.transform.localScale;

                    myHealthBarInstance.maxValue = maxHealth;
                    myHealthBarInstance.value = maxHealth;
                }
            }
            else
            {
                Debug.LogWarning("Enemy Health Bar가 생성되지 않았습니다. UIManager/Prefab을 확인하세요.");
            }
        }
    }

    // --- [ 3. (수정) LateUpdate 함수 ] ---
    void LateUpdate()
    {
        if (entityType == HealthSystem.EntityType.Enemy && myHealthBarInstance != null)
        {
            // [NEW] 바이러스의 현재 Y축 스케일(크기)을 가져옴 (X, Y가 같다고 가정)
            // (탱커가 0.85로 줄어들면 이 값은 0.85가 됨)
            float currentScaleFactor = transform.localScale.y;

            // 1. (수정) 위치: Offset(간격)도 바이러스 크기에 비례하여 조절
            // (바이러스가 작아지면, 체력바도 더 가까이 붙음)
            Vector3 scaledOffset = healthBarOffset * currentScaleFactor;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position + scaledOffset);

            myHealthBarInstance.transform.position = screenPos;

            // 2. (추가) 크기: 체력바의 크기도 바이러스 크기에 비례하여 조절
            // (원본 크기 * 현재 몬스터 크기)
            myHealthBarInstance.transform.localScale = initialHealthBarScale * currentScaleFactor *2/5;
        }
    }

    // --- (TakeDamage, Die, RespawnPlayer, Update 함수들은 모두 기존과 동일합니다) ---
    // (TakeDamage 함수)
    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current HP: " + currentHealth);
        if (UIManager.Instance != null && (entityType == HealthSystem.EntityType.Player || entityType == EntityType.Core))
        {
            UIManager.Instance.UpdateHealth(entityType, currentHealth, maxHealth);
        }
        if (entityType == HealthSystem.EntityType.Enemy && myHealthBarInstance != null)
        {
            myHealthBarInstance.value = currentHealth;
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    // (Die 함수)
    private void Die()
    {
        Debug.Log(gameObject.name + " has been destroyed!");
        if (entityType == EntityType.Player)
        {
            StartCoroutine(RespawnPlayer());
        }
        else if (entityType == EntityType.Core)
        {
            Time.timeScale = 0f;
            Debug.Log("GAME OVER - Core was destroyed.");
            Destroy(gameObject);
        }
        else if (entityType == HealthSystem.EntityType.Enemy)
        {
            OnEnemyDied?.Invoke();
            if (myHealthBarInstance != null)
            {
                Destroy(myHealthBarInstance.gameObject);
            }
            Destroy(gameObject);
        }
    }
    // (RespawnPlayer 코루틴)
    private IEnumerator RespawnPlayer()
    {
        IsPlayerDead = true;
        OnPlayerDied?.Invoke();
        Debug.Log("Player died. Respawning in " + respawnTime + " seconds...");
        if (playerController) playerController.enabled = false;
        if (spriteRenderer) spriteRenderer.enabled = false;
        if (playerCollider) playerCollider.enabled = false;
        if (rb) rb.linearVelocity = Vector2.zero;
        if (playerController && playerController.firePoint != null)
        {
            playerController.firePoint.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(respawnTime);
        Debug.Log("Player respawning!");
        currentHealth = maxHealth;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(entityType, currentHealth, maxHealth);
        }
        if (playerController) playerController.enabled = true;
        if (spriteRenderer) spriteRenderer.enabled = true;
        if (playerCollider) playerCollider.enabled = true;
        if (playerController && playerController.firePoint != null)
        {
            playerController.firePoint.gameObject.SetActive(true);
        }
        transform.position = new Vector3(0f, -2f, 0f);
        IsPlayerDead = false;
        OnPlayerRespawned?.Invoke();
    }
    // (Update 함수)
    void Update()
    {
#if UNITY_EDITOR
        if (entityType == EntityType.Player && Input.GetKeyDown(KeyCode.T)) { TakeDamage(35); }
        if (entityType == EntityType.Core && Input.GetKeyDown(KeyCode.Y)) { TakeDamage(100); }
#endif
    }
    public void ApplyStatModifiers(WaveStatData stats)
    {
        // '적' 타입에게만 적용
        if (entityType == EntityType.Enemy)
        {
            // (예: 기본 20 * 배율 1.2 = 24)
            maxHealth *= stats.healthMultiplier;
        }
    }
}