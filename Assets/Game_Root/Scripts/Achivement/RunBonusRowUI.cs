using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunBonusRowUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text label;
    public TMP_Text valueText;

    [SerializeField] private Sprite nodeBonusIcon;
    [SerializeField] private Sprite noDeathIcon;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite silverIcon;
    [SerializeField] private Sprite bronzeIcon;

    public void Setup(RunBonusType type, int value)
    {
        label.text = GetLabel(type);
        valueText.text = "+" + value.ToString();

        icon.sprite = GetIcon(type);
    }
    Sprite GetIcon(RunBonusType type) {
        switch (type) {
            case RunBonusType.NodeBonus:
                return nodeBonusIcon;

            case RunBonusType.NoDeathBonus:
                return noDeathIcon;

            case RunBonusType.FastGold:
                return goldIcon;

            case RunBonusType.FastSilver:
                return silverIcon;

            case RunBonusType.FastBronze:
                return bronzeIcon;
        }

        return null;
    }

    string GetLabel(RunBonusType type)
    {
        switch (type)
        {
            case RunBonusType.NodeBonus: return "Node Bonus";
            case RunBonusType.NoDeathBonus: return "No Death Bonus";
            case RunBonusType.FastGold: return "Fast Run (Gold)";
            case RunBonusType.FastSilver: return "Fast Run (Silver)";
            case RunBonusType.FastBronze: return "Fast Run (Bronze)";
        }

        return type.ToString();
    }
}