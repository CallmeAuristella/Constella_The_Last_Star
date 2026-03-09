using UnityEngine;

public class MainMenuInitializer : MonoBehaviour
{
    private AudioSource localBGM;

    private void Awake()
    {
        // 1. PAKSA ENGINE AUDIO AKTIF
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        Time.timeScale = 1f;

        // 2. OPEN MIXER VIA GLOBAL MANAGER
        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.ResetMixerForMenu();
        }

        localBGM = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // 3. KICKSTART MUSIK LOKAL
        if (localBGM != null && !localBGM.isPlaying)
        {
            localBGM.Play();
            Debug.Log("[Initialiser] BGM Main Menu Started.");
        }
    }
}