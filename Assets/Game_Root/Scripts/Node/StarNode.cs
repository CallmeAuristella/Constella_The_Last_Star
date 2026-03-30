using UnityEngine;
using UnityEngine.Events;
using TMPro;

public enum NodeType { Minor, Major }

[RequireComponent(typeof(Collider2D))]
public class StarNode : MonoBehaviour
{
    [Header("Identity")]
    public int nodeID;
    public NodeType nodeType = NodeType.Minor;

    [Header("State")]
    public bool isActivated = false;
    private bool hasBeenUsed = false;

    [Header("UI")]
    public string starName = "Unknown Star";
    public bool showNameText = true;
    public TMP_Text starNameTextUI;

    [Header("Major Node (Orbit)")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 150f;

    [Header("Visual")]
    public SpriteRenderer starSprite;
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.gray;
    public GameObject activeAuraObject;

    [Header("Events")]
    public UnityEvent<StarNode> OnNodeActivated;

    private Collider2D nodeCollider;

    // =========================
    // INIT
    // =========================

    private void Awake()
    {
        nodeCollider = GetComponent<Collider2D>();

        if (starSprite == null)
            starSprite = GetComponent<SpriteRenderer>();

        ResetNode();
    }

    private void Start()
    {
        InitializeUI();
        UpdateVisuals();
    }

    private void InitializeUI()
    {
        if (starNameTextUI != null)
        {
            starNameTextUI.text = starName;
            starNameTextUI.gameObject.SetActive(showNameText);
        }
    }

    // =========================
    // INTERACTION
    // =========================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenUsed) return;
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerMovementInput>();
        if (player == null) return;

        if (nodeType == NodeType.Minor)
        {
            HandleMinorNode(other);
        }
        else if (nodeType == NodeType.Major)
        {
            if (!player.isOrbiting)
            {
                player.EnterOrbit(transform, orbitRadius, orbitSpeed, true);
            }
        }
    }

    private void HandleMinorNode(Collider2D player)
    {
        UpdateCheckpoint(player);
        ActivateNode();
    }

    public void OnOrbitFinished()
    {
        ActivateNode();

        if (nodeCollider != null)
            nodeCollider.enabled = false;
    }

    // =========================
    // CORE LOGIC
    // =========================

    private void ActivateNode()
    {
        if (isActivated) return;

        isActivated = true;
        hasBeenUsed = true;

        UpdateVisuals();

        // 🔥 ONLY DATA FLOW (NO GAME PROGRESSION)
        ConstellationManager.Instance.OnStarCollected(nodeID);
        GameManager.Instance?.LogNodeCollection(nodeType);

        OnNodeActivated?.Invoke(this);


    }

    public void Activate()
    {
        ActivateNode();
    }

    private void UpdateCheckpoint(Collider2D player)
    {
        PlayerRespawn respawn = player.GetComponent<PlayerRespawn>();
        if (respawn != null)
            respawn.currentCheckpoint = transform.position;

        GameManager.Instance?.SetCheckpoint(transform.position);
    }

    // =========================
    // VISUAL
    // =========================

    private void UpdateVisuals()
    {
        bool active = isActivated || hasBeenUsed;

        if (starSprite != null)
            starSprite.color = active ? activeColor : inactiveColor;

        if (starNameTextUI != null)
            starNameTextUI.color = active ? activeColor : inactiveColor;

        if (activeAuraObject != null)
            activeAuraObject.SetActive(active);
    }

    private void ForceVisualOff()
    {
        if (starSprite != null)
            starSprite.color = inactiveColor;

        if (starNameTextUI != null)
            starNameTextUI.color = inactiveColor;

        if (activeAuraObject != null)
            activeAuraObject.SetActive(false);
    }

    public void ResetNode()
    {
        isActivated = false;
        hasBeenUsed = false;

        if (nodeCollider != null)
            nodeCollider.enabled = true;

        ForceVisualOff();
    }

    // =========================
    // DEBUG
    // =========================

    private void OnDrawGizmos()
    {
        if (nodeType == NodeType.Major)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, orbitRadius);
        }
    }
}