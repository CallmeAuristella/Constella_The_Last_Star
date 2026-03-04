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
    public float respawnDelay = 0.2f; // Gue kecilin dikit biar gak kerasa nge-lag

    private bool isRespawning = false;

    [Header("Current Stage Data")]
    public float globalTimer = 0f;
    public int currentScore = 0;
    public int minorNodesCollected = 0;
    public int majorNodesCollected = 0;
    public bool isRunActive = false;

    [Header("Global Stats (History)")]
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

    private void Start()
    {
        if (!isRunActive) StartRun();

        if (transitionScreen != null)
        {
            transitionScreen.alpha = 0;
            transitionScreen.blocksRaycasts = false;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerMovementInput>();
        }
    }

    private void Update()
    {
        if (isRunActive)
        {
            globalTimer += Time.deltaTime;
        }
    }

    // --- FUNGSI SCORING & SISTEM ---

    public void StartRun() => isRunActive = true;

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
        PlayerPrefs.SetInt("Stage_1_Complete", 1);
        PlayerPrefs.Save();
    }

    private void LoadRecords()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        bestTime = PlayerPrefs.GetFloat("BestTime", 999999f);
    }

    private void SaveRecords()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
        if (globalTimer < bestTime)
        {
            bestTime = globalTimer;
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

        // 1. MATIKAN INPUT & FISIK (INSTANT)
        if (player != null)
        {
            player.enabled = false; // Otak mati
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static; // Badan kaku
            }
        }

        // 2. FADE OUT (HITAM)
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

        // 3. TELEPORT & RESET LINGKUNGAN
        if (player != null)
            player.transform.position = hasCheckpoint ? (Vector3)lastCheckpointPos : Vector3.zero;

        ResetEnvironment();

        // Tunggu bentar pas layar item biar transisi gak kasar
        yield return new WaitForSecondsRealtime(respawnDelay);

        // 4. FADE IN (MULAI TERANG)
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

        // [SINKRONISASI BARU] 5. NYALAKAN INPUT SETELAH LAYAR TERANG
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic; // Badan gerak lagi
                rb.linearVelocity = Vector2.zero;
            }
            player.enabled = true; // Otak nyala lagi pas player udah bisa liat layar
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

        if (player != null) player.transform.position = targetPos;
        ResetEnvironment();

        yield return new WaitForSecondsRealtime(0.3f);

        if (player != null)
        {
            player.enabled = true;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
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
        if (allPlatforms.Length == 0)
        {
            allPlatforms = Resources.FindObjectsOfTypeAll<FallingPlatform>();
        }

        if (allPlatforms.Length == 0) return;

        foreach (FallingPlatform platform in allPlatforms)
        {
            if (platform.gameObject.scene.name != null)
            {
                platform.ResetPlatform();
            }
        }
    }
}