using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageSummaryController : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup uiCanvasGroup;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nodeText;

    [Header("Constellation Animation")]
    [Tooltip("Masukkan gambar UI Image rasi bintang secara berurutan")]
    public Image[] constellationStars;
    public Color activeStarColor = Color.white;
    public Color inactiveStarColor = new Color(1, 1, 1, 0.2f);
    [Tooltip("Jeda waktu antar bintang menyala (detik)")]
    public float delayBetweenStars = 0.5f;

    [Header("Buttons (Sprite Based)")]
    public Button nextStageButton;
    public Button retryButton;
    public Button menuButton;

    [Header("Navigation")]
    [Tooltip("Isi dengan nama scene selanjutnya (Misal: 'GameOverScene' kalau ini stage terakhir)")]
    public string nextSceneName = "Stage_2";
    public float fadeSpeed = 2f;

    private void Start()
    {
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);

        if (nextStageButton) nextStageButton.onClick.AddListener(OnNextStageClicked);
        if (retryButton) retryButton.onClick.AddListener(OnRetryClicked);
        if (menuButton) menuButton.onClick.AddListener(OnMenuClicked);
    }

    public void StartSummarySequence()
    {
        gameObject.SetActive(true);

        // 1. Matikan semua bintang di awal sebelum panel muncul
        foreach (Image star in constellationStars)
        {
            if (star != null) star.color = inactiveStarColor;
        }

        // 2. Tarik dan tampilkan data dari GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishGame();

            if (timeText) timeText.text = GameManager.Instance.GetFormattedTime();
            if (scoreText) scoreText.text = GameManager.Instance.currentScore.ToString();

            if (nodeText)
            {
                int total = GameManager.Instance.minorNodesCollected + GameManager.Instance.majorNodesCollected;
                nodeText.text = $"{total} Stars Collected";
            }
        }

        // 3. Bekukan waktu gameplay dan mulai urutan animasi
        Time.timeScale = 0f;
        StartCoroutine(SummaryRoutine());
    }

    private IEnumerator SummaryRoutine()
    {
        // FASE 1: Fade In Panel
        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * fadeSpeed;
            if (uiCanvasGroup) uiCanvasGroup.alpha = Mathf.Lerp(0, 1, timer);
            yield return null;
        }

        if (uiCanvasGroup)
        {
            uiCanvasGroup.alpha = 1;
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }

        // FASE 2: Animasi Rasi Bintang Menyala
        yield return new WaitForSecondsRealtime(0.5f); // Jeda dramatis sebelum bintang pertama nyala

        foreach (Image star in constellationStars)
        {
            if (star != null)
            {
                star.color = activeStarColor;
                // Opsi: Tambahkan AudioSource.PlayOneShot() di sini jika ingin efek suara "Ting!"
            }
            yield return new WaitForSecondsRealtime(delayBetweenStars);
        }
    }

    // --- LOGIC TOMBOL ---
    private void OnNextStageClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveCurrentStageStats();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}