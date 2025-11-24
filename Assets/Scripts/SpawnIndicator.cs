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

    public void Setup(GameObject enemyPrefab, WaveStatData stats)
    {
        Debug.Log("SpawnIndicator: Setup 함수가 호출되었습니다!");

        enemyPrefabToSpawn = enemyPrefab;
        statsToApply = stats;

        if (enemyPrefab != null)
        {
            Vector3 enemyScale = enemyPrefab.transform.localScale;
            transform.localScale = enemyScale;
        }
        // ---------------------------------------------------

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        float timer = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = outerCircle.localScale;

        innerCircle.localScale = initialScale;

        while (timer < spawnDelay)
        {
            timer += Time.deltaTime;
            float progress = timer / spawnDelay;

            innerCircle.localScale = Vector3.Lerp(initialScale, targetScale, progress);

            yield return null;
        }

        SpawnEnemy();
        Destroy(gameObject);
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabToSpawn != null)
        {
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