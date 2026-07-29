using UnityEngine;
using UnityEngine.AI;

public class AssistantController : MonoBehaviour
{
    [Header("Linkage")]
    public Transform player;
    public Animator anim;

    [Header("Animator Settings")]
    public string isWalkingBool = "IsWalking";

    [Header("Position Offsets")]
    [Tooltip("Позиция рядом с игроком (X = вправо/влево, Z = вперед/назад)")]
    public Vector3 peaceOffset = new Vector3(1.5f, 0f, 0.5f);

    [Header("Movement Sensitivity")]
    [Tooltip("Минимальное расстояние (в метрах), на которое должен сдвинуться игрок, чтобы робот обновил цель")]
    public float playerMoveThreshold = 0.3f;

    [Header("Speed")]
    public float normalSpeed = 3.5f;

    [Header("Stuck Insurance")]
    public float teleportDistance = 8f;

    private NavMeshAgent agent;

    // Переменные для отслеживания движения игрока
    private Vector3 lastPlayerPosition;
    private Vector3 targetDestination;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            lastPlayerPosition = player.position;
            targetDestination = player.TransformPoint(peaceOffset);
        }

        agent.speed = normalSpeed;
    }

    void Update()
    {
        if (player == null) return;

        // --- СТРАХОВКА: Телепорт при застревании в мирное время ---
        if (Vector3.Distance(transform.position, player.position) > teleportDistance)
        {
            TeleportToPlayer();
        }

        // Логика следования за игроком
        agent.stoppingDistance = 0.3f;
        agent.speed = normalSpeed;
        if (anim != null) anim.speed = 1.0f;

        float playerMovedDistance = Vector3.Distance(player.position, lastPlayerPosition);

        if (playerMovedDistance > playerMoveThreshold)
        {
            targetDestination = player.TransformPoint(peaceOffset);
            agent.SetDestination(targetDestination);
            lastPlayerPosition = player.position;
        }

        // --- УПРАВЛЕНИЕ АНИМАЦИЕЙ ---
        if (anim != null)
        {
            bool isMoving = agent.velocity.sqrMagnitude > 0.1f && agent.remainingDistance > agent.stoppingDistance;
            anim.SetBool(isWalkingBool, isMoving);
        }
    }

    void OnDisable()
    {
        if (anim != null) anim.speed = 1.0f;
    }

    /// <summary>
    /// Переключение боевого режима
    /// </summary>
    public void SetCombatState(bool inCombat)
    {
        if (inCombat)
        {
            // ПРЯЧЕМ: Отключаем весь GameObject робота во время боя
            gameObject.SetActive(false);
        }
        else
        {
            // ВОЗВРАЩАЕМ: Включаем обратно
            gameObject.SetActive(true);

            // Появляемся сразу сбоку от игрока, чтобы не бежать из старого места
            TeleportToPlayer();
        }
    }

    private void TeleportToPlayer()
    {
        if (player == null || agent == null) return;

        Vector3 warpPosition = player.TransformPoint(peaceOffset);
        agent.Warp(warpPosition);
        targetDestination = warpPosition;
        lastPlayerPosition = player.position;
    }
}