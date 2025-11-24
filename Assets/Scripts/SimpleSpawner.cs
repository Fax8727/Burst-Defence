using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    public GameObject virusPrefab;
    public float spawnRate = 2.0f;
    public float spawnRadius = 10f;

    void Start()
    {
        InvokeRepeating("SpawnVirus", 1.0f, spawnRate);
    }

    void SpawnVirus()
    {
        if (virusPrefab == null)
        {
            Debug.LogError("Virus Prefab이 할당되지 않았습니다!");
            return;
        }

        Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnRadius;

        Instantiate(virusPrefab, spawnPos, Quaternion.identity);
    }
}