using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VictoryScreen : MonoBehaviour
{
    [Header("Stage Info (PENTING)")]
    [SerializeField] private int currentStageIndex; // Isi di Inspector (misal: 1)

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI totalMinorText;
    [SerializeField] private TextMeshProUGUI totalMajorText;
    public TextMeshProUGUI totalDeathText;

    [Header("High Score System")]
    [SerializeField] private TextMeshProUGUI highScoreLabel;
    [SerializeField] private GameObject newRecordVisual;

    private const string HIGH_SCORE_KEY = "HighScore";

    [Header("Visual Reward (Optional)")]
    [SerializeField] private Image constellationRewardImage;
    [SerializeField] private Sprite stageCompletedSprite;

    [Header("Victory/Game Over Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victorySFX;
    [SerializeField] private AudioClip victoryBGM;

    private void Start()
    {
        RestoreGlobalAudio();

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager tidak ditemukan!");
            return;
        }

        // --- SINKRONISASI GALLERY ---
        // Simpan status stage ini selesai agar Gallery terbuka
        PlayerPrefs.SetInt($"Stage_{currentStageIndex}_Complete", 1);
        PlayerPrefs.Save();

        // --- DATA ACQUISITION ---
        int score = GameManager.Instance.grandTotalScore;
        float time = GameManager.Instance.grandTotalTime;
        int minor = GameManager.Instance.grandTotalMinorNodes;
        int major = GameManager.Instance.grandTotalMajorNodes;

        // --- UI DISPLAY ---
        DisplayStats(score, time, minor, major);
        DisplayFinalDeaths();
        ProcessHighScore(score);
        DisplayRewardVisual();

        StartCoroutine(AudioSequenceRoutine());
        SetCursorState(true);
    }

    private void RestoreGlobalAudio()
    {
        if (GlobalAudioManager.Instance != null)
            GlobalAudioManager.Instance.ResetMixerForMenu(); // Paksa buka semua jalur audio
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void DisplayStats(int score, float time, int minor, int major)
    {
        if (totalScoreText) totalScoreText.text = $"Total Score: {score}";

        if (totalTimeText)
        {
            int min = Mathf.FloorToInt(time / 60F);
            int sec = Mathf.FloorToInt(time % 60F);
            totalTimeText.text = $"Total Time: {min:00}:{sec:00}";
        }

        if (totalMinorText) totalMinorText.text = $"Stardust Collected: {minor}";
        if (totalMajorText) totalMajorText.text = $"Core Linked: {major}";
    }

    private void DisplayFinalDeaths()
    {
        if (totalDeathText != null)
        {
            int total = PlayerPrefs.GetInt("TotalDeaths", 0);
            totalDeathText.text = "Total Deaths: " + total.ToString();
        }
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

        if (highScoreLabel) highScoreLabel.text = $"High Score: {savedHighScore}";

        if (newRecordVisual != null)
        {
            newRecordVisual.SetActive(isNewRecord);
            // Hanya Animate jika pakai LeanTween
            // AnimateNewRecord(); 
        }
    }

    private void DisplayRewardVisual()
    {
        if (constellationRewardImage == null || stageCompletedSprite == null) return;
        constellationRewardImage.sprite = stageCompletedSprite;
        constellationRewardImage.preserveAspect = true;
        constellationRewardImage.gameObject.SetActive(true);
    }

    private IEnumerator AudioSequenceRoutine()
    {
        if (audioSource == null) yield break;

        if (victorySFX != null)
        {
            audioSource.PlayOneShot(victorySFX);
            yield return new WaitForSeconds(victorySFX.length);
        }

        if (victoryBGM != null)
        {
            audioSource.clip = victoryBGM;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // --- NAVIGATION CALLBACKS (CLEANED) ---

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (GlobalAudioManager.Instance != null)
            GlobalAudioManager.Instance.ResetMixerForMenu();

        // JANGAN Destroy(GameManager.Instance.gameObject) di sini!
        // Supaya GameManager tetep ada buat tombol Reset di Main Menu.

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}