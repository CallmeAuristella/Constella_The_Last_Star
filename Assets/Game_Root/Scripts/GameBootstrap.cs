using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Instantiate(gameManagerPrefab);
        }
    }
}