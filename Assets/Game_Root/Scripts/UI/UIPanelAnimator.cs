using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelAnimator : MonoBehaviour
{
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.9f, 0.9f, 0.9f);

    private CanvasGroup canvasGroup;
    private RectTransform rect;
    private Coroutine currentRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }

    public void Show()
    {
        // Pastikan object aktif sebelum menjalankan coroutine
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        // Jika sudah inactive, jangan jalankan coroutine
        if (!gameObject.activeSelf)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(HideRoutine());
    }

    IEnumerator ShowRoutine()
    {
        float t = 0f;

        rect.localScale = hiddenScale;
        canvasGroup.alpha = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, p);
            rect.localScale = Vector3.Lerp(hiddenScale, Vector3.one, p);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rect.localScale = Vector3.one;
        currentRoutine = null;
    }

    IEnumerator HideRoutine()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, p);
            rect.localScale = Vector3.Lerp(Vector3.one, hiddenScale, p);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        currentRoutine = null;
    }
}