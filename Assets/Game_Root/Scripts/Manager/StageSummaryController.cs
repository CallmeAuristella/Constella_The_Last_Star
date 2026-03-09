using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageSummaryController : MonoBehaviour
{
    [Header("Stage Config")]
    public int currentStageIndex;

    [Header("UI Components")]
    public CanvasGroup uiCanvasGroup;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nodeText;

    // --- LOGIKA ARRAY STARMAP BALIK LAGI ---
    [Header("Starmap HUD System")]
    public Image[] starmapParts; // Masukkan potongan rasi bintang di sini
    public Color starmapActiveColor = Color.white;
    public Color starmapInactiveColor = new Color(1, 1, 1, 0.1f);
    public float delayBetweenStarmapParts = 0.2f;

    [Header("Star Rating System")]
    public Image[] starRatingList;
    public int totalNodesInLevel = 10;
    private int starsEarned = 0;

    [Header("Visual Settings")]
    public Color activeStarColor = Color.white;
    public Color inactiveStarColor = new Color(1, 1, 1, 0.2f);
    public float delayBetweenStars = 0.5f;

    [Header("Audio & Navigation")]
    public AudioSource summaryAudioSource;
    public AudioClip congratulationClip;
    public AudioClip starPopClip;
    public AudioClip starmapPartClip; // Opsional: Bunyi 'beep' HUD
    public Button nextStageButton;
    public Button retryButton;
    public Button menuButton;
    public string nextSceneName = "Stage_2";
    public float fadeSpeed = 2f;

    private void Start()
    {
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);

        if (nextStageButton) nextStageButton.onClick.AddListener(OnNextStageClicked);
        if (retryButton) retryButton.onClick.AddListener(OnRetryClicked);
        if (menuButton) menuButton.onClick.AddListener(OnMenuClicked);
    }

    public void StartSummarySequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Save progress
        PlayerPrefs.SetInt($"Stage_{currentStageIndex}_Complete", 1);
        PlayerPrefs.SetInt("Game_Complete", 1);
        PlayerPrefs.Save();

        if (GlobalAudioManager.Instance != null)
            GlobalAudioManager.Instance.StopAllGameplayAudio();

        gameObject.SetActive(true);

        // --- RESET SEMUA VISUAL KE INACTIVE ---
        foreach (Image star in starRatingList)
            if (star != null) star.color = inactiveStarColor;

        foreach (Image part in starmapParts)
            if (part != null) part.color = starmapInactiveColor;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isRunActive = false;
            if (timeText) timeText.text = GameManager.Instance.GetFormattedTime();
            if (scoreText) scoreText.text = GameManager.Instance.currentScore.ToString();
            int collected = GameManager.Instance.minorNodesCollected + GameManager.Instance.majorNodesCollected;
            if (nodeText) nodeText.text = $"{collected} Stars Collected";
            CalculateStarRating(collected);
        }

        Time.timeScale = 0f;
        StartCoroutine(SummaryRoutine());
    }

    private void CalculateStarRating(int collected)
    {
        if (totalNodesInLevel <= 0) { starsEarned = 1; return; }
        float percentage = (float)collected / totalNodesInLevel;
        if (percentage >= 0.9f) starsEarned = 3;
        else if (percentage >= 0.5f) starsEarned = 2;
        else starsEarned = 1;
    }

    private IEnumerator SummaryRoutine()
    {
        if (summaryAudioSource != null && congratulationClip != null)
            summaryAudioSource.PlayOneShot(congratulationClip);

        // Fade in Panel
        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * fadeSpeed;
            if (uiCanvasGroup) uiCanvasGroup.alpha = Mathf.Lerp(0, 1, timer);
            yield return null;
        }
        if (uiCanvasGroup) { uiCanvasGroup.alpha = 1; uiCanvasGroup.interactable = true; uiCanvasGroup.blocksRaycasts = true; }

        yield return new WaitForSecondsRealtime(0.3f);

        // --- SEKUENS NYALA STARMAP (HUD STYLE) ---
        for (int i = 0; i < starmapParts.Length; i++)
        {
            if (starmapParts[i] != null)
            {
                starmapParts[i].color = starmapActiveColor;
                if (summaryAudioSource != null && starmapPartClip != null)
                    summaryAudioSource.PlayOneShot(starmapPartClip);
                yield return new WaitForSecondsRealtime(delayBetweenStarmapParts);
            }
        }

        yield return new WaitForSecondsRealtime(0.2f);

        // --- SEKUENS NYALA BINTANG RATING ---
        for (int i = 0; i < starRatingList.Length; i++)
        {
            if (i < starsEarned)
            {
                if (starRatingList[i] != null)
                {
                    starRatingList[i].color = activeStarColor;
                    if (summaryAudioSource != null && starPopClip != null)
                        summaryAudioSource.PlayOneShot(starPopClip);
                }
                yield return new WaitForSecondsRealtime(delayBetweenStars);
            }
        }
    }

    // --- NAVIGATION LOGIC ---
    private void OnNextStageClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveCurrentStageStats();
            if (nextSceneName == "VictoryScreen" || nextSceneName == "GameOverScene")
                GameManager.Instance.FinishGame();
        }
        PrepareForSceneLoad(nextSceneName);
    }

    private void OnRetryClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResetLevelStats();
        PrepareForSceneLoad(SceneManager.GetActiveScene().name);
    }

    public void OnMenuClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.ResetLevelStats();
        SceneManager.LoadScene("MainMenu");
    }

    private void PrepareForSceneLoad(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}