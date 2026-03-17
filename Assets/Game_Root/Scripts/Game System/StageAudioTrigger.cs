using UnityEngine;
using System.Collections;

public class StageAudioTrigger : MonoBehaviour
{
    public AudioClip stageBGM;

    IEnumerator Start()
    {
        yield return null; // 🔥 WAJIB: tunggu BGMManager register dulu

        if (GlobalAudioManager.Instance != null && stageBGM != null)
        {
            GlobalAudioManager.Instance.PlayMusic(stageBGM);
        }
        else
        {
            Debug.LogWarning("[StageAudioTrigger] BGM tidak tersedia!");
        }
    }
}