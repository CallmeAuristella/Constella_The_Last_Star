using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public enum NodeType { Minor, Major }
public enum PortalType { None, NextScene }

[RequireComponent(typeof(Collider2D))]
public class StarNode : MonoBehaviour
{
    [Header("Identity & System")]
    public string nodeID;
    public NodeType nodeType = NodeType.Minor;

    public bool isActivated = false;
    private bool _hasBeenUsed = false;
    public bool HasBeenUsed => _hasBeenUsed;

    [Header("UI & Hints")]
    public string starName = "Unknown Star";
    public bool showNameText = true;
    public TMP_Text starNameTextUI;

    [Header("Major Node Configuration")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 150f;

    [Header("Portal Configuration")]
    public PortalType portalType = PortalType.None;
    public string nextSceneName;
    public float finishSequenceDelay = 1.5f;

    [Header("References & Visuals")]
    public StageSummaryController summaryUI;
    public SpriteRenderer starSprite;
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.gray;

    [Header("Visuals")]
    public GameObject activeAuraObject;

    [Header("Events")]
    public UnityEvent<StarNode> OnNodeIgnited;

    private Collider2D _nodeCollider;

    private void Awake()
    {
        _nodeCollider = GetComponent<Collider2D>();
        if (starSprite == null) starSprite = GetComponent<SpriteRenderer>();

        // 🔥 FORCE RESET STATE (ANTI BUG)
        isActivated = false;
        _hasBeenUsed = false;

        ForceVisualOff(); // 🔥 INI KUNCI FIX LU
    }

    private void Start()
    {
        InitializeNode();
    }

    private void InitializeNode()
    {
        if (starNameTextUI != null)
        {
            starNameTextUI.text = starName;
            starNameTextUI.gameObject.SetActive(showNameText);
        }

        UpdateVisuals(); // sinkron state
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasBeenUsed || !collision.CompareTag("Player")) return;

        var playerMovement = collision.GetComponent<PlayerMovementInput>();
        if (playerMovement == null) return;

        if (nodeType == NodeType.Minor)
        {
            UpdateCheckpoint(collision);
            IgniteNode();
        }
        else if (nodeType == NodeType.Major && !playerMovement.isOrbiting)
        {
            playerMovement.EnterOrbit(transform, orbitRadius, orbitSpeed, true);
        }
    }

    public void IgniteNode()
    {
        if (isActivated) return;
        ActivateSystem();
    }

    private void ActivateSystem()
    {
        isActivated = true;
        _hasBeenUsed = true;

        UpdateVisuals();

        ConstellationManager.Instance?.OnStarCollected(nodeID);
        GameManager.Instance?.LogNodeCollection(nodeType);

        OnNodeIgnited?.Invoke(this);
    }

    public void OnOrbitFinished()
    {
        ActivateSystem();

        if (_nodeCollider != null)
            _nodeCollider.enabled = false;

        CheckAndTriggerPortal();
    }

    private void UpdateVisuals()
    {
        bool visualActive = isActivated || _hasBeenUsed;

        Color targetColor = visualActive ? activeColor : inactiveColor;

        if (starSprite != null)
            starSprite.color = targetColor;

        if (starNameTextUI != null)
            starNameTextUI.color = targetColor;

        if (activeAuraObject != null)
            activeAuraObject.SetActive(visualActive);
    }

    // 🔥 FIX UTAMA
    private void ForceVisualOff()
    {
        if (activeAuraObject != null)
            activeAuraObject.SetActive(false);

        if (starSprite != null)
            starSprite.color = inactiveColor;

        if (starNameTextUI != null)
            starNameTextUI.color = inactiveColor;
    }

    public void ResetNode()
    {
        isActivated = false;
        _hasBeenUsed = false;

        if (_nodeCollider != null)
            _nodeCollider.enabled = true;

        ForceVisualOff(); // 🔥 penting
        UpdateVisuals();
    }

    private void UpdateCheckpoint(Collider2D player)
    {
        PlayerRespawn respawn = player.GetComponent<PlayerRespawn>();
        if (respawn != null)
            respawn.currentCheckpoint = transform.position;

        GameManager.Instance?.SetCheckpoint(transform.position);
    }

    private void CheckAndTriggerPortal()
    {
        if (portalType == PortalType.NextScene && IsConstellationComplete())
        {
            StartCoroutine(DelayedStageSummary());
        }
    }

    private bool IsConstellationComplete()
    {
        StarNode[] allNodes = FindObjectsByType<StarNode>(FindObjectsSortMode.None);

        foreach (StarNode node in allNodes)
        {
            if (node.nodeType == NodeType.Major && !node.HasBeenUsed)
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

    private void OnDrawGizmos()
    {
        if (nodeType == NodeType.Major)
        {
            Gizmos.color = (portalType != PortalType.None) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, orbitRadius);
        }
    }
}