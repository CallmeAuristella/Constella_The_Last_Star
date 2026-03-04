using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("Hancur otomatis setelah X detik")]
    public float delay = 1.5f; // Sesuaikan sama durasi animasi ledakan lu

    void Start()
    {
        Destroy(gameObject, delay);
    }
}