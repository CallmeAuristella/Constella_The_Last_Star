using UnityEngine;
using TMPro; // WAJIB ADA BUAT TEXT MESH PRO

public class GameHUD : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText; // Drag Txt_Timer kesini
    public TextMeshProUGUI scoreText; // Drag Txt_Score kesini

    private void Update()
    {
        // Kita update setiap frame biar real-time
        if (GameManager.Instance != null)
        {
            // 1. UPDATE TIMER
            // Kita pake fungsi GetFormattedTime() yang udah kita bikin di GameManager
            if (timerText != null)
            {
                timerText.text = GameManager.Instance.GetFormattedTime();
            }

            // 2. UPDATE SCORE
            if (scoreText != null)
            {
                scoreText.text = "SCORE: " + GameManager.Instance.currentScore.ToString();
            }
        }
    }
}