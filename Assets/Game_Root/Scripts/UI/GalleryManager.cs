using UnityEngine;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    [Header("Data")]
    public List<ConstellationData> database;

    [Header("Grid Setup")]
    public Transform gridContainer;
    public GameObject itemPrefab;

    [Header("Detail Popup Controller")]
    [SerializeField] private DetailPopupController detailPopup;



    private void Start()
    {
        RefreshGallery();
    }

    public void RefreshGallery()
    {
        // Bersihkan item lama
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate item baru dari database
        foreach (var data in database)
        {
            GameObject item = Instantiate(itemPrefab, gridContainer);

            ArchiveItemUI ui = item.GetComponent<ArchiveItemUI>();

            bool unlocked =
                PlayerPrefs.GetInt($"{data.requiredStageKey}_Complete", 0) == 1;

            Debug.Log($"{data.requiredStageKey} = " + PlayerPrefs.GetInt($"{data.requiredStageKey}_Complete", 0));
            ui.Setup(data, unlocked, () => OpenDetail(data));


            
        }

        Debug.Log("[Gallery] Gallery Refreshed.");

        
    }

    void OpenDetail(ConstellationData data)
    {
        if (detailPopup != null)
        {
            detailPopup.Show(data);
        }
    }
    [ContextMenu("DEBUG Unlock All Constellations")]
    public void DebugUnlockAll()
    {
        foreach (var data in database)
        {
            PlayerPrefs.SetInt($"{data.requiredStageKey}_Complete", 1);
        }

        PlayerPrefs.Save();
        RefreshGallery();

        Debug.Log("All constellations unlocked (DEBUG).");
    }
    [ContextMenu("DEBUG Lock All Constellations")]
    public void DebugLockAll()
    {
        foreach (var data in database)
        {
            PlayerPrefs.DeleteKey($"{data.requiredStageKey }_Complete");
        }

        RefreshGallery();
    }
}