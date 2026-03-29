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
    [SerializeField] private GameObject newBestLabel;
    [SerializeField] private TMP_Text bestScoreText;

    private bool isNewBest = false;

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

    [Header("Score Breakdown UI")]
    public Transform breakdownContainer;
    public GameObject scoreRowPrefab;
    private List<ScoreRowUI> spawnedRows = new List<ScoreRowUI>();

    [Header("Achievement & Bonus UI")]
    public Transform achievementContainer;
    public Transform runBonusContainer;
    public GameObject achievementRowPrefab;
    public GameObject runBonusRowPrefab;

    private bool isSummaryRunning = false;
    private Coroutine playRoutine;


    [SerializeField]
    private List<AchievementType> orderedAchievements = new List<AchievementType>
{
    AchievementType.NoDeath_Run,
    AchievementType.GodSpeed,
    AchievementType.StarCollector,
    AchievementType.SkillIssue
};

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Stage_"))
        {
            string indexStr = SceneManager.GetActiveScene().name.Replace("Stage_", "");
            int.TryParse(indexStr, out currentStageIndex);
        }

        if (summaryAudioSource != null)
            summaryAudioSource.ignoreListenerPause = true;

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }

        if (newBestLabel)
            newBestLabel.SetActive(false);

        gameObject.SetActive(false);

        nextStageButton?.onClick.AddListener(OnNextStageClicked);
        retryButton?.onClick.AddListener(OnRetryClicked);
        menuButton?.onClick.AddListener(OnMenuClicked);
    }

    public void StartSummarySequence() {
        if (isSummaryRunning) return;
        isSummaryRunning = true;

        // 🔥 WAJIB: SAVE RUN DI SINI
        if (GameManager.Instance != null) {
            GameManager.Instance.SaveCurrentStageStats();
            currentStageIndex = GameManager.Instance.currentStageIndex;

        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GlobalAudioManager.Instance?.StopAllGameplayAudio();

        gameObject.SetActive(true);

        ResetConstellationUI();
        ResetStars();

        if (GameManager.Instance != null) {
            var gm = GameManager.Instance;

            float time = gm.lastStageTime;
            int score = gm.lastStageScore;
            int collected = gm.lastStageNodes;
            int bestScore = gm.GetBestScore(currentStageIndex);

            isNewBest = (score >= bestScore && score > 0);

            timeText.text = FormatTime(time);
            scoreText.text = $"Score: {score}";
            nodeText.text = $"{collected} Stars Collected";
            bestScoreText.text = $"Best: {bestScore}";
            newBestLabel.SetActive(isNewBest);

            CalculateStarRating(collected);
        }

        Time.timeScale = 0f;
        StartCoroutine(SummaryRoutine());
    }

    private IEnumerator SummaryRoutine()
    {
        // FADE IN
        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * fadeSpeed;
            uiCanvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        uiCanvasGroup.alpha = 1;
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;

        yield return new WaitForSecondsRealtime(0.25f);

        // =====================
        // 1. SCORE BREAKDOWN
        // =====================
        PopulateScoreBreakdown();

        yield return new WaitUntil(() => playRoutine == null);

        // =====================
        // 2. BONUS + ACHIEVEMENT
        // =====================
        PopulateSummary(GameManager.Instance.lastRunEvaluation);

        // =====================
        // 3. AUDIO
        // =====================
        if (summaryAudioSource && congratulationClip)
        {
            summaryAudioSource.PlayOneShot(congratulationClip);
            yield return new WaitForSecondsRealtime(congratulationClip.length);
        }

        // =====================
        // 4. CONSTELLATION + STAR
        // =====================
        yield return StartCoroutine(PlayConstellationSequence());
        yield return new WaitForSecondsRealtime(0.2f);
        yield return StartCoroutine(PlayStarRatingSequence());
    }

    void PopulateSummary(RunEvaluation eval)
    {
        if (eval == null) return;

        foreach (Transform c in achievementContainer) Destroy(c.gameObject);
        foreach (Transform c in runBonusContainer) Destroy(c.gameObject);

        var gm = GameManager.Instance;

        // =========================
        // ACHIEVEMENT (FIXED GRID)
        // =========================
        foreach (var type in orderedAchievements)
        {
            var obj = Instantiate(achievementRowPrefab, achievementContainer);
            var ui = obj.GetComponent<AchievementRowUI>();

            bool unlocked = gm.unlockedAchievements.Contains(type);

            ui.Setup(type);
            ui.SetLocked(!unlocked);

            // highlight kalau didapat di run ini (optional)
            bool achievedThisRun = eval.achievements.Exists(a => a.type == type && a.achieved);

            if (achievedThisRun)
            {
                ui.SetHighlight();
            }
        }

        // =========================
        // RUN BONUS
        // =========================
        foreach (var b in eval.runBonuses)
        {
            var obj = Instantiate(runBonusRowPrefab, runBonusContainer);
            var ui = obj.GetComponent<RunBonusRowUI>();

            ui.Setup(b.type, b.value);
        }
    }

    void PopulateScoreBreakdown()
    {
        spawnedRows.Clear();

        foreach (Transform child in breakdownContainer)
            Destroy(child.gameObject);

        var eval = GameManager.Instance.lastRunEvaluation;
        if (eval == null) return;

        foreach (var entry in eval.breakdown)
        {
            var row = Instantiate(scoreRowPrefab, breakdownContainer);
            var ui = row.GetComponent<ScoreRowUI>();

            ui.Setup(entry.label, entry.value, false, entry.isBonus);
            ui.PrepareForAnimation();

            spawnedRows.Add(ui);
        }

        var totalRow = Instantiate(scoreRowPrefab, breakdownContainer);
        var totalUI = totalRow.GetComponent<ScoreRowUI>();

        totalUI.Setup("TOTAL", eval.finalScore, true, false);
        totalUI.PrepareForAnimation();

        spawnedRows.Add(totalUI);

        PlayBreakdownSequence();
    }

    public void PlayBreakdownSequence()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return null;

        for (int i = 0; i < spawnedRows.Count; i++)
        {
            var row = spawnedRows[i];
            bool isLast = (i == spawnedRows.Count - 1);

            if (isLast)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                row.PlayRevealAnimation(0.35f, true);
            }
            else
            {
                row.PlayRevealAnimation();
            }

            yield return new WaitForSecondsRealtime(0.25f);
        }

        playRoutine = null;
    }

    private IEnumerator PlayConstellationSequence()
    {
        if (ConstellationManager.Instance == null)
            yield break;

        var collected = ConstellationManager.Instance.GetCollectedNodes();

        foreach (var nodeUI in ConstellationManager.Instance.summaryNodes)
        {
            if (nodeUI == null) continue;

            if (collected.Contains(nodeUI.nodeID))
            {
                nodeUI.ActivateNodeAnimated();

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
            if (i < starsEarned)
            {
                starRatingList[i].color = activeStarColor;

                if (summaryAudioSource && starPopClip)
                    summaryAudioSource.PlayOneShot(starPopClip);

                yield return new WaitForSecondsRealtime(delayBetweenStars);
            }
        }
    }

    private void ResetStars()
    {
        foreach (var star in starRatingList)
            star.color = inactiveStarColor;
    }

    private void ResetConstellationUI()
    {
        if (ConstellationManager.Instance == null) return;

        foreach (var node in ConstellationManager.Instance.summaryNodes)
            node?.ResetUI();
    }

    private void CalculateStarRating(int collected)
    {
        float percentage = (float)collected / totalNodesInLevel;

        if (percentage >= 0.9f) starsEarned = 3;
        else if (percentage >= 0.5f) starsEarned = 2;
        else starsEarned = 1;
    }

    private string FormatTime(float time)
    {
        int m = Mathf.FloorToInt(time / 60);
        int s = Mathf.FloorToInt(time % 60);
        return $"{m:00}:{s:00}";
    }

    private void OnNextStageClicked()
    {
        PrepareForSceneLoad(nextSceneName);
    }

    private void OnRetryClicked()
    {
        PrepareForSceneLoad(SceneManager.GetActiveScene().name);
    }

    private void OnMenuClicked()
    {
        PrepareForSceneLoad("MainMenu");
    }

    private void PrepareForSceneLoad(string sceneName)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(sceneName);
    }
}