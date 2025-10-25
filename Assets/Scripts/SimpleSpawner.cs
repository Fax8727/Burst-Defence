using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    public GameObject virusPrefab; // 인스펙터에서 바이러스 프리팹을 끌어다 놓으세요.
    public float spawnRate = 2.0f; // 2초마다 1마리 스폰
    public float spawnRadius = 10f; // 스폰 반경 (코어에서 10 유닛 떨어진 곳)

    void Start()
    {
        // (spawnRate)초마다 "SpawnVirus" 함수를 반복 실행
        InvokeRepeating("SpawnVirus", 1.0f, spawnRate);
    }

    void SpawnVirus()
    {
        if (virusPrefab == null)
        {
            Debug.LogError("Virus Prefab이 할당되지 않았습니다!");
            return;
        }

        // 원형 경기장 가장자리에서 랜덤한 위치에 스폰
        Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnRadius;

        Instantiate(virusPrefab, spawnPos, Quaternion.identity);
    }
}