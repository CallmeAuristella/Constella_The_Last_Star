using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel Animators")]
    [SerializeField] private UIPanelAnimator panelMainMenu;
    [SerializeField] private UIPanelAnimator panelArchive;
    [SerializeField] private UIPanelAnimator panelSettings;

    private void Start()
    {
        ShowMain();

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

    public void ShowMain()
    {
        HideAllPanels();
        panelMainMenu?.Show();
    }

    public void ShowArchive()
    {
        HideAllPanels();
        panelArchive?.Show();
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
}