using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f; // 이제 '최대 속도'가 아닌 '가속도'처럼 작동
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // ... (Aiming, Shooting, BurstSystem 변수들은 동일) ...
    [Header("Aiming")]
    private Camera mainCam;
    private Vector2 mousePos;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;
    public float baseBulletDamage = 10f;

    private BurstSystem burstSystem;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        rb.gravityScale = 0;

        // ... (firePoint, burstSystem 설정 코드는 동일) ...
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
        // --- [ 1. 이동 입력을 'Update'에서 받도록 변경 (FixedUpdate보다 정확) ] ---
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // (조준 및 발사 로직은 동일)
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

    // --- [ 2. FixedUpdate 수정 (물리 기반 이동) ] ---
    void FixedUpdate()
    {
        // 1. 이동: velocity를 직접 설정하는 대신 '힘(Force)'을 가합니다.
        // moveSpeed * 10f (또는 50f) 처럼 더 큰 힘이 필요할 수 있습니다.
        rb.AddForce(moveInput * moveSpeed * 10f); // 10f는 예시 배율입니다.

        // 2. 조준 (회전)
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f;
    }

    void Shoot()
    {
        // (Shoot 함수는 동일)
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
    }
}