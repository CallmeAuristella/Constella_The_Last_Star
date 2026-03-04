using UnityEngine;

[CreateAssetMenu(fileName = "New Constellation", menuName = "Cosmic/Constellation Data")]
public class ConstellationData : ScriptableObject
{
    [Header("Identitas")]
    public string id;
    public string displayName;

    [Header("Visual")]
    public Sprite iconUnlocked;    // Icon kecil di Grid
    public Sprite iconLocked;      // Icon Gembok

    [Header("Infographic")]
    public Sprite infographicImage; // Gambar GEDE (Desain lo yang udah jadi satu sama teks)

    [Header("Syarat Unlock")]
    public int requiredStageIndex;
}