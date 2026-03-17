using UnityEngine;

[CreateAssetMenu(fileName = "New Constellation", menuName = "Cosmic/Constellation Data")]
public class ConstellationData : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("Grid Icons")]
    public Sprite iconUnlocked;
    public Sprite iconLocked;

    [Header("Unlock Requirement")]
    public int requiredStageIndex;

    [Header("Infographic Pages")]
    public Sprite[] infographicPages;
}