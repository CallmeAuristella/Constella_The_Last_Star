using UnityEngine;
using System.Collections;

public class DangerVisual : MonoBehaviour
{
    [Tooltip("Waktu sebelum warning ini hancur (Akan ditimpa otomatis oleh Spawner)")]
    public float lifetime = 1f;

    private void Start()
    {
        // Mulai hitung mundur kehancuran sejak objek ini lahir
        StartCoroutine(SafeDestroyRoutine());
    }

    private IEnumerator SafeDestroyRoutine()
    {
        // 1. Tunggu sampai durasi habis
        yield return new WaitForSeconds(lifetime);

        // 2. Matikan objeknya dari layar (Biar Inspector kehilangan jejak)
        gameObject.SetActive(false);

        // 3. [KODE ANTI ERROR EDITOR] 
        // Paksa Unity Editor melepas seleksi jika lo gak sengaja nge-klik objek ini
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.Selection.activeGameObject = null;
        }
#endif

        // 4. Hancurkan dari memori secara permanen
        Destroy(gameObject);
    }
}