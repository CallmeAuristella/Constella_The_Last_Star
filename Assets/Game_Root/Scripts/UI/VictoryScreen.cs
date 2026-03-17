using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private int currentStageIndex;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI totalMinorText;
    [SerializeField] private TextMeshProUGUI totalMajorText;
    public TextMeshProUGUI totalDeathText;

    [Header("High Score")]
    [SerializeField] private TextMeshProUGUI highScoreLabel;
    [SerializeField] private GameObject newRecordVisual;

    [Header("Visual")]
    [SerializeField] private Image constellationRewardImage;
    [SerializeField] private Sprite stageCompletedSprite;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victorySFX;
    [SerializeField] private AudioClip victoryBGM;

    private const string HIGH_SCORE_KEY = "HighScore";

    private void Start()
    {
        GlobalAudioManager.Instance?.ResetForMenu(); // ✅ FIX

        if (GameManager.Instance == null) return;

        PlayerPrefs.SetInt($"Stage_{currentStageIndex}_Complete", 1);
        PlayerPrefs.Save();

        int score = GameManager.Instance.grandTotalScore;
        float time = GameManager.Instance.grandTotalTime;

        DisplayStats(score, time);
        DisplayDeaths();
        HandleHighScore(score);
        ShowReward();

        StartCoroutine(AudioRoutine());
        SetCursor(true);
    }

    private void SetCursor(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void DisplayStats(int score, float time)
    {
        if (totalScoreText) totalScoreText.text = $"Score: {score}";

        if (totalTimeText)
        {
            int m = Mathf.FloorToInt(time / 60);
            int s = Mathf.FloorToInt(time % 60);
            totalTimeText.text = $"{m:00}:{s:00}";
        }
    }

    private void DisplayDeaths()
    {
        if (totalDeathText)
            totalDeathText.text = "Deaths: " + PlayerPrefs.GetInt("TotalDeaths", 0);
    }

    private void HandleHighScore(int score)
    {
        int high = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        if (score > high)
        {
            high = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, high);
            PlayerPrefs.Save();

            if (newRecordVisual) newRecordVisual.SetActive(true);
        }

        if (highScoreLabel)
            highScoreLabel.text = $"High Score: {high}";
    }

    private void ShowReward()
    {
        if (!constellationRewardImage || !stageCompletedSprite) return;

        constellationRewardImage.sprite = stageCompletedSprite;
        constellationRewardImage.gameObject.SetActive(true);
    }

    private IEnumerator AudioRoutine()
    {
        if (!audioSource) yield break;

        if (victorySFX)
        {
            audioSource.PlayOneShot(victorySFX);
            yield return new WaitForSeconds(victorySFX.length);
        }

        if (victoryBGM)
        {
            audioSource.clip = victoryBGM;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        GlobalAudioManager.Instance?.ResetForMenu(); // ✅ FIX

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}