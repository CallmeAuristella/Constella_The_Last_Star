using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OrionHUDManager : MonoBehaviour
{
    [Header("Orion IDs")]
    public List<string> orionStarIDs = new List<string> { "Alnitak", "Alnilam", "Mintaka" };

    [Header("UI References")]
    public List<Image> hudStars;

    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.2f);

    void Update()
    {
        if (ConstellationManager.Instance == null) return;

        for (int i = 0; i < orionStarIDs.Count; i++)
        {
            bool active = ConstellationManager.Instance.IsCollected(orionStarIDs[i]);

            if (i < hudStars.Count && hudStars[i] != null)
            {
                hudStars[i].color = active ? activeColor : inactiveColor;
            }
        }
    }

    // 🔥 OPTIONAL: reset visual aja (BUKAN data global)
    public void ResetOrionHUDVisual()
    {
        foreach (var star in hudStars)
        {
            if (star != null)
                star.color = inactiveColor;
        }
    }
}