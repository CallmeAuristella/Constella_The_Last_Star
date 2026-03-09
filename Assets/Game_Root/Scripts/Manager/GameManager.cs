using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Core References (Auto-Filled)")]
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

    private void Start()
    {
        // Panggil pencarian saat startup pertama kali
        FindReferencesInScene();
        InitializeUI();
    }

    private void Update()
    {
        if (isRunActive) globalTimer += Time.deltaTime;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Reset Checkpoint agar tidak pakai posisi dari level sebelumnya
        hasCheckpoint = false;

        // 2. Cari ulang semua referensi
        FindReferencesInScene();

        // 3. Jalankan Coroutine Spawn agar menunggu objek Scene stabil
        StartCoroutine(DelayedSpawnSequence(scene.name));

        SetupCameraTarget();
        InitializeUI();

        Debug.Log($"[GameManager] Scene '{scene.name}' loaded. Checkpoint Reset & Spawn Sequence Started.");
    }
    private IEnumerator DelayedSpawnSequence(string sceneName)
    {
        // Tunggu sampai akhir frame agar semua Awake/Start di scene baru selesai
        yield return new WaitForEndOfFrame();

        if (sceneName != "MainMenu")
        {
            HandleSpawnPoint();
        }
    }
    // --- NEW: DYNAMIC REFERENCE SYSTEM ---

    private void FindReferencesInScene()
    {
        // 1. Cari Player (Jika tidak ada di slot)
        if (player == null)
            player = FindFirstObjectByType<PlayerMovementInput>();

        // 2. Cari Transition Screen (Cari CanvasGroup dengan Tag khusus atau yang ada di scene)
        GameObject canvasObj = GameObject.FindGameObjectWithTag("TransitionScreen");
        if (canvasObj != null)
        {
            transitionScreen = canvasObj.GetComponent<CanvasGroup>();
        }
        else
        {
            // Fallback: Cari CanvasGroup apapun jika tag tidak ditemukan
            transitionScreen = FindFirstObjectByType<CanvasGroup>();
        }
    }

    // --- SYSTEM INITIALIZATION ---

    private void InitializeUI()
    {
        if (transitionScreen != null)
        {
            transitionScreen.alpha = 0;
            transitionScreen.blocksRaycasts = false;
        }
    }

    private void FindPlayerInScene() // Tetap dipertahankan untuk kompatibilitas fungsi lain
    {
        if (player == null) player = FindFirstObjectByType<PlayerMovementInput>();
    }

    private void HandleSpawnPoint()
    {
        // Gunakan Find yang lebih spesifik jika tag bermasalah
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnPoint != null && player != null)
        {
            // Reset Velocity agar player tidak "terpental" saat pindah scene
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Teleport Paksa
            player.ForceTeleport(spawnPoint.transform.position);

            Debug.Log($"[Spawn System] Player successfully moved to SpawnPoint at {spawnPoint.transform.position}");
        }
        else
        {
            // Fallback: Jika SpawnPoint tidak ditemukan, taruh di 0,0 agar tidak hilang
            if (player != null) player.ForceTeleport(Vector3.zero);
            Debug.LogWarning("[Spawn System] SpawnPoint TIDAK DITEMUKAN! Player dipindah ke (0,0,0)");
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

    // --- SCORING & STATS SYSTEM ---

    public void StartRun() => isRunActive = true;
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
        isRunActive = true;
    }

    public void SaveCurrentStageStats()
    {
        grandTotalScore += currentScore;
        grandTotalTime += globalTimer;
        grandTotalMinorNodes += minorNodesCollected;
        grandTotalMajorNodes += majorNodesCollected;
        ResetLevelStats();
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(globalTimer / 60F);
        int seconds = Mathf.FloorToInt(globalTimer % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // --- DATA PERSISTENCE & CLEANING ---

    public void ResetGameProgress()
    {
        // 1. HAPUS DATA DARI STORAGE
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("TotalDeaths");
        PlayerPrefs.DeleteKey("BestTime");
        PlayerPrefs.DeleteKey("Game_Complete");

        // Sapu bersih status stage (asumsi sampai 20 stage)
        for (int i = 0; i <= 20; i++)
        {
            PlayerPrefs.DeleteKey($"Stage_{i}_Complete");
        }
        PlayerPrefs.Save();

        // 2. RESET VARIABEL MEMORI
        grandTotalScore = 0;
        grandTotalTime = 0f;
        totalDeaths = 0;
        highScore = 0;
        ResetLevelStats();

        // 3. FORCE REFRESH GALLERY (BAGIAN VITAL)
        // Cari GalleryManager bahkan jika dia sedang nonaktif di Hierarchy
        GalleryManager gallery = Object.FindFirstObjectByType<GalleryManager>(FindObjectsInactive.Include);

        if (gallery != null)
        {
            gallery.RefreshGallery();
            Debug.Log("<color=green>[GameManager]</color> Gallery Berhasil Di-lock Kembali.");
        }
        else
        {
            Debug.LogWarning("<color=red>[GameManager]</color> GalleryManager TIDAK DITEMUKAN di scene ini!");
        }
    }

    public void FinishGame()
    {
        isRunActive = false;
        SaveRecords();
        PlayerPrefs.SetInt("Game_Complete", 1);
        PlayerPrefs.Save();
    }

    private void LoadRecords()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        bestTime = PlayerPrefs.GetFloat("BestTime", 999999f);
        totalDeaths = PlayerPrefs.GetInt("TotalDeaths", 0);
    }

    private void SaveRecords()
    {
        if (grandTotalScore > highScore) PlayerPrefs.SetInt("HighScore", grandTotalScore);
        if (grandTotalTime < bestTime) PlayerPrefs.SetFloat("BestTime", grandTotalTime);
        PlayerPrefs.Save();
    }

    // --- RESPAWN & TELEPORT LOGIC ---

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
            Vector3 targetPos = hasCheckpoint ? (Vector3)lastCheckpointPos : Vector3.zero;
            player.ForceTeleport(targetPos);
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
        float startAlpha = transitionScreen.alpha;
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            transitionScreen.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }
        transitionScreen.alpha = targetAlpha;
    }

    private void ResetEnvironment()
    {
        FallingPlatform[] allPlatforms = FindObjectsByType<FallingPlatform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (FallingPlatform platform in allPlatforms)
        {
            if (platform.gameObject.scene.name != null) platform.ResetPlatform();
        }
    }
}