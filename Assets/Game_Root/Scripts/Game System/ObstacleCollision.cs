using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    // Fungsi untuk tabrakan fisik (Box Collider biasa)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KillPlayer();
        }
    }

    // Fungsi untuk tabrakan tembus (Is Trigger dicentang)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        // Pastikan GameManager ada sebelum panggil Respawn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
        }
        else
        {
            Debug.LogError("GameManager tidak ditemukan! Player harusnya mati disini.");
        }
    }
}