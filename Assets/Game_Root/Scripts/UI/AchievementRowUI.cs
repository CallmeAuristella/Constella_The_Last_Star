using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AchievementRowUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    [SerializeField] private Sprite noDeathIcon;
    [SerializeField] private Sprite godSpeedIcon;
    [SerializeField] private Sprite starCollectorIcon;
    [SerializeField] private Sprite skillIssueIcon;

    public CanvasGroup canvasGroup;
    public RectTransform rect;

    public void Setup(AchievementType type)
    {
        switch (type)
        {
            case AchievementType.NoDeath_Run:
                label.text = "No Death";
                icon.sprite = noDeathIcon;
                break;

            case AchievementType.GodSpeed:
                label.text = "God Speed";
                icon.sprite = godSpeedIcon;
                break;

            case AchievementType.StarCollector:
                label.text = "Star Collector";
                icon.sprite = starCollectorIcon;
                break;

            case AchievementType.SkillIssue:
                label.text = "Skill Issue";
                icon.sprite = skillIssueIcon;
                break;
        }
    }

    string GetLabel(AchievementType type)
    {
        switch (type)
        {
            case AchievementType.NoDeath_Run: return "No Death";
            case AchievementType.StarCollector: return "Star Collector";
            case AchievementType.GodSpeed: return "God Speed";
            case AchievementType.SkillIssue: return "Skill Issue";
        }

        return type.ToString();
    }

    public void Prepare()
    {
        canvasGroup.alpha = 0;
        rect.localScale = Vector3.one * 0.8f;
    }
    public void SetLocked(bool locked)
    {
        if (locked)
        {
            icon.color = new Color(1f, 1f, 1f, 0.2f);
            label.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            icon.color = Color.white;
            label.color = Color.white;
        }
    }
    public void SetHighlight()
    {
        // misalnya sedikit lebih terang / beda tone
        icon.color = new Color(1f, 1f, 1f, 1f);
    }
    public IEnumerator PlayReveal()
    {
        float t = 0f;
        float duration = 0.25f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float p = t / duration;

            canvasGroup.alpha = p;
            rect.localScale = Vector3.Lerp(
                Vector3.one * 0.8f,
                Vector3.one,
                p
            );

            yield return null;
        }

        canvasGroup.alpha = 1;
        rect.localScale = Vector3.one;
    }
}