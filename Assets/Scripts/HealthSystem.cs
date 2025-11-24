using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public static event System.Action OnEnemyDied;
    public static event System.Action OnPlayerDied;
    public static event System.Action OnPlayerRespawned;

    public enum EntityType { Player, Core, Enemy }

    [Header("Entity Type")]
    public EntityType entityType = EntityType.Player;

    [Header("Health Stats")]
    public float maxHealth = 100f;
    private float currentHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    [Header("Player Respawn (Player 전용)")]
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
    private Vector3 initialHealthBarScale;

    void Awake()
    {
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

    void Start()
    {
        currentHealth = maxHealth;

        if (UIManager.Instance != null && (entityType == EntityType.Player || entityType == EntityType.Core))
        {
            UIManager.Instance.InitializeHealthBar(entityType, maxHealth);
        }

        if (entityType == EntityType.Enemy)
        {
            if (UIManager.Instance != null && UIManager.Instance.mainCanvasRect != null && healthBarPrefab != null)
            {
                GameObject hb_GO = Instantiate(healthBarPrefab, UIManager.Instance.mainCanvasRect);
                myHealthBarInstance = hb_GO.GetComponent<Slider>();

                if (myHealthBarInstance != null)
                {
                    initialHealthBarScale = myHealthBarInstance.transform.localScale;
                    myHealthBarInstance.maxValue = maxHealth;
                    myHealthBarInstance.value = maxHealth;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (entityType == EntityType.Enemy && myHealthBarInstance != null)
        {
            float currentScaleFactor = transform.localScale.y;

            Vector3 scaledOffset = healthBarOffset * currentScaleFactor;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position + scaledOffset);

            myHealthBarInstance.transform.position = screenPos;
            myHealthBarInstance.transform.localScale = initialHealthBarScale * currentScaleFactor;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (UIManager.Instance != null && (entityType == EntityType.Player || entityType == EntityType.Core))
        {
            UIManager.Instance.UpdateHealth(entityType, currentHealth, maxHealth);
        }

        if (entityType == EntityType.Enemy && myHealthBarInstance != null)
        {
            myHealthBarInstance.value = currentHealth;
        }

        if (AudioManager.Instance != null)
        {
            if (entityType == EntityType.Enemy)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyHitClip, 1.5f);
            }
            else if (entityType == EntityType.Player)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.playerHitClip);
            }
            else if (entityType == EntityType.Core)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.coreHitClip);
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (entityType == EntityType.Player)
        {
            StartCoroutine(RespawnPlayer());
        }
        else if (entityType == EntityType.Core)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            Destroy(gameObject);
        }
        else if (entityType == EntityType.Enemy)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDeathClip);
            }

            OnEnemyDied?.Invoke();

            if (myHealthBarInstance != null)
            {
                Destroy(myHealthBarInstance.gameObject);
            }

            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnPlayer()
    {
        IsPlayerDead = true;
        OnPlayerDied?.Invoke();

        if (playerController) playerController.enabled = false;
        if (spriteRenderer) spriteRenderer.enabled = false;
        if (playerCollider) playerCollider.enabled = false;
        if (rb) rb.linearVelocity = Vector2.zero;

        if (playerController && playerController.firePoint != null)
        {
            playerController.firePoint.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(respawnTime);

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

    public void ApplyStatModifiers(WaveStatData stats)
    {
        if (entityType == EntityType.Enemy)
        {
            maxHealth *= stats.healthMultiplier;
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (entityType == EntityType.Player && Input.GetKeyDown(KeyCode.T)) 
        { 
            TakeDamage(35); 
        }
        
        if (entityType == EntityType.Core && Input.GetKeyDown(KeyCode.Y)) 
        { 
            TakeDamage(100); 
        }
#endif
    }
}