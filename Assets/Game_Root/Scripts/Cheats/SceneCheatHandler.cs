using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCheatHandler : MonoBehaviour
{
    [Header("Cheat Settings")]
    [Tooltip("Aktifkan atau matikan cheat lewat inspector")]
    public bool cheatsEnabled = true;

    private void Update()
    {
        if (!cheatsEnabled) return;

        // --- LOMPAT KE SCENE BERDASARKAN INDEX (Keyboard 1-9) ---
        // Scene Index bisa lo liat di Build Settings (Ctrl + Shift + B)
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                JumpToScene(i);
            }
        }

        // --- TOMBOL NAVIGASI CEPAT ---
        // N = Next Scene
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextScene();
        }

        // B = Back/Previous Scene
        if (Input.GetKeyDown(KeyCode.B))
        {
            PreviousScene();
        }

        // R = Restart Level Sekarang
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    public void JumpToScene(int sceneIndex)
    {
        // Cek apakah index scene tersedia di Build Settings
        if (sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[Cheat] Jumping to Scene Index: {sceneIndex}");
            SceneManager.LoadScene(sceneIndex);
        }
    }

    public void NextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
    }

    public void PreviousScene()
    {
        int prevIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (prevIndex >= 0)
        {
            SceneManager.LoadScene(prevIndex);
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}