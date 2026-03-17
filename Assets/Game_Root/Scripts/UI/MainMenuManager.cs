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
    }

    void HideAllPanels()
    {
        if (panelMainMenu != null) panelMainMenu.Hide();
        if (panelArchive != null) panelArchive.Hide();
        if (panelSettings != null) panelSettings.Hide();
    }

    public void ShowMain()
    {
        HideAllPanels();
        if (panelMainMenu != null) panelMainMenu.Show();
    }

    public void ShowArchive()
    {
        HideAllPanels();
        if (panelArchive != null) panelArchive.Show();
    }

    public void ShowSettings()
    {
        HideAllPanels();
        if (panelSettings != null) panelSettings.Show();
    }

    public void PlayGame()
    {
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
        Application.Quit();
        Debug.Log("Quit Game");
    }
}