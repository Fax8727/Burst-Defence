using UnityEngine;
using System.Collections;

public class BurstSystem : MonoBehaviour
{
    [Header("Gauge Settings")]
    public float maxBurstGauge = 100f;
    public float gaugePerKill = 10f;
    private float currentBurstGauge;

    [Header("Skill Settings")]
    public GameObject burstProjectilePrefab;
    public int projectileCount = 12;
    public float spreadAngle = 90f;
    public int burstCount = 3;
    public float timeBetweenBursts = 0.5f;

    // --- [ 1. (중요) Burst 스킬 총알의 데미지 변수 추가 ] ---
    // (인스펙터에서 이 값을 조절할 수 있습니다!)
    public float burstProjectileDamage = 30f;

    private bool isBursting = false;
    private Transform firePoint;
    public bool IsGaugeFull => currentBurstGauge >= maxBurstGauge;

    // (OnEnable, OnDisable, Start, SetFirePoint, HandleEnemyDeath 함수는 동일)
    void OnEnable()
    {
        HealthSystem.OnEnemyDied += HandleEnemyDeath;
    }

    void OnDisable()
    {
        HealthSystem.OnEnemyDied -= HandleEnemyDeath;
    }

    void Start()
    {
        currentBurstGauge = 0;
        UIManager.Instance?.InitializeBurstGauge(maxBurstGauge);
    }

    public void SetFirePoint(Transform newFirePoint)
    {
        firePoint = newFirePoint;
    }

    private void HandleEnemyDeath()
    {
        if (IsGaugeFull) return;

        currentBurstGauge += gaugePerKill;
        currentBurstGauge = Mathf.Clamp(currentBurstGauge, 0f, maxBurstGauge);
        UIManager.Instance?.UpdateBurstGauge(currentBurstGauge);

        if (IsGaugeFull)
        {
            Debug.Log("BURST SKILL READY!");
        }
    }

    public void ActivateBurstSkill()
    {
        if (!IsGaugeFull || isBursting)
        {
            if (isBursting) Debug.Log("스킬이 이미 시전 중입니다!");
            else Debug.Log("Burst 게이지 부족!");
            return;
        }

        if (firePoint == null || burstProjectilePrefab == null)
        {
            Debug.LogError("BurstSystem에 FirePoint 또는 Projectile Prefab이 없습니다!");
            return;
        }

        Debug.Log("BURST SKILL ACTIVATED! (3-Shot)");

        currentBurstGauge = 0;
        UIManager.Instance?.UpdateBurstGauge(currentBurstGauge);

        StartCoroutine(BurstFireCoroutine());
    }

    IEnumerator BurstFireCoroutine()
    {
        isBursting = true;

        float angleStep = spreadAngle / (projectileCount - 1);
        float startAngle = -spreadAngle / 2f;

        for (int b = 0; b < burstCount; b++)
        {
            // 샷건 발사 로직
            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngle = startAngle + (i * angleStep);
                Quaternion spreadRotation = Quaternion.Euler(0, 0, currentAngle);
                Quaternion finalRotation = firePoint.rotation * spreadRotation;

                // 1. Burst 총알 생성
                GameObject projectileGO = Instantiate(burstProjectilePrefab, firePoint.position, finalRotation);

                // 2. 생성된 총알의 'Bullet' 스크립트를 가져옴
                Bullet bulletScript = projectileGO.GetComponent<Bullet>();

                // 3. 'Bullet' 스크립트가 있다면, 데미지 값을 설정
                if (bulletScript != null)
                {
                    // 'burstProjectileDamage' 변수 값을 총알에 전달
                    bulletScript.SetDamage(burstProjectileDamage);
                }
                else
                {
                    Debug.LogError("Burst Projectile Prefab에 Bullet.cs 스크립트가 없습니다!");
                }
            }

            yield return new WaitForSeconds(timeBetweenBursts);
        }

        isBursting = false;
    }

    public void ActivateBurstSkillForTest()
    {
        if (isBursting)
        {
            Debug.Log("TEST: 스킬이 이미 시전 중입니다!");
            return;
        }

        if (firePoint == null || burstProjectilePrefab == null)
        {
            Debug.LogError("TEST: BurstSystem에 FirePoint 또는 Projectile Prefab이 없습니다!");
            return;
        }

        Debug.Log("--- TEST BURST SKILL ACTIVATED (B Key) ---");
        StartCoroutine(BurstFireCoroutine());
    }
}