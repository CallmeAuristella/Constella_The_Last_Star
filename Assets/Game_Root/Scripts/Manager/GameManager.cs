using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // =========================
    // CORE REFERENCES
    // =========================

    [Header("Core References")]
    public PlayerMovementInput player;
    public CanvasGroup transitionScreen;

    // =========================
    // CHECKPOINT & RESPAWN
    // =========================

    [Header("Checkpoint & Respawn")]
    public Vector2 lastCheckpointPos;
    public bool hasCheckpoint = false;
    public float respawnDelay = 0.2f;

    private bool isRespawning = false;

    // =========================
    // CURRENT RUN DATA
    // =========================

    [Header("Current Stage Data")]
    public float globalTimer = 0f;
    public int currentScore = 0;
    public int minorNodesCollected = 0;
    public int majorNodesCollected = 0;

    public bool isRunActive = false;
    public bool isRunDiscarded = false;

    // =========================
    // GLOBAL ACCUMULATION
    // =========================

    [Header("Global Accumulation")]
    public int grandTotalScore = 0;
    public float grandTotalTime = 0f;
    public int grandTotalMinorNodes = 0;
    public int grandTotalMajorNodes = 0;

    // =========================
    // PERSISTENT RECORDS
    // =========================

    [Header("Persistent Records")]
    public int highScore;
    public float bestTime;
    public int totalDeaths;
    public int playerDeathCount;

    // =========================
    // SNAPSHOT DATA
    // =========================

    [Header("Snapshot Data")]
    public int lastStageScore;
    public float lastStageTime;
    public int lastStageNodes;

    // =========================
    // SCORING DEPTH
    // =========================

    [Header("Run Evaluation (Scoring Depth)")]
    public RunEvaluation lastRunEvaluation;

    // =========================
    // INTERNAL STATE
    // =========================

    private bool hasSavedThisStage = false;

    private HashSet<int> completedThisSession = new HashSet<int>();
    public List<string> activatedNodes = new List<string>();

    public int currentStageIndex = 0;

    const string FIRST_RUN_KEY = "FIRST_RUN_DONE";

    // =========================
    // LIFECYCLE
    // =========================

    private void Awake()
    {
        HandleFirstRunReset();
        InitializeAudioPrefs();

        SetupSingleton();
        LoadRecords();
    }

    private void Update()
    {
        if (isRunActive)
            globalTimer += Time.deltaTime;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // =========================
    // INITIALIZATION HELPERS
    // =========================

    private void HandleFirstRunReset()
    {
        if (PlayerPrefs.GetInt(FIRST_RUN_KEY, 0) == 0)
        {
            Debug.Log("[SYSTEM] First Run → Reset All Data");

            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt(FIRST_RUN_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    private void InitializeAudioPrefs()
    {
        if (!PlayerPrefs.HasKey("SavedBGM"))
            PlayerPrefs.SetFloat("SavedBGM", 0.75f);

        if (!PlayerPrefs.HasKey("SavedSFX"))
            PlayerPrefs.SetFloat("SavedSFX", 0.75f);
    }

    private void SetupSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // =========================
    // SCENE FLOW
    // =========================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DebugRunState($"SCENE LOADED: {scene.name}");

        hasSavedThisStage = false;
        hasCheckpoint = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        FindReferencesInScene();
        StartCoroutine(DelayedSpawnSequence(scene.name));

        SetupCameraTarget();
        InitializeUI();

        if (IsGameplayScene(scene.name))
        {
            SetupStageIndex(scene.name);

            if (scene.name == "Stage_1")
            {
                ResetRunAccumulation();
                ResetLevelStats();
            }

            StartRun();
            GlobalAudioManager.Instance?.ResetForGameplay();
        }
    }

    private void SetupStageIndex(string sceneName)
    {
        if (sceneName.StartsWith("Stage_"))
        {
            string indexStr = sceneName.Replace("Stage_", "");
            currentStageIndex = int.Parse(indexStr);
        }
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName != "MainMenu" &&
               sceneName != "VictoryScreen" &&
               sceneName != "GameOverScene";
    }

    private IEnumerator DelayedSpawnSequence(string sceneName)
    {
        yield return new WaitForEndOfFrame();

        if (IsGameplayScene(sceneName))
            HandleSpawnPoint();
    }

    // =========================
    // REFERENCES
    // =========================

    private void FindReferencesInScene()
    {
        player = FindFirstObjectByType<PlayerMovementInput>();

        GameObject canvasObj = GameObject.FindGameObjectWithTag("TransitionScreen");

        transitionScreen = canvasObj != null
            ? canvasObj.GetComponent<CanvasGroup>()
            : FindFirstObjectByType<CanvasGroup>();
    }

    private void InitializeUI()
    {
        if (transitionScreen == null) return;

        transitionScreen.alpha = 0;
        transitionScreen.blocksRaycasts = false;
    }

    private void SetupCameraTarget()
    {
        if (player == null) return;

        CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();

        if (cam != null)
        {
            cam.target = player.transform;
            cam.SnapToTarget();
        }
    }

    // =========================
    // STAGE DATA
    // =========================

    public int GetTotalNodesInStage(int stageIndex)
    {
        switch (stageIndex)
        {
            case 1: return 10;
            case 2: return 15;
            case 3: return 18;
            default: return 0;
        }
    }

    // =========================
    // SPAWN
    // =========================

    private void HandleSpawnPoint()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (player == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (spawnPoint != null)
            player.ForceTeleport(spawnPoint.transform.position);
        else
            player.ForceTeleport(Vector3.zero);
    }

    // =========================
    // RUN CONTROL
    // =========================

    public void StartRun()
    {
        DebugRunState("START RUN");

        playerDeathCount = 0;

        ConstellationManager.Instance?.ResetCollectedNodes();
        activatedNodes.Clear();

        isRunActive = true;
    }

    public void AbortRun()
    {
        DebugRunState("BEFORE ABORT");

        isRunActive = false;
        isRunDiscarded = true;

        ResetLevelStats();
        hasSavedThisStage = false;

        DebugRunState("AFTER ABORT");
    }

    // =========================
    // GAMEPLAY DATA
    // =========================

    public void AddScore(int amount) => currentScore += amount;

    public void LogNodeCollection(NodeType type)
    {
        if (type == NodeType.Minor) minorNodesCollected++;
        else if (type == NodeType.Major) majorNodesCollected++;
    }

    public void AddDeath()
    {
        totalDeaths++;
        PlayerPrefs.SetInt("TotalDeaths", totalDeaths);
        PlayerPrefs.Save();
    }

    // =========================
    // SAVE & EVALUATION
    // =========================

    public void SaveCurrentStageStats()
    {
        Debug.Log(">>> SAVE TRIGGERED <<<");
        DebugRunState("BEFORE SAVE");

        if (!isRunActive)
        {
            Debug.LogWarning("[SAVE] Blocked — run not active");
            return;
        }

        if (hasSavedThisStage)
        {
            Debug.LogWarning("[SAVE] Already saved this stage");
            return;
        }

        hasSavedThisStage = true;

        int totalNodes = GetTotalNodesInStage(currentStageIndex);

        lastRunEvaluation = ScoreCalculator.EvaluateRun(
            currentStageIndex,
            currentScore,
            globalTimer,
            minorNodesCollected + majorNodesCollected,
            totalNodes,
            playerDeathCount
        );

        // =========================
        // DEBUG BREAKDOWN (DI SINI)
        // =========================

        foreach (var entry in lastRunEvaluation.breakdown)
        {
            Debug.Log($"[BREAKDOWN] {entry.label}: {entry.value}");
        }

#if UNITY_EDITOR
        Debug.Log($"[EVAL] Final Score: {lastRunEvaluation.finalScore}");
#endif

        lastStageScore = lastRunEvaluation.finalScore;
        lastStageTime = lastRunEvaluation.completionTime;
        lastStageNodes = lastRunEvaluation.nodesCollected;

        grandTotalScore += lastRunEvaluation.finalScore;

        TrySetBestScore(currentStageIndex, lastRunEvaluation.finalScore);

        grandTotalTime += globalTimer;
        grandTotalMinorNodes += minorNodesCollected;
        grandTotalMajorNodes += majorNodesCollected;

        DebugRunState("AFTER SAVE");

        ResetLevelStats();
    }

    // =========================
    // PROGRESSION
    // =========================

    public void CompleteStage(int stageIndex)
    {
        if (completedThisSession.Contains(stageIndex))
            return;

        completedThisSession.Add(stageIndex);

        string key = $"Stage_{stageIndex}_Complete";

        if (PlayerPrefs.GetInt(key, 0) == 1)
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }
    public void StartNewStageRun()
    {
        ResetRunAccumulation();
        ResetLevelStats();
    }
    public void FinishGame()
    {
        isRunActive = false;
        SaveRecords();
    }
    public void SetCheckpoint(Vector2 position)
    {
        lastCheckpointPos = position;
        hasCheckpoint = true;
    }

    public void ResetGameProgress()
    {
        StartCoroutine(ResetRoutine());
    }
    private IEnumerator ResetRoutine()
    {
        Debug.Log("=== HARD RESET ===");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // reset runtime data
        grandTotalScore = 0;
        grandTotalTime = 0;
        grandTotalMinorNodes = 0;
        grandTotalMajorNodes = 0;

        ResetLevelStats();

        Time.timeScale = 1f;
        AudioListener.pause = false;

        yield return null;

        SceneManager.LoadScene("MainMenu");

        Debug.Log("GM Instance: " + GameManager.Instance);
        Debug.Log("Active: " + GameManager.Instance.gameObject.activeInHierarchy);
    }
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(globalTimer / 60f);
        int seconds = Mathf.FloorToInt(globalTimer % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    // =========================
    // RESET
    // =========================

    public void ResetLevelStats()
    {
        currentScore = 0;
        globalTimer = 0f;
        minorNodesCollected = 0;
        majorNodesCollected = 0;

        isRunActive = false;
    }

    public void ResetRunAccumulation()
    {
        grandTotalScore = 0;
        grandTotalTime = 0f;
        grandTotalMinorNodes = 0;
        grandTotalMajorNodes = 0;

        lastStageScore = 0;
        lastStageTime = 0f;
        lastStageNodes = 0;
    }

    // =========================
    // RECORDS
    // =========================

    private void LoadRecords()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        bestTime = PlayerPrefs.GetFloat("BestTime", 999999f);
        totalDeaths = PlayerPrefs.GetInt("TotalDeaths", 0);
    }

    private void SaveRecords()
    {
        if (grandTotalScore > highScore)
            PlayerPrefs.SetInt("HighScore", grandTotalScore);

        if (grandTotalTime < bestTime)
            PlayerPrefs.SetFloat("BestTime", grandTotalTime);

        PlayerPrefs.Save();
    }

    public bool TrySetBestScore(int stageIndex, int score)
    {
        int bestScore = GetBestScore(stageIndex);

        if (score > bestScore)
        {
            SetBestScore(stageIndex, score);
            return true;
        }

        return false;
    }

    public int GetBestScore(int stageIndex)
    {
        return PlayerPrefs.GetInt("BEST_SCORE_STAGE_" + stageIndex, 0);
    }

    public void SetBestScore(int stageIndex, int score)
    {
        PlayerPrefs.SetInt("BEST_SCORE_STAGE_" + stageIndex, score);
        PlayerPrefs.Save();
    }

    // =========================
    // RESPAWN
    // =========================

    public void RespawnPlayer()
    {
        if (isRespawning) return;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        TogglePlayerPhysics(false);

        yield return StartCoroutine(FadeScreen(1f, 0.25f));

        if (player != null)
        {
            Vector3 pos = hasCheckpoint ? (Vector3)lastCheckpointPos : Vector3.zero;
            player.ForceTeleport(pos);
        }

        ResetEnvironment();

        yield return new WaitForSecondsRealtime(respawnDelay);
        yield return StartCoroutine(FadeScreen(0f, 0.4f));

        TogglePlayerPhysics(true);

        isRespawning = false;
    }

    private void TogglePlayerPhysics(bool enable)
    {
        if (player == null) return;

        player.enabled = enable;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = enable ? RigidbodyType2D.Dynamic : RigidbodyType2D.Static;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        if (transitionScreen == null) yield break;

        float start = transitionScreen.alpha;
        float t = 0;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            transitionScreen.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            yield return null;
        }

        transitionScreen.alpha = targetAlpha;
    }

    private void ResetEnvironment()
    {
        var platforms = FindObjectsByType<FallingPlatform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var p in platforms)
            p.ResetPlatform();
    }

    // =========================
    // DEBUG
    // =========================

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugRunState(string context)
    {
        Debug.Log(
            $"[DEBUG RUN] {context}\n" +
            $"Score: {currentScore}\n" +
            $"Time: {globalTimer}\n" +
            $"Nodes: {minorNodesCollected + majorNodesCollected}\n" +
            $"Grand Score: {grandTotalScore}\n" +
            $"SavedThisStage: {hasSavedThisStage}"
        );
    }
}