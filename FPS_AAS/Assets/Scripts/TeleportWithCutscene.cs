using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Playables;

public class TeleportWithCutscene : MonoBehaviour, IInteractable
{
    [Header("Interactive Settings")]
    public string promptText = "Press E to interact";

    [Header("What and where")]
    public Transform player;
    public Transform teleportTarget;

    [Header("Assistant Settings")]
    public bool disableAssistant = true;
    public GameObject assistantObject;

    [Header("UI Loading")]
    public CanvasGroup loadingScreenGroup;
    public float fadeSpeed = 4f;
    public float loadingDuration = 2f;

    [Header("Cutscene Video")]
    public VideoPlayer cutsceneVideo;
    public GameObject cutscenePanel; // RawImage панель
    public float manualCutsceneDuration = 4f;

    [Header("Player Controller")]
    public MonoBehaviour playerMovementScript;

    [Header("Freeze Environment")]
    public MonoBehaviour[] scriptsToDisableDuringCutscene;

    [Header("Boss")]
    public BossController bossController;
    public BossHealthController bossHealthController;
    public float delayBeforeBoss = 3f;

    private bool isActivating = false;

    public string GetPromptText() => promptText;

    public void Interact()
    {
        if (!isActivating)
            StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        isActivating = true;

        // 1. Блокируем игрока
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        ToggleWorldScripts(false);

        // 2. Fade Out
        if (loadingScreenGroup != null)
        {
            loadingScreenGroup.gameObject.SetActive(true);
            while (loadingScreenGroup.alpha < 1f)
            {
                loadingScreenGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            loadingScreenGroup.alpha = 1f;
        }

        // 3. Экран загрузки
        yield return new WaitForSeconds(loadingDuration);

        // 4. Отключаем ассистента
        if (disableAssistant && assistantObject != null)
            assistantObject.SetActive(false);

        // 5. Телепортация
        TeleportPlayerSafe();

        // 6. Запускаем видео катсцену
        if (cutsceneVideo != null && cutscenePanel != null)
        {
            cutscenePanel.SetActive(true);
            cutsceneVideo.Play();
        }

        // 7. Fade In
        if (loadingScreenGroup != null)
        {
            while (loadingScreenGroup.alpha > 0f)
            {
                loadingScreenGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            loadingScreenGroup.gameObject.SetActive(false);
        }

        // 8. Ждём окончания видео
        if (cutsceneVideo != null)
        {
            while (cutsceneVideo.isPlaying)
            {
                yield return null;
            }
            // скрываем панель с видео
            if (cutscenePanel != null)
                cutscenePanel.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(manualCutsceneDuration);
        }

        // 9. Включаем игрока
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        ToggleWorldScripts(true);

        // 10. Пауза перед боссом
        yield return new WaitForSeconds(delayBeforeBoss);

        // 11. Активируем босса
        if (bossController != null)
            bossController.bossActivated = true;

        if (bossHealthController != null)
            bossHealthController.ActivateBossUI();

        isActivating = false;
    }

    private void ToggleWorldScripts(bool state)
    {
        if (scriptsToDisableDuringCutscene == null) return;
        foreach (MonoBehaviour script in scriptsToDisableDuringCutscene)
        {
            if (script != null) script.enabled = state;
        }
    }

    private void TeleportPlayerSafe()
    {
        if (player == null || teleportTarget == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        UnityEngine.AI.NavMeshAgent agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        player.position = teleportTarget.position;
        player.rotation = teleportTarget.rotation;

        if (cc != null) cc.enabled = true;
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(teleportTarget.position);
        }
    }
}