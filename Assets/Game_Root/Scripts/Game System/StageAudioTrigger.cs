using UnityEngine;

public class StageAudioTrigger : MonoBehaviour
{
    public AudioClip stageBGM;

    void Start()
    {
        if (GlobalAudioManager.Instance != null && stageBGM != null)
        {
            GlobalAudioManager.Instance.PlayMusic(stageBGM);
            // Fungsi PlayMusic harus berisi audioSource.clip = clip; audioSource.Play();
        }
    }
}