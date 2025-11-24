using UnityEngine;

[CreateAssetMenu(fileName = "WaveStat_01", menuName = "Burst Defense/Wave Stat Data")]
public class WaveStatData : ScriptableObject
{
    [Header("Wave Stat Multipliers (배율)")]

    [Tooltip("체력 배율 (1.0 = 100%, 1.2 = 120%)")]
    public float healthMultiplier = 1f;

    [Tooltip("이동 속도 배율 (1.0 = 100%, 1.1 = 110%)")]
    public float speedMultiplier = 1f;

    [Tooltip("공격력 배율 (1.0 = 100%, 1.3 = 130%)")]
    public float damageMultiplier = 1f;
}