using UnityEngine;

public class ResetButtonHandler : MonoBehaviour
{
    public void OnResetClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameProgress();
        }
        else
        {
            Debug.LogError("GameManager Instance NULL!");
        }
    }
}