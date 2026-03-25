using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    private bool isInSummaryMode = false;
    private bool isActivated = false;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        SetInactiveInstant();
    }

    private void Update()
    {
        if (isInSummaryMode) return; // 🔥 FIX UTAMA

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
    // 🔹 CORE VISUAL (GAMEPLAY REALTIME)
    // ======================================================

    public void SetActiveInstant()
    {
        if (targetImage == null) return;

        targetImage.color = activeColor;
    }

    public void SetInactiveInstant()
    {
        if (targetImage == null) return;

        targetImage.color = inactiveColor;
    }

    // ======================================================
    // 🔹 ANIMATION (UNTUK SUMMARY)
    // ======================================================

    public void ActivateNodeAnimated()
    {
        StopAllCoroutines();

        isInSummaryMode = true; 
        isActivated = true;

        gameObject.SetActive(true);

        if (targetImage != null)
            targetImage.color = activeColor;

        transform.localScale = Vector3.zero;

        StartCoroutine(AnimatePop());
    }

    private IEnumerator AnimatePop()
    {
        float t = 0f;
        float duration = 0.25f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(0f, 1f, t / duration);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    // ======================================================
    // 🔹 RESET (UNTUK SUMMARY INIT)
    // ======================================================

    public void ResetUI()
    {
        StopAllCoroutines();

        isInSummaryMode = true; // 🔥 TAMBAH INI
        isActivated = false;

        transform.localScale = Vector3.one * 0.5f;

        if (targetImage != null)
            targetImage.color = inactiveColor;

        gameObject.SetActive(true);
    }
    public void SetGameplayMode()
    {
        isInSummaryMode = false;
    }
}