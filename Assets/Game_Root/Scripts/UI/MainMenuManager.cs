using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel Animators")]
    [SerializeField] private UIPanelAnimator panelMainMenu;
    [SerializeField] private UIPanelAnimator panelArchive;
    [SerializeField] private UIPanelAnimator panelSettings;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GalleryManager galleryManager;
    [SerializeField] private UIPanelAnimator panelStageSelect;
    [SerializeField] private Button stage1Button;
    [SerializeField] private Button stage2Button;
    [SerializeField] private Button stage3Button;
    [SerializeField] private TMP_Text lockMessageText;
    [SerializeField] private GameObject stage2LockIcon;
    [SerializeField] private GameObject stage3LockIcon;
    [SerializeField] private TMP_Text stage1NameText;
    [SerializeField] private TMP_Text stage2NameText;
    [SerializeField] private TMP_Text stage3NameText;
    [SerializeField] private TMP_Text stage1ProgressText;
    [SerializeField] private TMP_Text stage2ProgressText;
    [SerializeField] private TMP_Text stage3ProgressText;
    [SerializeField] private Color normalProgressColor = Color.white;
    [SerializeField]
    private Color perfectProgressColor =
    new Color(1f, 0.78f, 0.15f);
    [Header("First Selected")]
    [SerializeField] private GameObject mainMenuFirstButton;
    private void Start()
    {
        ShowMain();
        SyncSliders();
        // 🔥 SAFETY: pastikan state normal saat masuk menu
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // =========================
    // PANEL CONTROL
    // =========================

    private void HideAllPanels() {
        panelMainMenu?.Hide();
        panelArchive?.Hide();
        panelSettings?.Hide();
        panelStageSelect?.Hide();
    }

    public void ShowMain() {
        HideAllPanels();

        panelMainMenu?.Show();

        StartCoroutine(FocusMainNextFrame());
    }
    private System.Collections.IEnumerator FocusMainNextFrame() {
        yield return null;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainMenuFirstButton);
    }

    public void ShowArchive() {
        HideAllPanels();

        panelArchive?.Show();

        StartCoroutine(FocusArchiveNextFrame());
    }
    private System.Collections.IEnumerator FocusArchiveNextFrame() {
        yield return null;

        galleryManager?.FocusFirstItem();
    }
    public void ShowSettings()
    {
        HideAllPanels();
        panelSettings?.Show();
    }
    public void ShowStageSelect() {
        HideAllPanels();
        panelStageSelect?.Show();
        if (lockMessageText != null)
            lockMessageText.text = "";
        RefreshStageLocks();
    }
    private void RefreshStageLocks() {
        // Set Level
        Debug.Log("Completed Stages: " +
    string.Join(", ", GameManager.Instance.completedStages));

        bool stage2Unlocked =
            GameManager.Instance.completedStages.Contains(1);

        bool stage3Unlocked =
            GameManager.Instance.completedStages.Contains(2);


        //Set nama Stage
        stage1NameText.text = "CRUX";

        stage2NameText.text =
            stage2Unlocked ? "CYGNUS" : "???";

        stage3NameText.text =
            stage3Unlocked ? "ORION" : "???";
        // Set Progress text
        UpdateProgressText(
    stage1ProgressText,
    1,
    true);

        UpdateProgressText(
            stage2ProgressText,
            2,
            stage2Unlocked);

        UpdateProgressText(
            stage3ProgressText,
            3,
            stage3Unlocked);
        //Interactable Button
        stage2Button.interactable = true;
        stage3Button.interactable = true;

        stage2LockIcon.SetActive(!stage2Unlocked);
        stage3LockIcon.SetActive(!stage3Unlocked);
    }
    public void ShowLockedMessage(int requiredStage) {
        Debug.Log("SHOW LOCK MESSAGE CALLED");

        if (lockMessageText == null) {
            Debug.Log("TEXT IS NULL");
            return;
        }

        lockMessageText.text =
            $"Complete Stage {requiredStage} First";

        Debug.Log(lockMessageText.text);
    }
    public void PlayStage(int stageIndex) {
        if (GameManager.Instance != null) {
            GameManager.Instance.ResetRunAccumulation();
        }

        if (GlobalAudioManager.Instance != null) {
            AudioSource menuAudio =
                GlobalAudioManager.Instance.GetComponent<AudioSource>();

            if (menuAudio != null)
                menuAudio.Stop();
        }

        SceneManager.LoadScene($"Stage_{stageIndex}");
    }
    public void SelectStage(int stageIndex) {
        switch (stageIndex) {
            case 1:
                PlayStage(1);
                break;

            case 2:
                if (!GameManager.Instance.completedStages.Contains(1)) {
                    ShowLockedMessage(1);
                    return;
                }

                PlayStage(2);
                break;

            case 3:
                if (!GameManager.Instance.completedStages.Contains(2)) {
                    ShowLockedMessage(2);
                    return;
                }

                PlayStage(3);
                break;
        }
    }
    public void BackFromStageSelect() {
        ShowMain();
    }

    // =========================
    // GAME FLOW
    // =========================

    public void PlayGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetRunAccumulation(); 
        }

        if (GlobalAudioManager.Instance != null)
        {
            AudioSource menuAudio = GlobalAudioManager.Instance.GetComponent<AudioSource>();
            if (menuAudio != null)
                menuAudio.Stop();
        }

        SceneManager.LoadScene("Stage_1");
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quit Game");
        Application.Quit();
    }
    // =======================
    // SLIDER VOLUME
    // =======================
    private void SyncSliders()
    {
        if (bgmSlider)
            bgmSlider.value = PlayerPrefs.GetFloat("SavedBGM", 0.75f);

        if (sfxSlider)
            sfxSlider.value = PlayerPrefs.GetFloat("SavedSFX", 0.75f);
    }
    private string GetProgressText(int stageIndex) {
        if (GameManager.Instance == null)
            return "---";

        int totalNodes =
            GameManager.Instance.GetTotalNodesInStage(stageIndex);

        int collected = 0;

        if (GameManager.Instance.bestNodesPerStage.ContainsKey(stageIndex)) {
            collected =
                GameManager.Instance.bestNodesPerStage[stageIndex];
        }

        return $"{collected}/{totalNodes}";
    }
    private void UpdateProgressText(
    TMP_Text text,
    int stageIndex,
    bool stageUnlocked = true) {
        if (!stageUnlocked) {
            text.text = "---";
            text.color = normalProgressColor;
            return;
        }

        int totalNodes =
            GameManager.Instance.GetTotalNodesInStage(stageIndex);

        int collected = 0;

        if (GameManager.Instance.bestNodesPerStage.ContainsKey(stageIndex)) {
            collected =
                GameManager.Instance.bestNodesPerStage[stageIndex];
        }

        text.text = $"{collected}/{totalNodes}";

        bool perfect = collected >= totalNodes;

        text.color =
            perfect ? perfectProgressColor : normalProgressColor;
    }
}