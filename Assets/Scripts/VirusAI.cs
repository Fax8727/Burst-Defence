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

    // (기존 Tanker Settings의 speedIncreasePerHit 등은 삭제됨)

    private int currentTankerHits = 0;
    private Transform currentTarget;
    private Rigidbody2D rb;
    private HealthSystem selfHealth;

    // (OnEnable, OnDisable, Start, FixedUpdate, Switch... 함수들은 모두 동일)
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

        // 중요: 프리팹이 생성될 때 인스펙터의 '기본값'을 사용하므로
        // 이 스크립트의 moveSpeed, attackDamage 등은 수정할 필요 없이 
        // 프리팹이 가진 값을 그대로 사용합니다.

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

        // 현재 'moveSpeed' 변수 값을 사용 (이 값은 충돌 시 감소함)
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

    // --- [ 2. (핵심 수정) OnCollisionEnter2D ] ---
    void OnCollisionEnter2D(Collision2D other)
    {
        HealthSystem targetHealth = other.gameObject.GetComponent<HealthSystem>();
        if (targetHealth == null || targetHealth.entityType == HealthSystem.EntityType.Enemy)
        {
            return;
        }

        if (isTanker)
        {
            // 1. 현재 'attackDamage'로 데미지를 줍니다.
            targetHealth.TakeDamage(attackDamage);

            // 2. 현재 'knockbackForce'로 넉백을 줍니다.
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            Rigidbody2D targetRb = other.gameObject.GetComponent<Rigidbody2D>();
            if (targetRb != null && targetRb.bodyType == RigidbodyType2D.Dynamic)
            {
                targetRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
            rb.AddForce(-knockbackDir * knockbackForce, ForceMode2D.Impulse);

            // 3. 히트 카운트 증가
            currentTankerHits++;
            if (currentTankerHits >= tankerHitCount)
            {
                // (a) 3번 다 때렸으면 자폭
                if (selfHealth != null) selfHealth.TakeDamage(selfHealth.maxHealth);
                else Destroy(gameObject);
            }
            else
            {
                // (b) 아직 살아있다면: 4가지 스탯을 모두 퍼센트로 감소시킴

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
        else // 탱커가 아닐 경우 (자폭)
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
        // (예: 기본 50 * 배율 1.1 = 55)
        moveSpeed *= stats.speedMultiplier;

        // (예: 기본 10 * 배율 1.3 = 13)
        attackDamage *= stats.damageMultiplier;

        // (참고: 탱커의 넉백, 데미지 감소 등도 이 함수에서 배율을 적용할 수 있습니다)
        // knockbackForce *= stats.knockbackMultiplier; 
    }
}