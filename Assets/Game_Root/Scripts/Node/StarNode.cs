using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public enum NodeType { Minor, Major }
public enum PortalType { None, NextScene, NextChapter }

[RequireComponent(typeof(Collider2D))]
public class StarNode : MonoBehaviour
{
    [Header("Identity & System")]
    public string nodeID;
    public NodeType nodeType = NodeType.Minor;
    [Tooltip("Kelompokkan node berdasarkan Chapter. Misal: Semua node di area Chapter 1 set ID = 1")]
    public int chapterID = 1;

    public bool isActivated = false;
    private bool _hasBeenUsed = false;
    public bool HasBeenUsed => _hasBeenUsed;

    [Header("UI & Hints")]
    public string starName = "Unknown Star";
    public bool showNameText = true;
    public TMP_Text starNameTextUI;

    [Header("Major Node Configuration")]
    public bool autoReleaseOnComplete = true;
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 150f;

    [Header("Portal Configuration")]
    public PortalType portalType = PortalType.None;
    public string nextSceneName;
    public Transform chapterTargetPoint;
    public float finishSequenceDelay = 1.5f;

    [Header("References & Visuals")]
    public StageSummaryController summaryUI;
    public SpriteRenderer starSprite;
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.gray;
    public GameObject activationParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip igniteSound;
    public AudioClip portalSound;
    public AudioClip accessDeniedSound;

    [Header("Events")]
    public UnityEvent<StarNode> OnNodeIgnited;

    private Collider2D _nodeCollider;

    private void Awake()
    {
        _nodeCollider = GetComponent<Collider2D>();
        if (starSprite == null) starSprite = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (starNameTextUI != null)
        {
            starNameTextUI.text = starName;
            starNameTextUI.gameObject.SetActive(showNameText);
        }

        if (GameManager.Instance != null && GameManager.Instance.hasCheckpoint)
        {
            if (Vector2.Distance(transform.position, GameManager.Instance.lastCheckpointPos) < 0.1f)
            {
                isActivated = true;
                _hasBeenUsed = true;
            }
        }
        UpdateVisuals();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasBeenUsed || !collision.CompareTag("Player")) return;

        var playerMovement = collision.GetComponent<PlayerMovementInput>();
        if (playerMovement == null) return;

        // --- [ LOGIKA RESPAWN & CHECKPOINT ] ---
        // HANYA Minor Node yang boleh merubah titik Checkpoint
        if (nodeType == NodeType.Minor)
        {
            PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
            if (playerRespawn != null)
            {
                playerRespawn.currentCheckpoint = transform.position;
            }
        }

        // --- [ LOGIKA INTERAKSI ] ---
        if (nodeType == NodeType.Major)
        {
            if (!playerMovement.isOrbiting)
            {
                playerMovement.EnterOrbit(transform, orbitRadius, orbitSpeed, true);
            }
        }
        else
        {
            IgniteNode();
        }
    }

    public void IgniteNode()
    {
        if (isActivated) return;

        // Update di GameManager juga hanya jika Minor
        if (nodeType == NodeType.Minor && GameManager.Instance != null)
        {
            if (Vector2.Distance(GameManager.Instance.lastCheckpointPos, transform.position) > 0.1f)
            {
                GameManager.Instance.SetCheckpoint(transform.position);
            }
        }

        ActivateSystem();
    }

    public void ResetNode()
    {
        isActivated = false;
        _hasBeenUsed = false;
        if (_nodeCollider != null) _nodeCollider.enabled = true;
        UpdateVisuals();

        if (ConstellationManager.Instance != null)
            ConstellationManager.Instance.OnStarReset(nodeID);
    }

    public void OnOrbitFinished()
    {
        _hasBeenUsed = true;
        isActivated = true;

        if (_nodeCollider != null) _nodeCollider.enabled = false;

        if (ConstellationManager.Instance != null) ConstellationManager.Instance.OnStarCollected(nodeID);
        if (GameManager.Instance != null) GameManager.Instance.LogNodeCollection(NodeType.Major);

        UpdateVisuals();
        CheckAndTriggerPortal();
    }

    private void CheckAndTriggerPortal()
    {
        if (portalType == PortalType.None) return;

        if (CheckConstellationCompleteness())
        {
            PlaySound(portalSound);
            if (portalType == PortalType.NextScene)
                StartCoroutine(DelayedStageSummary());
            else if (portalType == PortalType.NextChapter)
                HandleTeleport();
        }
        else
        {
            PlaySound(accessDeniedSound);
        }
    }

    private bool CheckConstellationCompleteness()
    {
        StarNode[] allNodes = FindObjectsByType<StarNode>(FindObjectsSortMode.None);
        foreach (StarNode node in allNodes)
        {
            if (node.nodeType == NodeType.Major && node.chapterID == this.chapterID && !node.HasBeenUsed)
                return false;
        }
        return true;
    }

    private IEnumerator DelayedStageSummary()
    {
        yield return new WaitForSeconds(finishSequenceDelay);
        if (summaryUI == null)
            summaryUI = FindFirstObjectByType<StageSummaryController>(FindObjectsInactive.Include);

        if (summaryUI != null)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
                summaryUI.nextSceneName = this.nextSceneName;
            summaryUI.StartSummarySequence();
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void HandleTeleport()
    {
        if (chapterTargetPoint == null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        PlayerMovementInput playerScript = playerObj.GetComponentInChildren<PlayerMovementInput>();
        if (playerScript != null)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TeleportPlayer(chapterTargetPoint.position);
            else
                playerObj.transform.position = chapterTargetPoint.position;

            playerScript.ExitOrbit(Vector2.zero, 0f);
        }
    }

    private void ActivateSystem()
    {
        isActivated = true;
        _hasBeenUsed = true;
        UpdateVisuals();
        PlaySound(igniteSound);

        if (activationParticles != null)
            Instantiate(activationParticles, transform.position, Quaternion.identity);

        if (ConstellationManager.Instance != null) ConstellationManager.Instance.OnStarCollected(nodeID);

        // Logika collection tetap dipisah antara Major dan Minor
        if (GameManager.Instance != null) GameManager.Instance.LogNodeCollection(nodeType);

        OnNodeIgnited?.Invoke(this);
    }

    private void UpdateVisuals()
    {
        if (starSprite != null)
            starSprite.color = (isActivated || _hasBeenUsed) ? activeColor : inactiveColor;

        if (starNameTextUI != null)
            starNameTextUI.color = (isActivated || _hasBeenUsed) ? activeColor : inactiveColor;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmos()
    {
        if (nodeType == NodeType.Major)
        {
            Gizmos.color = (portalType != PortalType.None) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, orbitRadius);

            if (portalType == PortalType.NextChapter && chapterTargetPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, chapterTargetPoint.position);
                Gizmos.DrawSphere(chapterTargetPoint.position, 0.2f);
            }
        }
    }
}