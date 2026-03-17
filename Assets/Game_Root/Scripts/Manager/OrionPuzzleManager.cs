using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class OrionPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Urutan Wajib: 1. Alnitak, 2. Alnilam, 3. Mintaka")]
    public List<StarNode> starSequence;

    [Header("HUD Reference")]
    public OrionHUDManager orionHud;

    [Header("Puzzle Events")]
    public UnityEvent OnPuzzleSolved;
    public UnityEvent OnPuzzleFailed;

    private int _currentIndex = 0;
    private bool _isSolved = false;

    public void RegisterStarActivation(StarNode activatedStar)
    {
        if (_isSolved || starSequence == null || starSequence.Count == 0) return;

        // ✅ CEK URUTAN BENAR
        if (activatedStar == starSequence[_currentIndex])
        {
            Debug.Log($"[Puzzle] Step {_currentIndex + 1} OK");

            _currentIndex++;

            // ✅ PUZZLE SELESAI
            if (_currentIndex >= starSequence.Count)
            {
                _isSolved = true;
                Debug.Log("[Puzzle] Orion solved!");
                OnPuzzleSolved?.Invoke();
            }
        }
        else
        {
            // ❌ SALAH URUTAN
            if (starSequence.Contains(activatedStar))
            {
                Debug.Log("[Puzzle] Wrong order → reset");
                ResetPuzzle();
            }
        }
    }

    public void ResetPuzzle()
    {
        _currentIndex = 0;
        _isSolved = false;

        OnPuzzleFailed?.Invoke();

        // 🔁 Reset semua node gameplay
        foreach (StarNode star in starSequence)
        {
            if (star != null)
                star.ResetNode();
        }

        // 🔁 Reset HUD visual saja (tidak sentuh data global)
        if (orionHud != null)
        {
            orionHud.ResetOrionHUDVisual(); // ✅ FIX DI SINI
        }
    }
}