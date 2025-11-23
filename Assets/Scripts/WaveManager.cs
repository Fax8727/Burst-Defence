using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// (WaveSO.cs 파일과 WaveStatData.cs 파일, SpawnIndicator.cs 파일이 있어야 합니다.)

/// <summary>
/// 씬에 존재하며, 'WaveSO' 설계도를 읽어
/// 스폰 인디케이터를 생성하고 게임 흐름을 제어하는 '스폰 매니저'입니다.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [Tooltip("이 스테이지에서 진행할 '메인 웨이브(SO)'의 전체 목록")]
    public List<WaveSO> mainWaves;

    // --- [ 1. 인디케이터 프리팹 변수 ] ---
    [Tooltip("적 스폰 전 경고 표시(빨간 원) 프리팹")]
    public GameObject spawnIndicatorPrefab;

    public float spawnRadius = 12f;
    public float timeBetweenWaves = 5.0f; // (웨이브 클리어 후 다음 웨이브까지 대기 시간)

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0; // 씬에 살아있는 모든 적의 수

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
        StartCoroutine(GameLoop());
    }

    void Update()
    {
        // 모든 로직이 코루틴에 있으므로 비워둠
    }

    // --- (핵심) 게임 전체 흐름을 관리하는 메인 코루틴 ---
    IEnumerator GameLoop()
    {
        // 'mainWaves' 리스트에 있는 모든 웨이브를 순서대로 진행
        while (currentWaveIndex < mainWaves.Count)
        {
            WaveSO currentWaveSO = mainWaves[currentWaveIndex];

            Debug.Log("--- Starting Wave " + (currentWaveIndex + 1) + " (" + currentWaveSO.waveName + ") ---");
            UIManager.Instance?.UpdateWaveText(currentWaveIndex + 1);

            // 1. 이 메인 웨이브에서 스폰될 '총' 적의 수를 미리 계산
            int totalEnemiesInThisWave = 0;
            foreach (SubWave subWave in currentWaveSO.subWaves)
            {
                foreach (SpawnGroup group in subWave.spawnGroups)
                {
                    totalEnemiesInThisWave += group.count;
                }
            }
            enemiesAlive += totalEnemiesInThisWave;
            UIManager.Instance?.UpdateEnemiesLeftText(enemiesAlive);


            // 2. '세부 웨이브' (1-1, 1-2...) 루프 시작
            foreach (SubWave subWave in currentWaveSO.subWaves)
            {
                // 2-1. '동시 스폰' 루프 (SpawnGroup)
                foreach (SpawnGroup group in subWave.spawnGroups)
                {
                    // 2-2. 이 그룹의 적을 'count'만큼 '동시에' 스폰
                    for (int i = 0; i < group.count; i++)
                    {
                        SpawnEnemy(group.enemyPrefab, currentWaveSO.waveStats);
                    }
                }

                // 2-3. 다음 세부 웨이브까지 '시간' 대기
                yield return new WaitForSeconds(subWave.timeUntilNextSubWave);
            }

            // 3. 모든 세부 웨이브 스폰 완료!
            Debug.Log("Wave " + (currentWaveIndex + 1) + " spawn complete. Waiting for clear...");
            while (enemiesAlive > 0)
            {
                yield return null; // 다음 프레임까지 대기
            }

            // 4. (모든 적 사망) 메인 웨이브 클리어!
            Debug.Log("--- Wave " + (currentWaveIndex + 1) + " CLEARED! ---");

            // 5. 마지막 웨이브인지 확인
            if (currentWaveIndex >= mainWaves.Count - 1)
            {
                break; // 'VICTORY!'로 이동
            }

            // 6. 다음 웨이브 대기 (증강 로직 제거됨)
            Debug.Log("Next wave in " + timeBetweenWaves + "s.");
            yield return new WaitForSeconds(timeBetweenWaves);

            // 7. 다음 메인 웨이브로 인덱스 증가
            currentWaveIndex++;
        }

        // --- Victory ---
        Debug.Log("ALL WAVES CLEARED! VICTORY!");
        UIManager.Instance?.UpdateWaveText("VICTORY!");
        UIManager.Instance?.UpdateEnemiesLeftText("", false);
    }

    /// <summary>
    /// 적을 직접 생성하는 대신, 인디케이터를 생성하고 적 정보를 넘겨줍니다.
    /// </summary>
    private void SpawnEnemy(GameObject enemyPrefab, WaveStatData stats)
    {
        if (enemyPrefab == null) return;

        // 인디케이터 프리팹 확인
        if (spawnIndicatorPrefab == null)
        {
            Debug.LogError("Spawn Indicator Prefab이 WaveManager에 연결되지 않았습니다!");
            return;
        }

        // 1. 위치 결정
        Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnRadius;

        // 2. 적 대신 '인디케이터'를 먼저 생성
        GameObject indicatorGO = Instantiate(spawnIndicatorPrefab, spawnPos, Quaternion.identity);

        // 3. 인디케이터에게 "이 적을 1초 뒤에 만들어줘"라고 요청 (정보 전달)
        SpawnIndicator indicatorScript = indicatorGO.GetComponent<SpawnIndicator>();
        if (indicatorScript != null)
        {
            indicatorScript.Setup(enemyPrefab, stats);
        }
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