using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArchiveItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    public void Setup(ConstellationData data, bool unlocked, System.Action onClick)
    {
        if (unlocked)
        {
            icon.sprite = data.iconUnlocked;
            icon.color = Color.white;
            label.text = data.displayName;

            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }
        else
        {
            icon.sprite = data.iconLocked;
            icon.color = Color.gray;
            label.text = "???";

            button.interactable = false;
        }
    }
}