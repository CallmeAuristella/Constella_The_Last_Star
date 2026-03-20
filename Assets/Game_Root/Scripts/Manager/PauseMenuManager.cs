using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;

    public Slider musicSlider;
    public Slider sfxSlider;

    public bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused && settingsPanel.activeSelf)
                CloseSettings();
            else if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GlobalAudioManager.Instance?.ResumeGameplayAudio();
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GlobalAudioManager.Instance?.StopAllGameplayAudio();
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);

        SyncSliders();
    }

    private void SyncSliders()
    {
        if (musicSlider)
            musicSlider.value = PlayerPrefs.GetFloat("SavedBGM", 0.75f);

        if (sfxSlider)
            sfxSlider.value = PlayerPrefs.GetFloat("SavedSFX", 0.75f);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        GameManager.Instance?.AbortRun();

        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene("MainMenu");
    }
}