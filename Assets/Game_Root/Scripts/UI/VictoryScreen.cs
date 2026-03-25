using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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

    private void Start()
    {
        GlobalAudioManager.Instance?.ResetForMenu();

        if (GameManager.Instance == null) return;

        int score = GameManager.Instance.grandTotalScore;
        float time = GameManager.Instance.grandTotalTime;

        DisplayStats(score, time);
        DisplayDeaths();
        DisplayHighScore(score);

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
        // 🔥 SCORE
        if (totalScoreText)
            totalScoreText.text = $"Score: {score}";

        // 🔥 TIME
        if (totalTimeText)
        {
            int m = Mathf.FloorToInt(time / 60);
            int s = Mathf.FloorToInt(time % 60);
            totalTimeText.text = $"Time: {m:00}:{s:00}";
        }

        // 🔥 MINOR NODES
        if (totalMinorText)
            totalMinorText.text = $"Stars (Minor): {GameManager.Instance.grandTotalMinorNodes}";

        // 🔥 MAJOR NODES
        if (totalMajorText)
            totalMajorText.text = $"Stars (Major): {GameManager.Instance.grandTotalMajorNodes}";
    }

    private void DisplayDeaths()
    {
        if (totalDeathText)
            totalDeathText.text = "Deaths: " + GameManager.Instance.totalDeaths;
    }

    private void DisplayHighScore(int score)
    {
        int high = GameManager.Instance.highScore;

        if (score > high)
        {
            if (newRecordVisual) newRecordVisual.SetActive(true);
            high = score;
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
        GlobalAudioManager.Instance?.ResetForMenu();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}