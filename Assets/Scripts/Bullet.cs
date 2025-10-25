using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float moveSpeed = 20f;
    public float lifetime = 3.0f;

    // --- [ 1. damage 변수 변경 ] ---
    // public float damage = 10f; // 이 줄을 삭제하거나 주석 처리하고
    private float damage; // 데미지 값을 PlayerController로부터 받아올 변수

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Invoke("DestroyBullet", lifetime);
    }

    void Start()
    {
        rb.linearVelocity = transform.up * moveSpeed;
    }

    // --- [ 2. 데미지를 외부에서 설정하는 함수 추가 ] ---
    /// <summary>
    /// PlayerController가 총알을 생성할 때 이 함수를 호출하여 데미지를 설정합니다.
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // --- [ 3. OnTriggerEnter2D는 수정할 필요 없음 ] ---
    // (이 함수는 'damage' 변수를 그대로 사용하므로 잘 작동합니다)
    void OnTriggerEnter2D(Collider2D other)
    {
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null && health.entityType == HealthSystem.EntityType.Enemy)
        {
            health.TakeDamage(damage);
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}