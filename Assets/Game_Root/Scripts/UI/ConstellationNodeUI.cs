using UnityEngine;
using UnityEngine.UI;

public class ConstellationNodeUI : MonoBehaviour
{
    [Header("IDENTITY (WAJIB ISI SAMA DENGAN GAMEPLAY)")]
    [Tooltip("Contoh: 'Alnitak', 'Major_1', 'Minor_A'")]
    public string nodeID;

    [Header("Visual Settings (Color Tint)")]
    [Tooltip("Tarik komponen Image dari UI bintang ini ke sini")]
    public Image targetImage;

    [Tooltip("Warna saat bintang berhasil diambil (Nyala)")]
    public Color activeColor = Color.white;

    [Tooltip("Warna saat bintang belum diambil (Mati/Redup)")]
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Status (Read Only)")]
    public bool isActivated = false;

    private void Awake()
    {
        // Safety check: Kalau lupa narik Image di Inspector, ambil otomatis
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        // Pastikan game dimulai dengan kondisi bintang mati
        ResetUI();
    }

    public void SetActive()
    {
        isActivated = true;

        // Ubah ke warna nyala
        targetImage.color = activeColor;

        // Efek Visual Pop dikit pake LeanTween biar kerasa memuaskan
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;
        LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.2f).setEasePunch();
    }

    // Fungsi reset untuk fitur ulang level atau puzzle seperti rasi Orion
    public void ResetUI()
    {
        isActivated = false;

        // Kembalikan ke warna mati/redup
        targetImage.color = inactiveColor;
    }
}