using UnityEngine;

public class MenuPanelController : MonoBehaviour {
    public GameObject panelTutorial;
    public GameObject panelCredits;

    public void OpenTutorial() {
        panelTutorial.SetActive(true);
    }

    public void OpenCredits() {
        panelCredits.SetActive(true);
    }

    public void CloseTutorial() {
        panelTutorial.SetActive(false);
    }

    public void CloseCredits() {
        panelCredits.SetActive(false);
    }
}