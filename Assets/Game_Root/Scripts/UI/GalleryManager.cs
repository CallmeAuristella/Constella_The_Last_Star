using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GalleryManager : MonoBehaviour {
    [Header("Data")]
    public System.Collections.Generic.List<ConstellationData> database;

    [Header("Grid Setup")]
    public Transform gridContainer;
    public GameObject itemPrefab;

    [Header("Detail Popup Controller")]
    [SerializeField] private DetailPopupController detailPopup;

    private void Start() {
        RefreshGallery();
    }

    public void RefreshGallery() {
        // Bersihkan item lama
        foreach (Transform child in gridContainer) {
            Destroy(child.gameObject);
        }

        // Generate item baru dari database
        foreach (var data in database) {
            GameObject item = Instantiate(itemPrefab, gridContainer);
            ArchiveItemUI ui = item.GetComponent<ArchiveItemUI>();

            bool unlocked = false;

            if (GameManager.Instance != null) {
                unlocked = GameManager.Instance.completedStages.Contains(data.requiredStageKey);
            } else {
                Debug.LogWarning("[Gallery] GameManager not ready!");
            }

            ui.Setup(data, unlocked, () => OpenDetail(data));
        }

        // DEBUG STATE
        if (GameManager.Instance != null) {
            Debug.Log("[Gallery] CompletedStages: " +
                string.Join(",", GameManager.Instance.completedStages));
        }

        Debug.Log("[Gallery] Gallery Refreshed.");
    }

    void OpenDetail(ConstellationData data) {
        if (detailPopup != null) {
            detailPopup.Show(data);
        }
    }

    [ContextMenu("DEBUG Unlock All Constellations")]
    public void DebugUnlockAll() {
        if (GameManager.Instance == null) return;

        foreach (var data in database) {
            GameManager.Instance.completedStages.Add(data.requiredStageKey);
        }

        GameManager.Instance.SaveGame();
        RefreshGallery();

        Debug.Log("[DEBUG] All constellations unlocked.");
    }

    [ContextMenu("DEBUG Lock All Constellations")]
    public void DebugLockAll() {
        if (GameManager.Instance == null) return;

        GameManager.Instance.completedStages.Clear();

        GameManager.Instance.SaveGame();
        RefreshGallery();

        Debug.Log("[DEBUG] All constellations locked.");
    }

}