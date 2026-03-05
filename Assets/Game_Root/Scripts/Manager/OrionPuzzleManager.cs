using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class OrionPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Urutan Wajib: 1. Alnitak, 2. Alnilam, 3. Mintaka")]
    public List<StarNode> starSequence;

    [Header("Specialist HUD Reference")]
    // Pastikan nama variabel ini 'orionHud', jangan 'hudManager' biar gak bentrok
    public OrionHUDManager orionHud;

    [Header("Puzzle Events")]
    public UnityEvent OnPuzzleSolved;
    public UnityEvent OnPuzzleFailed;

    private int _currentIndex = 0;
    private bool _isSolved = false;

    public void RegisterStarActivation(StarNode activatedStar)
    {
        if (_isSolved || starSequence == null || starSequence.Count == 0) return;

        // 1. CEK URUTAN
        if (activatedStar == starSequence[_currentIndex])
        {
            Debug.Log($"[Puzzle] Step {_currentIndex + 1} OK");

            // Panggil fungsi UpdateOrionStep di OrionHUDManager
            if (orionHud != null)
            {
                orionHud.UpdateOrionStep(_currentIndex, true);
            }

            _currentIndex++;

            if (_currentIndex >= starSequence.Count)
            {
                _isSolved = true;
                OnPuzzleSolved?.Invoke();
            }
        }
        else
        {
            // 2. SALAH URUTAN (Hanya reset jika bintang bagian dari puzzle)
            if (starSequence.Contains(activatedStar))
            {
                ResetPuzzle();
            }
        }
    }

    public void ResetPuzzle()
    {
        _currentIndex = 0;
        _isSolved = false;
        OnPuzzleFailed?.Invoke();

        foreach (StarNode star in starSequence)
        {
            if (star != null) star.ResetNode();
        }

        // Pastikan memanggil ResetOrionHUD() yang ada di OrionHUDManager
        if (orionHud != null)
        {
            orionHud.ResetOrionHUD();
        }
    }
}