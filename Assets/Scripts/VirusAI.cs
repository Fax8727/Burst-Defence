using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class VirusAI : MonoBehaviour
{
    [Header("AI Stats")]
    // 이 값들은 '기본값'이며, 충돌할 때마다 이 변수들의 값이 직접 수정됩니다.
    public float moveSpeed = 30f;
    public float attackDamage = 10f;

    [Header("Physics Settings")]
    public float healthToMassMultiplier = 0.1f;

    [Header("Tanker Settings")]
    public bool isTanker = false;
    public int tankerHitCount = 3;

    // 이 값도 '기본값'이며, 충돌 시 수정됩니다.
    public float knockbackForce = 100f;

    // --- [ 1. (NEW) 퍼센트 감소 변수들 ] ---
    [Header("Tanker Reduction Factors (per hit)")]
    [Tooltip("매 충돌 시 이 값(%)만큼 크기가 작아집니다. (예: 0.85 = 85%로 축소)")]
    [Range(0.1f, 1f)]
    public float scaleReductionFactor = 0.85f;

    [Tooltip("매 충돌 시 이 값(%)만큼 이동 속도가 느려집니다. (예: 0.9 = 90%로 감속)")]
    [Range(0.1f, 1f)]
    public float speedReductionFactor = 0.9f;

    [Tooltip("매 충돌 시 이 값(%)만큼 공격력이 약해집니다. (예: 0.8 = 80%로 감소)")]
    [Range(0.1f, 1f)]
    public float damageReductionFactor = 0.8f;

    [Tooltip("매 충돌 시 이 값(%)만큼 넉백 힘이 약해집니다. (예: 0.9 = 90%로 감소)")]
    [Range(0.1f, 1f)]
    public float knockbackReductionFactor = 0.9f;


    private int currentTankerHits = 0;
    private Transform currentTarget;
    private Rigidbody2D rb;
    private HealthSystem selfHealth;

    void OnEnable()
    {
        HealthSystem.OnPlayerDied += SwitchTargetToCore;
        HealthSystem.OnPlayerRespawned += SwitchTargetToPlayer;
    }

    void OnDisable()
    {
        HealthSystem.OnPlayerDied -= SwitchTargetToCore;
        HealthSystem.OnPlayerRespawned -= SwitchTargetToPlayer;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        selfHealth = GetComponent<HealthSystem>();
        rb.gravityScale = 0;
        rb.linearDamping = 2f;

        if (selfHealth != null)
        {
            rb.mass = 1f + (selfHealth.maxHealth * healthToMassMultiplier);
        }
        else
        {
            rb.mass = 1f;
        }
        currentTankerHits = 0;


        if (HealthSystem.IsPlayerDead)
        {
            SwitchTargetToCore();
        }
        else
        {
            SwitchTargetToPlayer();
        }
    }

    void FixedUpdate()
    {
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 direction = (Vector2)currentTarget.position - rb.position;
        direction.Normalize();

        rb.AddForce(direction * moveSpeed * 10f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f;
    }

    private void SwitchTargetToCore()
    {
        currentTarget = HealthSystem.CoreTransform;
    }

    private void SwitchTargetToPlayer()
    {
        currentTarget = HealthSystem.PlayerTransform;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        HealthSystem targetHealth = other.gameObject.GetComponent<HealthSystem>();
        if (targetHealth == null || targetHealth.entityType == HealthSystem.EntityType.Enemy)
        {
            return;
        }

        if (isTanker)
        {
            targetHealth.TakeDamage(attackDamage);

            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            Rigidbody2D targetRb = other.gameObject.GetComponent<Rigidbody2D>();
            if (targetRb != null && targetRb.bodyType == RigidbodyType2D.Dynamic)
            {
                targetRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
            rb.AddForce(-knockbackDir * knockbackForce, ForceMode2D.Impulse);

            currentTankerHits++;
            if (currentTankerHits >= tankerHitCount)
            {
                if (selfHealth != null) selfHealth.TakeDamage(selfHealth.maxHealth);
                else Destroy(gameObject);
            }
            else
            {

                // 크기 (Scale)
                transform.localScale *= scaleReductionFactor;

                // 이동 속도 (Move Speed)
                moveSpeed *= speedReductionFactor;

                // 공격력 (Attack Damage)
                attackDamage *= damageReductionFactor;

                // 넉백 힘 (Knockback Force)
                knockbackForce *= knockbackReductionFactor;
            }
        }
        else
        {
            if (other.gameObject.transform == currentTarget)
            {
                targetHealth.TakeDamage(attackDamage);
                if (selfHealth != null) selfHealth.TakeDamage(selfHealth.maxHealth);
                else Destroy(gameObject);
            }
        }
    }
    public void ApplyStatModifiers(WaveStatData stats)
    {
        moveSpeed *= stats.speedMultiplier;
        attackDamage *= stats.damageMultiplier;
    }
}