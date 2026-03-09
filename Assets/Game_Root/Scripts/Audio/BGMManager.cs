using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Daftarkan diri ke Global Manager biar bisa dikontrol
        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.RegisterBGM(audioSource);
        }
    }

    // Fungsi tambahan kalau lo mau ganti lagu via kode di masa depan
    public void ChangeTrack(AudioClip newClip)
    {
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
    }
}