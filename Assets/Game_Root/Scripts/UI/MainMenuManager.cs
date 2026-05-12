using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel Animators")]
    [SerializeField] private UIPanelAnimator panelMainMenu;
    [SerializeField] private UIPanelAnimator panelArchive;
    [SerializeField] private UIPanelAnimator panelSettings;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GalleryManager galleryManager;

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

    private void HideAllPanels()
    {
        panelMainMenu?.Hide();
        panelArchive?.Hide();
        panelSettings?.Hide();
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
}