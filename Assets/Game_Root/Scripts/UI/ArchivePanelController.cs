using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ArchivePanelController : MonoBehaviour {
    public Transform container;
    public GameObject itemPrefab;
    [SerializeField] private AchievementDetailPopup detailPopup;

    public List<AchievementData> achievementDatabase;

    // =========================
    // RUNTIME MODE
    // =========================
    private void OnEnable() {
        if (Application.isPlaying) {
            PopulateRuntime();
        }
    }

    // =========================
    // EDITOR POPULATE (MANUAL)
    // =========================
    void PopulateEditor() {
        if (container == null || itemPrefab == null) return;

#if UNITY_EDITOR
        // 🔥 SAFE CLEAR (editor)
        while (container.childCount > 0) {
            DestroyImmediate(container.GetChild(0).gameObject);
        }
#endif

        foreach (var data in achievementDatabase) {
#if UNITY_EDITOR
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(itemPrefab, container);
#else
            GameObject obj = Instantiate(itemPrefab, container);
#endif

            var ui = obj.GetComponent<ArchiveAchievementUI>();

            // preview = semua unlocked
            ui.Setup(data, true, OnItemClicked);
        }

        Debug.Log("[ARCHIVE] Editor Preview Generated");
    }

    // =========================
    // RUNTIME POPULATE
    // =========================
    void PopulateRuntime() {
        if (container == null || itemPrefab == null) return;

        foreach (Transform c in container)
            Destroy(c.gameObject);

        var gm = GameManager.Instance;

        if (gm == null) {
            Debug.LogWarning("[ARCHIVE] GameManager not found!");
            return;
        }

        foreach (var data in achievementDatabase) {
            var obj = Instantiate(itemPrefab, container);
            var ui = obj.GetComponent<ArchiveAchievementUI>();

            bool unlocked = gm.unlockedAchievements.Contains(data.type);

            ui.Setup(data, unlocked, OnItemClicked);
            Debug.Log($"[ACH] {data.type} unlocked = {unlocked}");
        }

        Debug.Log("[ARCHIVE] Runtime Populate Done");

    }
    void OnItemClicked(AchievementData data) {
        if (detailPopup != null) {
            bool unlocked = CheckIfUnlocked(data);
            detailPopup.Show(data, unlocked);
        }
    }
    bool CheckIfUnlocked(AchievementData data) {
        return GameManager.Instance.unlockedAchievements.Contains(data.type);
    }
    // =========================
    // DEBUG BUTTON (EDITOR)
    // =========================
    [ContextMenu("Generate Preview")]
    void GeneratePreview() {
        PopulateEditor();
    }

    [ContextMenu("Clear Grid")]
    void ClearGrid() {
#if UNITY_EDITOR
        if (container == null) return;

        while (container.childCount > 0) {
            DestroyImmediate(container.GetChild(0).gameObject);
        }

        Debug.Log("[ARCHIVE] Grid Cleared");
#endif
    }
}