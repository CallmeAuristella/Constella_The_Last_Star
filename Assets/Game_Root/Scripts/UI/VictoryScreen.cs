using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI totalMinorText;
    [SerializeField] private TextMeshProUGUI totalMajorText;

    [Header("High Score System")]
    [SerializeField] private TextMeshProUGUI highScoreLabel;
    [SerializeField] private GameObject newRecordVisual;

    private const string HIGH_SCORE_KEY = "HighScore";

    [Header("Visual Reward (Optional)")]
    [SerializeField] private Image constellationRewardImage;
    [SerializeField] private Sprite stageCompletedSprite;

    private void Start()
    {
        // Safety Check: Jika GameManager tidak ada (misal run scene ini langsung)
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager tidak ditemukan!");
            return;
        }

        // 1. Ambil Data Akumulasi (Grand Total)
        int score = GameManager.Instance.grandTotalScore;
        float time = GameManager.Instance.grandTotalTime;
        int minor = GameManager.Instance.grandTotalMinorNodes;
        int major = GameManager.Instance.grandTotalMajorNodes;

        // 2. Tampilkan ke UI
        DisplayStats(score, time, minor, major);
        ProcessHighScore(score);
        DisplayRewardVisual();

        // Pastikan kursor muncul jika game sebelumnya dalam mode lock
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void DisplayStats(int score, float time, int minor, int major)
    {
        if (totalScoreText)
            totalScoreText.text = $"Total Score: {score}";

        if (totalTimeText)
        {
            int min = Mathf.FloorToInt(time / 60F);
            int sec = Mathf.FloorToInt(time % 60F);
            totalTimeText.text = $"Total Time: {min:00}:{sec:00}";
        }

        if (totalMinorText)
            totalMinorText.text = $"Stardust Collected: {minor}";

        if (totalMajorText)
            totalMajorText.text = $"Core Linked: {major}";
    }

    private void ProcessHighScore(int currentScore)
    {
        int savedHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        bool isNewRecord = currentScore > savedHighScore;

        if (isNewRecord)
        {
            savedHighScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, savedHighScore);
            PlayerPrefs.Save();
        }

        if (highScoreLabel)
            highScoreLabel.text = $"High Score: {savedHighScore}";

        if (newRecordVisual != null)
        {
            newRecordVisual.SetActive(isNewRecord);

            if (isNewRecord)
            {
                // Animasi Kedip menggunakan LeanTween
                newRecordVisual.transform.localScale = Vector3.one;
                LeanTween.scale(newRecordVisual, Vector3.one * 1.15f, 0.5f)
                    .setLoopPingPong()
                    .setIgnoreTimeScale(true); // Biar tetep jalan meski timeScale 0
            }
        }
    }

    private void DisplayRewardVisual()
    {
        if (constellationRewardImage == null || stageCompletedSprite == null)
            return;

        constellationRewardImage.sprite = stageCompletedSprite;
        constellationRewardImage.preserveAspect = true;
        constellationRewardImage.gameObject.SetActive(true);

        constellationRewardImage.transform.localScale = Vector3.zero;

        LeanTween.scale(constellationRewardImage.gameObject, Vector3.one, 0.8f)
            .setEaseOutBack()
            .setDelay(0.3f)
            .setIgnoreTimeScale(true);
    }

    // --- BUTTON FUNCTIONS ---

    public void LoadMainMenu()
    {
        // 1. Reset Time Scale dulu
        Time.timeScale = 1f;

        // 2. Hancurkan GameManager agar data total benar-benar reset (Clean Start)
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        // 3. Pindah Scene
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Keluar Game...");
        Application.Quit();
    }
}