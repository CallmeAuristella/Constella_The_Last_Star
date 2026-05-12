using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AchievementDetailPopup : MonoBehaviour {
    public GameObject root;

    public Image icon;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [SerializeField] private GameObject closeButton;

    private GameObject previousSelected;
    private GameObject sourceButton;

    public void Show(
    AchievementData data,
    bool unlocked,
    GameObject source) {
        sourceButton = source;

        root.SetActive(true);

        icon.sprite = data.icon;
        titleText.text = data.title;

        if (unlocked) {
            descriptionText.text = data.description;
            icon.color = Color.white;
        } else {
            descriptionText.text = "????";
            icon.color = new Color(0f, 0f, 0f, 0.4f);
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(closeButton);
    }

    public void Hide() {
        root.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if (sourceButton != null) {
            EventSystem.current.SetSelectedGameObject(sourceButton);
        }
    }
}