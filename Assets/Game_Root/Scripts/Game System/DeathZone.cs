using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [Header("Debug Visuals")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Ambil skrip Respawn yang ada di Player
            PlayerRespawn respawnScript = collision.GetComponent<PlayerRespawn>();

            if (respawnScript != null)
            {
                // Panggil fungsi yang sama dengan Obstacle agar durasi Fade & Delay identik
                respawnScript.DieAndRespawn();
            }
            else
            {
                // Fallback: Jika skrip respawn tidak ditemukan, gunakan GameManager (opsional)
                Debug.LogWarning($"[DeathZone] {collision.name} tidak punya skrip PlayerRespawn! Mencoba lewat GameManager...");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RespawnPlayer();
                }
            }
        }
    }

    // Visualisasi area di Editor
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}