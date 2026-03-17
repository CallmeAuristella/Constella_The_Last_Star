using UnityEngine;
using UnityEngine.SceneManagement;

public class DevCheatManager : MonoBehaviour
{

    public static DevCheatManager Instance;

    [Header("Settings")]
    public bool enableCheat = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!enableCheat) return;

        // =========================
        // 🔥 SCENE NAVIGATION
        // =========================

        if (Input.GetKeyDown(KeyCode.F1))
            LoadScene("MainMenu");

        if (Input.GetKeyDown(KeyCode.F2))
            LoadScene("Stage_1");

        if (Input.GetKeyDown(KeyCode.F3))
            LoadScene("Stage_2");

        if (Input.GetKeyDown(KeyCode.F4))
            LoadScene("Stage_3");

        // =========================
        // 🔥 COMPLETE LEVEL
        // =========================

        if (Input.GetKeyDown(KeyCode.F5))
            CompleteLevel();

        // =========================
        // 🔥 RESET PROGRESS
        // =========================

        if (Input.GetKeyDown(KeyCode.F9))
            ResetAll();
    }

    // =========================
    // 🎮 FUNCTIONS
    // =========================

    void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);

        Debug.Log("[DEV] Load Scene: " + sceneName);
    }

    void CompleteLevel()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[DEV] GameManager not found!");
            return;
        }

        // Simulasi selesai
        GameManager.Instance.grandTotalScore += 999;
        GameManager.Instance.grandTotalTime += 60f;

        // Trigger victory screen (pakai flow lu)
        SceneManager.LoadScene("StageSummary"); // ⚠️ sesuaikan nama scene lu

        Debug.Log("[DEV] Level Completed (Cheat)");
    }

    void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("[DEV] PlayerPrefs Reset");
    }


}
