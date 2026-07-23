using UnityEngine;
using System.Collections;

public class NetworkNode : MonoBehaviour, IInteractable
{
    [Header("Effects")]
    public ParticleSystem sparkEffect;      // Искры (быстро гаснут)
    public ParticleSystem smokeEffect;      // Дым (остается и уходит со временем)
    public AudioSource audioSource;
    public AudioClip destroySound;

    [Header("Visual")]
    public Renderer nodeRenderer;
    public Material damagedMaterial;        // Тёмный сгоревший материал

    [Header("Light")]
    public Light nodeLight;                 // Свет шкафа

    private bool isDestroyed = false;

    public string GetPromptText()
    {
        // Если уже сломан, подсказку больше не показываем
        return isDestroyed ? "" : "Press E to disable network node";
    }

    public void Interact()
    {
        if (isDestroyed) return;
        StartCoroutine(DestroyNode());
    }

    IEnumerator DestroyNode()
    {
        isDestroyed = true;

        if (sparkEffect != null)
            sparkEffect.Play();

        if (smokeEffect != null)
            smokeEffect.Play();

        if (audioSource != null && destroySound != null)
            audioSource.PlayOneShot(destroySound);

        if (nodeLight != null)
            nodeLight.color = Color.red;

        // Плавное изменение цвета материала (например, в течение 1 секунды)
        if (nodeRenderer != null && damagedMaterial != null)
        {
            // Получаем текущий цвет материала и целевой цвет сгоревшего
            Color startColor = nodeRenderer.material.color;
            Color targetColor = damagedMaterial.color; // или цвета, который задан в damagedMaterial

            float duration = 1.0f; // Время плавного перехода в секундах
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Плавный переход от одного цвета к другому
                nodeRenderer.material.color = Color.Lerp(startColor, targetColor, t);

                yield return null; // Ждем следующий кадр
            }

            // На всякий случай устанавливаем финальный цвет точно в конце
            nodeRenderer.material.color = targetColor;
        }

        // Выключаем искры, но дым оставляем
        if (sparkEffect != null)
            sparkEffect.Stop();

        if (nodeLight != null)
            nodeLight.intensity = 0;

        if (ObjectiveManager.instance != null)
            ObjectiveManager.instance.NodeDestroyed();

        if (UIController.instance != null && UIController.instance.interactPromptText != null)
            UIController.instance.interactPromptText.text = "";

        yield return new WaitForSeconds(1.5f);

        if (smokeEffect != null)
        {
            smokeEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}