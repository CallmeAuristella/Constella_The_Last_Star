using UnityEngine;

public class ResetConfirmPanel : MonoBehaviour
{
    public GameObject panel;

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ConfirmYes()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ResetGameProgress();
        panel.SetActive(false);
    }

    public void ConfirmNo()
    {
        Time.timeScale = 1f;
        panel.SetActive(false);
    }
}