using TMPro;
using UnityEngine;

public class ScoreRowUI : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI valueText;

    public void Setup(string label, int value, bool isTotal = false)
    {
        labelText.text = label;

        // ===== VALUE FORMAT =====
        if (label == "Base Score" || isTotal)
            valueText.text = value.ToString();
        else
            valueText.text = "+" + value.ToString();

        // ===== RESET STYLE =====
        labelText.fontSize = 22;
        valueText.fontSize = 22;

        labelText.fontStyle = FontStyles.Normal;
        valueText.fontStyle = FontStyles.Normal;

        Color baseColor = new Color(0.13f, 0.15f, 0.28f); // #202743
        labelText.color = baseColor;
        valueText.color = baseColor;

        // ===== BONUS DETECTION (FIX) =====
        bool isBonus =
            label.Contains("Bonus") ||
            label.Contains("Node");

        if (isBonus && !isTotal)
        {
            Color bonusColor = new Color(0.57f, 0.83f, 0.33f); // #91D455

            labelText.color = bonusColor;   // 🔥 label ikut berubah
            valueText.color = bonusColor;
        }

        // ===== TOTAL OVERRIDE =====
        if (isTotal)
        {
            labelText.fontSize = 28;
            valueText.fontSize = 28;

            labelText.fontStyle = FontStyles.Bold;
            valueText.fontStyle = FontStyles.Bold;

            Color totalColor = new Color(1f, 0.84f, 0.3f); // gold
            labelText.color = totalColor;
            valueText.color = totalColor;
        }
    }
}