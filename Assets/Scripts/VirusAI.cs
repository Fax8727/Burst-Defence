using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class VirusAI : MonoBehaviour
{
    [Header("AI Stats")]
    public float moveSpeed = 3f; // 이것도 이제 '가속도'처럼 작동
    public float attackDamage = 10f;

    private Transform currentTarget;
    private Rigidbody2D rb;
    private HealthSystem selfHealth;

    // (OnEnable, OnDisable, Start, Switch 함수들은 동일)
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
        selfHealth = GetComponent<HealthSystem>();

        if (HealthSystem.IsPlayerDead)
        {
            SwitchTargetToCore();
        }
        else
        {
            SwitchTargetToPlayer();
        }
    }

    // --- [ 1. FixedUpdate 수정 (물리 기반 이동) ] ---
    void FixedUpdate()
    {
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero; // 타겟이 없으면 멈춤
            return;
        }

        Vector2 direction = (Vector2)currentTarget.position - rb.position;
        direction.Normalize();

        // 1. 이동: velocity 대신 '힘(Force)'을 가합니다.
        rb.AddForce(direction * moveSpeed * 10f); // 10f는 예시 배율

        // 2. 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f;
    }

    private void SwitchTargetToCore()
    {
        currentTarget = HealthSystem.CoreTransform;
        Debug.Log(gameObject.name + " targets: CORE");
    }

    private void SwitchTargetToPlayer()
    {
        currentTarget = HealthSystem.PlayerTransform;
        Debug.Log(gameObject.name + " targets: PLAYER");
    }

    // --- [ 2. (핵심) OnTriggerEnter2D -> OnCollisionEnter2D로 변경 ] ---
    // 'Collider2D other' -> 'Collision2D other'로 매개변수가 바뀝니다.
    void OnCollisionEnter2D(Collision2D other)
    {
        // 부딪힌 대상이 나의 현재 타겟(플레이어 또는 코어)인지 확인
        // other.transform -> other.gameObject.transform 으로 변경
        if (other.gameObject.transform == currentTarget)
        {
            // 타겟의 HealthSystem을 찾아서 데미지를 줌
            // other.GetComponent -> other.gameObject.GetComponent 로 변경
            HealthSystem targetHealth = other.gameObject.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage);
            }

            // 자폭 로직 (동일)
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