using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementDetailPopup : MonoBehaviour {
    public GameObject root;

    public Image icon;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public void Show(AchievementData data, bool unlocked) {
        root.SetActive(true);

        icon.sprite = data.icon;
        titleText.text = data.title;

        if (unlocked) {
            descriptionText.text = data.description;
            icon.color = Color.white;
        } else {
            descriptionText.text = "????";
            icon.color = new Color(0f, 0f, 0f, 0.4f); // optional silhouette
        }
    }

    public void Hide() {
        root.SetActive(false);
    }
}