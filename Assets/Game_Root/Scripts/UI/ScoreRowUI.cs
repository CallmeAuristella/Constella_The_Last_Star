using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreRowUI : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI valueText;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();

        if (labelText == null || valueText == null)
        {
            Debug.LogError("ScoreRowUI: Text reference missing!", this);
        }
    }

    public void PrepareForAnimation()
    {
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.one * 0.8f;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void PlayRevealAnimation(float duration = 0.25f, bool isPunch = false)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateReveal(duration, isPunch));
    }

    private System.Collections.IEnumerator AnimateReveal(float duration, bool isPunch)
    {
        float time = 0f;

        float startScale = 0.8f;
        float endScale = isPunch ? 1.1f : 1f; // 🔥 lebih besar untuk TOTAL

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            t = 1f - Mathf.Pow(1f - t, 3f);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            float scale = Mathf.Lerp(startScale, endScale, t);
            rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;
    }

    public void Setup(string labelString, int value, bool isTotal = false, bool isBonus = false)
    {
        labelText.text = labelString;

        if (labelString == "Base Score" || isTotal)
            valueText.text = value.ToString();
        else
            valueText.text = "+" + value.ToString();

        labelText.fontSize = 22;
        valueText.fontSize = 22;

        labelText.fontStyle = FontStyles.Normal;
        valueText.fontStyle = FontStyles.Normal;

        Color baseColor = new Color(0.13f, 0.15f, 0.28f);
        labelText.color = baseColor;
        valueText.color = baseColor;

        if (isBonus && !isTotal)
        {
            Color bonusColor = new Color(0.57f, 0.83f, 0.33f);
            labelText.color = bonusColor;
            valueText.color = bonusColor;
        }

        if (isTotal)
        {
            labelText.fontSize = 28;
            valueText.fontSize = 28;

            labelText.fontStyle = FontStyles.Bold;
            valueText.fontStyle = FontStyles.Bold;

            Color totalColor = new Color(1f, 0.84f, 0.3f);
            labelText.color = totalColor;
            valueText.color = totalColor;
        }
    }
}