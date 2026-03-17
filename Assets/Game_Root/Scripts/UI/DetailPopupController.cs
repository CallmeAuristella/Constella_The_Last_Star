using UnityEngine;
using UnityEngine.UI;

public class DetailPopupController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image infographicDisplay;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("DEBUG")]
    [SerializeField] private bool debugUseDummyPages = false;
    [SerializeField] private Sprite[] debugPages;

    private ConstellationData currentData;
    private int currentPage = 0;

    public void Show(ConstellationData data)
    {
        currentData = data;
        currentPage = 0;
        // DEBUG MODE
        if (debugUseDummyPages && debugPages != null && debugPages.Length > 0)
        {
            infographicDisplay.sprite = debugPages[currentPage];
            prevButton.interactable = false;
            nextButton.interactable = debugPages.Length > 1;
            gameObject.SetActive(true);
            return;
        }

        UpdatePage();
        gameObject.SetActive(true);
    }

    public void NextPage()
    {
        if (debugUseDummyPages)
        {
            if (currentPage < debugPages.Length - 1)
            {
                currentPage++;
                infographicDisplay.sprite = debugPages[currentPage];
            }

            prevButton.interactable = currentPage > 0;
            nextButton.interactable = currentPage < debugPages.Length - 1;
            return;
        }

        if (currentData == null) return;

        if (currentPage < currentData.infographicPages.Length - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PrevPage()
    {
        if (debugUseDummyPages)
        {
            if (currentPage > 0)
            {
                currentPage--;
                infographicDisplay.sprite = debugPages[currentPage];
            }

            prevButton.interactable = currentPage > 0;
            nextButton.interactable = currentPage < debugPages.Length - 1;
            return;
        }

        if (currentData == null) return;

        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        if (currentData == null || currentData.infographicPages.Length == 0)
            return;

        infographicDisplay.sprite = currentData.infographicPages[currentPage];
        infographicDisplay.preserveAspect = true;

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < currentData.infographicPages.Length - 1;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}