using UnityEngine;

public class ScorePickup : MonoBehaviour
{
    [Header("Value")]
    public int scoreAmount = 50; // Bisa diubah di inspector (30, 50, 100)

    [Header("Feedback")]
    public GameObject pickupEffect; // Partikel pas diambil (Opsional)
    public AudioClip pickupSound;   // Suara 'Cling' (Opsional)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Tambah Skor ke Manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreAmount);
            }

            // 2. Mainkan Efek/Suara
            if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
            if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            // 3. Hapus Objek Ini
            Destroy(gameObject);
        }
    }
}