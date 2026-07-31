using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    [Header("Link to assistant")]
    public AssistantController companion;

    // Количество врагов, которые сейчас видят игрока и воюют
    private int activeEnemiesInCombat = 0;

    void Awake()
    {
        // Делаем Синглтон, чтобы легко вызывать из любого скрипта врага
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (companion == null)
            companion = FindAnyObjectByType<AssistantController>();
    }

    // Враг заметил игрока и начал агро
    public void RegisterEnemy()
    {
        activeEnemiesInCombat++;

        // Если это первый враг — включаем тревогу робота
        if (activeEnemiesInCombat == 1 && companion != null)
        {
            companion.SetCombatState(true);
            Debug.Log("БОЙ НАЧАЛСЯ! Робот прячется.");
        }
    }

    // Враг умер или потерял игрока
    public void UnregisterEnemy()
    {
        activeEnemiesInCombat = Mathf.Max(0, activeEnemiesInCombat - 1);

        // Если врагов больше нет — робот возвращается в мирный режим
        if (activeEnemiesInCombat == 0 && companion != null)
        {
            companion.SetCombatState(false);
            Debug.Log("ВСЕ ВРАГИ ПОВЕРЖЕНЫ! Робот выходит из боя.");
        }
    }
}