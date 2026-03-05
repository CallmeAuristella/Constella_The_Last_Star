using UnityEngine;
using System.Collections.Generic;

public class OrionHUDManager : MonoBehaviour
{
    [Header("Orion IDs Configuration")]
    [Tooltip("Isi dengan ID yang sama persis dengan yang ada di StarNode & ConstellationManager")]
    public List<string> orionStarIDs = new List<string> { "Alnitak", "Alnilam", "Mintaka" };

    // Dipanggil oleh OrionPuzzleManager saat urutan benar
    public void UpdateOrionStep(int index, bool state)
    {
        if (index >= 0 && index < orionStarIDs.Count)
        {
            string targetID = orionStarIDs[index];
            ConstellationManager.Instance.SetStarStatus(targetID, state);
        }
    }

    // Dipanggil saat salah urutan (Reset)
    public void ResetOrionHUD()
    {
        if (ConstellationManager.Instance != null)
        {
            ConstellationManager.Instance.ResetNodesByIDs(orionStarIDs);
        }
    }
}