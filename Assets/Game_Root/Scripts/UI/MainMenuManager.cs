using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject mainPanel;
    public GameObject archivePanel;
    public GameObject settingsPanel; // TAMBAHAN: Slot untuk panel settings lo

    void Start()
    {
        ShowMain();
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        if (archivePanel) archivePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false); // Pastikan settings tutup
    }

    public void ShowArchive()
    {
        mainPanel.SetActive(false);
        archivePanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    // TAMBAHAN: Fungsi untuk buka settings
    public void ShowSettings()
    {
        mainPanel.SetActive(false);
        if (archivePanel) archivePanel.SetActive(false);
        settingsPanel.SetActive(true); // Buka settings
    }

    public void PlayGame()
    {
        // Ambil AudioSource dari Global Manager dan matikan sebelum pindah scene
        AudioSource menuAudio = GlobalAudioManager.Instance.GetComponent<AudioSource>();
        if (menuAudio != null)
        {
            // Pilihan 1: Matikan langsung
            menuAudio.Stop();

            // Pilihan 2: Biarkan BGM_Manager di Level 1 yang meng-override (lebih halus)
        }

        SceneManager.LoadScene("Stage_1");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit!");
    }
}