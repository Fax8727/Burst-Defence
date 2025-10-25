using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    // --- [ 1. '적 사망' 이벤트를 외부에 알리기 위한 코드 추가 ] ---
    public static event System.Action OnEnemyDied;

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

    // (기존 컴포넌트 참조...)
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Rigidbody2D rb;

    // (기존 정적 변수 및 이벤트...)
    public static Transform PlayerTransform { get; private set; }
    public static Transform CoreTransform { get; private set; }
    public static bool IsPlayerDead { get; private set; }
    public static event System.Action OnPlayerDied;
    public static event System.Action OnPlayerRespawned;

    void Awake()
    {
        // (기존 Awake 내용과 동일)
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
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current HP: " + currentHealth);

        if (UIManager.Instance != null && (entityType == EntityType.Player || entityType == EntityType.Core))
        {
            UIManager.Instance.UpdateHealth(entityType, currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

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
        else if (entityType == EntityType.Enemy)
        {
            // --- [ 2. '적 사망' 신호(이벤트)를 방송하는 코드 추가 ] ---
            OnEnemyDied?.Invoke(); // "적이 죽었다!" 라고 외침

            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnPlayer()
    {
        // (기존 부활 로직과 동일)
        IsPlayerDead = true;
        OnPlayerDied?.Invoke();
        Debug.Log("Player died. Respawning...");
        if (playerController) playerController.enabled = false;
        if (spriteRenderer) spriteRenderer.enabled = false;
        if (playerCollider) playerCollider.enabled = false;
        if (rb) rb.linearVelocity = Vector2.zero;

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

        IsPlayerDead = false;
        OnPlayerRespawned?.Invoke();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (entityType == EntityType.Player && Input.GetKeyDown(KeyCode.T)) { TakeDamage(35); }
        if (entityType == EntityType.Core && Input.GetKeyDown(KeyCode.Y)) { TakeDamage(100); }
#endif
    }
}