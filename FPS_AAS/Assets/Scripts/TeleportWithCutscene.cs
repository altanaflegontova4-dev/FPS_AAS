using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class TeleportWithCutscene : MonoBehaviour, IInteractable
{
    [Header("Interactive Settings")]
    public string promptText = "Press E to interact";

    [Header("What and where")]
    public Transform player;
    public Transform teleportTarget;

    [Header("UI Loading")]
    public CanvasGroup loadingScreenGroup;
    public float fadeSpeed = 4f;
    public float loadingDuration = 2f;

    [Header("Cutscene")]
    public PlayableDirector cutsceneDirector;
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

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact()
    {
        if (!isActivating)
        {
            StartCoroutine(TeleportSequence());
        }
    }

    private IEnumerator TeleportSequence()
    {
        isActivating = true;

        // 1. Блокируем игрока
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        ToggleWorldScripts(false);

        // 2. Плавное затемнение (Fade Out)
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

        // 4. Телепортация
        TeleportPlayerSafe();

        // 5. Запускаем катсцену
        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();
        }

        // 6. Плавное появление (Fade In)
        if (loadingScreenGroup != null)
        {
            while (loadingScreenGroup.alpha > 0f)
            {
                loadingScreenGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            loadingScreenGroup.gameObject.SetActive(false);
        }

        // 7. Ждем окончания катсцены
        if (cutsceneDirector != null)
        {
            while (cutsceneDirector.state == PlayState.Playing)
            {
                yield return null;
            }
        }
        else if (manualCutsceneDuration > 0f)
        {
            yield return new WaitForSeconds(manualCutsceneDuration);
        }

        // 8. Включаем игрока обратно
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        ToggleWorldScripts(true);

        // 9. Пауза перед активацией босса — игрок готовится
        yield return new WaitForSeconds(delayBeforeBoss);

        // 10. Активируем босса
        if (bossController != null)
        {
            bossController.bossActivated = true;
            Debug.Log("Босс активирован! Бой начинается!");
        }

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