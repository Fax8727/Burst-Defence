using UnityEngine;
using System.Collections.Generic;

// --- 데이터의 가장 작은 단위 ---
/// <summary>
/// "기본형 5마리", "탱커 2마리" 등
/// 한 번에 '동시'에 스폰할 적의 그룹 정보입니다.
/// </summary>
[System.Serializable]
public class SpawnGroup
{
    public GameObject enemyPrefab;
    public int count;
}

// --- 중간 단위 (1-1, 1-2) ---
/// <summary>
/// "세부 웨이브" (Sub-Wave)입니다.
/// 여러 'SpawnGroup'을 동시에 스폰시키고,
/// 다음 세부 웨이브까지의 대기 시간을 가집니다.
/// </summary>
[System.Serializable]
public class SubWave
{
    [Tooltip("이 세부 웨이브에서 '동시에' 스폰될 적 그룹 목록 (예: 기본형 5, 속도형 3)")]
    public List<SpawnGroup> spawnGroups;

    [Tooltip("이 세부 웨이브가 시작된 후, 다음 세부 웨이브가 시작될 때까지의 시간(초). 적이 남아있어도 이 시간이 지나면 다음으로 넘어갑니다.")]
    public float timeUntilNextSubWave;
}

// --- 가장 큰 단위 (Wave 1 SO, Wave 2 SO) ---
/// <summary>
/// "메인 웨이브 (Wave 1, Wave 2)"의 전체 설계를 담는 스크립터블 오브젝트입니다.
/// </summary>
[CreateAssetMenu(fileName = "WaveConfig_01", menuName = "Burst Defense/Wave Configuration (SO)")]
public class WaveSO : ScriptableObject
{
    [Header("Display")]
    public string waveName; // (UI 표시용)

    [Header("Stats")]
    [Tooltip("이 웨이브의 모든 적에게 적용될 스탯 배율 SO")]
    public WaveStatData waveStats; // (이전에 만든 WaveStatData SO)

    [Header("Spawning Logic")]
    [Tooltip("이 웨이브를 구성하는 '세부 웨이브(Sub-Wave)'의 목록")]
    public List<SubWave> subWaves;
}