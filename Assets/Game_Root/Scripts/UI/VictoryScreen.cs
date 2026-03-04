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

    // Kunci untuk simpan data di HP/PC (Biar gak typo)
    private const string HIGH_SCORE_KEY = "HighScore";

    [Header("Visual Reward (Optional)")]
    [Tooltip("Boleh dikosongkan jika tidak ada gambar hadiah")]
    [SerializeField] private Image constellationRewardImage;
    [SerializeField] private Sprite stageCompletedSprite;

    private void Start()
    {
        // Safety Check: Pastikan GameManager ada
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager tidak ditemukan! Victory Screen mungkin kosong.");
            return;
        }

        // 1. Ambil Data
        int score = GameManager.Instance.grandTotalScore;
        float time = GameManager.Instance.grandTotalTime;
        int minor = GameManager.Instance.grandTotalMinorNodes;
        int major = GameManager.Instance.grandTotalMajorNodes;

        // 2. Jalankan Fungsi-fungsi
        DisplayStats(score, time, minor, major);
        ProcessHighScore(score);
        DisplayRewardVisual();
    }

    private void DisplayStats(int score, float time, int minor, int major)
    {
        // Update Score
        if (totalScoreText)
            totalScoreText.text = $"Total Score: {score}";

        // Update Time (Format 00:00)
        if (totalTimeText)
        {
            int min = Mathf.FloorToInt(time / 60F);
            int sec = Mathf.FloorToInt(time % 60F);
            totalTimeText.text = $"Total Time: {min:00}:{sec:00}";
        }

        // Update Stars/Nodes
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
            // Simpan Rekor Baru
            savedHighScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, savedHighScore);
            PlayerPrefs.Save();
        }

        // Update UI Label
        if (highScoreLabel)
            highScoreLabel.text = $"High Score: {savedHighScore}";

        // Efek Visual "NEW RECORD"
        if (newRecordVisual != null)
        {
            newRecordVisual.SetActive(isNewRecord);

            if (isNewRecord)
            {
                // Animasi Kedip (PingPong)
                LeanTween.scale(newRecordVisual, Vector3.one * 1.1f, 0.5f)
                    .setLoopPingPong();
            }
        }
    }

    private void DisplayRewardVisual()
    {
        // Cek apakah slot di inspector diisi? Kalau kosong, skip aja (Gak Error).
        if (constellationRewardImage == null || stageCompletedSprite == null)
            return;

        constellationRewardImage.sprite = stageCompletedSprite;
        constellationRewardImage.preserveAspect = true;
        constellationRewardImage.gameObject.SetActive(true);

        // Reset Scale ke 0 dulu biar bisa muncul membesar
        constellationRewardImage.transform.localScale = Vector3.zero;

        // Animasi Muncul (Pop Up)
        LeanTween.scale(constellationRewardImage.gameObject, Vector3.one, 0.8f)
            .setEaseOutBack()
            .setDelay(0.3f);
    }

    // --- BUTTON FUNCTIONS ---

    public void LoadMainMenu()
    {
        // Hancurkan GameManager (Reset total stats jadi 0)
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        // Pastikan waktu jalan normal lagi
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Keluar Game...");
        Application.Quit();
    }
}