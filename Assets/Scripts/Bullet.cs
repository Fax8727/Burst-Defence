using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float moveSpeed = 20f;
    public float lifetime = 3.0f;

    private float damage;
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

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        HealthSystem health = other.gameObject.GetComponent<HealthSystem>();

        if (health != null)
        {
            if (health.entityType == HealthSystem.EntityType.Enemy)
            {
                health.TakeDamage(damage);
                DestroyBullet();
            }
            else if (health.entityType == HealthSystem.EntityType.Core)
            {
                DestroyBullet();
            }
            else if (health.entityType == HealthSystem.EntityType.Player)
            {
                DestroyBullet();
            }
        }
        else
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}