using UnityEngine;
using System.Collections;

public class PlayerShield : MonoBehaviour
{
    [Header("Visual Components")]
    [Tooltip("Masukkan SpriteRenderer tameng di sini.")]
    public SpriteRenderer shieldSprite;

    [Header("Status")]
    public bool isShieldActive = false;

    private Coroutine shieldCoroutine;

    private void Awake()
    {
        // Jika lupa mengisi di Inspector, coba cari di anak objek
        if (shieldSprite == null) shieldSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        // Pastikan saat game mulai, shield mati total
        isShieldActive = false;
        if (shieldSprite != null) shieldSprite.enabled = false;
    }

    public void ActivateShield(float duration)
    {
        // Jika sedang aktif, stop coroutine yang lama agar durasi ter-reset (Refresh)
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        shieldCoroutine = StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        // FASE AKTIF
        isShieldActive = true;
        if (shieldSprite != null) shieldSprite.enabled = true;
        Debug.Log("[Shield] System & Sprite: ON");

        yield return new WaitForSeconds(duration);

        // FASE MATI
        DeactivateShieldInternal();
    }

    // Fungsi internal untuk memastikan reset status bersih
    private void DeactivateShieldInternal()
    {
        isShieldActive = false;
        if (shieldSprite != null) shieldSprite.enabled = false;

        shieldCoroutine = null;
        Debug.Log("[Shield] System & Sprite: OFF");
    }

    // Gunakan ini jika player mati atau level reset secara paksa
    public void ForceDeactivate()
    {
        if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
        isShieldActive = false;
        if (shieldSprite != null) shieldSprite.enabled = false;
        shieldCoroutine = null;
    }
}