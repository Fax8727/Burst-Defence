using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class VirusAI : MonoBehaviour
{
    [Header("AI Stats")]
    public float moveSpeed = 30f;
    public float attackDamage = 10f;

    // --- [ 1. (추가) 탱커 전용 설정 ] ---
    [Header("Tanker Settings")]
    [Tooltip("체크하면 '자폭' 대신 '탱커' 로직을 사용합니다.")]
    public bool isTanker = false;

    [Tooltip("탱커가 파괴되기 전까지 공격(충돌)할 수 있는 횟수")]
    public int tankerHitCount = 3;

    [Tooltip("충돌 시 플레이어와 자신을 밀어내는 힘 (Impulse)")]
    public float knockbackForce = 100f;

    [Tooltip("한 번 충돌할 때마다 이 값만큼 이동 속도가 증가합니다.")]
    public float speedIncreasePerHit = 10f;

    [Tooltip("한 번 충돌할 때마다 이 비율만큼 크기가 작아집니다. (예: 0.85 = 85%)")]
    public float shrinkFactorPerHit = 0.85f;

    private int currentTankerHits = 0; // 탱커의 현재 충돌 횟수

    // --- (기존 변수들) ---
    private Transform currentTarget;
    private Rigidbody2D rb;
    private HealthSystem selfHealth;

    // (OnEnable, OnDisable은 동일)
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
        rb.gravityScale = 0;
        rb.linearDamping = 2f;
        selfHealth = GetComponent<HealthSystem>();

        // --- [ 2. (추가) 탱커 히트 카운트 초기화 ] ---
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

    // (FixedUpdate, SwitchTargetToCore, SwitchTargetToPlayer는 동일)
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

    // --- [ 3. (핵심 수정) OnCollisionEnter2D 변경 ] ---
    void OnCollisionEnter2D(Collision2D other)
    {
        // 부딪힌 대상이 나의 현재 타겟(플레이어 또는 코어)인지 확인
        if (other.gameObject.transform == currentTarget)
        {
            // --- [ A. '탱커'일 경우의 로직 ] ---
            if (isTanker)
            {
                // 1. 타겟에게 데미지 주기
                HealthSystem targetHealth = other.gameObject.GetComponent<HealthSystem>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(attackDamage);
                }

                // 2. 폭발적인 넉백(Knockback) 적용
                // 충돌 지점에서 타겟으로 향하는 방향 벡터
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;

                // (a) 타겟(플레이어)에게 넉백 적용 (코어는 Static이라 안 밀려남)
                Rigidbody2D targetRb = other.gameObject.GetComponent<Rigidbody2D>();
                if (targetRb != null && targetRb.bodyType == RigidbodyType2D.Dynamic)
                {
                    targetRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }

                // (b) 나 자신(탱커)에게 반대 방향으로 넉백 적용 (반동)
                rb.AddForce(-knockbackDir * knockbackForce, ForceMode2D.Impulse);

                // 3. 히트 카운트 증가
                currentTankerHits++;

                // 4. 파괴 여부 결정
                if (currentTankerHits >= tankerHitCount)
                {
                    // (a) 3번 다 때렸으면 자폭 (HealthSystem.Die() 호출)
                    if (selfHealth != null)
                    {
                        selfHealth.TakeDamage(selfHealth.maxHealth);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    // (b) 아직 살아있다면 작아지고 빨라짐
                    // 5. 크기 축소
                    transform.localScale *= shrinkFactorPerHit;

                    // 6. 이동 속도 증가
                    moveSpeed += speedIncreasePerHit;
                }
            }
            // --- [ B. '탱커가 아닐' 경우의 기존 로직 (자폭) ] ---
            else
            {
                // 타겟에게 데미지 주기
                HealthSystem targetHealth = other.gameObject.GetComponent<HealthSystem>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(attackDamage);
                }

                // 즉시 자폭
                if (selfHealth != null)
                {
                    selfHealth.TakeDamage(selfHealth.maxHealth);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}