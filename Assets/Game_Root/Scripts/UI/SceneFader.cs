using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFader : MonoBehaviour
{
    private CanvasGroup group;

    private void Awake()
    {
        // Ambil komponen CanvasGroup
        group = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // 1. WAJIB HILANG PAS MULAI (Alpha 0)
        // Ini kunci biar layar gak gelap pas start, walau objeknya Active.
        group.alpha = 0;
        group.blocksRaycasts = false; // Biar mouse bisa tembus klik

        // 2. LAPOR KE GAMEMANAGER
        if (GameManager.Instance != null)
        {
            GameManager.Instance.transitionScreen = group;
        }
    }
}