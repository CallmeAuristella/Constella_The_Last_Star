using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class PauseMenuManager : MonoBehaviour
{

    [SerializeField] private RunDiscardedUI runDiscardedUI;
    public GameObject pausePanel;
    public GameObject settingsPanel;

    public Slider musicSlider;
    public Slider sfxSlider;

    public bool isPaused = false;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    [SerializeField] private GameObject pauseFirstButton;
    [SerializeField] private GameObject settingsFirstButton;

    private GameObject previousPauseSelection;

    

    private void Update()
    {
       
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EventSystem.current.SetSelectedGameObject(null);

        GlobalAudioManager.Instance?.ResumeGameplayAudio();
    }

    public void Pause() {
        pausePanel.SetActive(true);

        if (settingsPanel)
            settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GlobalAudioManager.Instance?.StopAllGameplayAudio();

        // 🔥 UI FOCUS
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
    }
    private void OnEnable() {
        if (pauseAction != null) {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePressed;
        }
    }

    private void OnDisable() {
        if (pauseAction != null) {
            pauseAction.action.performed -= OnPausePressed;
            pauseAction.action.Disable();
        }
    }
    private void OnPausePressed(InputAction.CallbackContext context) {
        if (isPaused && settingsPanel.activeSelf)
            CloseSettings();
        else if (isPaused)
            Resume();
        else
            Pause();
    }
    public void OpenSettings() {
        previousPauseSelection =
            EventSystem.current.currentSelectedGameObject;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);

        SyncSliders();

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsFirstButton);
    }

    private void SyncSliders()
    {
        if (musicSlider)
            musicSlider.value = PlayerPrefs.GetFloat("SavedBGM", 0.75f);

        if (sfxSlider)
            sfxSlider.value = PlayerPrefs.GetFloat("SavedSFX", 0.75f);
    }

    public void CloseSettings() {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);

        if (previousPauseSelection != null) {
            EventSystem.current.SetSelectedGameObject(previousPauseSelection);
        } else {
            EventSystem.current.SetSelectedGameObject(pauseFirstButton);
        }
    }

    public void GoToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AbortRun();
            GameManager.Instance.DebugRunState("ABORT RUN");
        }

        // 🔥 HANDLE RUN DISCARDED UI
        if (runDiscardedUI != null)
        {
            // 🔥 HIDE PAUSE UI DULU
            pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            // 🔥 SHOW DISCARDED
            runDiscardedUI.ShowAndExit("MainMenu");
            return;
        }

        // fallback (kalau UI belum ke-assign)
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}