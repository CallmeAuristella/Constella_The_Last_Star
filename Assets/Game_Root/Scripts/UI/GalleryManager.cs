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
    public Image infographicDisplay; // Tempat naruh gambar gede
    public TextMeshProUGUI titleText; // Judul doang

    private void Start()
    {
        RefreshGallery();
    }

    public void RefreshGallery()
    {
        // Bersihkan tombol lama
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        foreach (var data in database)
        {
            GameObject btnObj = Instantiate(itemPrefab, gridContainer);

            // Setup Visual Tombol
            // Pastikan nama child di Prefab lo bener ("Image" dan "Text (TMP)")
            Image icon = btnObj.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI label = btnObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            Button btn = btnObj.GetComponent<Button>();

            // Cek Unlock
            bool isUnlocked = PlayerPrefs.GetInt($"Stage_{data.requiredStageIndex}_Complete", 0) == 1;

            if (isUnlocked)
            {
                icon.sprite = data.iconUnlocked;
                label.text = data.displayName;
                icon.color = Color.white;
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
    }

    void OpenDetail(ConstellationData data)
    {
        detailPopup.SetActive(true);
        // Cuma ganti Gambar dan Judul
        infographicDisplay.sprite = data.infographicImage;
        titleText.text = data.displayName;

        // PENTING: Preserve Aspect biar gambar infografis lo gak gepeng
        infographicDisplay.preserveAspect = true;
    }

    public void CloseDetail()
    {
        detailPopup.SetActive(false);
    }
}