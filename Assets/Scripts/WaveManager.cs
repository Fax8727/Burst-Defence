using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    public List<WaveSO> mainWaves;

    public GameObject spawnIndicatorPrefab;

    public float spawnRadius = 12f;
    public float timeBetweenWaves = 5.0f;

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

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    void Update()
    {
    }

    IEnumerator GameLoop()
    {
        while (currentWaveIndex < mainWaves.Count)
        {
            WaveSO currentWaveSO = mainWaves[currentWaveIndex];

            Debug.Log("--- Starting Wave " + (currentWaveIndex + 1) + " (" + currentWaveSO.waveName + ") ---");
            UIManager.Instance?.UpdateWaveText(currentWaveIndex + 1);

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

            foreach (SubWave subWave in currentWaveSO.subWaves)
            {
                foreach (SpawnGroup group in subWave.spawnGroups)
                {
                    for (int i = 0; i < group.count; i++)
                    {
                        SpawnEnemy(group.enemyPrefab, currentWaveSO.waveStats);
                    }
                }

                yield return new WaitForSeconds(subWave.timeUntilNextSubWave);
            }

            Debug.Log("Wave " + (currentWaveIndex + 1) + " spawn complete. Waiting for clear...");
            while (enemiesAlive > 0)
            {
                yield return null;
            }

            Debug.Log("--- Wave " + (currentWaveIndex + 1) + " CLEARED! ---");

            if (currentWaveIndex >= mainWaves.Count - 1)
            {
                break;
            }

            Debug.Log("Next wave in " + timeBetweenWaves + "s.");
            yield return new WaitForSeconds(timeBetweenWaves);

            currentWaveIndex++;
        }

        Debug.Log("ALL WAVES CLEARED!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerVictory();
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab, WaveStatData stats)
    {
        if (enemyPrefab == null) return;

        if (spawnIndicatorPrefab == null)
        {
            Debug.LogError("Spawn Indicator Prefab이 WaveManager에 연결되지 않았습니다!");
            return;
        }

        Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnRadius;

        GameObject indicatorGO = Instantiate(spawnIndicatorPrefab, spawnPos, Quaternion.identity);

        SpawnIndicator indicatorScript = indicatorGO.GetComponent<SpawnIndicator>();
        if (indicatorScript != null)
        {
            indicatorScript.Setup(enemyPrefab, stats);
        }
    }

    private void HandleEnemyDeath()
    {
        if (enemiesAlive > 0)
        {
            enemiesAlive--;
            UIManager.Instance?.UpdateEnemiesLeftText(enemiesAlive);
        }
    }
}