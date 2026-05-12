using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class CreditsPanelController : MonoBehaviour {
    [SerializeField] private Button closeButton;

    public Image displayImage;
    public Sprite[] pages;

    public Button nextButton;
    public Button prevButton;
    public TMP_Text pageText;

    private GameObject previousSelected;
    [SerializeField] private GameObject fallbackSelected;

    int index = 0;

    void OnEnable() {
        previousSelected =
            EventSystem.current.currentSelectedGameObject;

        index = 0;
        UpdatePage();

        StartCoroutine(FocusCloseNextFrame());
    }
    public void Close() {
        gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if (previousSelected != null) {
            EventSystem.current.SetSelectedGameObject(previousSelected);
        } else {
            EventSystem.current.SetSelectedGameObject(fallbackSelected);
        }
    }

    private IEnumerator FocusCloseNextFrame() {
        yield return null;

        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(
            closeButton.gameObject
        );
    }

    public void Next() {
        if (index < pages.Length - 1) {
            index++;
            UpdatePage();
        }
    }

    public void Prev() {
        if (index > 0) {
            index--;
            UpdatePage();
        }
    }

    void UpdatePage() {
        displayImage.sprite = pages[index];


        if (pageText != null)
            pageText.text = (index + 1) + " / " + pages.Length;
    }
}