using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Core References")]
    public PlayerMovementInput player;
    public CanvasGroup transitionScreen;

    [Header("Checkpoint & Respawn")]
    public Vector2 lastCheckpointPos;
    public bool hasCheckpoint = false;
    public float respawnDelay = 0.2f;
    //private bool isRespawning = false;

    [Header("Current Stage Data")]
    public float globalTimer = 0f;
    public int currentScore = 0;
    public int minorNodesCollected = 0;
    public int majorNodesCollected = 0;

    public bool isRunActive = false;
    public bool isRunDiscarded = false;

    [Header("Global Accumulation")]
    public int grandTotalScore = 0;
    public float grandTotalTime = 0f;
    public int grandTotalMinorNodes = 0;
    public int grandTotalMajorNodes = 0;

    [Header("Persistent Records")]
    public int highScore;
    public float bestTime;
    public int totalDeaths;
    public int playerDeathCount;

    [Header("Snapshot Data")]
    public int lastStageScore;
    public float lastStageTime;
    public int lastStageNodes;

    [Header("Run Evaluation")]
    public RunEvaluation lastRunEvaluation;

    private bool hasSavedThisStage = false;

    public int currentStageIndex = 0;

    const string FIRST_RUN_KEY = "FIRST_RUN_DONE";

    // =========================
    // GLOBAL SYSTEM
    // =========================

    public GlobalProgress globalProgress;
    public Dictionary<int, int> bestNodesPerStage = new Dictionary<int, int>();
    public HashSet<AchievementType> unlockedAchievements = new HashSet<AchievementType>();
    public HashSet<int> completedStages = new HashSet<int>();
    const string GAME_VERSION = "1.1";
    const string VERSION_KEY = "GAME_VERSION";

    // =========================
    // LIFECYCLE
    // =========================

    private void Awake()
    {
        HandleVersioningReset();
        SetupSingleton();
        LoadRecords();
        LoadGame();

        if (globalProgress == null)
        {
            globalProgress = new GlobalProgress();
        }
    }

    private void Update()
    {
        if (isRunActive)
            globalTimer += Time.deltaTime;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // =========================
    // SCENE FLOW
    // =========================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasSavedThisStage = false;
        hasCheckpoint = false;

        FindReferencesInScene();

        if (scene.name.StartsWith("Stage_"))
        {
            SetupStageIndex(scene.name);

            if (scene.name == "Stage_1")
            {
                ResetRunAccumulation();
                ResetLevelStats();
            }

            StartRun();
        }
    }

    private void SetupStageIndex(string sceneName)
    {
        string indexStr = sceneName.Replace("Stage_", "");
        currentStageIndex = int.Parse(indexStr);
    }

    // =========================
    // RUN CONTROL
    // =========================

    public void StartRun()
    {
        playerDeathCount = 0;
        isRunActive = true;
        isRunDiscarded = false;
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
        globalProgress.totalDeaths++;
        playerDeathCount++;
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

    public bool TrySetBestScore(int stageIndex, int score)
    {
        int best = GetBestScore(stageIndex);

        if (score > best)
        {
            SetBestScore(stageIndex, score);
            return true;
        }

        return false;
    }

    // =========================
    // SAVE FLOW (CORE LOGIC)
    // =========================

    public void SaveCurrentStageStats()
    {
        Debug.Log($"[SAVE] Stage {currentStageIndex} START SAVE");
        if (!isRunActive || hasSavedThisStage || isRunDiscarded)
            return;

    
        hasSavedThisStage = true;

        int totalNodes = GetTotalNodesInStage(currentStageIndex);

        // 1. Evaluate
        lastRunEvaluation = ScoreCalculator.EvaluateRun(
            currentStageIndex,
            currentScore,
            globalTimer,
            minorNodesCollected + majorNodesCollected,
            totalNodes,
            playerDeathCount
        );

        // 2. Update Best Nodes (STRICT)
        int actualNodes = minorNodesCollected + majorNodesCollected;

        UpdateBestNodes(currentStageIndex, actualNodes);

        // DEBUG
        Debug.Log($"[NODE] Stage {currentStageIndex}: {actualNodes}/{GetTotalNodesInStage(currentStageIndex)}");

        // 3. Apply Global
        ApplyRunToGlobal(lastRunEvaluation);

        // 4. Evaluate Global Achievements
        var globalAch = EvaluateGlobalAchievements(currentStageIndex);

        // 5. Inject
        lastRunEvaluation.achievements.AddRange(globalAch);

        // Snapshot
        lastStageScore = lastRunEvaluation.finalScore;
        lastStageTime = lastRunEvaluation.completionTime;
        lastStageNodes = lastRunEvaluation.nodesCollected;

        // Accumulate
        grandTotalScore += lastRunEvaluation.finalScore;
        grandTotalTime += globalTimer;
        grandTotalMinorNodes += minorNodesCollected;
        grandTotalMajorNodes += majorNodesCollected;

        //Trigger Stage Completion (for potential stage-specific achievements or logic)
        completedStages.Add(currentStageIndex);

        //TryBestScore
        TrySetBestScore(currentStageIndex, lastRunEvaluation.finalScore);

        // Save 
        SaveGame();
        Debug.Log($"[SAVE] Stage {currentStageIndex} COMPLETED");
        Debug.Log($"[SAVE CHECK] Stage {currentStageIndex} SAVED");
        Debug.Log("CompletedStages count: " + completedStages.Count);

        //Reset
        ResetLevelStats();
    }
    // =========================
    // COMPATIBILITY LAYER (LEGACY SUPPORT)
    // =========================

    public void RespawnPlayer()
    {
        if (player == null) return;

        Vector3 pos = hasCheckpoint ? (Vector3)lastCheckpointPos : Vector3.zero;
        player.transform.position = pos;
    }

    public void AbortRun()
    {
        isRunActive = false;
        isRunDiscarded = true;
        if (isRunDiscarded)
        {
            Debug.Log("[SAVE] Blocked — run discarded");
            return;
        }
        ResetLevelStats();
    }

    public void CompleteStage(int stageIndex)
    {
        // Optional: bisa lo expand nanti
        Debug.Log($"[PROGRESSION] Stage {stageIndex} completed");
    }


    public void FinishGame()
    {
        isRunActive = false;
        SaveGame();
    }
    public void SaveGame()
    {
        SaveData data = new SaveData();

        // GLOBAL
        data.totalDeaths = globalProgress.totalDeaths;
        data.totalNodesCollected = globalProgress.totalNodesCollected;
        data.starCollectorUnlocked = globalProgress.starCollectorUnlocked;
        data.skillIssueUnlocked = globalProgress.skillIssueUnlocked;

        // DICTIONARY → LIST
        data.bestNodesPerStage = new List<StageNodeData>();
        foreach (var kvp in bestNodesPerStage)
        {
            data.bestNodesPerStage.Add(new StageNodeData
            {
                stageIndex = kvp.Key,
                bestNodes = kvp.Value
            });
        }

        // HASHSET → LIST
        data.unlockedAchievements = new List<int>();
        data.completedStages = new List<int>(completedStages);
        foreach (var ach in unlockedAchievements)
        {
            data.unlockedAchievements.Add((int)ach);
        }

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString("SAVE_DATA", json);
        PlayerPrefs.Save();

        Debug.Log("[SAVE] Game Saved");
    }
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SAVE_DATA"))
        {
            Debug.Log("[LOAD] No Save Found");
            return;
        }

        string json = PlayerPrefs.GetString("SAVE_DATA");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // GLOBAL
        globalProgress.totalDeaths = data.totalDeaths;
        globalProgress.totalNodesCollected = data.totalNodesCollected;
        globalProgress.starCollectorUnlocked = data.starCollectorUnlocked;
        globalProgress.skillIssueUnlocked = data.skillIssueUnlocked;

        // LIST → DICTIONARY
        bestNodesPerStage.Clear();
        foreach (var entry in data.bestNodesPerStage)
        {
            bestNodesPerStage[entry.stageIndex] = entry.bestNodes;
        }

        // LIST → HASHSET
        unlockedAchievements.Clear();
        if (data.bestNodesPerStage != null) {
            foreach (var entry in data.bestNodesPerStage) {
                bestNodesPerStage[entry.stageIndex] = entry.bestNodes;
            }
        }
        if (data.completedStages != null)
            completedStages = new HashSet<int>(data.completedStages);
        else
            completedStages = new HashSet<int>();

        Debug.Log("[LOAD] Game Loaded");
    }

    public void SetCheckpoint(Vector2 position)
    {
        lastCheckpointPos = position;
        hasCheckpoint = true;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(globalTimer / 60f);
        int seconds = Mathf.FloorToInt(globalTimer % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void ResetGameProgress() {
        Debug.Log("[RESET] FULL RESET TRIGGERED");

        // CLEAR PREFS
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // CLEAR RUNTIME DATA
        bestNodesPerStage.Clear();
        unlockedAchievements.Clear();
        completedStages.Clear();

        globalProgress = new GlobalProgress();

        // RESET STATS
        currentScore = 0;
        globalTimer = 0f;
        minorNodesCollected = 0;
        majorNodesCollected = 0;

        grandTotalScore = 0;
        grandTotalTime = 0f;
        grandTotalMinorNodes = 0;
        grandTotalMajorNodes = 0;

        totalDeaths = 0;
        playerDeathCount = 0;

        // 🔥 PENTING: overwrite save kosong
        SaveGame();

        // RELOAD SCENE
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugRunState(string context)
    {
        Debug.Log(
            $"[DEBUG RUN] {context}\n" +
            $"Score: {currentScore}\n" +
            $"Time: {globalTimer}\n" +
            $"Nodes: {minorNodesCollected + majorNodesCollected}"
        );
    }

    // =========================
    // GLOBAL ACHIEVEMENT
    // =========================

    public List<AchievementEntry> EvaluateGlobalAchievements(int currentStageIndex)
    {
        List<AchievementEntry> unlocked = new List<AchievementEntry>();

        if (currentStageIndex != 3)
            return unlocked;

        if (!globalProgress.starCollectorUnlocked && AllStagesPerfect())
        {
            globalProgress.starCollectorUnlocked = true;

            unlockedAchievements.Add(AchievementType.StarCollector);
            SaveGame();

            unlocked.Add(new AchievementEntry
            {
                type = AchievementType.StarCollector,
                achieved = true
            });
        }

        if (!globalProgress.skillIssueUnlocked &&
            globalProgress.totalDeaths >= 10)
        {
            globalProgress.skillIssueUnlocked = true;

            unlockedAchievements.Add(AchievementType.SkillIssue);
            SaveGame();

            unlocked.Add(new AchievementEntry
            {
                type = AchievementType.SkillIssue,
                achieved = true
            });
        }
        Debug.Log("[CHECK] AllStagesPerfect = " + AllStagesPerfect());
        return unlocked;
    }

    bool AllStagesPerfect()
    {
        for (int i = 1; i <= 3; i++)
        {
            int totalNodes = GetTotalNodesInStage(i);

            if (!bestNodesPerStage.ContainsKey(i))
                return false;

            if (bestNodesPerStage[i] < totalNodes)
                return false;
        }

        return true;
    }

    public void UpdateBestNodes(int stageIndex, int nodesCollected)
    {
        if (!bestNodesPerStage.ContainsKey(stageIndex))
        {
            bestNodesPerStage[stageIndex] = nodesCollected;
            return;
        }

        if (nodesCollected > bestNodesPerStage[stageIndex])
        {
            bestNodesPerStage[stageIndex] = nodesCollected;
        }
    }

    public void ApplyRunToGlobal(RunEvaluation eval)
    {
        globalProgress.totalNodesCollected += eval.nodesCollected;
    }

    // =========================
    // HELPERS
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
    }

    private void LoadRecords()
    {
        totalDeaths = PlayerPrefs.GetInt("TotalDeaths", 0);
        highScore = PlayerPrefs.GetInt("HIGH_SCORE", 0);
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

    private void HandleVersioningReset() {
        if (!PlayerPrefs.HasKey(VERSION_KEY)) {
            PlayerPrefs.SetString(VERSION_KEY, GAME_VERSION);
            PlayerPrefs.Save();
            return;
        }

        string savedVersion = PlayerPrefs.GetString(VERSION_KEY);

        if (savedVersion != GAME_VERSION) {
            Debug.Log("[RESET] Version changed → clearing data");

            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString(VERSION_KEY, GAME_VERSION);
            PlayerPrefs.Save();
        }
    }

    private void FindReferencesInScene()
    {
        player = FindFirstObjectByType<PlayerMovementInput>();
    }
    public void UnlockAchievement(AchievementType type)
    {
        if (unlockedAchievements.Contains(type))
            return;

        unlockedAchievements.Add(type);

        Debug.Log($"[ACHIEVEMENT] {type} UNLOCKED");

        SaveGame();
    }

    // =========================
    // DATA CLASS
    // =========================

    [System.Serializable]
    public class GlobalProgress
    {
        public int totalNodesCollected;
        public int totalDeaths;

        public bool starCollectorUnlocked;
        public bool skillIssueUnlocked;
    }
}

