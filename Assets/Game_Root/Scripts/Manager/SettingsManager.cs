using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("Mixer Reference")]
    public AudioMixer mainMixer;

    [Header("Slider References")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Navigation")]
    [SerializeField] private GameObject previousPanel;

    private const string BGM_PARAMS = "MusicVol";
    private const string SFX_PARAMS = "SFXVol";

    private void OnEnable()
    {
        SyncSlidersWithMixer();
    }

    private void SyncSlidersWithMixer()
    {
        if (mainMixer == null) return;

        float bgmValue, sfxValue;

        // Ambil dB dari Mixer
        mainMixer.GetFloat(BGM_PARAMS, out bgmValue);
        mainMixer.GetFloat(SFX_PARAMS, out sfxValue);

        // Konversi dB balik ke Linear (0.0001 - 1) untuk visual Slider
        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(Mathf.Pow(10, bgmValue / 20));

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(Mathf.Pow(10, sfxValue / 20));
    }

    public void SetBGMVolume(float volume)
    {
        mainMixer.SetFloat(BGM_PARAMS, Mathf.Log10(volume) * 20);
        // SAVE: Simpan nilai slider (0-1) ke memori
        PlayerPrefs.SetFloat("SavedBGM", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat(SFX_PARAMS, Mathf.Log10(volume) * 20);
        // SAVE: Simpan nilai slider (0-1) ke memori
        PlayerPrefs.SetFloat("SavedSFX", volume);
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
        if (previousPanel != null) previousPanel.SetActive(true);
    }
}