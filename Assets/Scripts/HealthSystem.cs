using UnityEngine;
using System.Collections; // 코루틴(RespawnPlayer)을 사용하기 위해 필요

/// <summary>
/// 모든 '생명체' (플레이어, 코어, 적)의 체력을 관리하는 중앙 스크립트.
/// 데미지 처리, 사망, 부활 로직을 담당합니다.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    // --- 이벤트 (Event) ---
    // 'static event'는 "게임 전체에 특정 사건이 발생했음"을 방송(Broadcast)하는 기능입니다.
    // 다른 스크립트들(WaveManager, BurstSystem 등)이 이 사건을 "구독(Subscribe)"하여 반응합니다.

    /// <summary>
    /// 적(Enemy)이 죽었을 때 발생하는 이벤트.
    /// (WaveManager가 남은 적 수를 계산하고, BurstSystem이 게이지를 채우기 위해 구독)
    /// </summary>
    public static event System.Action OnEnemyDied;

    /// <summary>
    /// 플레이어가 죽었을 때 발생하는 이벤트.
    /// (VirusAI가 타겟을 코어로 바꾸기 위해 구독)
    /// </summary>
    public static event System.Action OnPlayerDied;

    /// <summary>
    /// 플레이어가 부활했을 때 발생하는 이벤트.
    /// (VirusAI가 타겟을 다시 플레이어로 바꾸기 위해 구독)
    /// </summary>
    public static event System.Action OnPlayerRespawned;

    // --- 타입 정의 ---
    /// <summary>
    /// 이 HealthSystem 컴포넌트가 누구의 것인지 구분하는 타입.
    /// </summary>
    public enum EntityType { Player, Core, Enemy }

    [Header("Entity Type")]
    [Tooltip("이 스크립트가 붙어있는 오브젝트의 타입 (Player, Core, Enemy)")]
    public EntityType entityType = EntityType.Player;

    [Header("Health Stats")]
    [Tooltip("최대 체력")]
    public float maxHealth = 100f;

    private float currentHealth; // 현재 체력 (내부에서만 관리)

    // '프로퍼티' (Property): 다른 스크립트에서 현재 값을 '읽기 전용'으로 가져갈 수 있게 함
    public float CurrentHealth => currentHealth; // 현재 체력 반환
    public bool IsDead => currentHealth <= 0;    // 사망 상태 여부 반환

    [Header("Player Respawn (Player 전용)")]
    [Tooltip("플레이어 사망 시 부활에 걸리는 시간(초)")]
    public float respawnTime = 5.0f;

    // --- 컴포넌트 참조 ---
    // 플레이어 사망/부활 시 비활성화/활성화할 컴포넌트들을 미리 저장
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Rigidbody2D rb;

    // --- 정적 변수 (Static) ---
    // 'static' 변수는 이 스크립트의 인스턴스(객체) 없이도
    // HealthSystem.PlayerTransform 처럼 게임 어디서든 접근할 수 있는 '공용' 변수입니다.

    /// <summary>
    /// VirusAI가 플레이어의 위치를 쉽게 찾을 수 있도록 함
    /// </summary>
    public static Transform PlayerTransform { get; private set; } // 'private set'은 다른 스크립트에서 값을 변경(set)할 수 없게 막음

    /// <summary>
    /// VirusAI가 코어의 위치를 쉽게 찾을 수 있도록 함
    /// </summary>
    public static Transform CoreTransform { get; private set; }

    /// <summary>
    /// VirusAI가 플레이어의 사망 상태를 즉시 알 수 있도록 함
    /// </summary>
    public static bool IsPlayerDead { get; private set; }


    /// <summary>
    /// Awake: Start()보다 먼저, 씬의 모든 오브젝트가 로드될 때 1회 실행됩니다.
    /// 다른 스크립트들이 Start()에서 이 변수들을 참조해야 하므로, Awake()에서 미리 설정합니다.
    /// </summary>
    void Awake()
    {
        // 이 스크립트가 'Player' 타입이면
        if (entityType == EntityType.Player)
        {
            PlayerTransform = transform; // '나(Player)'의 Transform을 공용 변수에 등록
            IsPlayerDead = false;        // 게임 시작 시 플레이어는 살아있음

            // 플레이어의 컴포넌트들을 미리 찾아서 변수에 저장 (매번 찾는 것보다 효율적)
            playerController = GetComponent<PlayerController>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerCollider = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
        }
        // 이 스크립트가 'Core' 타입이면
        else if (entityType == EntityType.Core)
        {
            CoreTransform = transform; // '나(Core)'의 Transform을 공용 변수에 등록
        }
    }

    /// <summary>
    /// Start: Awake() 이후, 첫 번째 프레임이 실행되기 전 1회 실행됩니다.
    /// </summary>
    void Start()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHealth = maxHealth;

        // UIManager가 (싱글톤으로) 씬에 존재하고, 내 타입이 플레이어 또는 코어일 경우
        if (UIManager.Instance != null && (entityType == EntityType.Player || entityType == EntityType.Core))
        {
            // UIManager에게 내 체력바(Slider)를 초기 설정해달라고 요청
            UIManager.Instance.InitializeHealthBar(entityType, maxHealth);
        }
    }

    /// <summary>
    /// 외부에서 이 오브젝트에 데미지를 줄 때 호출하는 공용 함수
    /// </summary>
    /// <param name="damageAmount">입힐 데미지 양</param>
    public void TakeDamage(float damageAmount)
    {
        // 이미 죽었다면(체력이 0 이하라면) 함수 종료 (중복 사망 방지)
        if (IsDead) return;

        // 체력 감소
        currentHealth -= damageAmount;
        // 체력이 0보다 낮아지거나 maxHealth보다 높아지지 않도록 보정
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // 디버깅용: 콘솔에 데미지 로그 출력
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current HP: " + currentHealth);

        // UIManager가 존재하고, 내 타입이 플레이어 또는 코어일 경우
        if (UIManager.Instance != null && (entityType == EntityType.Player || entityType == EntityType.Core))
        {
            // UIManager에게 체력바 UI를 현재 체력으로 업데이트해달라고 요청
            UIManager.Instance.UpdateHealth(entityType, currentHealth, maxHealth);
        }

        // 체력이 0 이하가 되었다면, 사망 처리 함수 호출
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력이 0이 되었을 때 호출되는 내부 사망 처리 함수
    /// </summary>
    private void Die()
    {
        Debug.Log(gameObject.name + " has been destroyed!");

        // --- 1. 'Player'가 죽었을 때 ---
        if (entityType == EntityType.Player)
        {
            // 즉시 게임 오버 대신 '부활' 코루틴을 시작
            StartCoroutine(RespawnPlayer());
        }
        // --- 2. 'Core'가 죽었을 때 ---
        else if (entityType == EntityType.Core)
        {
            // 즉시 게임 오버
            Time.timeScale = 0f; // 게임 시간을 멈춤 (프로토타입용)
            Debug.Log("GAME OVER - Core was destroyed.");
            // (추후) 여기에 게임 오버 UI를 띄우는 로직 추가
            Destroy(gameObject);
        }
        // --- 3. 'Enemy'가 죽었을 때 ---
        else if (entityType == EntityType.Enemy)
        {
            // "적이 죽었다!" 라고 이벤트 방송 (WaveManager, BurstSystem이 이 신호를 받음)
            OnEnemyDied?.Invoke(); // '?'는 구독자가 없을 때 오류가 발생하는 것을 방지

            // 적 오브젝트 파괴
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 플레이어 부활 로직을 처리하는 코루틴 (시간차 로직)
    /// </summary>
    private IEnumerator RespawnPlayer()
    {
        // --- 1. 사망 처리 ---
        IsPlayerDead = true;         // AI가 참조할 공용 변수 업데이트
        OnPlayerDied?.Invoke();      // "플레이어 사망" 이벤트 방송 (VirusAI가 받음)

        Debug.Log("Player died. Respawning in " + respawnTime + " seconds...");

        // 플레이어의 조작/보이기/충돌/물리 기능을 모두 비활성화
        if (playerController) playerController.enabled = false;
        if (spriteRenderer) spriteRenderer.enabled = false;
        if (playerCollider) playerCollider.enabled = false;
        if (rb) rb.linearVelocity = Vector2.zero; // 물리적 움직임 정지
        if (playerController && playerController.firePoint != null)
        {
            playerController.firePoint.gameObject.SetActive(false);
        }
        // --- 2. 'respawnTime' 만큼 대기 ---
        yield return new WaitForSeconds(respawnTime); // 이 줄에서 코루틴이 잠시 멈췄다가 재개됨

        // --- 3. 부활 처리 ---
        Debug.Log("Player respawning!");
        currentHealth = maxHealth; // 체력을 가득 채움

        // UIManager에 체력바 UI 업데이트 요청
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(entityType, currentHealth, maxHealth);
        }

        // 비활성화했던 기능들 다시 활성화
        if (playerController) playerController.enabled = true;
        if (spriteRenderer) spriteRenderer.enabled = true;
        if (playerCollider) playerCollider.enabled = true;
        if (playerController && playerController.firePoint != null)
        {
            playerController.firePoint.gameObject.SetActive(true);
        }
        //플레이어 위치를 (0,-2,0)으로 초기화 시킴
        transform.position = new Vector3(0f, -2f, 0f);

        IsPlayerDead = false;          // AI가 참조할 공용 변수 업데이트
        OnPlayerRespawned?.Invoke();   // "플레이어 부활" 이벤트 방송 (VirusAI가 받음)
    }

    /// <summary>
    /// 테스트용 단축키 (T, Y)
    /// #if UNITY_EDITOR ... #endif : 유니티 에디터에서만 실행되고,
    /// 실제 게임으로 빌드(출시)할 때는 이 코드가 자동으로 제외됩니다.
    /// </summary>
    void Update()
    {
#if UNITY_EDITOR
        // T 키: 플레이어에게 35 데미지
        if (entityType == EntityType.Player && Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(35);
        }

        // Y 키: 코어에게 100 데미지
        if (entityType == EntityType.Core && Input.GetKeyDown(KeyCode.Y))
        {
            TakeDamage(100);
        }
#endif
    }
}