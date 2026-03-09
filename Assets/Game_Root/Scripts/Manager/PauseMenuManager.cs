using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // WAJIB ADA UNTUK SLIDER

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Audio Sliders (Tarik ke sini!)")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Status")]
    public bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.ResumeGameplayAudio();
        }
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.StopAllGameplayAudio();
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null && pausePanel != null)
        {
            pausePanel.SetActive(false);
            settingsPanel.SetActive(true);

            // --- FIX SLIDER BALIK KE 0 ---
            SyncSlidersWithData();
            Debug.Log("Settings opened & Sliders synced.");
        }
        else
        {
            Debug.LogError("TOD! Slot Panel masih KOSONG di Inspector!");
        }
    }

    // FUNGSI BARU UNTUK SINKRONISASI VISUAL SLIDER
    private void SyncSlidersWithData()
    {
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("SavedBGM", 0.75f);

        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("SavedSFX", 0.75f);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.ResetMixerForMenu();
        }

        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        SceneManager.LoadScene("MainMenu");
        Debug.Log("[Navigation] Returning to Menu.");
    }
}