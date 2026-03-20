using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StageSummaryController : MonoBehaviour
{
    [Header("Stage Config")]
    public int currentStageIndex;

    [Header("UI Components")]
    public CanvasGroup uiCanvasGroup;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nodeText;

    [Header("Star Rating System")]
    public Image[] starRatingList;
    public int totalNodesInLevel = 10;
    private int starsEarned = 0;

    [Header("Visual Settings")]
    public Color activeStarColor = Color.white;
    public Color inactiveStarColor = new Color(1, 1, 1, 0.2f);
    public float delayBetweenStars = 0.5f;

    [Header("Audio")]
    public AudioSource summaryAudioSource;
    public AudioClip congratulationClip;
    public AudioClip starPopClip;
    public AudioClip nodeActivateClip;

    [Header("Navigation")]
    public Button nextStageButton;
    public Button retryButton;
    public Button menuButton;
    public string nextSceneName = "Stage_2";
    public float fadeSpeed = 2f;

    private void Start()
    {
        if (summaryAudioSource != null)
            summaryAudioSource.ignoreListenerPause = true;

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);

        nextStageButton?.onClick.AddListener(OnNextStageClicked);
        retryButton?.onClick.AddListener(OnRetryClicked);
        menuButton?.onClick.AddListener(OnMenuClicked);
    }

    public void StartSummarySequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GlobalAudioManager.Instance?.StopAllGameplayAudio();

        gameObject.SetActive(true);

        ResetConstellationUI();
        ResetStars();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isRunActive = false;

            // 🔥 PAKAI SNAPSHOT (FIX UTAMA)
            int score = GameManager.Instance.lastStageScore;
            float time = GameManager.Instance.lastStageTime;
            int collected = GameManager.Instance.lastStageNodes;

            if (timeText)
                timeText.text = FormatTime(time);

            if (scoreText)
                scoreText.text = $"Score: {score}";

            if (nodeText)
                nodeText.text = $"{collected} Stars Collected";

            CalculateStarRating(collected);
        }

        Time.timeScale = 0f;

        StartCoroutine(SummaryRoutine());
    }

    private void ResetStars()
    {
        foreach (var star in starRatingList)
        {
            if (star != null)
                star.color = inactiveStarColor;
        }
    }

    private string FormatTime(float time)
    {
        int m = Mathf.FloorToInt(time / 60);
        int s = Mathf.FloorToInt(time % 60);
        return $"{m:00}:{s:00}";
    }

    private void ResetConstellationUI()
    {
        if (ConstellationManager.Instance == null) return;

        foreach (var node in ConstellationManager.Instance.uiNodes)
        {
            node?.ResetUI();
        }
    }

    private void CalculateStarRating(int collected)
    {
        if (totalNodesInLevel <= 0)
        {
            starsEarned = 1;
            return;
        }

        float percentage = (float)collected / totalNodesInLevel;

        if (percentage >= 0.9f)
            starsEarned = 3;
        else if (percentage >= 0.5f)
            starsEarned = 2;
        else
            starsEarned = 1;
    }

    private IEnumerator SummaryRoutine()
    {
        float timer = 0;

        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * fadeSpeed;

            if (uiCanvasGroup)
                uiCanvasGroup.alpha = Mathf.Lerp(0, 1, timer);

            yield return null;
        }

        if (uiCanvasGroup)
        {
            uiCanvasGroup.alpha = 1;
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }

        yield return new WaitForSecondsRealtime(0.25f);

        if (summaryAudioSource && congratulationClip)
        {
            summaryAudioSource.PlayOneShot(congratulationClip);
            yield return new WaitForSecondsRealtime(congratulationClip.length);
        }

        yield return StartCoroutine(PlayConstellationSequence());
        yield return new WaitForSecondsRealtime(0.2f);
        yield return StartCoroutine(PlayStarRatingSequence());
    }

    private IEnumerator PlayConstellationSequence()
    {
        if (ConstellationManager.Instance == null)
            yield break;

        var collected = ConstellationManager.Instance.collectedNodes;

        foreach (string id in collected)
        {
            var node = ConstellationManager.Instance.uiNodes
                .Find(x => x != null && x.nodeID == id);

            if (node != null)
            {
                node.ActivateNodeAnimated();

                if (summaryAudioSource && nodeActivateClip)
                {
                    summaryAudioSource.PlayOneShot(nodeActivateClip);
                    yield return new WaitForSecondsRealtime(nodeActivateClip.length);
                }
                else
                {
                    yield return new WaitForSecondsRealtime(0.2f);
                }
            }
        }
    }

    private IEnumerator PlayStarRatingSequence()
    {
        for (int i = 0; i < starRatingList.Length; i++)
        {
            if (i < starsEarned && starRatingList[i] != null)
            {
                starRatingList[i].color = activeStarColor;

                if (summaryAudioSource && starPopClip)
                    summaryAudioSource.PlayOneShot(starPopClip);

                yield return new WaitForSecondsRealtime(delayBetweenStars);
            }
        }
    }

    private void OnNextStageClicked()
    {
        if (GameManager.Instance != null)
        {
            if (nextSceneName == "VictoryScreen" || nextSceneName == "GameOverScene")
                GameManager.Instance.FinishGame();
        }

        PrepareForSceneLoad(nextSceneName);
    }

    private void OnRetryClicked()
    {
        if (GameManager.Instance != null)
        {
            
            GameManager.Instance.ResetRunAccumulation();
            GameManager.Instance.ResetLevelStats();
            GameManager.Instance.StartNewStageRun();
        }

        PrepareForSceneLoad(SceneManager.GetActiveScene().name);
    }

    private void OnMenuClicked()
    {
        GameManager.Instance?.ResetLevelStats();
        PrepareForSceneLoad("MainMenu");
    }

    private void PrepareForSceneLoad(string sceneName)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(sceneName);
    }
}