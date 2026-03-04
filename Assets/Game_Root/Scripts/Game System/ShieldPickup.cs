using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Berapa lama shield bertahan saat item ini diambil?")]
    public float shieldDuration = 5f;

    [Header("Visual Effects")]
    public GameObject pickupVFX;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pastikan yang nabrak adalah Player
        if (collision.CompareTag("Player"))
        {
            // Cari komponen PlayerShield di karakter yang nabrak
            PlayerShield playerShield = collision.GetComponent<PlayerShield>();

            if (playerShield != null)
            {
                // Suruh player nyalain shield-nya sesuai durasi item ini
                playerShield.ActivateShield(shieldDuration);

                // Eksekusi kehancuran item ini
                ConsumeItem();
            }
        }
    }

    private void ConsumeItem()
    {
        // Munculkan efek partikel diambil (kalau ada)
        if (pickupVFX != null)
        {
            Instantiate(pickupVFX, transform.position, Quaternion.identity);
        }

        // Hancurkan item pelindung dari map
        Destroy(gameObject);
    }
}