using UnityEngine;
using System.Collections;

public class RunDiscardedUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private float displayTime = 1.5f;

    private void Start()
    {
        if (panel)
            panel.SetActive(false);
    }

    public void ShowAndExit(string sceneName)
    {
        StartCoroutine(ShowRoutine(sceneName));
    }

    private IEnumerator ShowRoutine(string sceneName)
    {
        if (panel)
            panel.SetActive(true);

        yield return new WaitForSecondsRealtime(displayTime);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}