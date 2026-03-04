using UnityEngine;
using UnityEngine.EventSystems; // Wajib ada

// Script ini bikin tombol otomatis terpilih pas kena mouse
public class UI_ButtonHover : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    // Pas Mouse Masuk Area Tombol
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Suruh EventSystem milih tombol ini
        EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    // Pas Tombol Terpilih (baik via Mouse atau Keyboard/Gamepad)
    public void OnSelect(BaseEventData eventData)
    {
        // Opsional: Bisa pasang SFX "Ting!" disini kalau mau
        // AudioManager.Instance.PlaySFX("UI_Hover"); 
    }
}