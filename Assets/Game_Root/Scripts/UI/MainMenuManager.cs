using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject mainPanel;
    public GameObject archivePanel;

    void Start()
    {
        ShowMain(); // Awal mulai pastikan Main Menu yang kebuka
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        archivePanel.SetActive(false);
    }

    public void ShowArchive()
    {
        mainPanel.SetActive(false);
        archivePanel.SetActive(true);
    }

    public void PlayGame()
    {
        // Ganti nama scene sesuai Stage 1 lo
        SceneManager.LoadScene("Stage_1");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit!");
    }
}