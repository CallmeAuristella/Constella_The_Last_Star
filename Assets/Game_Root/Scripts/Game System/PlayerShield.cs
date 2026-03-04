using UnityEngine;
using System.Collections;

public class PlayerShield : MonoBehaviour
{
    [Header("Shield Setup")]
    [Tooltip("Masukkan Game Object ANAK yang punya Tag 'Shield' ke sini.")]
    public GameObject shieldObject;

    // Memori untuk menyimpan tugas hitung mundur
    private Coroutine shieldCoroutine;

    private void Start()
    {
        // KUNCI KEAMANAN: Pastikan tameng mati total saat game baru mulai
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
    }

    // Fungsi ini DIBUKA UNTUK UMUM agar bisa dipanggil oleh Item Pick Up
    public void ActivateShield(float duration)
    {
        if (shieldObject == null)
        {
            Debug.LogError("[PlayerShield] Objek shield belum dimasukkan ke Inspector!");
            return;
        }

        // Kalau player ambil shield lagi pas shield masih nyala, batalkan hitungan lama
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        // Mulai hitung mundur durasi baru
        shieldCoroutine = StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        // FASE 1: NYALAKAN SHIELD (Visual & Fisika pelindung aktif)
        shieldObject.SetActive(true);

        // FASE 2: TUNGGU DURASI HABIS
        yield return new WaitForSeconds(duration);

        // FASE 3: MATIKAN SHIELD TOTAL (Reset status ke normal)
        shieldObject.SetActive(false);
        shieldCoroutine = null;
    }
}