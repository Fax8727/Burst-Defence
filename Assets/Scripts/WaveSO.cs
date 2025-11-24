using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnGroup
{
    public GameObject enemyPrefab;
    public int count;
}

[System.Serializable]
public class SubWave
{
    [Tooltip("이 세부 웨이브에서 '동시에' 스폰될 적 그룹 목록 (예: 기본형 5, 속도형 3)")]
    public List<SpawnGroup> spawnGroups;

    [Tooltip("이 세부 웨이브가 시작된 후, 다음 세부 웨이브가 시작될 때까지의 시간(초). 적이 남아있어도 이 시간이 지나면 다음으로 넘어갑니다.")]
    public float timeUntilNextSubWave;
}

[CreateAssetMenu(fileName = "WaveConfig_01", menuName = "Burst Defense/Wave Configuration (SO)")]
public class WaveSO : ScriptableObject
{
    [Header("Display")]
    public string waveName;

    [Header("Stats")]
    [Tooltip("이 웨이브의 모든 적에게 적용될 스탯 배율 SO")]
    public WaveStatData waveStats;

    [Header("Spawning Logic")]
    [Tooltip("이 웨이브를 구성하는 '세부 웨이브(Sub-Wave)'의 목록")]
    public List<SubWave> subWaves;
}