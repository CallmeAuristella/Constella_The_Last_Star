using UnityEngine;
using UnityEngine.EventSystems;

public class MenuPanelController : MonoBehaviour {
    public GameObject panelTutorial;
    public GameObject panelCredits;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialCloseButton;

    private GameObject previousSelected;

    public void OpenTutorial() {
        previousSelected =
            EventSystem.current.currentSelectedGameObject;

        panelTutorial.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(tutorialCloseButton);
    }
    public void OpenCredits() {
        panelCredits.SetActive(true);
    }

    public void CloseTutorial() {
        panelTutorial.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if (previousSelected != null) {
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }
    }

    public void CloseCredits() {
        panelCredits.SetActive(false);
    }
}