using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    public string waveName;
    public GameObject enemyPrefab;
    public int count;
    public float spawnInterval;
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<Wave> waves;
    public float spawnRadius = 12f;
    public float timeBetweenWaves = 5.0f;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    // 'isSpawning' 플래그는 더 이상 필요 없습니다.

    // --- 이벤트 구독/해제는 동일 ---
    void OnEnable()
    {
        HealthSystem.OnEnemyDied += HandleEnemyDeath;
    }

    void OnDisable()
    {
        HealthSystem.OnEnemyDied -= HandleEnemyDeath;
    }

    // --- Start() 변경 ---
    // Update() 대신, 게임의 전체 흐름을 관리하는 코루틴을 1회 실행
    void Start()
    {
        StartCoroutine(WaveLoop());
    }

    // --- Update() 변경 ---
    // 이제 Update() 함수는 아무 일도 하지 않습니다.
    void Update()
    {
        // 비워둠
    }

    // --- 새로운 메인 코루틴: WaveLoop ---
    IEnumerator WaveLoop()
    {
        // 현재 웨이브 인덱스가 총 웨이브 수보다 적은 동안 계속 반복
        while (currentWaveIndex < waves.Count)
        {
            // --- 1. 웨이브 스폰 ---
            Wave currentWave = waves[currentWaveIndex];

            if (currentWave.enemyPrefab == null)
            {
                Debug.LogError("Enemy Prefab이 wave " + (currentWaveIndex + 1) + "에 설정되지 않았습니다!");
                yield break; // 코루틴 중지
            }

            Debug.Log("Spawning Wave " + (currentWaveIndex + 1));

            // UI 업데이트
            UIManager.Instance?.UpdateWaveText(currentWaveIndex + 1);

            // 살아있는 적 수 설정
            enemiesAlive = currentWave.count;
            UIManager.Instance?.UpdateEnemiesLeftText(enemiesAlive);

            // 적 스폰 시작
            for (int i = 0; i < currentWave.count; i++)
            {
                Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnRadius;
                Instantiate(currentWave.enemyPrefab, spawnPos, Quaternion.identity);

                // 스폰 간격만큼 대기
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }

            // --- 2. 모든 적이 죽을 때까지 대기 ---
            // (isSpawning = false; 코드가 필요 없어짐)
            // enemiesAlive가 0보다 클 동안, 이 코루틴은 매 프레임 여기서 대기함
            while (enemiesAlive > 0)
            {
                yield return null; // 다음 프레임까지 대기
            }

            // --- 3. 웨이브 클리어! 다음 웨이브 전까지 대기 ---
            Debug.Log("Wave " + (currentWaveIndex + 1) + " cleared! Next wave in " + timeBetweenWaves + "s.");
            yield return new WaitForSeconds(timeBetweenWaves);

            // --- 4. 다음 웨이브로 인덱스 증가 ---
            currentWaveIndex++;
        }

        // --- 5. 모든 웨이브 클리어 (Victory) ---
        // while 루프가 끝났다는 것은 모든 웨이브가 끝났다는 의미
        Debug.Log("ALL WAVES CLEARED! VICTORY!");
        UIManager.Instance?.UpdateWaveText("VICTORY!");
        // (선택) 남은 적 UI 숨기기
        UIManager.Instance?.UpdateEnemiesLeftText("", false); // UIManager 수정 필요
    }

    // --- 적 사망 처리 (변경 없음) ---
    private void HandleEnemyDeath()
    {
        if (enemiesAlive > 0)
        {
            enemiesAlive--;
            UIManager.Instance?.UpdateEnemiesLeftText(enemiesAlive);
        }
    }
}