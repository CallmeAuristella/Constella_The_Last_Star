using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    [Header("Data")]
    public List<ConstellationData> database;

    [Header("Setup Grid")]
    public Transform gridContainer;
    public GameObject itemPrefab;

    [Header("Setup Detail Popup")]
    public GameObject detailPopup;
    public Image infographicDisplay;
    public TextMeshProUGUI titleText;

    private void Start()
    {
        RefreshGallery();
    }

    // Fungsi Vital: Jangan diubah namanya agar sinkron dengan GameManager
    public void RefreshGallery()
    {
        // Bersihkan tombol lama
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        foreach (var data in database)
        {
            GameObject btnObj = Instantiate(itemPrefab, gridContainer);

            Image icon = btnObj.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI label = btnObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            Button btn = btnObj.GetComponent<Button>();

            // --- SINKRONISASI KEY ---
            // Menggunakan format yang sama dengan logika save game lo
            bool isUnlocked = PlayerPrefs.GetInt($"Stage_{data.requiredStageIndex}_Complete", 0) == 1;

            if (isUnlocked)
            {
                icon.sprite = data.iconUnlocked;
                label.text = data.displayName;
                icon.color = Color.white;
                btn.interactable = true; // Pastikan bisa diklik
                btn.onClick.AddListener(() => OpenDetail(data));
            }
            else
            {
                icon.sprite = data.iconLocked;
                label.text = "???";
                btn.interactable = false;
                icon.color = Color.gray;
            }
        }
        Debug.Log("[Gallery] Gallery Refreshed and Sync with PlayerPrefs.");
    }

    void OpenDetail(ConstellationData data)
    {
        detailPopup.SetActive(true);
        infographicDisplay.sprite = data.infographicImage;
        titleText.text = data.displayName;
        infographicDisplay.preserveAspect = true;
    }

    public void CloseDetail()
    {
        detailPopup.SetActive(false);
    }
}