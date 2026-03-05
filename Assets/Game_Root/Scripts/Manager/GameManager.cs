using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Checkpoint System")]
    public Vector2 lastCheckpointPos;
    public bool hasCheckpoint = false;

    [Header("Respawn Settings")]
    public PlayerMovementInput player;
    public CanvasGroup transitionScreen;
    public float respawnDelay = 0.2f;
    private bool isRespawning = false;

    [Header("Current Stage Data (Level Ini)")]
    public float globalTimer = 0f;
    public int currentScore = 0;
    public int minorNodesCollected = 0;
    public int majorNodesCollected = 0;
    public bool isRunActive = false;

    [Header("Global Stats (Akumulasi Total)")]
    public int grandTotalScore = 0;
    public float grandTotalTime = 0f;
    public int grandTotalMinorNodes = 0;
    public int grandTotalMajorNodes = 0;

    [Header("Saved Records")]
    public int highScore;
    public float bestTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRecords();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Subscribe ke event sceneLoaded agar otomatis urus player tiap pindah level
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (!isRunActive) StartRun();

        if (transitionScreen != null)
        {
            transitionScreen.alpha = 0;
            transitionScreen.blocksRaycasts = false;
        }

        // Cari player jika belum ada
        if (player == null) FindPlayerInScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Cari player lagi (siapa tau player baru di-load dari scene sebelumnya)
        FindPlayerInScene();

        // 2. Cari SpawnPoint di scene baru
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnPoint != null && player != null)
        {
            // Pindah kan player ke titik muncul level baru secara aman
            player.ForceTeleport(spawnPoint.transform.position);
        }

        // 3. Hubungkan Kamera dengan Player baru
        SetupCameraTarget();
    }

    private void FindPlayerInScene()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerMovementInput>();
        }
    }

    private void SetupCameraTarget()
    {
        if (player == null) return;

        // Cari script CameraFollow lo di main camera
        CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.target = player.transform;
            cam.SnapToTarget(); // Langsung tempel kamera ke player tanpa lerp diawal scene
        }
    }

    private void Update()
    {
        if (isRunActive)
        {
            globalTimer += Time.deltaTime;
        }
    }

    // --- FUNGSI DATA & SCORING ---

    public void StartRun() => isRunActive = true;

    public void ResetLevelStats()
    {
        currentScore = 0;
        globalTimer = 0f;
        minorNodesCollected = 0;
        majorNodesCollected = 0;
        isRunActive = true;
        Debug.Log("GameManager: Data stage ini telah di-reset ke 0.");
    }

    public void SaveCurrentStageStats()
    {
        grandTotalScore += currentScore;
        grandTotalTime += globalTimer;
        grandTotalMinorNodes += minorNodesCollected;
        grandTotalMajorNodes += majorNodesCollected;

        Debug.Log("GameManager: Data stage berhasil disimpan ke Grand Total.");

        ResetLevelStats();
    }

    public void AddScore(int amount) => currentScore += amount;

    public void LogNodeCollection(NodeType type)
    {
        if (type == NodeType.Minor) minorNodesCollected++;
        else if (type == NodeType.Major) majorNodesCollected++;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(globalTimer / 60F);
        int seconds = Mathf.FloorToInt(globalTimer % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
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
    }

    private void SaveRecords()
    {
        if (grandTotalScore > highScore)
        {
            highScore = grandTotalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
        if (grandTotalTime < bestTime)
        {
            bestTime = grandTotalTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
        }
        PlayerPrefs.Save();
    }

    // --- FUNGSI UTAMA (RESPAWN, TELEPORT & RESET) ---

    public void SetCheckpoint(Vector2 position)
    {
        lastCheckpointPos = position;
        hasCheckpoint = true;
    }

    public void RespawnPlayer()
    {
        if (isRespawning) return;
        StartCoroutine(SoftRespawnRoutine());
    }

    private IEnumerator SoftRespawnRoutine()
    {
        isRespawning = true;

        if (player != null)
        {
            player.enabled = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }

        if (transitionScreen != null)
        {
            float timer = 0;
            while (timer < 0.25f)
            {
                timer += Time.unscaledDeltaTime;
                transitionScreen.alpha = Mathf.Lerp(0, 1, timer / 0.25f);
                yield return null;
            }
            transitionScreen.alpha = 1;
        }

        if (player != null)
        {
            Vector3 targetPos = hasCheckpoint ? (Vector3)lastCheckpointPos : Vector3.zero;
            player.ForceTeleport(targetPos); // Gunakan ForceTeleport agar kamera ikut snap
        }

        ResetEnvironment();

        yield return new WaitForSecondsRealtime(respawnDelay);

        if (transitionScreen != null)
        {
            float timer = 0;
            while (timer < 0.4f)
            {
                timer += Time.unscaledDeltaTime;
                transitionScreen.alpha = Mathf.Lerp(1, 0, timer / 0.4f);
                yield return null;
            }
            transitionScreen.alpha = 0;
        }

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
            }
            player.enabled = true;
        }

        Time.timeScale = 1f;
        isRespawning = false;
    }

    public void TeleportPlayer(Vector3 targetPosition)
    {
        if (isRespawning) return;
        StartCoroutine(TeleportRoutine(targetPosition));
    }

    private IEnumerator TeleportRoutine(Vector3 targetPos)
    {
        isRespawning = true;
        if (player != null)
        {
            player.enabled = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }

        if (transitionScreen != null)
        {
            float timer = 0;
            while (timer < 0.3f)
            {
                timer += Time.unscaledDeltaTime;
                transitionScreen.alpha = Mathf.Lerp(0, 1, timer / 0.3f);
                yield return null;
            }
            transitionScreen.alpha = 1;
        }

        if (player != null) player.ForceTeleport(targetPos);
        ResetEnvironment();

        yield return new WaitForSecondsRealtime(0.3f);

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
            }
            player.enabled = true;
        }

        if (transitionScreen != null)
        {
            float timer = 0;
            while (timer < 0.4f)
            {
                timer += Time.unscaledDeltaTime;
                transitionScreen.alpha = Mathf.Lerp(1, 0, timer / 0.4f);
                yield return null;
            }
            transitionScreen.alpha = 0;
        }
        isRespawning = false;
    }

    private void ResetEnvironment()
    {
        FallingPlatform[] allPlatforms = FindObjectsByType<FallingPlatform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allPlatforms.Length == 0) return;

        foreach (FallingPlatform platform in allPlatforms)
        {
            if (platform.gameObject.scene.name != null) platform.ResetPlatform();
        }
    }
}