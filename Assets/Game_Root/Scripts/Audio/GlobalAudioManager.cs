using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;
    public AudioMixer mainMixer;

    // Referensi AudioSource (Bisa otomatis cari atau di-register)
    [HideInInspector] public AudioSource currentBGM;

    private void Awake()
    {
        if (mainMixer == null)
        {
            Debug.LogError("TOD! Main Mixer belum dipasang di GlobalAudioManager!");
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Konsistenkan Key PlayerPrefs dengan LoadVolumeSettings
        LoadVolumeSettings();
        Debug.Log($"[Audio Sync] Scene {scene.name} Loaded. Mixer Synced.");
    }

    // --- FIX ERROR: FUNGSI PLAYMUSIC (VITAL) ---
    public void PlayMusic(AudioClip clip)
    {
        // Jika currentBGM belum terdaftar, coba cari di scene
        if (currentBGM == null)
        {
            GameObject bgmObj = GameObject.Find("BGM_Source"); // Sesuaikan nama objek di scene
            if (bgmObj != null) currentBGM = bgmObj.GetComponent<AudioSource>();
        }

        if (currentBGM != null)
        {
            if (currentBGM.clip == clip && currentBGM.isPlaying) return;
            currentBGM.clip = clip;
            currentBGM.Play();
        }
        else
        {
            Debug.LogWarning("[GlobalAudio] Tidak ada AudioSource (currentBGM) untuk memutar musik!");
        }
    }

    // --- FUNGSI SETTINGS (KONTROL MIXER) ---

    public void LoadVolumeSettings()
    {
        if (mainMixer == null) return;

        // Gunakan KEY YANG KONSISTEN (SavedBGM & SavedSFX)
        float bVol = PlayerPrefs.GetFloat("SavedBGM", 0.75f);
        float sVol = PlayerPrefs.GetFloat("SavedSFX", 0.75f);

        ApplyVolume("MusicVol", bVol);
        ApplyVolume("SFXVol", sVol);
    }

    public void ApplyVolume(string parameterName, float linearVolume)
    {
        if (mainMixer == null) return;
        float dB = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20 : -80f;
        mainMixer.SetFloat(parameterName, dB);

        // Simpan agar saat pindah scene tidak reset
        if (parameterName == "MusicVol") PlayerPrefs.SetFloat("SavedBGM", linearVolume);
        if (parameterName == "SFXVol") PlayerPrefs.SetFloat("SavedSFX", linearVolume);
        PlayerPrefs.Save();
    }

    // --- FUNGSI MANAJEMEN BGM SCENE ---

    public void RegisterBGM(AudioSource source)
    {
        currentBGM = source;
        if (currentBGM != null)
        {
            currentBGM.UnPause();
            Debug.Log($"[GlobalAudio] BGM Scene '{source.gameObject.name}' terdaftar.");
        }
    }

    // --- FUNGSI KONTROL STATE (PAUSE/RESUME) ---

    public void StopAllGameplayAudio()
    {
        if (currentBGM != null && currentBGM.isPlaying) currentBGM.Pause();

        // Mute SFX via Mixer
        mainMixer.SetFloat("SFXVol", -80f);
        AudioListener.pause = true;
    }

    public void ResumeGameplayAudio()
    {
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        LoadVolumeSettings();

        if (currentBGM != null)
        {
            currentBGM.UnPause();
            if (!currentBGM.isPlaying) currentBGM.Play();
        }
    }

    public void ResetMixerForMenu()
    {
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        LoadVolumeSettings();
        currentBGM = null;
        Debug.Log("[GlobalAudio] Audio System Reset for Main Menu.");
    }
}