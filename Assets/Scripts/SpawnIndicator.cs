using UnityEngine;
using System.Collections;

public class SpawnIndicator : MonoBehaviour
{
    [Header("Visuals")]
    public Transform outerCircle;
    public Transform innerCircle;
    public float spawnDelay = 1.0f;

    private GameObject enemyPrefabToSpawn;
    private WaveStatData statsToApply;

    /// <summary>
    /// WaveManager가 호출. 적 정보(Prefab)를 받아옵니다.
    /// </summary>
    public void Setup(GameObject enemyPrefab, WaveStatData stats)
    {
        Debug.Log("SpawnIndicator: Setup 함수가 호출되었습니다!");

        enemyPrefabToSpawn = enemyPrefab;
        statsToApply = stats;

        // --- [ 핵심: 적의 크기에 맞춰 인디케이터 크기 변경 ] ---
        if (enemyPrefab != null)
        {
            // 1. 스폰될 적 프리팹의 크기(Scale)를 가져옵니다.
            // (예: 탱커는 (1.5, 1.5, 1), 속도형은 (0.8, 0.8, 1))
            Vector3 enemyScale = enemyPrefab.transform.localScale;

            // 2. 인디케이터 자신의 크기를 적과 똑같이 맞춥니다.
            // 부모가 커지면 자식인 Outer/Inner Circle도 같이 커집니다.
            transform.localScale = enemyScale;
        }
        // ---------------------------------------------------

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        float timer = 0f;
        Vector3 initialScale = Vector3.zero;

        // OuterCircle은 이미 크기가 고정되어 있으므로 그 크기를 목표로 잡습니다.
        Vector3 targetScale = outerCircle.localScale;

        // InnerCircle은 0에서 시작
        innerCircle.localScale = initialScale;

        while (timer < spawnDelay)
        {
            timer += Time.deltaTime;
            float progress = timer / spawnDelay;

            // InnerCircle만 점점 커지는 애니메이션 (Lerp)
            innerCircle.localScale = Vector3.Lerp(initialScale, targetScale, progress);

            yield return null;
        }

        SpawnEnemy(); // 적 진짜 스폰
        Destroy(gameObject); // 인디케이터 삭제
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabToSpawn != null)
        {
            // 인디케이터의 현재 위치(transform.position)에 적 생성
            GameObject enemyGO = Instantiate(enemyPrefabToSpawn, transform.position, Quaternion.identity);

            if (statsToApply != null)
            {
                HealthSystem hs = enemyGO.GetComponent<HealthSystem>();
                VirusAI ai = enemyGO.GetComponent<VirusAI>();

                if (hs != null) hs.ApplyStatModifiers(statsToApply);
                if (ai != null) ai.ApplyStatModifiers(statsToApply);
            }
        }
    }
}