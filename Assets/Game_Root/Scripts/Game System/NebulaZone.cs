using UnityEngine;

public class NebulaZone : MonoBehaviour
{
    [Header("Nebula Settings")]
    public float nebulaGravity = 0.4f;   // Gravitasi rendah (melayang)
    public float nebulaDrag = 3f;        // Gesekan udara (biar gak ngebut)
    public float nebulaSpeed = 6f;       // Speed limit di dalam nebula
    public float nebulaFallMult = 1.2f;    // Matikan fast falling (biar gak gedebuk)
    

    // Simpan data asli
    private float defaultGravity;
    private float defaultDrag;
    private float defaultSpeed;
    private float defaultFallMult;
    private float defaultApexMult;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerMovementInput>();
            var rb = collision.GetComponent<Rigidbody2D>();

            if (player != null && rb != null)
            {
                // 1. Backup data asli
                defaultGravity = rb.gravityScale;
                defaultDrag = player.airDrag;
                defaultSpeed = player.moveSpeed;
                defaultFallMult = player.fallMultiplier;

                // 2. Apply Nebula Physics
                rb.gravityScale = nebulaGravity;
                player.airDrag = nebulaDrag;
                player.moveSpeed = nebulaSpeed;
                player.fallMultiplier = nebulaFallMult; // Penting biar gak ditarik ke bawah
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerMovementInput>();
            var rb = collision.GetComponent<Rigidbody2D>();

            if (player != null && rb != null)
            {
                // 3. Restore data asli
                rb.gravityScale = defaultGravity;
                player.airDrag = defaultDrag;
                player.moveSpeed = defaultSpeed;
                player.fallMultiplier = defaultFallMult;
            }
        }
    }
}