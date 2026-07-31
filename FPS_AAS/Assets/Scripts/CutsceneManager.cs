using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance;

    [Header("Cutscenes")]
    public VideoPlayer cutscenePlayer;
    public GameObject cutscenePanel;

    [Header("Videos")]
    public VideoClip introCutscene;
    public VideoClip outroCutscene;

    [Header("Settings")]
    public float fadeSpeed = 2f;
    public CanvasGroup fadePanel;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // играем вступительную катсцену при старте
        StartCoroutine(PlayIntro());
    }

    // вступительная катсцена
    IEnumerator PlayIntro()
    {
        // блокируем игрока
        if (PlayerController.instance != null)
            PlayerController.instance.enabled = false;

        yield return StartCoroutine(PlayCutscene(introCutscene));

        // разблокируем игрока
        if (PlayerController.instance != null)
            PlayerController.instance.enabled = true;
    }

    // финальная катсцена — вызывается когда босс умирает
    public void PlayOutro()
    {
        StartCoroutine(PlayOutroSequence());
    }

    IEnumerator PlayOutroSequence()
    {
        // блокируем игрока
        if (PlayerController.instance != null)
            PlayerController.instance.enabled = false;

        yield return StartCoroutine(PlayCutscene(outroCutscene));

        // после финальной катсцены загружаем главное меню
        SceneManager.LoadScene("MainMenu");
    }

    // общий метод воспроизведения
    IEnumerator PlayCutscene(VideoClip clip)
    {
        if (clip == null || cutscenePlayer == null) yield break;

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        // fade in
        if (fadePanel != null)
        {
            fadePanel.alpha = 1f;
            while (fadePanel.alpha > 0f)
            {
                fadePanel.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        // ставим клип
        cutscenePlayer.clip = clip;
        cutscenePlayer.Prepare();

        // ждём пока видео подготовится
        while (!cutscenePlayer.isPrepared)
        {
            yield return null;
        }

        cutscenePlayer.Play();

        // небольшая задержка чтобы isPlaying успел стать true
        yield return new WaitForSeconds(0.5f);

        // ждём окончания
        while (cutscenePlayer.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                cutscenePlayer.Stop();
                break;
            }
            yield return null;
        }

        // скрываем панель
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        if (fadePanel != null)
            fadePanel.alpha = 0f;
    }
}