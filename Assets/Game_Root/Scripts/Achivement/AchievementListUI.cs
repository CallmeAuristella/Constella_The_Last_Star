using System.Collections.Generic;
using UnityEngine;

public class AchievementListUI : MonoBehaviour
{
    public Transform container;
    public AchievementRowUI prefab;

    public void Show(List<AchievementType> achievements)
    {
        // clear lama
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // spawn baru
        foreach (var type in achievements)
        {
            var row = Instantiate(prefab, container);
            row.Setup(type);
            row.Prepare();
        }
    }
}