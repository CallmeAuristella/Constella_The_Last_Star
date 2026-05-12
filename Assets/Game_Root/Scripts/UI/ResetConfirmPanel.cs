using UnityEngine;
using UnityEngine.EventSystems;

public class ResetConfirmPanel : MonoBehaviour {
    public GameObject panel;

    [Header("Focus")]
    [SerializeField] private GameObject firstSelectedButton;

    private GameObject previousSelected;

    public void Show() {
        previousSelected =
            EventSystem.current.currentSelectedGameObject;

        panel.SetActive(true);

        Time.timeScale = 0f;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void ConfirmYes() {
        Time.timeScale = 1f;

        GameManager.Instance.ResetGameProgress();

        ClosePanel();
    }

    public void ConfirmNo() {
        Time.timeScale = 1f;

        ClosePanel();
    }

    void ClosePanel() {
        panel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if (previousSelected != null) {
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }
    }
}