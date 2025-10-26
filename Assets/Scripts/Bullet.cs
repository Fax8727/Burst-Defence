using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float moveSpeed = 20f;
    public float lifetime = 3.0f;

    // 넉백 변수가 더 이상 필요 없으므로 삭제
    // public float knockbackForce = 50f; 

    private float damage;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Kinematic + Continuous Collision Detection을 권장

        Invoke("DestroyBullet", lifetime);
    }

    void Start()
    {
        rb.linearVelocity = transform.up * moveSpeed;
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // --- [ (핵심 수정) OnTriggerEnter2D -> OnCollisionEnter2D ] ---
    // 총알(Collider)이 다른 Collider와 '물리 충돌'했을 때
    // 매개변수가 'Collider2D'에서 'Collision2D'로 바뀝니다.
    void OnCollisionEnter2D(Collision2D other)
    {
        // 부딪힌 대상의 GameObject에서 HealthSystem을 찾습니다.
        HealthSystem health = other.gameObject.GetComponent<HealthSystem>();

        if (health != null)
        {
            // 1. 대상이 'Enemy'일 경우: 데미지를 주고 파괴
            // (넉백은 물리 엔진이 자동으로 처리)
            if (health.entityType == HealthSystem.EntityType.Enemy)
            {
                health.TakeDamage(damage);
                DestroyBullet();
            }
            // 2. 대상이 'Core'일 경우: 그냥 파괴 (관통 방지)
            else if (health.entityType == HealthSystem.EntityType.Core)
            {
                DestroyBullet();
            }
            // (플레이어와 부딪혀도 파괴)
            else if (health.entityType == HealthSystem.EntityType.Player)
            {
                DestroyBullet();
            }
        }
        else
        {
            // HealthSystem이 없는 대상(예: 미래에 만들 '벽')에 부딪혀도 파괴
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}