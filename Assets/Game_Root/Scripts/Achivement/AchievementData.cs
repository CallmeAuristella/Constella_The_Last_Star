using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Game/Achievement")]
public class AchievementData : ScriptableObject {
    public AchievementType type;

    public string title;
    public string description;

    public Sprite icon;
}