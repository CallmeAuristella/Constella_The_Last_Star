using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Selector : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 15f;
    public bool enableBobbing = true;
    public float bobSpeed = 5f;
    public float bobAmount = 5f;

    [Header("Targeting")]
    [Tooltip("Nama object anak di dalam tombol yang jadi target posisi")]
    public string mountPointName = "SelectorPos";

    private GameObject currentSelection;
    private Vector3 targetPosition;
    private bool hasTarget = false;

    private void Update()
    {
        // 1. Cek tombol apa yang lagi dipilih
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        if (selectedObj != null && selectedObj != currentSelection)
        {
            currentSelection = selectedObj;

            // Coba cari titik parkir di dalam tombol tersebut
            Transform mountPoint = currentSelection.transform.Find(mountPointName);

            if (mountPoint != null)
            {
                // Kalau ada, jadikan itu targetnya
                targetPosition = mountPoint.position;
                hasTarget = true;
            }
            else
            {
                // Kalau lupa bikin titik parkir, dia bakal diem di tengah tombol (Fallback)
                targetPosition = currentSelection.transform.position;
                hasTarget = true;
            }
        }

        // 2. Gerakkan Selector
        if (hasTarget && currentSelection != null)
        {
            // Update posisi target terus menerus (biar kalau tombol gerak/animasi, dia ngikut)
            Transform mountPoint = currentSelection.transform.Find(mountPointName);
            Vector3 finalTarget = (mountPoint != null) ? mountPoint.position : currentSelection.transform.position;

            // Tambah efek bobbing (naik turun)
            if (enableBobbing)
            {
                finalTarget.y += Mathf.Sin(Time.unscaledTime * bobSpeed) * bobAmount;
            }

            // Lerp Movement
            transform.position = Vector3.Lerp(transform.position, finalTarget, moveSpeed * Time.unscaledDeltaTime);
        }
    }
}