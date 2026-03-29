using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ArchiveAchievementUI : MonoBehaviour {
    public Image icon;
    public TMP_Text label;
    public Image lockIcon;
    public TMP_Text descriptionText;
    private AchievementData currentData;
    private Action<AchievementData> onClick;
    

    public void Setup(AchievementData data, bool unlocked, Action<AchievementData> clickCallback) {
        currentData = data;
        onClick = clickCallback;

        label.text = data.title;
        icon.sprite = data.icon;

        

        SetLocked(!unlocked);
    }

    public void OnClick() {
        if (onClick != null && currentData != null) {
            onClick(currentData);
        }
    }

    void SetLocked(bool locked) {
        if (locked) {
            // 🔹 ICON: jadi gelap (silhouette feel)
            icon.color = new Color(0f, 0f, 0f, 0.4f);

            // 🔹 LABEL: masih terbaca tapi redup
            label.color = new Color(1f, 1f, 1f, 0.6f);
        } else {
            icon.color = Color.white;
            label.color = Color.white;
        }

        if (lockIcon != null)
            lockIcon.gameObject.SetActive(locked);
    }
}