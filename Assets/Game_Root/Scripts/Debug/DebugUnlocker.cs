using UnityEngine;

public class DebugUnlocker : MonoBehaviour
{
    // Pasang ini di objek kosong di Main Menu
    // Tekan U di keyboard pas Play Mode buat unlock semua
    // Tekan R buat reset (kunci lagi)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            PlayerPrefs.SetInt("Stage_1_Complete", 1);
            PlayerPrefs.SetInt("Stage_2_Complete", 1);
            PlayerPrefs.SetInt("Stage_3_Complete", 1);
            PlayerPrefs.Save();
            Debug.Log("CHEAT: Semua Rasi Terbuka!");

            // Refresh Gallery biar langsung keliatan
            FindObjectOfType<GalleryManager>().RefreshGallery();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("RESET: Data Dihapus!");
            FindObjectOfType<GalleryManager>().RefreshGallery();
        }
    }
}