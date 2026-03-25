using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Smoothing")]
    [SerializeField] private float smoothingSpeed = 6f;

    const string MUSIC_PARAM = "MusicVol";
    const string SFX_PARAM = "SFXVol";

    const string SAVE_BGM = "SavedBGM";
    const string SAVE_SFX = "SavedSFX";

    const float MIN_VOLUME = 0.0001f;

    Coroutine bgmRoutine;
    Coroutine sfxRoutine;


    private void Awake()
    {
        LoadSavedVolumes(); // 🔥 pindahin ke Awake
    }
    private void Start()
    {
        HookSliderEvents();
    }

    void HookSliderEvents()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    
    

    public void SetBGMVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, 1f);

        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        bgmRoutine = StartCoroutine(SmoothVolume(MUSIC_PARAM, value));

        PlayerPrefs.SetFloat(SAVE_BGM, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, 1f);

        if (sfxRoutine != null)
            StopCoroutine(sfxRoutine);

        sfxRoutine = StartCoroutine(SmoothVolume(SFX_PARAM, value));

        PlayerPrefs.SetFloat(SAVE_SFX, value);
        PlayerPrefs.Save();
    }

    IEnumerator SmoothVolume(string parameter, float targetLinear)
    {
        float currentDB;
        mainMixer.GetFloat(parameter, out currentDB);

        float targetDB = Mathf.Log10(targetLinear) * 20f;

        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * smoothingSpeed;

            float newDB = Mathf.Lerp(currentDB, targetDB, time);
            mainMixer.SetFloat(parameter, newDB);

            yield return null;
        }

        mainMixer.SetFloat(parameter, targetDB);
    }

    void ApplyVolumeImmediate(string parameter, float linear)
    {
        float dB = Mathf.Log10(linear) * 20f;
        mainMixer.SetFloat(parameter, dB);
    }
    void LoadSavedVolumes()
    {
        if (!PlayerPrefs.HasKey(SAVE_BGM))
            PlayerPrefs.SetFloat(SAVE_BGM, 0.75f);

        if (!PlayerPrefs.HasKey(SAVE_SFX))
            PlayerPrefs.SetFloat(SAVE_SFX, 0.75f);

        float savedBGM = PlayerPrefs.GetFloat(SAVE_BGM);
        float savedSFX = PlayerPrefs.GetFloat(SAVE_SFX);

        ApplyVolumeImmediate(MUSIC_PARAM, savedBGM);
        ApplyVolumeImmediate(SFX_PARAM, savedSFX);

        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(savedBGM);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(savedSFX);
    }
}