using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DetailPopupController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image infographicDisplay;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private GameObject firstSelectedButton;

    private GameObject previousSelected;

    [Header("DEBUG")]
    [SerializeField] private bool debugUseDummyPages = false;
    [SerializeField] private Sprite[] debugPages;



    private ConstellationData currentData;
    private int currentPage = 0;

    public void Show(ConstellationData data) {
        previousSelected =
            EventSystem.current.currentSelectedGameObject;
        UISelectionVisual.ForceControllerMode();

        currentData = data;
        currentPage = 0;

        gameObject.SetActive(true);

        if (debugUseDummyPages && debugPages != null && debugPages.Length > 0) {
            infographicDisplay.sprite = debugPages[currentPage];
        } else {
            UpdatePage();
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
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

        
        nextButton.interactable = currentPage < currentData.infographicPages.Length - 1;
    }

    public void Close() {
        gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if (previousSelected != null) {
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }
    }
}