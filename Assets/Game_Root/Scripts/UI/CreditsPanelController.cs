using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditsPanelController : MonoBehaviour {
    public Image displayImage;
    public Sprite[] pages;

    public Button nextButton;
    public Button prevButton;
    public TMP_Text pageText;

    int index = 0;

    void OnEnable() {
        index = 0;
        UpdatePage();
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

        prevButton.interactable = index > 0;
        nextButton.interactable = index < pages.Length - 1;

        if (pageText != null)
            pageText.text = (index + 1) + " / " + pages.Length;
    }
}