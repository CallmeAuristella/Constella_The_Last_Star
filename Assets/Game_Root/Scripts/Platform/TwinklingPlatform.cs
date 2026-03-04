using UnityEngine;
using TMPro; // Wajib
using System.Collections;

public class TwinklingPlatform : MonoBehaviour
{
    [Header("Timing Settings")]
    public float activeDuration = 3f;
    public float inactiveDuration = 2f;

    [Header("Clean Text Reference (No Canvas)")]
    [Tooltip("Tarik objek TextMeshPro (Non-UI) ke sini")]
    public TextMeshPro timerText;

    private Collider2D col;
    private SpriteRenderer sr;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (timerText != null) timerText.text = "";
    }

    private void Start()
    {
        StartCoroutine(TwinkleRoutine());
    }

    private IEnumerator TwinkleRoutine()
    {
        while (true)
        {
            // --- FASE AKTIF ---
            TogglePlatform(true);
            float timeLeft = activeDuration;

            while (timeLeft > 0)
            {
                if (timerText != null)
                {
                    // Update angka tanpa desimal
                    timerText.text = Mathf.CeilToInt(timeLeft).ToString();

                    // Ganti warna pas mau abis
                    timerText.color = (timeLeft < 1.1f) ? Color.red : Color.white;
                }

                timeLeft -= Time.deltaTime;
                yield return null;
            }

            // --- FASE MATI ---
            TogglePlatform(false);
            if (timerText != null) timerText.text = "";

            yield return new WaitForSeconds(inactiveDuration);
        }
    }

    private void TogglePlatform(bool state)
    {
        col.enabled = state;

        // Atur visual platform (transparan pas mati)
        Color c = sr.color;
        c.a = state ? 1f : 0.1f;
        sr.color = c;
    }
}