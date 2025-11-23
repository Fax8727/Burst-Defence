using UnityEngine;

// Assets 폴더에서 우클릭 > Create > Burst Defense/Wave Stat Data 로 생성 가능
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

    // (참고: 질량(Mass)은 HealthSystem의 체력에 비례하므로
    // healthMultiplier가 높아지면 자동으로 무거워져서 넉백 저항이 강해집니다.)
}