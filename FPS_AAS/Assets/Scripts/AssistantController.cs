using UnityEngine;
using UnityEngine.AI;


public class AssistantController : MonoBehaviour
{
    [Header("Linkage")]
    public Transform player;
    public Animator anim;

    [Header("Animator Settings")]
    public string isWalkingBool = "IsWalking";
    public float combatAnimSpeed = 1.4f;

    [Header("Position Offsets")]
    [Tooltip("Позиция рядом с игроком (X = вправо/влево, Z = вперед/назад)")]
    public Vector3 peaceOffset = new Vector3(1.5f, 0f, 0.5f);

    [Tooltip("На сколько метров робот отходит за спину во время БОЯ")]
    public float hideDistanceInCombat = 4.5f;

    [Header("Movement Sensitivity")]
    [Tooltip("Минимальное расстояние (в метрах), на которое должен сдвинуться игрок, чтобы робот обновил цель (чтобы игрок мог свободно крутить мышкой)")]
    public float playerMoveThreshold = 0.3f;

    [Header("Speed")]
    public float normalSpeed = 3.5f;
    public float panicRunSpeed = 5.5f;

    [Header("Stuck Insurance")]
    public float teleportDistance = 8f;

    private bool isInCombat = false;
    private NavMeshAgent agent;

    // Переменные для отслеживания движения игрока
    private Vector3 lastPlayerPosition;
    private Vector3 targetDestination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

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

        // --- СТРАХОВКА: Телепорт при застревании ---
        if (Vector3.Distance(transform.position, player.position) > teleportDistance)
        {
            Vector3 warpPosition = player.TransformPoint(peaceOffset);
            agent.Warp(warpPosition);
            targetDestination = warpPosition;
            lastPlayerPosition = player.position;
        }

        if (!isInCombat)
        {
            agent.stoppingDistance = 0.3f;
            agent.speed = normalSpeed;
            if (anim != null) anim.speed = 1.0f;

            // Проверяем: насколько игрок сдвинулся ИМЕННО ПО КООРДИНАТАМ (игнорируя вращение мыши)
            float playerMovedDistance = Vector3.Distance(player.position, lastPlayerPosition);

            if (playerMovedDistance > playerMoveThreshold)
            {
                // Игрок действительно идет — вычисляем новую точку рядом и отправляем туда робота
                targetDestination = player.TransformPoint(peaceOffset);
                agent.SetDestination(targetDestination);

                // Запоминаем новую позицию игрока
                lastPlayerPosition = player.position;
            }
        }
        else
        {
            // === БОЕВОЙ РЕЖИМ ===
            agent.stoppingDistance = 0.5f;
            agent.speed = panicRunSpeed;
            if (anim != null) anim.speed = combatAnimSpeed;

            Vector3 hidePosition = player.position - (player.forward * hideDistanceInCombat);
            agent.SetDestination(hidePosition);
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

    public void SetCombatState(bool inCombat)
    {
        isInCombat = inCombat;
    }
}