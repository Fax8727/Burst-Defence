using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Aiming")]
    private Camera mainCam;
    private Vector2 mousePos;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;
    public float baseBulletDamage = 10f;

    [Range(0f, 1f)]
    public float shootSoundVolume = 0.4f;

    private BurstSystem burstSystem;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        rb.gravityScale = 0;

        if (firePoint == null)
        {
            firePoint = transform;
        }

        burstSystem = GetComponent<BurstSystem>();
        if (burstSystem == null)
        {
            Debug.LogWarning("Player 오브젝트에 BurstSystem.cs가 없습니다!");
        }

        if (burstSystem != null)
        {
            burstSystem.SetFirePoint(firePoint);
        }
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
        {
            if (burstSystem != null)
            {
                burstSystem.ActivateBurstSkill();
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (burstSystem != null)
            {
                Debug.Log("Test Key 'B' pressed: Forcing Burst Skill");
                burstSystem.ActivateBurstSkillForTest();
            }
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(moveInput * moveSpeed * 10f);

        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f;
    }

    void Shoot()
    {
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bulletScript = bulletGO.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDamage(baseBulletDamage);
        }
        else
        {
            Debug.LogError("Bullet Prefab에 Bullet.cs 스크립트가 없습니다!");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, shootSoundVolume);
        }
    }
}