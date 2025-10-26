using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 필요

// "어떤 적(prefab)을 몇 마리(count)나"에 대한 정보
[System.Serializable]
public class EnemySpawnGroup
{
    public GameObject enemyPrefab;  // 이 그룹에서 스폰할 적 프리팹
    public int count;               // 스폰할 적의 수
    public float spawnInterval;     // 이 그룹의 적 스폰 간격 (예: 0.5초)
}

// Wave는 'EnemySpawnGroup'의 리스트(목록)를 가집니다.
[System.Serializable]
public class Wave
{
    public string waveName; // (구분용) "1. Basic Wave"
    public List<EnemySpawnGroup> enemyGroups;
    public float timeBetweenGroups = 1.0f; // 그룹과 그룹 사이의 대기 시간
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<Wave> waves;
    public float spawnRadius = 12f;
    public float timeBetweenWaves = 5.0f; // 웨이브와 웨이브 사이의 대기 시간

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;

    void OnEnable()
    {
        HealthSystem.OnEnemyDied += HandleEnemyDeath;
    }

    void OnDisable()
    {
        HealthSystem.OnEnemyDied -= HandleEnemyDeath;
    }

    // 게임 시작 시 메인 루프 코루틴을 실행
    void Start()
    {
        StartCoroutine(WaveLoop());
    }

    void Update()
    {
        // 모든 로직이 코루틴에 있으므로 Update는 비워둠
    }

    // 메인 게임 흐름을 관리하는 코루틴
    IEnumerator WaveLoop()
    {
        // 1. 메인 웨이브 루프 (예: Wave 1, Wave 2...)
        while (currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];

            Debug.Log("Spawning Wave " + (currentWaveIndex + 1) + " (" + currentWave.waveName + ")");
            UIManager.Instance?.UpdateWaveText(currentWaveIndex + 1);

            // 이 웨이브에서 스폰될 총 적의 수를 미리 계산
            int totalEnemiesInThisWave = 0;
            foreach (EnemySpawnGroup group in currentWave.enemyGroups)
            {
                totalEnemiesInThisWave += group.count;
            }
            enemiesAlive = totalEnemiesInThisWave;
            UIManager.Instance?.UpdateEnemiesLeftText(enemiesAlive);

            // 2. "세부 웨이브" (그룹) 스폰 루프
            foreach (EnemySpawnGroup group in currentWave.enemyGroups)
            {
                if (group.enemyPrefab == null)
                {
                    Debug.LogError("Enemy Prefab이 group에 설정되지 않았습니다!");
                    continue; // 이 그룹은 건너뛰고 다음 그룹으로
                }

                // 2-1. 안쪽 루프: 이 그룹의 적을 'count'만큼 스폰
                for (int i = 0; i < group.count; i++)
                {
                    Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnRadius;
                    Instantiate(group.enemyPrefab, spawnPos, Quaternion.identity);

                    yield return new WaitForSeconds(group.spawnInterval);
                }

                // 2-2. 한 그룹 스폰 완료 후, 다음 그룹 전까지 대기
                // (이때 몬스터 클리어 여부와 상관없이 진행됨)
                yield return new WaitForSeconds(currentWave.timeBetweenGroups);
            }

            // 3. (모든 그룹 스폰 종료) 모든 적이 죽을 때까지 대기
            while (enemiesAlive > 0)
            {
                yield return null; // 다음 프레임까지 대기
            }

            // 4. (모든 적 사망) 웨이브 클리어! 다음 웨이브 전까지 대기
            Debug.Log("Wave " + (currentWaveIndex + 1) + " cleared! Next wave in " + timeBetweenWaves + "s.");
            yield return new WaitForSeconds(timeBetweenWaves);

            // 5. 다음 웨이브로 인덱스 증가
            currentWaveIndex++;
        }

        // --- 6. 모든 웨이브 클리어 (Victory) ---
        Debug.Log("ALL WAVES CLEARED! VICTORY!");
        UIManager.Instance?.UpdateWaveText("VICTORY!");
        UIManager.Instance?.UpdateEnemiesLeftText("", false); // "Enemies: 0" 숨기기
    }

    // 적이 죽을 때마다 호출 (HealthSystem.OnEnemyDied 구독)
    private void HandleEnemyDeath()
    {
        if (enemiesAlive > 0)
        {
            enemiesAlive--;
            UIManager.Instance?.UpdateEnemiesLeftText(enemiesAlive);
        }
    }
}