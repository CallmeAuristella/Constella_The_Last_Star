using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.RegisterBGM(audioSource);
        }
        else
        {
            Debug.LogWarning("[BGMManager] GlobalAudioManager belum siap!");
        }
    }

    public void ChangeTrack(AudioClip newClip)
    {
        if (newClip == null) return;

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();
    }
}