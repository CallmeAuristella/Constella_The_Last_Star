using UnityEngine;

public class MainMenuInitializer : MonoBehaviour
{
    private AudioSource localBGM;

    private void Awake()
    {
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        Time.timeScale = 1f;

        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.ResetForMenu(); // ✅ FIX
        }

        localBGM = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (localBGM == null) return;

        if (GlobalAudioManager.Instance != null)
        {
            GlobalAudioManager.Instance.RegisterBGM(localBGM);
        }

        if (!localBGM.isPlaying)
        {
            localBGM.loop = true;
            localBGM.Play();
        }

        Debug.Log("[MainMenu] BGM Started");
    }
}