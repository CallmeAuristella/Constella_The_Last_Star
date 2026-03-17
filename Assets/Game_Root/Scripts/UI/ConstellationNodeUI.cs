using UnityEngine;
using UnityEngine.UI;

public class ConstellationNodeUI : MonoBehaviour
{
    [Header("Identity")]
    public string nodeID;

    [Header("Reference")]
    public Image targetImage;

    [Header("Visual")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.2f);

    private bool lastState = false;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        SetInactiveInstant();
    }

    private void Update()
    {
        if (ConstellationManager.Instance == null) return;

        bool isActive = ConstellationManager.Instance.IsCollected(nodeID);

        if (isActive != lastState)
        {
            lastState = isActive;

            if (isActive)
                SetActiveInstant();
            else
                SetInactiveInstant();
        }
    }

    // ======================================================
    // 🔹 CORE VISUAL
    // ======================================================

    public void SetActiveInstant()
    {
        targetImage.color = activeColor;
    }

    public void SetInactiveInstant()
    {
        targetImage.color = inactiveColor;
    }

    // ======================================================
    // 🔹 ANIMATION (UNTUK SUMMARY)
    // ======================================================

    public void ActivateNodeAnimated()
    {
        SetActiveInstant();

        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.25f).setEaseOutBack();
    }

    // ======================================================
    // 🔹 RESET
    // ======================================================

    public void ResetUI()
    {
        lastState = false;
        SetInactiveInstant();
    }
}