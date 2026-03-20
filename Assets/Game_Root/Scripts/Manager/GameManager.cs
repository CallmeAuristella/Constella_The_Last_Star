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
    private bool isRespawning = false;

    [Header("Current Stage Data")]
    public float globalTimer = 0f;
    public int currentScore = 0;
    public int minorNodesCollected = 0;
    public int majorNodesCollected = 0;
    public bool isRunActive = false;

    [Header("Global Accumulation")]
    public int grandTotalScore = 0;
    public float grandTotalTime = 0f;
    public int grandTotalMinorNodes = 0;
    public int grandTotalMajorNodes = 0;

    [Header("Persistent Records")]
    public int highScore;
    public float bestTime;
    public int totalDeaths;

    [Header("Snapshot Data")]
    public int lastStageScore;
    public float lastStageTime;
    public int lastStageNodes;

    private bool hasSavedThisStage = false;
    public bool isFirstStage = false;

    private HashSet<int> completedThisSession = new HashSet<int>();
    public List<string> activatedNodes = new List<string>();

    // =========================
    // LIFECYCLE
    // =========================

    private void Awake()
    {
        SetupSingleton();
        LoadRecords();


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

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Update()
    {
        if (isRunActive)
            globalTimer += Time.deltaTime;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene Loaded: {scene.name}");
        hasSavedThisStage = false;

        hasCheckpoint = false;

        // 🔥 RESET GLOBAL STATE YANG SERING BIKIN BUG
        Time.timeScale = 1f;
        AudioListener.pause = false;

        FindReferencesInScene();
        StartCoroutine(DelayedSpawnSequence(scene.name));

        SetupCameraTarget();
        InitializeUI();

        if (IsGameplayScene(scene.name))
        {
            // 🔥 INI FIX UTAMA (PINDAH KE SINI)
            if (isFirstStage)
            {
                Debug.Log("[GameManager] NEW RUN → RESET TOTAL");

                ResetRunAccumulation();
                ResetLevelStats();
            }

            StartRun();
            GlobalAudioManager.Instance?.ResetForGameplay();
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
    // REFERENCE SYSTEM
    // =========================

    private void FindReferencesInScene()
    {
        player = FindFirstObjectByType<PlayerMovementInput>();

        GameObject canvasObj = GameObject.FindGameObjectWithTag("TransitionScreen");

        if (canvasObj != null)
            transitionScreen = canvasObj.GetComponent<CanvasGroup>();
        else
            transitionScreen = FindFirstObjectByType<CanvasGroup>();
    }

    private void InitializeUI()
    {
        if (transitionScreen != null)
        {
            transitionScreen.alpha = 0;
            transitionScreen.blocksRaycasts = false;
        }
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
    // SPAWN SYSTEM
    // =========================

    private void HandleSpawnPoint()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnPoint != null && player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            player.ForceTeleport(spawnPoint.transform.position);
        }
        else if (player != null)
        {
            player.ForceTeleport(Vector3.zero);
        }
    }

    // =========================
    // GAMEPLAY STATS
    // =========================

    public void StartRun()
    {
        globalTimer = 0f;
        isRunActive = true;
    }

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

    public void ResetLevelStats()
    {
        currentScore = 0;
        globalTimer = 0f;
        minorNodesCollected = 0;
        majorNodesCollected = 0;

        isRunActive = false;
    }

    public void SaveCurrentStageStats()
    {
        if (!isRunActive)
        {
            Debug.LogWarning("[SAVE] Ignored - run not active");
            return;
        }

        if (hasSavedThisStage) return;
        hasSavedThisStage = true;

        // 🔥 SIMPAN SNAPSHOT (INI KUNCI)
        lastStageScore = currentScore;
        lastStageTime = globalTimer;
        lastStageNodes = minorNodesCollected + majorNodesCollected;

        // 🔥 AKUMULASI
        grandTotalScore += currentScore;
        grandTotalTime += globalTimer;
        grandTotalMinorNodes += minorNodesCollected;
        grandTotalMajorNodes += majorNodesCollected;

        Debug.Log($"[SAVE] Stage Score: {lastStageScore}");

        // 🔥 BARU RESET
        ResetLevelStats();
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(globalTimer / 60f);
        int seconds = Mathf.FloorToInt(globalTimer % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // =========================
    // PROGRESSION
    // =========================


    public void CompleteStage(int stageIndex)
    {
        if (completedThisSession.Contains(stageIndex))
        {
            Debug.Log($"[Progression] Stage {stageIndex} already completed this session");
            return;
        }

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

    public void AbortRun()
    {
        Debug.Log("[GameManager] Abort Run");

        isRunActive = false;

        // 🔥 RESET DATA LEVEL
        ResetLevelStats();

        // 🔥 RESET SAVE FLAG (PENTING BANGET)
        hasSavedThisStage = false;
    }

    public void FinishGame()
    {
        isRunActive = false;
        SaveRecords();
    }

    // =========================
    // RESET SYSTEM
    // =========================

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
    public void ResetRunAccumulation()
    {
        Debug.Log("[GameManager] Reset RUN accumulation");

        grandTotalScore = 0;
        grandTotalTime = 0f;
        grandTotalMinorNodes = 0;
        grandTotalMajorNodes = 0;

        // 🔥 RESET SNAPSHOT JUGA
        lastStageScore = 0;
        lastStageTime = 0f;
        lastStageNodes = 0;
    }

    // =========================
    // RECORD SYSTEM
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

    // =========================
    // RESPAWN SYSTEM
    // =========================

    public void SetCheckpoint(Vector2 position)
    {
        lastCheckpointPos = position;
        hasCheckpoint = true;
    }

    public void RespawnPlayer()
    {
        if (isRespawning) return;

        AddDeath();
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
}