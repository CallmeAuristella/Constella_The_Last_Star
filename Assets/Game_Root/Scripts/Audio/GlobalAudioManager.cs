using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    public AudioMixer mainMixer;

    [HideInInspector] public AudioSource currentBGM;

    private void Awake()
    {
        if (mainMixer == null)
        {
            Debug.LogError("[GlobalAudio] MainMixer belum dipasang!");
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadVolumeSettings();
        Debug.Log($"[Audio] Scene Loaded: {scene.name}");
    }

    // ======================================================
    // 🎧 BGM SYSTEM
    // ======================================================

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (currentBGM == null)
        {
            TryFindBGMSource();
        }

        if (currentBGM == null)
        {
            Debug.LogWarning("[GlobalAudio] BGM Source belum ditemukan!");
            return;
        }

        if (currentBGM.clip == clip && currentBGM.isPlaying)
            return;

        currentBGM.clip = clip;
        currentBGM.loop = true;
        currentBGM.Play();

        Debug.Log("[GlobalAudio] Playing: " + clip.name);
    }

    private void TryFindBGMSource()
    {
        var bgm = FindFirstObjectByType<BGMManager>();
        if (bgm != null)
        {
            RegisterBGM(bgm.GetComponent<AudioSource>());
        }
    }

    public void RegisterBGM(AudioSource source)
    {
        currentBGM = source;

        if (currentBGM != null)
        {
            currentBGM.loop = true;
            currentBGM.UnPause();
            Debug.Log("[GlobalAudio] BGM Registered: " + source.name);
        }
    }

    // ======================================================
    // 🔊 MIXER CONTROL
    // ======================================================

    public void LoadVolumeSettings()
    {
        float bVol = PlayerPrefs.GetFloat("SavedBGM", 0.75f);
        float sVol = PlayerPrefs.GetFloat("SavedSFX", 0.75f);

        ApplyVolume("MusicVol", bVol);
        ApplyVolume("SFXVol", sVol);
    }

    public void ApplyVolume(string param, float value)
    {
        float dB = value > 0.0001f ? Mathf.Log10(value) * 20 : -80f;
        mainMixer.SetFloat(param, dB);

        if (param == "MusicVol") PlayerPrefs.SetFloat("SavedBGM", value);
        if (param == "SFXVol") PlayerPrefs.SetFloat("SavedSFX", value);

        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value) => ApplyVolume("MusicVol", value);
    public void SetSFXVolume(float value) => ApplyVolume("SFXVol", value);

    // ======================================================
    // ⏸ STATE CONTROL
    // ======================================================

    public void StopAllGameplayAudio()
    {
        if (currentBGM != null && currentBGM.isPlaying)
            currentBGM.Pause();

        mainMixer.SetFloat("SFXVol", -80f);
        AudioListener.pause = true;
    }

    public void ResumeGameplayAudio()
    {
        AudioListener.pause = false;
        LoadVolumeSettings();

        if (currentBGM != null)
        {
            currentBGM.UnPause();
            if (!currentBGM.isPlaying)
                currentBGM.Play();
        }
    }

    public void ResetForMenu()
    {
        AudioListener.pause = false;
        LoadVolumeSettings();
        currentBGM = null;

        Debug.Log("[GlobalAudio] Reset for Menu");
    }
}