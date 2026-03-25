using UnityEngine;
using System.Collections;

public class LevelFinishTrigger : MonoBehaviour
{
    [Header("Stage Config")]
    public int stageIndex;
    public bool isFinalStage;
    public string nextSceneName;

    [Header("References")]
    public StageSummaryController summary;

    private bool triggered = false;

    private bool isFinishing = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        if (isFinishing) yield break;
        isFinishing = true;

        Debug.Log("[FINISH] Triggered");
        GameManager.Instance.DebugRunState("BEFORE SAVE");
        GameManager.Instance.SaveCurrentStageStats();
        GameManager.Instance.DebugRunState("AFTER SAVE");

        // 🔥 Tunggu 1 frame biar semua node commit dulu
        yield return null;

        // 🔥 VALIDASI INSTANCE (HARUS DI ATAS)
        if (GameManager.Instance == null)
        {
            Debug.LogError("[FINISH] GameManager NULL");
            yield break;
        }

        // ======================================================
        // 🔥 CORE FLOW (URUTAN PENTING - JANGAN DIUBAH)
        // ======================================================

        // 1️⃣ SAVE CURRENT STAGE → KE GRAND TOTAL
        GameManager.Instance.SaveCurrentStageStats();

        // 2️⃣ MARK PROGRESSION (UNLOCK ARCHIVE)
        GameManager.Instance.CompleteStage(stageIndex);

        // 3️⃣ FINAL STAGE → SIMPAN RECORD GLOBAL
        if (isFinalStage)
        {
            GameManager.Instance.FinishGame();
        }

        Debug.Log($"[FINISH] Total Score: {GameManager.Instance.grandTotalScore}");

        // ======================================================
        // 🔥 TRIGGER SUMMARY
        // ======================================================

        if (summary != null)
        {
            summary.nextSceneName = nextSceneName;
            summary.StartSummarySequence();
        }
        else
        {
            Debug.LogError("[FINISH] Summary NULL");
        }
    }
}